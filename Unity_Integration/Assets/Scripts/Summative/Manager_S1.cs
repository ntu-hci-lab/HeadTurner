using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using UnityEditor.UI;


public class Manager_Summative_T1 : MonoBehaviour
{
    // Track Setting
    private LineRenderer Track;
    private readonly int segments = 50;
    private readonly float radius = 10f;
    private readonly float lineWidth = 0.6f;
    private Color lineColor = new Color(0.1f, 0.5f, 0.9f, 0.8f);
    private Color triggerColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
    private Color completeColor = new Color(0.9f, 0.1f, 0.1f, 0.8f);
    public Material lineMaterial;
    private MeshCollider Collider;

    // Objects
    public GameObject Message;
    private TextMeshProUGUI MessageText;
    public GameObject Cam;
    public GameObject Viewport;
    public GameObject BodyAnchor;

    // Game Control
    private bool isRedirect = false;
    private bool isReadyForNextTask = false;
    private bool isHitTrack = false;
    private bool isTesting = false;
    private bool isResting = false;
    private bool isAllTaskComplete = false;
    private int tcount = 1; // count for each task
    private int count = 0; // total task count

    // Redirect Setting
    private Quaternion StartRotation;
    private Vector3 StartPosition;

    // Data Setting
    public enum ConditionE { NormalBed, ActuatedBed }
    public Dictionary<string, int> DirectionDict = new()
    {
        {"Right", 0},
        {"Left", 180},
        {"Up", 90},
        {"Down", 270}
    };

    [Header("Task Setting")]
    public int ParticipantID = 0;
    public ConditionE Condition = ConditionE.NormalBed;
    private List<string> DirectionList = new();
    public bool enableEmg = false;

    // Data Recording
    private Vector3 HeadStartVector, BodyStartVector;
    private float MaxViewingRange, MaxBodyRange;

    // CSV File Setting
    [Header("File Setting")]
    public string MaterialsFolder = @"Materials";
    public string ResultFolder = @"Result S";
    private FileStream fs;
    private StreamWriter sw;

    // Emg
    private EMGLogger_O _emg_logger;

    void Start()
    {
        if (Condition == ConditionE.ActuatedBed)
        {
            FindObjectOfType<HeadController>().enableActuation = true;
        }
        else
        {
            FindObjectOfType<HeadController>().enableActuation = false;
        }
        MessageText = Message.GetComponent<TextMeshProUGUI>();

        // Reading Task Order
        string orderFilePath = Path.Combine(MaterialsFolder, "Summative_S1_Order.csv");
        if (File.Exists(orderFilePath)) {
            using (var reader = new StreamReader(orderFilePath)) {
                reader.ReadLine(); // skip header
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    int participants = int.Parse(values[0]);
                    string condition = values[1];

                    if(participants == ParticipantID && condition == Condition.ToString()) {
                        for (int i = 2; i < values.Length; i++) {
                            DirectionList.Add(values[i]);
                        }
                        Debug.Log("Read Direction Order: " + string.Join(", ", DirectionList));
                        break;
                    }
                }
            }
        }

        // Setting Result File
        string resultFilePath = Path.Combine(ResultFolder, "Summative_T1_P" + ParticipantID.ToString() + "_" + Condition.ToString() + ".csv");
        fs = new FileStream(resultFilePath, FileMode.OpenOrCreate);
        sw = new StreamWriter(fs);
        string Header = "Direction,tCount,MaxViewingRange,HeadStartVector,HeadEndVector,MaxBodyRange,BodyStartVector,BodyEndVector,StartRotation";
        sw.WriteLine(Header);
    
        // Emg
        if (enableEmg)
        {
            string emg_folder = Path.Combine(ResultFolder, "emg_data", "Summative_T1_P" + ParticipantID.ToString() + "_" + Condition.ToString());
            _emg_logger = new EMGLogger_O(dirname: emg_folder);
        }
    }

    void Update()
    {
        if (!isRedirect)
        {
            MessageText.text = "按下 [A] 鍵來重新定向";

            if (OVRInput.GetDown(OVRInput.Button.One)){
                StartPosition = Cam.transform.position;
                StartRotation = Cam.transform.rotation;
                Viewport.transform.position = StartPosition;
                Viewport.transform.rotation = StartRotation;
                isRedirect = true;
            }

            return;
        }

        if (isAllTaskComplete) {
            MessageText.text = "第一階段測試已全部完成，請稍待實驗人員指示";
            return;
        }

        if (isResting) {
            if (count >= DirectionList.Count) {
                isAllTaskComplete = true;

                // close log file
                sw.Close();
                fs.Close();
                if (enableEmg) _emg_logger.close();
                return;
            }

            MessageText.text = "此方向測試完成\n請稍待實驗人員指示";
            if (OVRInput.GetDown(OVRInput.Button.Two)) {
                isResting = false;
            }
            return;
        }

        if (isTesting) {
            if (Track == null) {
                Debug.LogWarning("Track is null");
                return;
            }

            // change color if touch the track
            if (isHitTrack) {
                Track.startColor = triggerColor;
                Track.endColor = triggerColor;
                isHitTrack = false;
            } else {
                Track.startColor = lineColor;
                Track.endColor = lineColor;
            }

            // if end task
            if (OVRInput.GetDown(OVRInput.Button.One)) {
                Track.startColor = completeColor;
                Track.endColor = completeColor;

                // log data
                Vector3 HeadEndVector = Camera.main.transform.forward;
                MaxViewingRange = Vector3.SignedAngle(HeadStartVector, HeadEndVector, Vector3.up);

                Vector3 BodyEndVector = BodyAnchor.transform.forward;
                MaxBodyRange = Vector3.SignedAngle(BodyStartVector, BodyEndVector, Vector3.up);

                string Data = $"{DirectionList[count]},{tcount},{MaxViewingRange},{HeadStartVector.ToString().Replace(",", "*")},{HeadEndVector.ToString().Replace(",", "*")},{MaxBodyRange},{BodyStartVector.ToString().Replace(",", "*")},{BodyEndVector.ToString().Replace(",", "*")},{StartRotation.eulerAngles.ToString().Replace(",", "*")}";
                sw.WriteLine(Data);

                if (enableEmg) _emg_logger.end_logging();

                tcount ++;
                if (tcount > 3) {
                    // Enter rest section
                    isResting = true;
                    tcount = 1;
                    count ++;
                }

                isTesting = false;
                isReadyForNextTask = false;
            }
        }
        else {
            if (isReadyForNextTask) {
                MessageText.text = "按下 [A] 鍵來開始第 " + (count+1).ToString() + " 個方向的第 " + tcount.ToString() +" 次測試";

                // start new task
                if (OVRInput.GetDown(OVRInput.Button.One)) {
                    int rotationAngle = DirectionDict[DirectionList[count]];

                    CreateNewTrack(rotationAngle, 240);

                    MessageText.text = "請沿著軌道方向旋轉到最大距離\n按下 [A] 鍵來結束測試";

                    HeadStartVector = Camera.main.transform.forward;
                    BodyStartVector = BodyAnchor.transform.forward;

                    if (enableEmg) _emg_logger.start_logging(rotationAngle.ToString(), Condition.ToString());

                    isTesting = true;
                }

                isReadyForNextTask = false;
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
            rotatedPoint += StartPosition;

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

    public void HitManager()
    {
        isHitTrack = true;
    }

    public void HitStartArea()
    {
        isReadyForNextTask = true;
    }

    public void OnDestroy()
    {
        sw.Close();
        fs.Close();
        if (enableEmg) _emg_logger.close();
    }
}