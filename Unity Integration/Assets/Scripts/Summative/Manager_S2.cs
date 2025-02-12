using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class Manager_S2 : MonoBehaviour
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

    // Objects
    public GameObject EndArea;
    public GameObject Message;
    private TextMeshProUGUI MessageText;
    public GameObject Cam;
    public GameObject Viewport;
    public OrientationUtility headOT, bodyOT;

    // Game Control
    private bool isRedirect = false;
    private bool isReadyForNextTask = false;
    private bool isHitTrack = false;
    private bool isTesting = false;
    private bool isCurrentTaskComplete = false;
    private bool isResting = false;
    private bool isAllTaskComplete = false;
    private int tcount = 1; // count for each task
    private int count = 0; // total task count

    // isRedirect Setting
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
    public Dictionary<string, int> RangeDict = new(){
        {"Right", 65},
        {"Left", 65},
        {"Up", 45},
        {"Down", 55}
    };

    [Header("Task Setting")]
    public int ParticipantID = 0;
    public ConditionE Condition = ConditionE.NormalBed;
    private List<string> DirectionList = new();
    public bool enableEmg = false;

    // Data Recording
    private float Interval = 0.02f;
    private float Timer = 0f;

    // CSV File Setting
    [Header("File Setting")]
    public string MaterialsFolder = @"Materials"; // for task order file
    public string ResultFolder = @"Result S";
    private FileStream fs, fs2;
    private StreamWriter sw, sw2;

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

        EndArea.transform.position = new Vector3(0, 20, 0);
        EndArea.GetComponent<Renderer>().enabled = false;

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
        string resultFilePath = Path.Combine(ResultFolder, "Summative_T2_P" + ParticipantID.ToString() + "_" + Condition.ToString() + ".csv");
        fs = new FileStream(resultFilePath, FileMode.OpenOrCreate);
        sw = new StreamWriter(fs);
        string Header = "Direction,tCount,Time,HeadPitch,HeadYaw,HeadRoll,BodyPitch,BodyYaw,BodyRoll";
        sw.WriteLine(Header);

        string incompleteTaskFilePath = Path.Combine(ResultFolder, "Summative_T2_P" + ParticipantID.ToString() + "_" + Condition.ToString() + "_Incomplete.csv");
        fs2 = new FileStream(incompleteTaskFilePath, FileMode.OpenOrCreate);
        sw2 = new StreamWriter(fs2);
        string Header2 = "Direction,tCount";
        sw2.WriteLine(Header2);

        // Emg
        if (enableEmg)
        {
            string emg_folder = Path.Combine(ResultFolder, "emg_data", "Summative_T2_P" + ParticipantID.ToString() + "_" + Condition.ToString());
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

        if (isAllTaskComplete)
        {
            MessageText.text = "此輪測試已全部完成，請稍待實驗人員指示";
            return;
        }

        if (isResting)
        {
            MessageText.text = "此方向測試完成，請稍待實驗人員指示來回答問題";

            if (OVRInput.GetDown(OVRInput.Button.Two))
            {

                isResting = false;
                count++;

                if (count >= DirectionList.Count)
                {
                    isAllTaskComplete = true;

                    // close log file
                    sw.Close();
                    fs.Close();
                    if (enableEmg) _emg_logger.close();
                }
            }
            return;
        }

        if (isTesting)
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
            if (isHitTrack)
            {
                Track.startColor = triggerColor;
                Track.endColor = triggerColor;
                isHitTrack = false;
            }
            else
            {
                Track.startColor = lineColor;
                Track.endColor = lineColor;
            }

            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                isCurrentTaskComplete = true;
                Debug.LogWarning("[Task Incomplete] " + DirectionList[count] + " " + tcount);
                sw2.WriteLine(DirectionList[count] + "," + tcount.ToString());
            }

            if (isCurrentTaskComplete)
            {
                Track.startColor = completeColor;
                Track.endColor = completeColor;

                tcount++;
                if (tcount > 3)
                {
                    // Enter rest section
                    isResting = true;
                    tcount = 1;
                }

                isTesting = false;
                isReadyForNextTask = false;
                isCurrentTaskComplete = false;
            }
        }
        else
        {
            if (isReadyForNextTask)
            {
                MessageText.text = "按下 [A] 鍵來開始第 " + (count + 1).ToString() + " 個方向的第 " + tcount.ToString() + " 次測試";

                // start new task
                if (OVRInput.GetDown(OVRInput.Button.One))
                {
                    int rotationAngle = DirectionDict[DirectionList[count]];
                    int viewingRange = RangeDict[DirectionList[count]];

                    CreateNewTrack(rotationAngle, viewingRange);

                    MessageText.text = "請沿著軌道方向旋轉到終點\n如果無法轉到，請按下 [B] 鍵來結束測試";

                    if (enableEmg) _emg_logger.start_logging(rotationAngle.ToString(), Condition.ToString());

                    isTesting = true;
                    isCurrentTaskComplete = false;
                }

                isReadyForNextTask = false;
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
            rotatedPoint += StartPosition;

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

    public void HitManager()
    {
        isHitTrack = true;
    }

    public void HitStartArea()
    {
        isReadyForNextTask = true;
    }

    public void HitEndArea()
    {
        isCurrentTaskComplete = true;
    }

    public void OnDestroy()
    {
        // close log file
        sw.Close();
        sw2.Close();
        fs.Close();
        fs2.Close();
        if (enableEmg) _emg_logger.close();
    }

    public void DataRecorder()
    {
        if (Timer - Time.deltaTime < 0)
        {
            Timer = Interval;

            if (count >= 0 && count < DirectionList.Count)
            {                
                string Data = $"{DirectionList[count]},{tcount},{Time.time},{headOT.PitchAngle},{headOT.YawAngle},{headOT.RollAngle},{bodyOT.PitchAngle},{bodyOT.YawAngle},{bodyOT.RollAngle}";
                sw.WriteLine(Data);
            }
        }
        else
        {
            Timer -= Time.deltaTime;
        }
    }
}