///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
/// Reward2D.cs                                                                         ///
/// by Joshua Calugay                                                                   ///
/// jc10487@nyu.edu                                                                     ///
/// jcal1696@gmail.com (use this to contact me since, whoever is reading this is        ///
///                     probably my successor, which means I no longer work at          ///
///                     NYU, which means I no longer have my NYU email.)                ///
/// Last Updated: 6/17/2020                                                             ///
/// For the Angelaki Lab                                                                ///
///                                                                                     ///
/// <summary>                                                                           ///
/// This script takes care of the FF behavior.                                          ///
///                                                                                     ///
/// There are 3 modes: "ON, Flash, Fixed". ON means it's always on, Flash means it      ///
/// flashes based on user-specified frequency and duty cycle. Fixed means it stays on   ///
/// for a fixed amount of time that is specified by the user.                           ///
///                                                                                     ///
/// This code handles up to 5 FF. The code waits for the player to be completely still. ///
/// Once that condition is met, the FF(s) spawn. After a user-specified amount of time, ///
/// the trial will timeout, and the next one will begin once the player is completely   ///
/// still. If the trial hasn't timed out, the code waits for the player to start        ///
/// moving. Once the player moves, the code waits for the player to stop moving before  ///
/// checking the player's position against a FF. If the player ends up near a FF, they  ///
/// win; otherwise, they lose. This repeats until the user exits the application.       ///
/// </summary>                                                                          ///
///////////////////////////////////////////////////////////////////////////////////////////
#undef CALIBRATING
//#define MOTIONCUEING

using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
//using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static BallController;
using static JuiceController;
using static SyncController;
using static AccelerometerController;
using static MotionCueingController;
using static MotorController;
using static LabJackController;
using static RingSensor;
using static LickDetector;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;
//using System.IO.Ports;
//using System.Net.Sockets;
using System.Runtime.InteropServices;

public class RewardArena : MonoBehaviour
{
    public static RewardArena SharedReward;

    [DllImport("MotionCueing")] public static extern IntPtr Create();
    public ParticleSystem particleSystem;
    public GameObject firefly;
    public GameObject distalObject;
    public GameObject walls;
    public GameObject MainCamera;
    public Camera MonitorCam;
    public GameObject Menu;
    [Tooltip("Radius of firefly")]
    [HideInInspector] public float fireflySize;
    [Tooltip("Maximum distance allowed from center of firefly")]
    [HideInInspector] public float fireflyZoneRadius;
    // Enumerable experiment mode selector
    public enum Modes
    {
        ON,
        Flash,
        Fixed
    }
    public Modes mode;
    // Toggle for whether trial is an always on trial or not
    private bool toggle;
    [Tooltip("Ratio of trials that will have fireflies always on")]
    [HideInInspector] public float ratio;
    [Tooltip("Frequency of flashing firefly (Flashing Firefly Only)")]
    [HideInInspector] public float freq;
    [Tooltip("Duty cycle for flashing firefly (percentage of one period determing how long it stays on during one period) (Flashing Firefly Only)")]
    [HideInInspector] public float duty;
    // Pulse Width; how long in seconds it stays on during one period
    public float PW;
    public GameObject player;
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip neutralSound;
    public AudioClip loseSound;
    [Tooltip("Minimum distance firefly can spawn")]
    [HideInInspector] public float minDrawDistance;
    [Tooltip("Maximum distance firefly can spawn")]
    [HideInInspector] public float maxDrawDistance;
    [Tooltip("Minimum angle from forward axis that firefly can spawn")]
    [HideInInspector] public float minPhi;
    [Tooltip("Maximum angle from forward axis that firefly can spawn")]
    [HideInInspector] public float maxPhi;
    [Tooltip("Indicates whether firefly spawns more on the left or right; < 0.5 means more to the left, > 0.5 means more to the right, = 0.5 means equally distributed between left and right")]
    [HideInInspector] public float LR;
    [Tooltip("How long the firefly stays from the beginning of the trial (Fixed Firefly Only)")]
    [HideInInspector] public float lifeSpan;

    //[Tooltip("How many fireflies can appear at once")]
    //[HideInInspector] public float nFF;
    readonly public List<float> velocities = new List<float>();
    readonly public List<float> v_ratios = new List<float>();
    readonly public List<Vector3> directions = new List<Vector3>()
    {
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };
    readonly public List<float> durations = new List<float>();
    readonly public List<float> ratios = new List<float>();
    public bool isMoving;
    public bool LRFB;
    private Vector3 move;
    [Tooltip("Trial timeout (how much time player can stand still before trial ends")]
    [HideInInspector] public float timeout;
    [Tooltip("Minimum x value to plug into exponential distribution from which time to wait before check is pulled")]
    [HideInInspector] public float checkMin;
    [Tooltip("Maximum x value to plug into exponential distribution from which time to wait before check is pulled")]
    [HideInInspector] public float checkMax;
    [Tooltip("Minimum x value to plug into exponential distribution from which time to wait before new trial is pulled")]
    [HideInInspector] public float interMax;
    [Tooltip("Maximum x value to plug into exponential distribution from which time to wait before new trial is pulled")]
    [HideInInspector] public float interMin;
    [Tooltip("Player height")]
    [HideInInspector] public float p_height;
    // 1 / Mean for check time exponential distribution
    [HideInInspector] public float c_lambda;
    // 1 / Mean for intertrial time exponential distribution
    [HideInInspector] public float i_lambda;
    // x values for exponential distribution
    [HideInInspector] public float c_min;
    [HideInInspector] public float c_max;
    [HideInInspector] public float i_min;
    [HideInInspector] public float i_max;
    [HideInInspector] public float velMin;
    [HideInInspector] public float velMax;
    [HideInInspector] public float rotMin;
    [HideInInspector] public float rotMax;
    public enum Phases
    {
        begin = 0,
        trial = 1,
        check = 2,
        question = 3,
        none = 4,
        startup = 9,
        shutdown = 10
    }
    [HideInInspector] public Phases phase;
    
    private Vector3 pPos;
    private bool isTimeout = false;

    // Trial number
    //readonly List<int> trial = new List<int>();
    //readonly List<int> n = new List<int>();

    // Firefly on/off
    readonly List<bool> onoff = new List<bool>();

    // Firefly ON Duration
    readonly List<float> onDur = new List<float>();

    // Firefly Check Coords
    readonly List<string> ffPos = new List<string>();
    readonly List<string> ffPos_frame = new List<string>();
    readonly List<Vector3> ffPositions = new List<Vector3>();

    //// Player position at Check()
    //readonly List<string> cPos = new List<string>();

    //// Player rotation at Check()
    //readonly List<string> cRot = new List<string>();

    //// Player origin at beginning of trial
    //readonly List<string> origin = new List<string>();

    //// Player rotation at origin
    //readonly List<string> heading = new List<string>();

    //// Player position, continuous
    //readonly List<Vector3> position = new List<Vector3>();
    //readonly List<string> position_frame = new List<string>();

    //// Player rotation, continuous
    //readonly List<Quaternion> rotation = new List<Quaternion>();
    //readonly List<string> rotation_frame = new List<string>();

    //// Firefly position, continuous
    //readonly List<Vector3> f_position = new List<Vector3>();

    // Player linear and angular velocity
    //readonly List<float> v = new List<float>();
    //readonly List<float> w = new List<float>();
    //readonly List<float> max_v = new List<float>();
    //readonly List<float> max_w = new List<float>();

    //// Firefly velocity
    //readonly List<float> fv = new List<float>();
    //readonly List<float> currFV = new List<float>();

    // Distances from player to firefly
    //readonly List<string> dist = new List<string>();
    //readonly List<float> distances = new List<float>();
    public float distToFF;

    // Times
    readonly List<float> beginTime = new List<float>();
    readonly List<float> frameTime = new List<float>();
    readonly List<float> trialTime = new List<float>();
    readonly List<float> checkTime = new List<float>();
    //readonly List<float> rewardTime = new List<float>();
    readonly List<float> endTime = new List<float>();
    readonly List<float> checkWait = new List<float>();
    readonly List<float> interWait = new List<float>();

    private float rewardTime;
    // add when firefly disappears

    // Rewarded?
    //readonly List<int> score = new List<int>();
    public int score;

    // Timed Out?
    //readonly List<int> timedout = new List<int>();
    private int timedout;

    // Current Phase
    readonly List<int> epoch = new List<int>();

    // Was Always ON?
    readonly List<bool> alwaysON = new List<bool>();

    // File paths
    private string path;
    private string mouseID;
    private string date;

    [HideInInspector] public int trialNum;
    private float trialT0;
    private float trialT;
    private float programT0 = 0.0f;

    private float points = 0;
    [Tooltip("How much the player receives for successfully completing the task")]
    [HideInInspector] public float juiceTime;

    private int seed;
    private System.Random rand;

    private bool on = true;
    
    // above/below threshold
    private bool ab = true;

    // Full data record
    private bool isFull = false;

    private bool isBegin = false;
    private bool isCheck = false;
    private bool isEnd = false;

    private Phases currPhase;

    readonly private List<GameObject> pooledFF = new List<GameObject>();

    private bool first = true;
    // private List<GameObject> pooledI = new List<GameObject>();
    // private List<GameObject> pooledO = new List<GameObject>();

    private readonly char[] toTrim = { '(', ')' };

    [HideInInspector] public float initialD = 0.0f;

    private float velocity;
    public Vector3 player_origin;

    private string contPath;

    //private int loopCount = 0;

    private StringBuilder sb = new StringBuilder();
    public bool playing = true;

    public bool ramp;
    public float rampTime;
    public float rampDelay;

    //private SerialPort sp;
    //private string port = "COM7";
    //private int baudrate = 115200;

    private float pitch;
    private float roll;
    private float yaw;

    public int dim;

    private float deltaX;
    private float deltaZ;
    private float deltaYaw;

    private float velThresh;
    private float rotThresh;

    private bool onStop;
    //private bool giveJuice;
    private bool proximity = false;
    private int goodTrials = 0;
    private int maxTrials;
    private int expDur;

    [HideInInspector] public bool distalOn;
    [HideInInspector] public float gain;

    //SerialPort _serialPort;
    List<string> timeRecieved = new List<string>();
    List<int> pulse = new List<int>();

    IntPtr MotionCueingClass;

    float dt_test;
    float totalX;
    float totalZ;
    float totalYaw;
    float startX = 0.0f;
    float startZ = 0.0f;
    float startYaw = 0.0f;
    bool calibrateZ = true;
    bool calibrateX = true;
    bool calibrateYaw = true;
    float rotFactor = 0.5f * (11.875f * 0.0254f * 180.0f) / (4.0f * Mathf.Atan2(1.0f, 1.0f));

    List<int> TTL = new List<int>();
    int sync_ttl = 0;
    int lick = 0;
    List<int> epochCopy = new List<int>();
    List<int> trialCopy = new List<int>();
    List<float> timeCopy = new List<float>();
    List<bool> onoffCopy = new List<bool>();
    List<Vector3> positionCopy = new List<Vector3>();
    List<Quaternion> rotationCopy = new List<Quaternion>();
    List<float> vCopy = new List<float>();
    List<float> wCopy = new List<float>();
    List<float> fvCopy = new List<float>();
    List<Vector3> fposCopy = new List<Vector3>();
    List<int> TTLCopy = new List<int>();
    float timeSinceLastSave = 0.0f;
    int idx = 0;
    
 
    bool flagMotionCue = true;
    bool flagMotionCueActive = false;

    float head_dir;
    List<float> head_dir_list = new List<float>();  /*in deg*/
    float prevYaw;

    public float currSpeed = 0.0f;
    public float angSpeed = 0.0f;
    float autoGain = 0.0f;
    int autoIdx = 0;
    bool rampingDown = false;
    bool enterPressed = false;
    public bool IsStop = false;
    private bool areWalls;
    // replay settings
    bool isReplay;
    //string replayPath;
    //readonly List<float> replayX = new List<float>();
    //readonly List<float> replayZ = new List<float>();
    //readonly List<float> replayYaw = new List<float>();
    //int replayIdx = 0;
    //int replayMaxIdx;
    // MC settings
    public bool activeMC;

    //Variables for graphing
    List<GameObject> lineList = new List<GameObject>();
    private DD_DataDiagram m_DataDiagram;
    private bool m_IsContinueInput = false;
    private float m_Input = 0f;
    private float h = 0;


    void AddALine()
    {
        if (null == m_DataDiagram)
            return;
        Color color = Color.HSVToRGB((h += 0.1f) > 1 ? (h - 1) : h, 0.8f, 0.8f);
        GameObject line = m_DataDiagram.AddLine(color.ToString(), color);
        if (null != line)
            lineList.Add(line);
    }

    // Start is called before the first frame update
    /// <summary>
    /// From "GoToSettings.cs" you can see that I just hard-coded each of the key
    /// strings in order to retrieve the values associated with each key and
    /// assign them to their respective variable here. Also initialize some 
    /// variables depending on what mode is selected.
    /// 
    /// Catch exception if no mode detected from PlayerPrefs and default to Fixed
    /// 
    /// Set head tracking for VR headset OFF
    /// </summary>
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 50;
        //Physics2D.autoSimulation = false;
        
        SharedReward = this;

        try
        {
            head_dir = ringSensor.dir;
        }
        catch
        {
            print("Ring sensor is not connected");
        }

        try
        {
            lick = lickDetector.TTL;
        }
        catch
        {
            print("Beam break sensor is not connected");
        }

        try
        {
            sync_ttl = syncController.TTL;
        }
        catch
        {
            print("Sync arduino is not connected");
        }

        //Graphing
        GameObject dd = GameObject.Find("DataDiagram");
        if (null == dd)
        {
            Debug.LogWarning("can not find a gameobject of DataDiagram");
            return;
        }
        m_DataDiagram = dd.GetComponent<DD_DataDiagram>();
        m_DataDiagram.PreDestroyLineEvent += (s, e) => { lineList.Remove(e.line); };
        AddALine();
        AddALine();

        // if path defined replay is true
        //isReplay = PlayerPrefs.GetInt("Is Replay") == 1;

        //isReplay = false;
        //if (isReplay)
        //{
        //    replayPath = PlayerPrefs.GetString("Replay Path");
        //    print(replayPath);
        //    StreamReader sr = new StreamReader(replayPath);

        //    string[] split;
        //    while (!sr.EndOfStream)
        //    {
        //        split = sr.ReadLine().Split(',');
        //        replayX.Add(float.Parse(split[0]));
        //        replayZ.Add(float.Parse(split[1]));
        //        replayYaw.Add(float.Parse(split[2]));
        //    }

        //    replayMaxIdx = replayX.Count;
        //}

        seed = UnityEngine.Random.Range(1, 10000);
        rand = new System.Random(seed);
        p_height = 0.075f;
     
        c_lambda = 1.0f / PlayerPrefs.GetFloat("Mean 1");
        i_lambda = 1.0f / PlayerPrefs.GetFloat("Mean 2");
        checkMin = PlayerPrefs.GetFloat("Minimum Wait to Check");
        checkMax = PlayerPrefs.GetFloat("Maximum Wait to Check");
        // iti after win
        interMin = PlayerPrefs.GetFloat("Minimum Intertrial Wait");
        // iti after timeout
        interMax = PlayerPrefs.GetFloat("Maximum Intertrial Wait");

        c_min = Tcalc(checkMin, c_lambda);
        c_max = Tcalc(checkMax, c_lambda);
        i_min = Tcalc(interMin, c_lambda);
        i_max = Tcalc(interMax, c_lambda);

        velMin = PlayerPrefs.GetFloat("Min Linear Speed");
        velMax = PlayerPrefs.GetFloat("Max Linear Speed");


        rotMin = PlayerPrefs.GetFloat("Min Angular Speed");
        rotMax = PlayerPrefs.GetFloat("Max Angular Speed");
        dim = PlayerPrefs.GetInt("Dimensions"); // 0 = 1D (F/B), 1 = 2D (L/R/F/B), 2 - yaw rot
     
        minDrawDistance = PlayerPrefs.GetFloat("Minimum Firefly Distance");
        maxDrawDistance = PlayerPrefs.GetFloat("Maximum Firefly Distance");
        LR = PlayerPrefs.GetFloat("Left Right");
        
        LR = 0.5f;
        if (LR == 0.5f)
        {
            maxPhi = PlayerPrefs.GetFloat("Max Angle");
            minPhi = -maxPhi;
        }
        else
        { 
            maxPhi = PlayerPrefs.GetFloat("Max Angle");
            minPhi = PlayerPrefs.GetFloat("Min Angle");
        }
        //print(maxPhi);

        fireflyZoneRadius = PlayerPrefs.GetFloat("Reward Zone Radius");
        fireflySize = PlayerPrefs.GetFloat("Size");
        firefly.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        ratio = PlayerPrefs.GetFloat("Ratio");
        ramp = PlayerPrefs.GetInt("Ramp") == 1;
       
        rampTime = PlayerPrefs.GetFloat("Ramp Time");
        rampDelay = PlayerPrefs.GetFloat("Ramp Delay");

        velocities.Add(PlayerPrefs.GetFloat("V1"));
        velocities.Add(PlayerPrefs.GetFloat("V2"));
      

        v_ratios.Add(PlayerPrefs.GetFloat("VR1"));
        v_ratios.Add(PlayerPrefs.GetFloat("VR2"));


        for (int i = 1; i < 2; i++)
        {
            v_ratios[i] = v_ratios[i] + v_ratios[i - 1];
        }

        durations.Add(PlayerPrefs.GetFloat("D1"));
        durations.Add(PlayerPrefs.GetFloat("D2"));
        durations.Add(PlayerPrefs.GetFloat("D3"));
        durations.Add(PlayerPrefs.GetFloat("D4"));
        durations.Add(PlayerPrefs.GetFloat("D5"));

        ratios.Add(PlayerPrefs.GetFloat("R1"));
        ratios.Add(PlayerPrefs.GetFloat("R2"));
        ratios.Add(PlayerPrefs.GetFloat("R3"));
        ratios.Add(PlayerPrefs.GetFloat("R4"));
        ratios.Add(PlayerPrefs.GetFloat("R5"));

        for (int i = 1; i < 5; i++)
        {
            ratios[i] = ratios[i] + ratios[i - 1];
        }

        isMoving = PlayerPrefs.GetInt("Moving ON") == 1;
        LRFB = PlayerPrefs.GetInt("VertHor") == 0;
        isFull = PlayerPrefs.GetInt("Full ON") == 1;
        try
        {
            switch (PlayerPrefs.GetString("Switch Behavior"))
            {
                case "always on":
                    mode = Modes.ON;
                    break;
                case "flashing":
                    mode = Modes.Flash; 
                    freq = PlayerPrefs.GetFloat("Frequency");
                    duty = PlayerPrefs.GetFloat("Duty Cycle") / 100f;
                    PW = duty / freq;
                    break;
                case "fixed":
                    mode = Modes.Fixed;
                    break;
                default:
                    throw new System.Exception("No mode selected, defaulting to ON");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.Log(e, this);
            mode = Modes.ON;
        }
        mode = Modes.ON;
        //nFF = PlayerPrefs.GetFloat("Number of Fireflies");
        //if (nFF > 1)
        //{
        //    for (int i = 0; i < nFF; i++)
        //    {
        //        GameObject obj = Instantiate(firefly);
        //        // GameObject in_ = Instantiate(inner);
        //        // GameObject out_ = Instantiate(outer);
        //        obj.name = ("Firefly " + i).ToString();
        //        // in_.name = ("Inner " + i).ToString();
        //        // out_.name = ("Outer " + i).ToString();
        //        pooledFF.Add(obj);
        //        // pooledI.Add(in_);
        //        // pooledO.Add(out_);
        //        obj.SetActive(true);
        //        // in_.SetActive(true);
        //        // out_.SetActive(true);
        //        obj.GetComponent<SpriteRenderer>().enabled = true;
        //        switch (i)
        //        {
        //            case 0:
        //                obj.GetComponent<SpriteRenderer>().color = Color.black;
        //                break;
        //            case 1:
        //                obj.GetComponent<SpriteRenderer>().color = Color.red;
        //                break;
        //            case 2:
        //                obj.GetComponent<SpriteRenderer>().color = Color.blue;
        //                break;
        //            case 3:
        //                obj.GetComponent<SpriteRenderer>().color = Color.yellow;
        //                break;
        //            case 4:
        //                obj.GetComponent<SpriteRenderer>().color = Color.green;
        //                break;
        //        }
        //    }
        //    // inner.SetActive(false);
        //    // outer.SetActive(false);
        //    firefly.SetActive(false);
        //}

        timeout = PlayerPrefs.GetFloat("Timeout");
        onStop = PlayerPrefs.GetInt("End Trial On Stop") == 1;
        velThresh = PlayerPrefs.GetFloat("Velocity Threshold");
        rotThresh = PlayerPrefs.GetFloat("Rotation Threshold");
        gain = PlayerPrefs.GetFloat("Gain");

        path = PlayerPrefs.GetString("Path");
        mouseID = PlayerPrefs.GetString("Name");
        date = PlayerPrefs.GetString("Date");

        //print(string.Format(" Mouse id = {0}, Date = {1}, Path = {2}", mouseID, date, path));

        isReplay = PlayerPrefs.GetInt("Is Replay") == 1;
        activeMC = (int)PlayerPrefs.GetFloat("Enable MC") == 1;
        print(String.Format("activeMC: {0} replay: {1}", isReplay, activeMC));

        juiceTime = PlayerPrefs.GetFloat("Juice Time");
        distalOn = PlayerPrefs.GetInt("Distal Object") != 5;
        areWalls = (int)PlayerPrefs.GetFloat("Walls") == 1;
        maxTrials = (int)PlayerPrefs.GetFloat("Num Trials");
        expDur = (int)PlayerPrefs.GetFloat("ExpDur");
        //for (int i = 0; i < 3; i++)
        //{
        //    GameObject child = walls.transform.GetChild(i).gameObject;
        //    child.GetComponent<MeshRenderer>().enabled = false;
        //}
        if (areWalls)
        {
            walls.SetActive(true);

        } else
        {
            walls.SetActive(false);
        }
        


        distalOn = true;
        if (distalOn)
        {
            distalObject.SetActive(true);
        }
        else
        {
            distalObject.SetActive(false);
        }

        Vector3 point = MonitorCam.ScreenToWorldPoint(new Vector3(-900, 0.5f, 900));

        //distalObject.transform.position = point;
        //distalObject.transform.Rotate(0, -45, 0.0f);
        //float angle = Mathf.Atan2(995.0f, distalObject.transform.position.x) * Mathf.Rad2Deg - 40.0f;
        //float width = (distalObject.transform.position.x - Mathf.Tan(angle)) * 2.0f * Mathf.Sqrt(2);
        //distalObject.transform.localScale = new Vector3(900.0f, 1500.0f, 1.0f);

        trialNum = 0;

        player.transform.position = new Vector3(0.0f, p_height, 0.0f);
        player.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        //print("Begin test.");
        contPath = "C:\\Users\\lab\\Desktop\\DevData" + "/continuous_data_" + mouseID + "_" + System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".csv";
        //print(contPath);
        // string firstLine = "TrialNum,TrialTime,Phase,OnOff,PosX,PosY,PosZ,RotX,RotY,RotZ, zVel,xVel,yawVel,FFX,FFY,FFZ,FFV,AccX,AccY,AccZ,GyroX,GyroY,GyroZ,TTL,Tx,Ty,Tz,Rx,Ry,Rz,head_dir";
        string firstLine = "TrialNum,TrialTime,Phase,OnOff,PosX,PosY,PosZ,RotX,RotY,RotZ,zVel,xVel,yawVel,FFX,FFY,FFZ,FFV,distToFF,score,rewardTime,timedout,TTL,head_dir";
    
        // firstLine = "n,max_v,max_w,ffv,onDuration,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,RotW0,ffX,ffY,ffZ,pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,rCheckW,distToFF,rewarded,timeout,beginTime,checkTime,rewardTime,duration,delays,ITI";
   
        File.AppendAllText(contPath, firstLine + "\n");

        programT0 = Time.realtimeSinceStartup;
        //prevYaw = accelController.gyroY;
        if (activeMC)
        {
            currPhase = Phases.startup;
            phase = Phases.startup;

        } else
        {
            currPhase = Phases.begin;
            phase = Phases.begin;
        }
  

        dt_test = 0.0f;

        //ActiveMotionCue();
        //MotionCue();
        //Read();

        //HexaTest();
    }

    private void OnApplicationQuit()
    {
    

        StringBuilder stringBuilder = new StringBuilder();

        for (int i = 0; i < timeRecieved.Count; i++)
        {
            stringBuilder.AppendLine(string.Format("{0},{1}", timeRecieved[i], pulse[i]));
        }

        //File.WriteAllText("C:\\Users\\jc10487\\Documents\\timingTest.txt", stringBuilder.ToString());
    }

    /// <summary>
    /// Update is called once per frame
    /// 
    /// for some reason, I can't set the camera's local rotation to 0,0,0 in Start()
    /// so I'm doing it here, and it gets called every frame, so added bonus of 
    /// ensuring it stays at 0,0,0.
    /// 
    /// SharedInstance.fill was an indicator of how many objects loaded in properly,
    /// but I found a way to make it so that everything loads pretty much instantly,
    /// so I don't really need it, but it's nice to have to ensure that the experiment
    /// doesn't start until the visual stimulus (i.e. floor triangles) are ready. 
    /// 
    /// Every frame, add the time it occurs, the trial time (resets every new trial),
    /// trial number, and position and rotation of player.
    /// 
    /// Switch phases here to ensure that each phase occurs on a frame
    /// 
    /// For Flashing and Fixed, toggle will be true or false depending on whether or not
    /// nextDouble returns a number smaller than or equal to the ratio
    /// 
    /// In the case of multiple FF, I turned the sprite renderer on and off, rather than
    /// using SetActive(). I was trying to do something will colliders to detect whether
    /// or not there is already another FF within a certain range, and in order to do that
    /// I would have to keep the sprite active, so I couldn't use SetActive(false). 
    /// The thing I was trying to do didn't work, but I already started turning the 
    /// sprite renderer on and off, and it works fine, so it's staying like that. This
    /// applies to all other instances of GetComponent<SpriteRenderer>() in the code.
    /// </summary>
    async void Update()
    {
        float d1 = 0;
        float d2 = 0;

        if (isMoving)
        {
            firefly.transform.position += move * Time.deltaTime;
        }
        distToFF = Vector3.Distance(firefly.transform.position, player.transform.position);
        //if (isReplay & replayIdx < replayMaxIdx)
        //{
        //    print(string.Format("x {0}, yaw {1}, z {2}", replayX[replayIdx], replayYaw[replayIdx], replayZ[replayIdx]));

        //    player.transform.position += player.transform.forward * replayZ[replayIdx];
        //    player.transform.Rotate(0.0f, replayYaw[replayIdx], 0.0f);

        //    //player.transform.position += new Vector3(replayX[replayIdx], p_height, replayZ[replayIdx]);
        //    //player.transform.rotation = Quaternion.Euler(0f, replayYaw[replayIdx], 0f);
        //    replayIdx++;

        //}

        particleSystem.transform.position = new Vector3(player.transform.position.x, 0.001f, player.transform.position.z);

        //Debug.Log(string.Format("Player {0}", player.transform.position));


        switch (phase)
        {
            case Phases.begin:
                phase = Phases.none;
                if (first)
                {
                    toggle = true;
                    first = false;
                }
                else
                {
                    toggle = rand.NextDouble() <= ratio;
                }
                Begin();
                //tracker.UpdateView();
                break;

            case Phases.trial:
                phase = Phases.none;
                Trial();
                break;

            case Phases.check:
                phase = Phases.none;
                if (mode == Modes.ON)
                {
                    //if (nFF > 1)
                    //{
                    //    for (int i = 0; i < nFF; i++)
                    //    {
                    //        pooledFF[i].GetComponent<SpriteRenderer>().enabled = false;
                    //    }
                    //}
                    //else
                    //{
                    //    firefly.SetActive(false);
                    //}
                    firefly.SetActive(false);
                }
                Check();
                break;

            case Phases.startup:
                phase = Phases.none;
                Startup();
                break;

            case Phases.shutdown:
                phase = Phases.none;
                Shutdown();
                break;

            case Phases.none:
                break;
        }

        if (playing)
        {

            activeMC = false;
            var zVel = 0.0f;
            var xVel = 0.0f;
            var yawVel = 0.0f;

            try
            {
                head_dir = ringSensor.dir;
            } catch
            {
                head_dir = float.NaN;
            }

            try
            {
                lick = lickDetector.TTL;
            } catch
            {
                lick = -1;
            }
            try
            {
                sync_ttl = syncController.TTL;
            }
            catch
            {
                sync_ttl = -1;
            }

            //print(String.Format("head_dir: {0}, lick: {1}, sync: {2}", head_dir, lick, sync_ttl));

            if (activeMC)
            {

                zVel = (float)motionCueingController.motionCueing.filtered[0][2];
                xVel = (float)motionCueingController.motionCueing.filtered[1][2];
                yawVel = (float)motionCueingController.motionCueing.filtered[2][2];

            }
            else
            {
                // calibration does not seem to give right numbers
                zVel = Ball.zVel*gain*3f;
                yawVel = Ball.yawVel*gain;
                xVel = Ball.xVel*gain*2.5f;

                //print(String.Format("zVel: {0}, xVel: {1}, yawVel: {2}", Ball.zVel, Ball.xVel, Ball.yawVel));

                //if (autoIdx > 300 && autoGain < 1.0f)
                //{
                //    autoGain += 1f / 60f;
                //    autoIdx++;
                //}

                //switch (dim)
                //{
                //    case 1:
                //        var currYaw = accelController.gyroY;
                //        yawVel = ((currYaw - prevYaw) * Mathf.Rad2Deg) / Time.deltaTime;
                //        prevYaw = currYaw;
                //        break;

                //    case 2:
                //        //deltaYaw = Ball.yaw * 0.1713298528f;
                //        deltaYaw = Ball.yaw;
                //        yawVel = deltaYaw / Time.deltaTime;
                //        break;

                //    default:
                //        deltaYaw = Ball.yaw * 0.1713298528f;
                //        yawVel = deltaYaw / Time.deltaTime;
                //        break;
                //}
            }
#if CALIBRATING
            // for some reason these numbers do not work, try to calibrate using a static floor with a defined 
            // pattern and see how pattern moves with the ball movement
            // calibrated values
            //deltaZ = Ball.pitch * -0.0085384834f;
            //deltaX = Ball.roll * -0.0093862942f;
            //deltaYaw = Ball.yaw * 0.1713298528f;

            deltaZ = Ball.pitch;
            deltaX = Ball.roll;
            deltaYaw = Ball.yaw;

            totalZ += Ball.pitch;
            totalX += Ball.roll;
            totalYaw += Ball.yaw;
            // ball diam = 0.3m, z -> -0.0418, 
            //var circumference = Mathf.PI * 11.875f * 0.0254f;
            var circumference = Mathf.PI * 0.3f;

            if (Keyboard.current.zKey.isPressed && calibrateZ)
            {
                calibrateZ = false;
                print(string.Format("z scale: {0}", circumference / totalZ));
            }
            if (Keyboard.current.xKey.isPressed && calibrateX)
            {
                calibrateX = false;
                print(string.Format("x scale: {0}", circumference / totalX));
            }
            if (Keyboard.current.yKey.isPressed && calibrateYaw)
            {
                calibrateYaw = false;
                print(string.Format("yaw scale: {0}", 360.0f / totalYaw));
            }
#endif


            if (zVel > velMax)
            {
                zVel = velMax;
            }
            else if (zVel < velMin)
            {
                zVel = velMin;
            }
            else if (xVel > velMax)
            {
                xVel = velMax;
            }
            else if (xVel < velMin)
            {
                xVel = velMin;
            }

         

            //print(string.Format("{0}, {1}", zVel, xVel));

            switch (dim)
            {
  
                case 0:
                    // head fixed, rotate the ball
                    player.transform.position += player.transform.forward * zVel * Time.deltaTime;
                    player.transform.Rotate(0.0f, yawVel * Time.deltaTime, 0.0f);
                    break;

                case 1:
                    // head free to rotate
                    //player.transform.position += new Vector3(0.0f, 0.0f, deltaZ);
                    player.transform.position += new Vector3(xVel * Time.deltaTime, 0.0f, zVel * Time.deltaTime);
                    break;

                default:
                    player.transform.position += player.transform.forward * zVel * Time.deltaTime;
                    player.transform.Rotate(0.0f, yawVel * Time.deltaTime, 0.0f);
                    //var dx = deltaZ - deltaX;
                    //var dy = deltaZ + deltaX;
                    //player.transform.position += player.transform.forward * Mathf.Sqrt(Mathf.Pow(dx, 2.0f) + Mathf.Pow(dy, 2.0f));

                    break;
            }
            // if angle is defined, we are in training stage
            if (areWalls == true)
            {
                var vr_arena_limit = 0.25f;
                if (player.transform.position.x < -vr_arena_limit)
                {
                    player.transform.position = new Vector3(-vr_arena_limit, p_height, player.transform.position.z);
                }
                if (player.transform.position.x > vr_arena_limit)
                {
                    player.transform.position = new Vector3(vr_arena_limit, p_height, player.transform.position.z);
                }
                if (player.transform.position.z < -vr_arena_limit)
                {
                    player.transform.position = new Vector3(player.transform.position.x, p_height, -vr_arena_limit);
                }
                if (player.transform.position.z > vr_arena_limit)
                {
                    player.transform.position = new Vector3(player.transform.position.x, p_height, vr_arena_limit);
                }
            }




            var p_speed = (float)Math.Sqrt(zVel* zVel + xVel * xVel);
            m_DataDiagram.InputPoint(lineList[0], new Vector2(1, p_speed*100));
            // 6 deg bins
            m_DataDiagram.InputPoint(lineList[1], new Vector2(1, head_dir/6));

            // for MC, this should be changed in ball script
            //if (player.transform.position.x > Math.Abs(vr_arena_limit) | player.transform.position.z > Math.Abs(vr_arena_limit))
            //{
            //    currSpeed = 0.0f;
            //}


            if (isBegin)
            {
                trialT0 = Time.realtimeSinceStartup;
                beginTime.Add(trialT0 - programT0);
                trialNum++;
      
                isBegin = false;
            }
            if (isCheck)
            {
                checkTime.Add(Time.realtimeSinceStartup - programT0);
                isCheck = false;
            }
            if (isEnd)
            {
                endTime.Add(Time.realtimeSinceStartup - trialT0);
                if (toggle)
                {
                    onDur.Add(endTime[endTime.Count - 1]);
                }
                isEnd = false;
            }



            //if (Time.realtimeSinceStartup - programT0 - dt_test > 0.025f)
            //{
            //    Debug.Log(Time.realtimeSinceStartup - programT0 - dt_test);
            //}

            //dt_test = Time.realtimeSinceStartup - programT0;


            //if (nFF > 1)
            //{
            //    onoff.Add(pooledFF[0].GetComponent<SpriteRenderer>().enabled);
            //}
            //else
            //{
            //    onoff.Add(firefly.activeInHierarchy);
            //}
            if (activeMC)
                {
                    sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}",
                    trialNum,
                    Time.realtimeSinceStartup - programT0,
                    (int)currPhase,
                    firefly.activeInHierarchy ? 1 : 0,
                    player.transform.position.ToString("F5").Trim(toTrim).Replace(" ", ""),
                    player.transform.eulerAngles.ToString("F5").Trim(toTrim).Replace(" ", ""),
                    zVel,
                    yawVel,
                    firefly.transform.position.ToString("F5").Trim(toTrim).Replace(" ", ""),
                    velocity,
                    distToFF,
                    score,
                    rewardTime,
                    timedout,
                    accelController.IsConnected ? accelController.reading : "NaN,NaN,NaN,NaN,NaN,NaN",
                    syncController.IsConnected ? syncController.TTL : float.NaN,
                    motionCueingController.motionCueing.frame.surge,
                    motionCueingController.motionCueing.frame.lateral,
                    motionCueingController.motionCueing.frame.heave,
                    motionCueingController.motionCueing.frame.roll,
                    motionCueingController.motionCueing.frame.pitch,
                    motionCueingController.motionCueing.frame.yaw) + "\n");
                    //string.Join(",", labJackController.ValueAIN)) + "\n");
                } else
                {
                    sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                    trialNum,
                    Time.realtimeSinceStartup - programT0,
                    (int)currPhase,
                    firefly.activeInHierarchy ? 1 : 0,
                    player.transform.position.ToString("F5").Trim(toTrim).Replace(" ", ""),
                    player.transform.eulerAngles.ToString("F5").Trim(toTrim).Replace(" ", ""),
                    zVel,
                    xVel,
                    yawVel,
                    firefly.transform.position.ToString("F5").Trim(toTrim).Replace(" ", ""),
                    velocity,
                    distToFF,
                    score,
                    rewardTime,
                    timedout,
                    //accelController.IsConnected ? accelController.reading : "NaN,NaN,NaN,NaN,NaN,NaN",
                    sync_ttl,
                    head_dir
                    ) + "\n");
                    //string.Join(",", labJackController.ValueAIN)) + "\n");
                }
                // playing
        }
        score = 0;
        timedout = 0;


        Keyboard keyboard = Keyboard.current;
        //print(string.Format(" exp dur = {0}, time = {1}", expDur, Time.realtimeSinceStartup));
        if ((keyboard.enterKey.wasReleasedThisFrame | trialNum > maxTrials | expDur*60 < Time.realtimeSinceStartup) & !enterPressed)
        {
            enterPressed = true;
            //currPhase = Phases.shutdown;
            //phase = Phases.shutdown;
            playing = false;

            //motionCueingController.IsStop = true;
            if (activeMC)
            {
                await motionCueingController.StopMovement();
            }
            
            PlayerPrefs.SetInt("Good Trials", goodTrials);
            PlayerPrefs.SetInt("Total Trials", trialNum - 1);
            // Environment.Exit(Environment.ExitCode);
            File.AppendAllText(contPath, sb.ToString());
            sb.Clear();

            //Save();


            SceneManager.LoadScene("MainMenu");

        }
        
        if (rampingDown)
        {
            if (autoGain > 0 && autoIdx > 60)
            {
                autoGain -= 1f / 60f;
                autoIdx--;
            }
            else
            {
                autoIdx--;
            }

            if (autoIdx <= 0 && playing)
            {

                PlayerPrefs.SetInt("Good Trials", goodTrials);
                PlayerPrefs.SetInt("Total Trials", trialNum - 1);
                // Environment.Exit(Environment.ExitCode);
                File.AppendAllText(contPath, sb.ToString());
                sb.Clear();

                //Save();


                SceneManager.LoadScene("MainMenu");
            }

        }

        idx++;

        

    }

    /// <summary>
    /// Capture data at 100 Hz
    /// 
    /// Set Unity's fixed timestep to 1/100 in order to get 100 Hz recording
    /// Edit -> Project Settings -> Time -> Fixed Timestep
    /// </summary>
    //public void FixedUpdate()
    //{

    //}

    private void OnDisable()
    {
        File.AppendAllText(contPath, sb.ToString());
        sb.Clear();
    }

    //async void WriteData()
    //{
    //    List<int> counts = new List<int>()
    //    {
    //        trialCopy.Count,
    //        timeCopy.Count,
    //        epochCopy.Count,
    //        onoffCopy.Count,
    //        positionCopy.Count,
    //        rotationCopy.Count,
    //        vCopy.Count,
    //        wCopy.Count,
    //        fposCopy.Count,
    //        fvCopy.Count,
    //        TTLCopy.Count
    //    };
    //    counts.Sort();
    //    var length = counts[0];

    //    for (int i = 0; i < length; i++)
    //    {
    //        sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
    //            trialCopy[i],
    //            timeCopy[i] + timeSinceLastSave,
    //            epochCopy[i],
    //            onoffCopy[i] ? 1 : 0,
    //            positionCopy[i].ToString("F5").Trim(toTrim).Replace(" ", ""),
    //            rotationCopy[i].ToString("F5").Trim(toTrim).Replace(" ", ""),
    //            vCopy[i],
    //            wCopy[i],
    //            fposCopy[i],
    //            fvCopy[i],
    //            TTLCopy[i]) + "\n");
    //    }

    //    timeSinceLastSave = timeCopy[length - 1];
    //    File.AppendAllText(contPath, sb.ToString());
    //    sb.Clear();

    //    await new WaitForSeconds(0.0f);
    //}

    async void Startup()
    {
        await new WaitForSeconds(60.0f);

        currPhase = Phases.begin;
        phase = Phases.begin;
    }

    async void Shutdown()
    {
        await new WaitForSeconds(30.0f);

        motionCueingController.End();

        motionCueingController.IsStop = true;

        //playing = false;

        // Wait 3 times because there will always be 3 commands to wait to finish
        await new WaitUntil(() => motionCueingController.currentTask.IsCompleted);
        await new WaitUntil(() => motionCueingController.currentTask.IsCompleted);
        await new WaitUntil(() => motionCueingController.currentTask.IsCompleted);

        PlayerPrefs.SetInt("Good Trials", goodTrials);
        PlayerPrefs.SetInt("Total Trials", trialNum - 1);
        // Environment.Exit(Environment.ExitCode);
        File.AppendAllText(contPath, sb.ToString());
        sb.Clear();

        //Save();

        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Wait until the player is not moving, then:
    /// 1. Add trial begin time to respective list
    /// 2. Update position; 
    ///     r is calculated so that all distances between the min and max are equally likely to occur,
    ///     angle is calculated in much the same way,
    ///     side just determines whether it'll appear on the left or right side of the screen,
    ///     position is calculated by adding an offset to the player's current position;
    ///         Quaternion.AngleAxis calculates a rotation based on an angle (first arg)
    ///         and an axis (second arg, Vector3.up is shorthand for the y-axis). Multiply that by the 
    ///         forward vector and radius r (or how far away the firefly should be from the player) to 
    ///         get the final position of the firefly
    /// 3. Record player origin and rotation, as well as firefly location
    /// 4. Start firefly behavior depending on mode, and switch phase to trial
    /// </summary>
    async void Begin()
    {
        await new WaitForSeconds(0.0f);

        //Debug.Log("Begin Phase Start.");
        //float maxV = RandomizeSpeeds(velMin, velMax);
        //float maxW = RandomizeSpeeds(rotMin, rotMax);
        float maxV = 0.0f;
        float maxW = 0.0f;

        //await new WaitForSeconds(0.5f);
        //loopCount = 0;

        currPhase = Phases.begin;
        isBegin = true;

        Vector3 position;
        
        float R = minDrawDistance + (maxDrawDistance - minDrawDistance) * Mathf.Sqrt((float)rand.NextDouble());
        //float angle = Mathf.Sqrt(Mathf.Pow(minPhi, 2.0f) + Mathf.Pow(maxPhi - minPhi, 2.0f) * (float)rand.NextDouble());
        float angle = (float)rand.NextDouble() * (maxPhi - minPhi) + minPhi;
        //float angle = (float)rand.NextDouble() * 360.0f;
        if (LR != 0.5f)
        {
            float side = rand.NextDouble() < LR ? 1 : -1;
            position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle * side, Vector3.up) * player.transform.forward * R;
        }
        else
        {
            if (dim == 1 & head_dir > 3)
            {
                position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle + head_dir, Vector3.up) * player.transform.forward * R;
            } else
            {
                position = (player.transform.position - new Vector3(0.0f, p_height, 0.0f)) + Quaternion.AngleAxis(angle, Vector3.up) * player.transform.forward * R;
            }
            
        }
       


        // spawn ff randomly in unit circle with radius 0.25
        if (areWalls)
        {
            //position = UnityEngine.Random.insideUnitSphere * 0.25f;
            float ff_x = UnityEngine.Random.Range(-0.25f, 0.25f);
            float ff_z = UnityEngine.Random.Range(-0.25f, 0.25f);
            position.x = ff_x;
            position.z = ff_z;

        }
  
        // set ff position
        position.y = 0.05f;
        firefly.transform.position = position;

   
        initialD = Vector3.Distance(player.transform.position, firefly.transform.position);
        //}

        // Here, I do something weird to the Vector3. "F8" is how many digits I want when I
        // convert to string, Trim takes off the parenthesis at the beginning and end of 
        // the converted Vector3 (Vector3.zero.ToString("F2"), for example, outputs:
        //
        //      "(0.00, 0.00, 0.00)"
        //
        // Replace(" ", "") removes all whitespace characters, so the above string would
        // look like this:
        //
        //      "0.00,0.00,0.00"
        //
        // Which is csv format.

        //player_origin = player.transform.position;
        //origin.Add(player_origin.ToString("F8").Trim(toTrim).Replace(" ", ""));
        //heading.Add(player.transform.rotation.ToString("F8").Trim(toTrim).Replace(" ", ""));

        if (isMoving)
        {
            //if ((float)rand.NextDouble() < moveRatio)
            //{
                float r = (float)rand.NextDouble();

                if (r <= v_ratios[0])
                {
                    //v1
                    velocity = velocities[0];
                }
                else if (r > v_ratios[0] && r <= v_ratios[1])
                {
                    //v2
                    velocity = velocities[1];
                }
                else if (r > v_ratios[1] && r <= v_ratios[2])
                {
                    //v3
                    velocity = velocities[2];
                }
                else if (r > v_ratios[2] && r <= v_ratios[3])
                {
                    //v4
                    velocity = velocities[3];
                }
                else if (r > v_ratios[3] && r <= v_ratios[4])
                {
                    //v5
                    velocity = velocities[4];
                }
                else if (r > v_ratios[4] && r <= v_ratios[5])
                {
                    //v6
                    velocity = velocities[5];
                }
                else if (r > v_ratios[5] && r <= v_ratios[6])
                {
                    //v7
                    velocity = velocities[6];
                }
                else if (r > v_ratios[6] && r <= v_ratios[7])
                {
                    //v8
                    velocity = velocities[7];
                }
                else if (r > v_ratios[7] && r <= v_ratios[8])
                {
                    //v9
                    velocity = velocities[8];
                }
                else if (r > v_ratios[8] && r <= v_ratios[9])
                {
                    //v10
                    velocity = velocities[9];
                }
                else if (r > v_ratios[10] && r <= v_ratios[11])
                {
                    //v11
                    velocity = velocities[10];
                }
                else
                {
                    //v12
                    velocity = velocities[11];
                }

            Vector3 direction;
            if (LRFB)
            {
                direction = Vector3.right;
            }
            else
            {
                direction = Vector3.forward;
            }
      
            move = direction * velocity;
        }
        else
        {
            velocity = 0;
        }

        switch (mode)
        {
            case Modes.ON:
                firefly.SetActive(true);
                break;
            case Modes.Flash:
                on = true;
                Flash(firefly);
                break;
            case Modes.Fixed:
                if (toggle)
                {
                    firefly.SetActive(true);
                    alwaysON.Add(true);
                }
                else
                {
                    alwaysON.Add(false);
                    float r = (float)rand.NextDouble();

                    if (r <= ratios[0])
                    {
                        // duration 1
                        lifeSpan = durations[0];
                    }
                    else if (r > ratios[0] && r <= ratios[1])
                    {
                        // duration 2
                        lifeSpan = durations[1];
                    }
                    else if (r > ratios[1] && r <= ratios[2])
                    {
                        // duration 3
                        lifeSpan = durations[2];
                    }
                    else if (r > ratios[2] && r <= ratios[3])
                    {
                        // duration 4
                        lifeSpan = durations[3];
                    }
                    else
                    {
                        // duration 5
                        lifeSpan = durations[4];
                    }
                    onDur.Add(lifeSpan);
                    OnOff(lifeSpan);
                }
                break;
        }

        if (ramp) Ramp(firefly, rampTime, rampDelay);
        //}
        phase = Phases.trial;
        currPhase = Phases.trial;
        //Debug.Log(phase);
    }

    /// <summary>
    /// Doesn't really do much besides wait for the player to start moving, and, afterwards,
    /// wait until the player stops moving and then start the check phase. Also will go back to
    /// begin phase if player doesn't move before timeout
    /// </summary>
    async void Trial()
    {
        //Debug.Log("Trial Phase Start.");

        CancellationTokenSource source = new CancellationTokenSource();


        distToFF = Vector3.Distance(firefly.transform.position, player.transform.position);
        Task t = Task.CompletedTask;

        Task t1 = Task.Run(async () => {
            await new WaitForSeconds(timeout);
        }, source.Token);

        if (onStop)
        {
            t = Task.Run(async () => {
                await new WaitUntil(() => (Ball.pitch < 5.0f && Ball.roll < 5.0f) || t1.IsCompleted);
            }, source.Token);
        }
        else
        {
            t = Task.Run(async () => {
                // changed from: Mathf.Abs(ff.magnitude - player.magnitude), which did not work for 2d case
                await new WaitUntil(() => Vector3.Distance(firefly.transform.position, player.transform.position) <= fireflyZoneRadius || t1.IsCompleted);// Used to be rb.velocity.magnitude
            }, source.Token);
        }

        //Debug.Log(fireflyZoneRadius);

        if (await Task.WhenAny(t, t1) == t)
        {
            
            if (onStop)
            {
                // Used to be rb.velocity.magnitude // || (angleL > 3.0f or angleR > 3.0f)
                //distance = Mathf.Abs(firefly.transform.position.magnitude - player.transform.position.magnitude);
                
                
           
                if (distToFF <= fireflyZoneRadius)
                {
                    proximity = true;
                }
            }
            else
            {
                
                proximity = true;
            }

            if (t1.IsCompleted)
            {
                isTimeout = true;
            }
        }
        else
        {
            //print("Timed out");
            isTimeout = true;
        }

        source.Cancel();

        if (mode == Modes.Flash)
        {
            on = false;
        }

        if (toggle)
        {
            firefly.SetActive(false);
        }
       

        //move = new Vector3(0.0f, 0.0f, 0.0f);
        //velocity = 0.0f;
        phase = Phases.check;
        currPhase = Phases.check;
        // print(phase);
        // Debug.Log("Trial Phase End.");
    }

    /// <summary>
    /// Save the player's position (pPos) and the firefly (reward zone)'s position (fPos)
    /// and start a coroutine to wait for some random amount of time between the user's
    /// specified minimum and maximum wait times
    /// </summary>
    async void Check()
    {
        //await new WaitForSeconds(0.2f);

        string ffPosStr = "";
        float wait = 0.0f;
        bool isReward = true;

        //Vector3 pos = new Vector3();
        //Quaternion rot = new Quaternion();

        //pPos = player.transform.position - new Vector3(0.0f, p_height, 0.0f);

        //pos = player.transform.position;
        //rot = player.transform.rotation;

        //if (Vector3.Distance(pPos, firefly.transform.position) <= fireflyZoneRadius) proximity = true;
        //ffPosStr = firefly.transform.position.ToString("F8").Trim(toTrim).Replace(" ", "");
        //distances.Add(Vector3.Distance(pPos, firefly.transform.position));

      
        if (!isTimeout)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            // Debug.Log("Check Phase Start.");

            float delay = c_lambda * Mathf.Exp(-c_lambda * ((float)rand.NextDouble() * (c_max - c_min) + c_min));
            // Debug.Log("firefly delay: " + delay);
            checkWait.Add(delay);

            // print("check delay average: " + checkWait.Average());

            // Wait until this condition is met in a different thread(?...not actually sure if its
            // in a different thread tbh), or until the check delay time is up. If the latter occurs
            // and the player is close enough to a FF, then the player gets the reward.
            //var t = Task.Run(async () =>
            //{
            //    await new WaitUntil(() => Vector3.Distance(pos, player.transform.position) > 0.05f); // Used to be rb.velocity.magnitude
            //    //UnityEngine.Debug.Log("exceeded threshold");
            //}, source.Token);

            //if (await Task.WhenAny(t, Task.Delay((int)(delay * 1000))) == t)
            //{
            //    //audioSource.clip = winSound;
            //    //points += rewardAmt;
            //    //isReward = true;
            //    //UnityEngine.Debug.Log("rewarded");
            //    audioSource.clip = loseSound;
            //    isReward = false;
            //}
            //source.Cancel();
        }
        else
        {
            isReward = false;

            checkWait.Add(0.0f);

            audioSource.clip = loseSound;
        }

        isCheck = true;

        if (isReward && proximity)
        {
            audioSource.clip = winSound;
            juiceController.GiveJuice(juiceTime);
            goodTrials++;
            score = 1;
            wait = interMin;
            proximity = false;
        }
        else
        {
            audioSource.clip = loseSound;
            wait = interMax;
        }
        audioSource.Play();

        timedout = isTimeout ? 1 : 0;
    

        //ffPos.Add(ffPosStr);
        //dist.Add(distances[0].ToString("F8"));
        //cPos.Add(pos.ToString("F8").Trim(toTrim).Replace(" ", ""));
        //cRot.Add(rot.ToString("F8").Trim(toTrim).Replace(" ", ""));


        
        isTimeout = false;

        //player.transform.position = Vector3.up * p_height;
        //player.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

        //float wait = i_lambda * Mathf.Exp(-i_lambda * ((float)rand.NextDouble() * (i_max - i_min) + i_min));

        

        //if (isReward && proximity)
        //{
        //    rewardTime = Time.realtimeSinceStartup - programT0;
        //    wait = interMin;
 
        //    juiceController.GiveJuice(juiceTime);
        //    await new WaitForSeconds(juiceTime / 1000.0f);
        //}
        //else
        //{
        //    rewardTime = 0.0f;
        //    wait = interMax;
        //}

        //print(wait);

        interWait.Add(wait);

        isEnd = true;
        // print("inter delay average: " + interWait.Average());

        //particleSystem.GetComponent<ParticleSystemRenderer>().enabled = false;

        await new WaitForSeconds(wait);

        //particleSystem.GetComponent<ParticleSystemRenderer>().enabled = true;


        firefly.GetComponent<MeshRenderer>().material.color = new Vector4(0f, 0f, 1f, 1f);

        phase = Phases.begin;
        // Debug.Log("Check Phase End.");
        //}
    }

    /// <summary>
    /// Used when user specifies that the FF flashes.
    /// 
    /// Pulse Width (s) is the length of the pulse, i.e. how long the firefly stays on. This
    /// is calculated with Duty Cycle (%), which is a percentage of the frequency of the
    /// desired signal. Frequency (Hz) is how often you want the object to flash per second.
    /// Here, we have 1 / frequency because the inverse of frequency is Period (s), denoted
    /// as T, which is the same definition as Frequency except it is given in seconds.
    /// </summary>
    /// <param name="obj">Object to flash</param>
    public async void Flash(GameObject obj)
    {
        while (on)
        {
            if (toggle && !obj.activeInHierarchy)
            {
                obj.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                obj.GetComponent<SpriteRenderer>().enabled = true;
                await new WaitForSeconds(PW);
                if (ramp) Ramp(obj, rampTime, rampDelay);
                obj.GetComponent<SpriteRenderer>().enabled = false;
                await new WaitForSeconds((1f / freq) - PW);
            }
        }
    }

    public async void Ramp(GameObject obj, float time, float delay)
    {
        bool ramp = true;


        if (delay > 0) 
        {
            await new WaitForSeconds(delay);
        }

        while (ramp)
        {
            Color col = obj.GetComponent<MeshRenderer>().material.color;
            float delta = 1f / (time / Time.deltaTime);
            float alpha = col.a - delta;
            if (alpha < 0f)
            {
                alpha = 0f;
                ramp = false;
            }
            col = new Vector4(0f, 0f, 1f, alpha);
            obj.GetComponent<MeshRenderer>().material.color = col;
            await new WaitForUpdate();
        }

        obj.SetActive(false);
    }

    public async void OnOff(float time)
    {
        CancellationTokenSource source = new CancellationTokenSource();

        firefly.SetActive(true);

        var t = Task.Run(async () =>
        {
            await new WaitForSeconds(time);
        }, source.Token);

        if (ramp) Ramp(firefly, rampTime, rampDelay);

        if (await Task.WhenAny(t, Task.Run(async () => { await new WaitUntil(() => currPhase == Phases.check); })) == t)
        {
            firefly.SetActive(false);
        }
        else
        {
            firefly.SetActive(false);
        }

        source.Cancel();
    }

    public float Tcalc(float t, float lambda)
    {
        return -1.0f / lambda * Mathf.Log(t / lambda);
    }

    public float RandomizeSpeeds(float min, float max)
    {
        return (float)(rand.NextDouble() * (max - min) + min);
    }

    /// <summary>
    /// Cast a ray towards the floor in the direction of the user's gaze and return the 
    /// intersection of that ray and the floor. Record the location and distance to the
    /// intersection in lists.
    /// </summary>
    /// <param name="origin"></param> Vector3 describing origin of ray
    /// <param name="direction"></param> Vector3 describing direction of ray
    /// <returns></returns>
    //public (Vector3, float) CalculateConvergenceDistanceAndCoords(Vector3 origin, Vector3 direction, int layerMask)
    //{
    //    Vector3 coords = Vector3.zero;
    //    float hit = Mathf.Infinity;

    //    if (Physics.Raycast(origin, Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, player.transform.forward, Vector3.up), Vector3.up) * direction, out RaycastHit hitInfo, Mathf.Infinity, layerMask))
    //    {
    //        coords = hitInfo.point;
    //        hit = hitInfo.distance;
    //        //HitLocations.Add(coords.ToString("F8").Trim(toTrim).Replace(" ", ""));
    //        //ConvergenceDistanceVerbose.Add(hitInfo.distance);
    //    }

    //    return (coords, hit);
    //}

    /// <summary>
    /// If you provide filepaths beforehand, the program will save all of your data as .csv files.
    /// 
    /// I did something weird where I saved the rotation/position data as strings; I did this
    /// because the number of columns that the .csv file will have will vary depending on the
    /// number of FF. Each FF has it's own position and distance from the player, and that data
    /// has to be saved along with everything else, and I didn't want to allocate memory for all
    /// the maximum number of FF if not every experiment will have 5 FF, so concatenating all of
    /// the available FF positions and distances into one string and then adding each string as
    /// one entry in a list was my best idea.
    /// </summary>
    /// 
    // dont really save configs now
    //public void Save()
    //{ 
    //    try
    //    {
    //        string firstLine;

    //        List<int> temp;

    //        StringBuilder csvDisc = new StringBuilder();

    //        //if (nFF > 1)
    //        //{
    //        //    string ffPosStr = "";
    //        //    string distStr = "";

    //        //    for (int i = 0; i < nFF; i++)
    //        //    {
    //        //        ffPosStr = string.Concat(ffPosStr, string.Format("ffX{0},ffY{0},ffZ{0},", i));
    //        //        distStr = string.Concat(distStr, string.Format("distToFF{0},", i));
    //        //    }
    //        //    if (isMoving)
    //        //    {
    //        //        firstLine = string.Format("n,max_v,max_w,f,ffv,answer,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,{0}pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,{1}rewarded,timeout,beginTime,checkTime,duration,delays,ITI", ffPosStr, distStr);
    //        //    }
    //        //    else
    //        //    {
    //        //        firstLine = string.Format("n,max_v,max_w,f,ffv,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,{0}pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,{1}rewarded,timeout,beginTime,checkTime,duration,delays,ITI", ffPosStr, distStr);
    //        //    }
    //        //}
    //        //else
    //        //{
    //        if (isMoving)
    //        {
    //            firstLine = "n,max_v,max_w,ffv,onDuration,answer,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,RotW0,ffX,ffY,ffZ,pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,rCheckW,distToFF,rewarded,timeout,beginTime,checkTime,rewardTime,duration,delays,ITI";
    //        }
    //        else
    //        {
    //            firstLine = "n,max_v,max_w,ffv,onDuration,PosX0,PosY0,PosZ0,RotX0,RotY0,RotZ0,RotW0,ffX,ffY,ffZ,pCheckX,pCheckY,pCheckZ,rCheckX,rCheckY,rCheckZ,rCheckW,distToFF,rewarded,timeout,beginTime,checkTime,rewardTime,duration,delays,ITI";
    //        }
    //        //}

    //        csvDisc.AppendLine(firstLine);

    //        temp = new List<int>()
    //        {
    //            origin.Count,
    //            heading.Count,
    //            ffPos.Count,
    //            dist.Count,
    //            n.Count,
    //            cPos.Count,
    //            cRot.Count,
    //            beginTime.Count,
    //            checkTime.Count,
    //            endTime.Count,
    //            checkWait.Count,
    //            interWait.Count,
    //            score.Count,
    //            rewardTime.Count,
    //            timedout.Count,
    //            max_v.Count,
    //            max_w.Count,
    //            fv.Count,
    //            onDur.Count
    //        };
    //        temp.Sort();

    //        for (int i = 0; i < temp[0]; i++)
    //        {
    //            var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
    //                n[i],
    //                max_v[i],
    //                max_w[i],
    //                fv[i],
    //                onDur[i],
    //                origin[i],
    //                heading[i],
    //                ffPos[i],
    //                cPos[i], 
    //                cRot[i],
    //                dist[i],
    //                timedout[i],
    //                beginTime[i],
    //                checkTime[i],
    //                rewardTime[i],
    //                endTime[i],
    //                checkWait[i], 
    //                interWait[i]);
    //            csvDisc.AppendLine(line);
    //        }

    //        string discPath = path + "/discontinuous_data_" + PlayerPrefs.GetInt("Optic Flow Seed").ToString() + ".csv";

    //        //File.Create(discPath);
    //        File.WriteAllText(discPath, csvDisc.ToString());

    //        //PlayerPrefs.GetInt("Save") == 1)

    //        string configPath = path + "/config_" + PlayerPrefs.GetInt("Optic Flow Seed").ToString() + ".xml";

    //        XmlWriter xmlWriter = XmlWriter.Create(configPath);

    //        xmlWriter.WriteStartDocument();

    //        xmlWriter.WriteStartElement("Settings");

    //        xmlWriter.WriteStartElement("Setting");
    //        xmlWriter.WriteAttributeString("Type", "Optic Flow Settings");

    //        xmlWriter.WriteStartElement("LifeSpan");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Life Span").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("DrawDistance");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Draw Distance").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Density");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Density").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("DistalObject");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("Distal Object").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("ObjectHeight");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Object Height").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("ObjectWidth");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("Object Width").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Setting");
    //        xmlWriter.WriteAttributeString("Type", "Joystick Settings");

    //        xmlWriter.WriteStartElement("Dimensions");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("Dimensions").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MinLinearSpeed");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Min Linear Speed").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MaxLinearSpeed");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max Linear Speed").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MinAngularSpeed");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Min Angular Speed").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MaxAngularSpeed");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max Angular Speed").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("EndTrialOnStop");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("End Trial On Stop").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("VelocityThreshold");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Velocity Threshold").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("RotationThreshold");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("RotationThreshold").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Gain");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Gain").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Setting");
    //        xmlWriter.WriteAttributeString("Type", "Firefly Settings");

    //        xmlWriter.WriteStartElement("Size");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Size").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("RewardZoneRadius");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Reward Zone Radius").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MinimumFireflyDistance");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Minimum Firefly Distance").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MaximumFireflyDistance");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Maximum Firefly Distance").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MinAngle");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Min Angle").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MaxAngle");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Max Angle").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("JuiceTime");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Juice Time").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Ratio");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Ratio").ToString());
    //        xmlWriter.WriteEndElement();

    //        //xmlWriter.WriteStartElement("Reward");
    //        //xmlWriter.WriteString(PlayerPrefs.GetFloat("Reward").ToString());
    //        //xmlWriter.WriteEndElement();

    //        //xmlWriter.WriteStartElement("NumberofFireflies");
    //        //xmlWriter.WriteString(PlayerPrefs.GetFloat("Number of Fireflies").ToString());
    //        //xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("D1");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("D1").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("D2");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("D2").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("D3");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("D3").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("D4");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("D4").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("D5");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("D5").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("R1");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("R1").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("R2");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("R2").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("R3");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("R3").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("R4");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("R4").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("R5");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("R5").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Timeout");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Timeout").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Ramp");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("Ramp").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("RampTime");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Ramp Time").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("RampDelay");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Ramp Delay").ToString());
    //        xmlWriter.WriteEndElement();

    //        //xmlWriter.WriteStartElement("FireflyLifeSpan");
    //        //xmlWriter.WriteString(PlayerPrefs.GetFloat("Firefly Life Span").ToString());
    //        //xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MinimumWaittoCheck");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Minimum Wait to Check").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MaximumWaittoCheck");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Maximum Wait to Check").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Mean1");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Mean 1").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MinimumIntertrialWait");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Minimum Intertrial Wait").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("MaximumIntertrialWait");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Maximum Intertrial Wait").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Mean2");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("Mean 2").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("OpticFlowSeed");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("Optic Flow Seed").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("FireflySeed");
    //        xmlWriter.WriteString(seed.ToString());
    //        xmlWriter.WriteEndElement();

    //        //xmlWriter.WriteStartElement("SwitchBehavior");
    //        //xmlWriter.WriteString(PlayerPrefs.GetString("Switch Behavior"));
    //        //xmlWriter.WriteEndElement();

    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Setting");
    //        xmlWriter.WriteAttributeString("Type", "Moving Firefly Settings");

    //        xmlWriter.WriteStartElement("MovingON");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("Moving ON").ToString());
    //        xmlWriter.WriteEndElement();

    //        //xmlWriter.WriteStartElement("RatioMoving");
    //        //xmlWriter.WriteString(PlayerPrefs.GetFloat("Ratio Moving").ToString());
    //        //xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("VertHor");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("VertHor").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("V1");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("V1").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("V2");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("V2").ToString());
    //        xmlWriter.WriteEndElement();


    //        xmlWriter.WriteStartElement("VR1");
    //        xmlWriter.WriteString(PlayerPrefs.GetFloat("VR1").ToString());
    //        xmlWriter.WriteEndElement();



    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("Setting");
    //        xmlWriter.WriteAttributeString("Type", "Data Collection Settings");

    //        xmlWriter.WriteStartElement("Path");
    //        xmlWriter.WriteString(PlayerPrefs.GetString("Path"));
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteStartElement("FullON");
    //        xmlWriter.WriteString(PlayerPrefs.GetInt("Full ON").ToString());
    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteEndElement();

    //        xmlWriter.WriteEndDocument();
    //        xmlWriter.Close();

    //        //SceneManager.LoadScene("MainMenu");
    //        //SceneManager.UnloadSceneAsync("Mouse Arena");
    //    }
    //    catch (Exception e)
    //    {
    //        UnityEngine.Debug.LogError(e);
    //    }
    //}
}