using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class Manager1 : MonoBehaviour
{
    private LineRenderer Track;
    private readonly int segments = 50;
    private readonly float radius = 10f;
    private readonly float lineWidth = 0.6f;
    private Color lineColor = new Color(0.1f, 0.5f, 0.9f, 0.8f);
    private Color triggerColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
    private Color completeColor = new Color(0.9f, 0.1f, 0.1f, 0.8f);
    public Material lineMaterial;
    private MeshCollider Collider;
    private bool hitTrack = false;

    // Objects
    public GameObject Message;
    private TextMeshProUGUI MessageText;
    public GameObject Cam;
    public GameObject Viewport;
    private Quaternion StartRotation;

    // Game Control
    private bool redirect = false;
    private bool ready = false; // if is ready to start new task (enter start area)
    private bool testing = false; // if is testing
    private bool resting = false; // if is resting
    private bool completeAll = false; // if all tasks are complete
    private int tcount = 1; // count for each task
    private int count = 0; // total task count

    private int frameCount = -1;
    private Quaternion AvgRotation;

    // Data Setting
    public enum PostureE { Standing, Lying }

    [Header("Task Setting")]
    public int ParticipantID = 0;
    public PostureE Posture = PostureE.Standing;
    private List<int> DirectionList = new();

    // Data Recording
    private Vector3 StartVector;
    private float MaxViewingRange;

    // CSV File Setting
    [Header("File Setting")]
    public string MaterialsFolder = @"Materials";
    public string ResultFolder = @"Result";
    private string FullPath = "";
    private FileStream fs;
    private StreamWriter sw;

    void Start()
    {
        MessageText = Message.GetComponent<TextMeshProUGUI>();

        // Reading Task Order
        FullPath = Path.Combine(MaterialsFolder, "Formative_Order.csv");
        if (File.Exists(FullPath)) {
            using (var reader = new StreamReader(FullPath)) {
                reader.ReadLine(); // skip header
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    int participants = int.Parse(values[0]);
                    string posture = values[1];

                    if(participants == ParticipantID && posture == Posture.ToString()) {
                        for (int i = 2; i < values.Length; i++) {
                            DirectionList.Add(int.Parse(values[i]));
                        }
                    }
                }
            }
        }

        // Setting Result File
        FullPath = Path.Combine(ResultFolder, "Formative_T1_P" + ParticipantID.ToString() + "_" + Posture.ToString() + ".csv");
        fs = new FileStream(FullPath, FileMode.OpenOrCreate);
        sw = new StreamWriter(fs);
        string Header = "Participant,Posture,tCount,Direction,MaxViewingRange,StartVector,EndVector,StartRotation";
        sw.WriteLine(Header);
    }

    void Update()
    {
        // Redirect
        if (!redirect)
        {
            MessageText.text = "按下 [A] 鍵來重新定向";

            if (frameCount < 0)
            {
                if (OVRInput.GetDown(OVRInput.Button.One))
                {
                    AvgRotation = Cam.transform.rotation;
                    frameCount = 0;
                }
            }
            else if (frameCount >= 10) {
                StartRotation = Quaternion.Lerp(AvgRotation, Cam.transform.rotation, 0.5f);
                Viewport.transform.Rotate(StartRotation.eulerAngles.x, StartRotation.eulerAngles.y, StartRotation.eulerAngles.z, Space.World);
                redirect = true;
            }
            else {
                AvgRotation = Quaternion.Lerp(AvgRotation, Cam.transform.rotation, 0.5f);
                frameCount ++;
            }
            return;
        }

        // All tasks are complete
        if (completeAll) {
            MessageText.text = "此輪測試已全部完成，請稍待實驗人員指示";
            return;
        }

        // Resting
        if (resting) {
            if (count >= DirectionList.Count) {
                completeAll = true;

                // close log file
                sw.Close();
                fs.Close();
                return;
            }

            MessageText.text = "此方向測試完成\n請稍待實驗人員指示";
            // if (Input.GetKeyDown("space")) {
            if (OVRInput.GetDown(OVRInput.Button.Two)) {
                resting = false;
            }
            return;
        }

        if (testing) {
            if (Track == null) {
                Debug.LogWarning("Track is null");
                return;
            }

            // change color if touch the track
            if (hitTrack) {
                Track.startColor = triggerColor;
                Track.endColor = triggerColor;
                hitTrack = false;
            } else {
                Track.startColor = lineColor;
                Track.endColor = lineColor;
            }

            // if end task
            if (OVRInput.GetDown(OVRInput.Button.One)) {
                Track.startColor = completeColor;
                Track.endColor = completeColor;

                // log data
                Vector3 EndVector = Camera.main.transform.forward;
                MaxViewingRange = Vector3.SignedAngle(StartVector, EndVector, Vector3.up);

                if (DirectionList[count] == 90 || DirectionList[count] == 270) {
                    MaxViewingRange = Mathf.Abs(MaxViewingRange);
                }
                // turn left
                else if (DirectionList[count] > 90 || DirectionList[count] < 270) {
                    MaxViewingRange = MaxViewingRange < 0 ? Mathf.Abs(MaxViewingRange) : 360 - MaxViewingRange;
                }
                // turn right
                else {
                    MaxViewingRange = MaxViewingRange < 0 ? 360 + MaxViewingRange : MaxViewingRange;
                }

                // if (count >= 0 && count < DirectionList.Count) {
                string Data = ParticipantID.ToString() + ',' + Posture.ToString() + ',' + tcount.ToString() + ','
                    + DirectionList[count].ToString() + ',' + MaxViewingRange.ToString() + ','
                    + StartVector.ToString().Replace(",", "*") + ',' + EndVector.ToString().Replace(",", "*") + ',' + StartRotation.eulerAngles.ToString().Replace(",", "*");

                sw.WriteLine(Data);
                // }

                tcount ++;
                if (tcount > 3) {
                    // Enter rest section
                    resting = true;
                    tcount = 1;
                    count ++;
                }

                testing = false;
                ready = false;
            }
        }
        else {
            // wait for ready
            if (ready) {
                MessageText.text = "按下 [A] 鍵來開始第 " + (count+1).ToString() + " 個方向的第 " + tcount.ToString() +" 次測試";

                // start new task
                if (OVRInput.GetDown(OVRInput.Button.One)) {
                    int rotationAngle = DirectionList[count];

                    CreateNewTrack(rotationAngle, 240);

                    MessageText.text = "請沿著軌道方向旋轉到最大距離\n按下 [A] 鍵來結束測試";

                    StartVector = Camera.main.transform.forward;

                    testing = true;
                }

                ready = false;
            } else {
                MessageText.text = "請回到起始區域";
            }
        }

        return;
    }

    public void CreateNewTrack(float rotationAngle, float viewingRange)
    {
        if (!gameObject.TryGetComponent<LineRenderer>(out Track))
        {
            Track = gameObject.AddComponent<LineRenderer>();
        }

        Track.positionCount = segments + 1;
        Track.startWidth = lineWidth;
        Track.endWidth = lineWidth;
        Track.startColor = lineColor;
        Track.endColor = lineColor;
        Track.material = lineMaterial;

        // Draw curve track
        float angleStep = viewingRange / segments;
        float currentAngle = 0f;

        for (int i = 0; i < segments + 1; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * radius;

            Vector3 rotatedPoint = StartRotation * Quaternion.Euler(0, 0, rotationAngle) * new Vector3(x, 0, z);

            Track.SetPosition(i, rotatedPoint);

            currentAngle += angleStep;
        }

        // Generate Mesh Collider
        if (!gameObject.TryGetComponent<MeshCollider>(out Collider))
        {
            Collider = gameObject.AddComponent<MeshCollider>();
        }
        Mesh mesh = new();
        Track.BakeMesh(mesh);
        Collider.sharedMesh = mesh;
    }

    public void HitTrack()
    {
        hitTrack = true;
    }

    public void HitArea()
    {
        ready = true;
    }

    public void OnDestroy()
    {
        sw.Close();
        fs.Close();
    }
}