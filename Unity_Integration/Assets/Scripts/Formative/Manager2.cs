using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class Manager2 : MonoBehaviour
{
    // Track Setting
    private LineRenderer Track;
    private readonly int segments = 50;
    private readonly float radius = 10f;
    private readonly float lineWidth = 0.8f;
    private Color lineColor = new(0.1f, 0.5f, 0.9f, 0.8f);
    private Color triggerColor = new(0.2f, 0.9f, 0.2f, 0.8f);
    private Color completeColor = new(0.9f, 0.1f, 0.1f, 0.8f);
    public Material lineMaterial;
    private MeshCollider Collider;
    private bool hitTrack = false;

    // Objects
    public GameObject EndArea;
    public GameObject TrunkAnchor;
    public GameObject Message;
    private TextMeshProUGUI MessageText;
    public GameObject Cam;
    public GameObject Viewport;
    private Quaternion StartRotation;

    // Game Control
    private bool redirect = false;
    private bool ready = false; // if is ready to start new task (enter start area)
    private bool testing = false; // if is testing
    private bool complete = false; // if current task is done (enter end area)
    private bool resting = false; // if is resting
    private bool completeAll = false; // if all tasks are complete
    private int tcount = 1; // count for each task
    private int count = 0; // total task count

    private int frameCount = -1;
    private Quaternion AvgRotation;

    // Data Setting
    public enum PostureE { Standing, Lying }
    public Dictionary<int, int> RangeDict = new(){
        {0, 55},
        {45, 45},
        {90, 35},
        {135, 45},
        {180, 55},
        {225, 45},
        {270, 35},
        {315, 45},
    };

    [Header("Task Setting")]
    public int ParticipantID = 0;
    public PostureE Posture = PostureE.Standing;
    private List<int> DirectionList = new();
    public bool enableTrunk = false;
    public bool enableEmg = false;

    // Data Recording
    private float Interval = 0.02f;
    private float Timer = 0f;
    private Vector3 HeadStartVector;
    private Vector3 TrunkStartVector;
    private float timestamp, hPitch, hYaw, tPitch, tYaw;

    // CSV File Setting
    [Header("File Setting")]
    public string MaterialsFolder = @"Materials"; // for task order file
    public string ResultFolder = @"Result";
    private string FullPath = "";
    private FileStream fs;
    private StreamWriter sw;

    // Emg
    private EMGLogger_O _emg_logger;


    void Start()
    {
        MessageText = Message.GetComponent<TextMeshProUGUI>();

        EndArea.transform.position = new Vector3(0, 20, 0);
        EndArea.GetComponent<Renderer>().enabled = false;

        // Reading Task Order
        FullPath = Path.Combine(MaterialsFolder, "Formative_Order.csv");
        if (File.Exists(FullPath))
        {
            using (var reader = new StreamReader(FullPath))
            {
                reader.ReadLine(); // skip header
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    int participants = int.Parse(values[0]);
                    string posture = values[1];

                    if (participants == ParticipantID && posture == Posture.ToString())
                    {
                        for (int i = 2; i < values.Length; i++)
                        {
                            DirectionList.Add(int.Parse(values[i]));
                        }
                    }
                }
            }
        }

        // Setting Result File
        FullPath = Path.Combine(ResultFolder, "Formative_O3_P" + ParticipantID.ToString() + "_" + Posture.ToString() + ".csv");
        fs = new FileStream(FullPath, FileMode.OpenOrCreate);
        sw = new StreamWriter(fs);
        string Header = "Direction,tCount,Time,HeadRotation,HeadPitch,HeadYaw,TrunkRotation,TrunkPitch,TrunkYaw,StartRotation";
        sw.WriteLine(Header);

        // Emg
        if (enableEmg)
        {
            string emg_folder = Path.Combine(ResultFolder, "emg_data", "Formative_O3_P" + ParticipantID.ToString() + "_" + Posture.ToString());
            _emg_logger = new EMGLogger_O(dirname: emg_folder);
        }
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
        if (completeAll)
        {
            MessageText.text = "此輪測試已全部完成，請稍待實驗人員指示";
            return;
        }

        // Resting
        if (resting)
        {
            MessageText.text = "此方向測試完成，請回答下方問題：\n請問剛才任務的費力程度為何？\n1分為完全不費力、7分為非常費力";

            // if (Input.GetKeyDown("space")) {
            if (OVRInput.GetDown(OVRInput.Button.Two))
            {

                resting = false;
                count++;

                if (count >= DirectionList.Count)
                {
                    completeAll = true;

                    // close log file
                    sw.Close();
                    fs.Close();
                    if (enableEmg) _emg_logger.close();
                }
            }
            return;
        }

        if (testing)
        {
            // log data
            DataRecorder();
            if (enableEmg) _emg_logger.end_logging();


            if (Track == null)
            {
                Debug.LogWarning("Track is null");
                return;
            }

            // change color if touch the track
            if (hitTrack)
            {
                Track.startColor = triggerColor;
                Track.endColor = triggerColor;
                hitTrack = false;
            }
            else
            {
                Track.startColor = lineColor;
                Track.endColor = lineColor;
            }


            // enter end area
            if (complete)
            {
                Track.startColor = completeColor;
                Track.endColor = completeColor;

                tcount++;
                if (tcount > 3)
                {
                    // Enter rest section
                    resting = true;
                    tcount = 1;
                }

                testing = false;
                ready = false;
                complete = false;
            }
        }
        else
        {
            // wait for ready
            if (ready)
            {
                MessageText.text = "按下 [A] 鍵來開始第 " + (count + 1).ToString() + " 個方向的第 " + tcount.ToString() + " 次測試";

                // start new task
                if (OVRInput.GetDown(OVRInput.Button.One))
                {
                    int rotationAngle = DirectionList[count];
                    int viewingRange = RangeDict[rotationAngle];

                    CreateNewTrack(rotationAngle, viewingRange);

                    MessageText.text = "請沿著軌道方向旋轉到終點";

                    HeadStartVector = Camera.main.transform.forward;

                    if (enableTrunk) TrunkStartVector = TrunkAnchor.transform.forward;
                    if (enableEmg) _emg_logger.start_logging(rotationAngle.ToString(), Posture.ToString());

                    testing = true;
                    complete = false;
                }

                ready = false;
            }
            else
            {
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

            if (i == segments)
            {
                EndArea.transform.position = rotatedPoint;
            }

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

    public void HitStartArea()
    {
        ready = true;
    }

    public void HitEndArea()
    {
        complete = true;
    }

    public void OnDestroy()
    {
        // close log file
        sw.Close();
        fs.Close();
        if (enableEmg) _emg_logger.close();
    }

    public void DataRecorder()
    {
        if (Timer - Time.deltaTime < 0)
        {
            Timer = Interval;

            timestamp = Time.time;
            Quaternion HeadDiffRotation = Camera.main.transform.rotation * Quaternion.Inverse(StartRotation);
            hPitch = HeadDiffRotation.eulerAngles.x;
            // turn down positive, turn up negative
            if (hPitch > 180) hPitch -= 360;
            hYaw = HeadDiffRotation.eulerAngles.y;
            // turn left positive, turn right negative
            if (hYaw > 180) hYaw -= 360;

            if (enableTrunk) {
                Quaternion TrunkDiffRotation = TrunkAnchor.transform.rotation * Quaternion.Inverse(StartRotation);
                tPitch = TrunkDiffRotation.eulerAngles.x;
                if (tPitch > 180) tPitch -= 360;
                tYaw = TrunkDiffRotation.eulerAngles.y;
                if (tYaw > 180) tYaw -= 360;
            }

            if (count >= 0 && count < DirectionList.Count)
            {
                string Data = DirectionList[count].ToString() + "," + tcount.ToString() + "," + timestamp.ToString() + ","
                    + Camera.main.transform.rotation.eulerAngles.ToString().Replace(",", "*") + "," + hPitch.ToString() + "," + hYaw.ToString() + ","
                    + TrunkAnchor.transform.rotation.eulerAngles.ToString().Replace(",", "*") + "," + tPitch.ToString() + "," + tYaw.ToString() + ","
                    + StartRotation.eulerAngles.ToString().Replace(",", "*");

                sw.WriteLine(Data);
            }
        }
        else
        {
            Timer -= Time.deltaTime;
        }
    }

    public static void ForwardToSpherical(Vector3 cartCoords, out float outPitch, out float outYaw)
    {
        // Radius is 1
        if (cartCoords.z == 0)
            cartCoords.z = Mathf.Epsilon;
        outPitch = Mathf.Atan(cartCoords.x / cartCoords.z) * Mathf.Rad2Deg;
        if (cartCoords.z < 0)
            outPitch += Mathf.PI;
        outYaw = Mathf.Asin(cartCoords.y / 1) * Mathf.Rad2Deg;
    }
}