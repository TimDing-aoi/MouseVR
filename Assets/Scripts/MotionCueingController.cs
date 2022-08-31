#undef USING_100HZ
//#define TEST
#define ACTIVE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BallController;
using static AccelerometerController;
using static RewardArena;
using UnityEngine.InputSystem;

public class MotionCueingController : MonoBehaviour
{
    public static float freq = 50;
    public bool test;
    public bool active;

    public GameObject player;
    public CMotionCueing motionCueing;
    public static MotionCueingController motionCueingController;

    readonly List<double> t = new List<double>();
    readonly List<double> vel = new List<double>();
    readonly List<double> x = new List<double>();
    readonly List<double> y = new List<double>();
    readonly List<double> yaw = new List<double>();

    [ShowOnly] public bool flagMCActive = false;
    [ShowOnly] public bool flagStart = false;
    [ShowOnly] public bool IsStop = false;
    int i = 0;

    double limit1 = 0.23;
    double limit2 = 0.4;
    double limit3 = 4;
    double limit4 = 10;
    double limit5 = 30;
    double limit6 = 150;
    double maxVelX = 1;
    double maxVelY = 1;
    double maxVelAng = 90;

    public double speed = 0;
    public double angSpeed = 0;

    public double curSpeed = 0;
    public double prevSpeed = 0;
    public double decayRate = 0.005;

    public Task<int> currentTask;

    Keyboard keyboard;

    // Start is called before the first frame update
    void OnEnable()
    {
        active = (int)PlayerPrefs.GetFloat("Enable MC") == 1;
        print(active);
        limit1 = PlayerPrefs.GetFloat("Limit 1") == 0f ? 0.23 : (double)PlayerPrefs.GetFloat("Limit 1");
        limit2 = PlayerPrefs.GetFloat("Limit 2") == 0f ? 0.4 : (double)PlayerPrefs.GetFloat("Limit 2");
        limit3 = PlayerPrefs.GetFloat("Limit 3") == 0f ? 4 : (double)PlayerPrefs.GetFloat("Limit 3");
        limit4 = PlayerPrefs.GetFloat("Limit 4") == 0f ? 10 : (double)PlayerPrefs.GetFloat("Limit 4");
        limit5 = PlayerPrefs.GetFloat("Limit 5") == 0f ? 30 :(double)PlayerPrefs.GetFloat("Limit 5");
        limit6 = PlayerPrefs.GetFloat("Limit 6") == 0f ? 150 : (double)PlayerPrefs.GetFloat("Limit 6");
        maxVelX = PlayerPrefs.GetFloat("Max X Vel") == 0f ? 1 : (double)PlayerPrefs.GetFloat("Max X Vel");
        maxVelY = PlayerPrefs.GetFloat("Max Y Vel") == 0f ? 1 : (double)PlayerPrefs.GetFloat("Max Y Vel");
        maxVelAng = PlayerPrefs.GetFloat("Max Ang Vel") == 0f ? 90 : (double)PlayerPrefs.GetFloat("Max Ang Vel");

        keyboard = Keyboard.current;

        DontDestroyOnLoad(this);



        //#if ACTIVE
        //#if TEST
        //        TestMotionCueing();
        //#else
        //        ActivateMotionCueing();
        //#endif
        //#endif


        if (active)
        {
            // ip, commands, errt, arena limit, max speed, max acc, 10, 30, 150, 1, max x vel, max ang vel
            motionCueing = new CMotionCueing("192.168.16.10", 61557, 61559, limit1, limit2, limit3, limit4, limit5, limit6, maxVelX, maxVelY, maxVelAng);
            motionCueingController = this;


            if (test)
            {
                TestMotionCueing();
            }
            else
            {
                ActivateMotionCueing();
            }


        }
    }

    private void Update()
    {
//#if TEST
//        if (flagMCActive && i < x.Count)
//#else
//        if (flagMCActive && SharedReward.playing)
//#endif
//        {
//            if (!flagStart) flagStart = true;
//#if ACTIVE
//#if TEST
//            motionCueing.Calculate(x[i], y[i], yaw[i], 0, (uint)i, 0, i == t.Count - 1);
//#else
//            //var t1 = Time.realtimeSinceStartup;
//            motionCueing.Calculate(speed, 0, angSpeed, 0, (uint)i, 0, IsStop);
//            //var t2 = Time.realtimeSinceStartup;
//#endif
//#endif
//            //print(t2 - t1);
//        }
//        else
//        {
//            //motionCueing.Calculate(0, 0, 0, 0, (uint)i, 0, true);
//        }

        if (active)
        {
            if (test)
            {
                if (flagMCActive && i < x.Count)
                {
                    if (!flagStart) flagStart = true;
                    motionCueing.Calculate(x[i], y[i], yaw[i], 0, (uint)i, 0, i == t.Count - 1);
                }
            }
            else
            {
                if (flagMCActive && SharedReward.playing)
                {
                    motionCueing.Calculate(speed, 0, angSpeed, 0, (uint)i, 0, IsStop);
                }
            }
        }
        else
        {
            
        }

        i++;

        //Keyboard keyboard = Keyboard.current;
        //if (keyboard.enterKey.wasReleasedThisFrame)
        //{
        //    IsStop = true;

        //    GoToRetractedPos();
        //    ControlOffAndDisable();

        //}

    }

    //#if ACTIVE
    //#if !TEST
    //    private void FixedUpdate()
    //    {
    //        speed = (double)SharedReward.currSpeed;
    //        angSpeed = (double)SharedReward.angSpeed;
    //        IsStop = SharedReward.IsStop;
    //    }
    //#endif
    //#endif
    private void FixedUpdate()
    {
        if (active)
        {
            // calibrated values


            speed = Ball.zVel;
            angSpeed = Ball.yawVel;


            if (speed > 0.2f)
            {
                speed = 0.2f;
                //Debug.Log("zVel upperbound-------------");
            }
            else if (speed < -0.2f)
            {
                speed = -0.2f;
                //Debug.Log("zVel lowerbound-------------");
            }

            prevSpeed = curSpeed;
            curSpeed = speed;

            if (curSpeed < prevSpeed && curSpeed - decayRate > 0)
            {
                speed -= decayRate;
            }



            // ask Jean whether we can use roll in MC
            IsStop = SharedReward.IsStop;

        }
    }
    public async Task StopMovement()
    {
        //flagMCActive = false;

        currentTask = motionCueing.Stop(); // Send STOP

        await new WaitUntil(() => currentTask.IsCompleted);

        currentTask = motionCueing.GoToRetracted(); // Send MOVE#SPECIFICPOS2

        await new WaitUntil(() => currentTask.IsCompleted);

        currentTask = motionCueing.ControlOff(); // Send CONTROLOFF, DISABLE

        await new WaitUntil(() => currentTask.IsCompleted);

        motionCueing.Dispose();
    }

    public async void ControlOnAndEnable()
    {
        await motionCueing.ControlOn();
    }

    public async void ControlOffAndDisable()
    {
        await motionCueing.ControlOff();
    }

    public async void GoToZeroPos()
    {
        await motionCueing.GoToZero();
    }

    public async void GoToRetractedPos()
    {
        await motionCueing.GoToRetracted();
    }

    public async void ConfigureERTT()
    {
        await motionCueing.ConfigureERTT(0.01f, 0, 1, 15, 0.3f);
    }

    async void TestMotionCueing()
    {
        //Task<int> currentTask;
        //Task<int> mcTask;
#if USING_100HZ
        currentTask = motionCueing.ConfigureERTT(0.01f, 0, 1, 15, 0); // Send CFG#ERTT...
#else
        currentTask = motionCueing.ConfigureERTT(1f / freq, 0, 1, 15, 0); // Send CFG#ERTT...
#endif
        await new WaitUntil(() => currentTask.IsCompleted);

        motionCueing.ConnectERTT();

        currentTask = motionCueing.ControlOff(); // Send CONTROLOFF, DISABLE
         
        await new WaitUntil(() => currentTask.IsCompleted);

        currentTask = motionCueing.ControlOn(); // Send CONTROLON, ENABLE

        await new WaitUntil(() => currentTask.IsCompleted);


        currentTask = motionCueing.GoToZero(); // Send MOVE#SPECIFICPOS1

        await new WaitUntil(() => currentTask.IsCompleted);

        //currentTask = motionCueing.Move();

        //StreamReader sr = new StreamReader("C:\\Users\\jc10487\\Documents\\MATLAB\\test.txt");
        //string line = sr.ReadLine();

        int mode = 1; // 1 = straight 2 = circle

        if (mode == 1)
        {
            for (double i = -25; i < 15; i += 0.01667)
            {
                t.Add(i);
            }

            for (int i = 0; i < t.Count; i++)
            {
                vel.Add(0);
            }
            for (int i = 0; i < t.Count; i++)
            {
                if (t[i] > 0)
                {
                    vel[i] = 1;
                }
            }
            for (int i = 0; i < t.Count; i++)
            {
                if (t[i] > 0 && t[i] < 1)
                {
                    vel[i] = t[i];
                }
            }
            for (int i = 0; i < t.Count; i++)
            {
                x.Add(vel[i] * 0.2);
                y.Add(0);
                yaw.Add(0);
            }
        }
        else if (mode == 2)
        {
            for (double i = -5; i < 35.03; i += 0.01)
            {
                t.Add(i);
            }
            for (int i = 0; i < t.Count; i++)
            {
                vel.Add(0);
            }
            for (int i = 0; i < t.Count; i++)
            {
                if (t[i] > 0)
                {
                    vel[i] = 0.1;
                }
            }
            for (int i = 0; i < t.Count; i++)
            {
                if (t[i] > 0 && t[i] < 1)
                {
                    vel[i] = t[i] * 0.1;
                }
            }
            for (int i = 0; i < t.Count; i++)
            {
                x.Add(vel[i]);
                y.Add(0);
                if (t[i] > 2.0)
                {
                    yaw.Add(28.6479);
                }
                else
                {
                    yaw.Add(0);
                }
            }
        }
        else if (mode == 3)
        {
            for (double i = -5; i < 40.01; i += 0.01)
            {
                t.Add(i);
            }
            for (int i = 0; i < t.Count; i++)
            {
                vel.Add(0);
                yaw.Add(0);
            }
            for (int i = 0; i < 6; i++)
            {
                for (int k = 0; k < t.Count; k++)
                {
                    if (t[k] > i * 10 && t[k] <= i * 10 + 3)
                    {
                        vel[k] = 0.05;
                    }
                    if (t[k] > i * 10 + 5 && t[k] <= i * 10 + 9)
                    {
                        yaw[k] = 90 / 4;
                    }
                }
            }
            for (int i = 0; i < t.Count; i++)
            {
                x.Add(vel[i]);
                y.Add(0);
            }
        }
        else if (mode == 4)
        {
            for (double i = -5; i < 40.01; i += 0.01)
            {
                t.Add(i);
            }
            for (int i = 0; i < t.Count; i++)
            {
                vel.Add(0);
                yaw.Add(0);
            }
            for (int i = 0; i < 6; i++)
            {
                for (int k = 0; k < t.Count; k++)
                {
                    if (t[k] > i * 10 && t[k] <= i * 10 + 3)
                    {
                        vel[k] = 0.05;
                    }
                    if (t[k] > i * 10 + 5 && t[k] <= i * 10 + 9)
                    {
                        yaw[k] = 90 / 4;
                    }
                }
            }
            for (int i = 0; i < t.Count; i++)
            {
                y.Add(vel[i]);
                x.Add(0);
            }
        }

        print("motion cue start");

        flagMCActive = true;

        await new WaitUntil(() => flagStart);
        //mcTask = Go();

        currentTask = motionCueing.Move(); // Send MOVE#TRAJ

        await new WaitUntil(() => currentTask.IsCompleted);

        print("done");

        flagMCActive = false;

        currentTask = motionCueing.Stop(); // Send STOP

        await new WaitUntil(() => currentTask.IsCompleted);

        currentTask = motionCueing.GoToRetracted(); // Send MOVE#SPECIFICPOS2

        await new WaitUntil(() => currentTask.IsCompleted);

        currentTask = motionCueing.ControlOff(); // Send CONTROLOFF, DISABLE

        await new WaitUntil(() => currentTask.IsCompleted);

        motionCueing.Dispose();
    }


    IEnumerator WaitCoroutine()
    {

        Debug.Log("MC waiting to connect at : " + Time.time);
        yield return new WaitForSeconds(15);
        Debug.Log("MC connecting at : " + Time.time);
    }

    async void ActivateMotionCueing()
    {
        // safety measure
        StartCoroutine(WaitCoroutine());

        //Task<int> currentTask;
        //Task<int> mcTask;
#if USING_100HZ
        currentTask = motionCueing.ConfigureERTT(0.01f, 0, 1, 15, 0); // Send CFG#ERTT...
#else
        currentTask = motionCueing.ConfigureERTT(1f / freq, 0, 1, 15, 0); // Send CFG#ERTT...
#endif
        await new WaitUntil(() => currentTask.IsCompleted);

        motionCueing.ConnectERTT();

        currentTask = motionCueing.ControlOff(); // Send CONTROLOFF, DISABLE

        await new WaitUntil(() => currentTask.IsCompleted);

        currentTask = motionCueing.ControlOn(); // Send CONTROLON, ENABLE

        await new WaitUntil(() => currentTask.IsCompleted);


        currentTask = motionCueing.GoToZero(); // Send MOVE#SPECIFICPOS1

        await new WaitUntil(() => currentTask.IsCompleted);

        print("motion cue start");

        flagMCActive = true;

        //await new WaitUntil(() => flagStart);
        //mcTask = Go();

        currentTask = motionCueing.Move(); // Send MOVE#TRAJ
    }

    public void End()
    {
        motionCueing.Dispose();
    }


    private void OnDisable()
    {
        //GoToRetractedPos();
        //ControlOffAndDisable();
    }

    private void OnApplicationQuit()
    {
        //GoToRetractedPos();
        //ControlOffAndDisable();

    }

public class CMotionCueing : IDisposable
{
        public struct MotionCueingInputs
        {
            public double ballX;
            public double ballY;
            public double ballYaw;
            public double moogYaw;
            public double arenaR;
            public double avoidanceR;
            public double vrX;
            public double vrY;
            public double vrYaw;

        };

        public struct Frame
        {
            public double lateral;
            public double surge;
            public double heave;
            public double pitch;
            public double roll;
            public double yaw;
        }
    #if USING_100HZ
        [DllImport("MotionCueing_50.dll")] static private extern IntPtr Create();

        [DllImport("MotionCueing_50.dll")] static private extern void Destroy(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern void CallUpdateParameters(IntPtr pObj, IntPtr gpList);

        [DllImport("MotionCueing_50.dll")] static public extern void CallResetLastMoogXYVel(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern void CallCalculation(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern void CallImportInputs(IntPtr pObj, double ball_x, double ball_y, double ball_yaw, double moog_yaw, double arena_radius, double avoidance_radius);

        [DllImport("MotionCueing_50.dll")] static public extern float GetHeave(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetSurge(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetLateral(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetYaw(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetPitch(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetRoll(IntPtr pObj);
    #else
        [DllImport("MotionCueing_50.dll")] static private extern IntPtr Create(double limit1, double limit2, double limit3, double limit4, double limit5, double limit6, double maxX, double maxY, double maxRot);

        [DllImport("MotionCueing_50.dll")] static private extern void Destroy(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern void CallUpdateParameters(IntPtr pObj, IntPtr gpList);

        [DllImport("MotionCueing_50.dll")] static public extern void CallResetLastMoogXYVel(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern void CallCalculation(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern void CallImportInputs(IntPtr pObj, double ball_x, double ball_y, double ball_yaw, double moog_yaw, double arena_radius, double avoidance_radius);

        [DllImport("MotionCueing_50.dll")] static public extern float GetHeave(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetSurge(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetLateral(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetYaw(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetPitch(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern float GetRoll(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetLimitOne(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetLimitTwo(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetLimitThree(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetLimitFour(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetLimitFive(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetLimitSix(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetBallMaxX(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetBallMaxY(IntPtr pObj);

        [DllImport("MotionCueing_50.dll")] static public extern double GetBallMaxRot(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetVRPosX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetVRPosY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetVRGIAErrorX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetVRGIAErrorY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetRatGIAX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetRatGIAY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetRatXGIAErrorX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetRatXGIAErrorY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetDesiredMoogAccX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetDesiredMoogAccY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCInternalVariable0X(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCInternalVariable0Y(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCInternalVariable1X(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCInternalVariable1Y(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCInternalVariable2X(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCInternalVariable2Y(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCAccX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCAccY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCTiltPosX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCTiltPosY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCTiltVelX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCTiltVelY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCTiltAccX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMCTiltAccY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogAccX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogAccY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogVelX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogVelY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogPosX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogPosY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogTiltAccX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogTiltAccY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogTiltVelX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogTiltVelY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogTiltPosX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetMoogTiltPosY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetFinalMoogVelX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetFinalMoogVelY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetFinalMoogAccX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetFinalMoogAccY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetFinalMoogGIAX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetFinalMoogGIAY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetRatGIAErrorX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetRatGIAErrorY(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetFinalVRVelX(IntPtr pObj);
        [DllImport("MotionCueing_50.dll")] static public extern double GetFinalVRVelY(IntPtr pObj);

#endif
        //private string address;
        //private int port;
        private TcpClient hexaClient;
        private TcpClient commandClient;
        private NetworkStream hexaStream;
        private NetworkStream commandStream;
        private IntPtr m_pNativeObject;
        private float ainOffset = -0.053f;
        private float ainVScale = 40.0f;
        private double[][] unfiltered = new double[3][];
        public double[][] filtered = new double[3][];
        private double[] limit = new double[3];
        private double[] FA = new double[3];
        private double[] FB = new double[3];
        private string hexaAddress;
        private int hexaPort;
        public Frame frame;

        public CMotionCueing(string hexaAddress, int hexaPort, int commandPort, double limit1, double limit2, double limit3, double limit4, double limit5, double limit6, double maxX, double maxY, double maxRot)
        {
            // init pointer to motion cueing object
            this.m_pNativeObject = Create(limit1, limit2, limit3, limit4, limit5, limit6, maxX, maxY, maxRot);

            Debug.Log("MC Controller Created");

            Debug.Log(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
                GetLimitOne(m_pNativeObject), 
                GetLimitTwo(m_pNativeObject), 
                GetLimitThree(m_pNativeObject), 
                GetLimitFour(m_pNativeObject),
                GetLimitFive(m_pNativeObject),
                GetLimitSix(m_pNativeObject),
                GetBallMaxX(m_pNativeObject),
                GetBallMaxY(m_pNativeObject),
                GetBallMaxRot(m_pNativeObject)));
            // init 6 points for hexapod (lateral -> tx, surge -> ty, heave -> tz, pitch -> rx, roll -> ry, yaw -> rz;
            // if you look this up i'm pretty sure axes won't be lined up but this is how it was defined originally
            // so just roll with it
            this.frame.lateral = 0;
            this.frame.surge = 0;
            this.frame.heave = 0;
            this.frame.pitch = 0;
            this.frame.roll = 0;
            this.frame.yaw = 0;

            // init tcp/ip connection
            //this.address = addy;
            //this.port = portnum;
            this.hexaAddress = hexaAddress;
            this.hexaPort = hexaPort;
            this.commandClient = new TcpClient(hexaAddress, commandPort);
            this.commandStream = commandClient.GetStream();

            //hexaClient.SendBufferSize = 60;
            //this.motorClient = new TcpClient(motorAddress, motorPort);
            //this.motorStream = hexaClient.GetStream();

            // butter filter coefficients; use [filter_b,filter_a]=butter(2,1*0.01,'low') in matlab
            // lp cutoff = 5 => 2.5hz
            FA[0] = 1;
            FA[1] = -1.632993161855452;
            FA[2] = 0.690598923241497;

            FB[0] = 0.014401440346511;
            FB[1] = 0.028802880693022;
            FB[2] = 0.014401440346511;

            limit[0] = 1;
            limit[1] = 1;
            limit[2] = 90;

            unfiltered[0] = new double[3] { 0, 0, 0 };
            unfiltered[1] = new double[3] { 0, 0, 0 };
            unfiltered[2] = new double[3] { 0, 0, 0 };

            filtered[0] = new double[3] { 0, 0, 0 };
            filtered[1] = new double[3] { 0, 0, 0 };
            filtered[2] = new double[3] { 0, 0, 0 };
        }
        public void Dispose()
        {
            hexaStream.Close();
            commandStream.Close();
            hexaClient.Close();
            commandClient.Close();
            Dispose(true);
        }

        protected virtual void Dispose(bool bDisposing)
        {
            if (this.m_pNativeObject != IntPtr.Zero)
            {
                Destroy(this.m_pNativeObject);
                this.m_pNativeObject = IntPtr.Zero;
            }
            if (bDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }
        ~CMotionCueing()
        {
            Dispose(false);
        }

        public void SetInputs(double ball_x, double ball_y, double ball_yaw, double moog_yaw)
        {
            // double ball_x = *get from computer mouse input*
            // double ball_y = *get from computer mouse input*
            // double ball_yaw = *get from computer mouse input*
            // double moog_yaw = GetMotorYaw();
            // double arena_radius = 1.0;
            // double avoidance_radius = 0.6;

            //Debug.Log(string.Format("Unfiltered:{0},{1},{2}", ball_x, ball_y, ball_yaw));
            //Tuple<double, double, double> tuple = ApplyLowPassFilter(ball_x, ball_y, ball_yaw);

            //ball_x = tuple.Item1;
            //ball_y = tuple.Item2;
            //ball_yaw = tuple.Item3;
            ////Debug.Log(string.Format("Filtered:{0},{1},{2}", ball_x, ball_y, ball_yaw));

            //CallImportInputs(this.m_pNativeObject, ball_x, ball_y, ball_yaw, moog_yaw, 1.0, 0.6);
        }

        public void SetInputsTest()
        {
            CallImportInputs(this.m_pNativeObject, 0.1, 0.1, 10, 10, 10, 4);
        }

        public async Task Test()
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes("DISABLE\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            while (temp[0] != 10 || commandStream.DataAvailable)
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            }

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                Debug.Log(code.ToString());
                return;
            }
            Array.Clear(buffer, 0, buffer.Length);

            //Debug.Log("Still going");

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(buffer, 0, send.Length + 3);
            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                MonoBehaviour.print(code);
                return;
            }
            Array.Clear(buffer, 0, send.Length + 3);
        }

        public async Task<int> Reset()
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes("RESET\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                Array.Clear(buffer, 0, buffer.Length);

                await new WaitUntil(() => commandStream.DataAvailable);

                commandStream.Read(temp, 0, 1);

                i = 0;
                do
                {
                    buffer[i] = temp[0];
                    Array.Clear(temp, 0, 1);
                    commandStream.Read(temp, 0, 1);
                    i++;
                } while (temp[0] != 10 || commandStream.DataAvailable);

                Debug.Log(Encoding.ASCII.GetString(buffer));
                code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                Array.Clear(buffer, 0, buffer.Length);
                if (code < 0)
                {
                    return code;
                }
                else return 0;
            }
        }
        public async Task<int> ControlOn()
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes("ENABLE\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                Array.Clear(buffer, 0, buffer.Length);

                await new WaitUntil(() => commandStream.DataAvailable);

                commandStream.Read(temp, 0, 1);

                i = 0;
                do
                {
                    buffer[i] = temp[0];
                    Array.Clear(temp, 0, 1);
                    commandStream.Read(temp, 0, 1);
                    i++;
                } while (temp[0] != 10 || commandStream.DataAvailable);

                Debug.Log(Encoding.ASCII.GetString(buffer));
                code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                Array.Clear(buffer, 0, buffer.Length);
                if (code < 0)
                {
                    return code;
                }
            }
            Array.Clear(buffer, 0, buffer.Length);

            send = Encoding.ASCII.GetBytes("CONTROLON\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                Array.Clear(buffer, 0, buffer.Length);

                await new WaitUntil(() => commandStream.DataAvailable);

                commandStream.Read(temp, 0, 1);

                i = 0;
                do
                {
                    buffer[i] = temp[0];
                    Array.Clear(temp, 0, 1);
                    commandStream.Read(temp, 0, 1);
                    i++;
                } while (temp[0] != 10 || commandStream.DataAvailable);

                Debug.Log(Encoding.ASCII.GetString(buffer));
                code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                Array.Clear(buffer, 0, buffer.Length);
                if (code < 0)
                {
                    return code;
                }
                else return 0;
            }
        }

        public async Task<int> ControlOff()
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes("CONTROLOFF\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                Array.Clear(buffer, 0, buffer.Length);

                await new WaitUntil(() => commandStream.DataAvailable);

                commandStream.Read(temp, 0, 1);

                i = 0;
                do
                {
                    buffer[i] = temp[0];
                    Array.Clear(temp, 0, 1);
                    commandStream.Read(temp, 0, 1);
                    i++;
                } while (temp[0] != 10 || commandStream.DataAvailable);

                Debug.Log(Encoding.ASCII.GetString(buffer));
                code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                Array.Clear(buffer, 0, buffer.Length);
                if (code < 0)
                {
                    return code;
                }
            }
            Array.Clear(buffer, 0, buffer.Length);

            send = Encoding.ASCII.GetBytes("DISABLE\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                Array.Clear(buffer, 0, buffer.Length);

                await new WaitUntil(() => commandStream.DataAvailable);

                commandStream.Read(temp, 0, 1);

                i = 0;
                do
                {
                    buffer[i] = temp[0];
                    Array.Clear(temp, 0, 1);
                    commandStream.Read(temp, 0, 1);
                    i++;
                } while (temp[0] != 10 || commandStream.DataAvailable);

                Debug.Log(Encoding.ASCII.GetString(buffer));
                code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                Array.Clear(buffer, 0, buffer.Length);
                if (code < 0)
                {
                    return code;
                }
                else return 0;
            }
        }

        public async Task<int> GoToZero()
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes("MOVE#SPECIFICPOS1\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                Array.Clear(buffer, 0, buffer.Length);

                await new WaitUntil(() => commandStream.DataAvailable);

                commandStream.Read(temp, 0, 1);

                i = 0;
                do
                {
                    buffer[i] = temp[0];
                    Array.Clear(temp, 0, 1);
                    commandStream.Read(temp, 0, 1);
                    i++;
                } while (temp[0] != 10 || commandStream.DataAvailable);

                Debug.Log(Encoding.ASCII.GetString(buffer));
                code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                Array.Clear(buffer, 0, buffer.Length);
                if (code < 0)
                {
                    return code;
                }
                else return 0;
            }
        }

        public async Task<int> GoToRetracted()
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes("MOVE#SPECIFICPOS2\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                Array.Clear(buffer, 0, buffer.Length);

                await new WaitUntil(() => commandStream.DataAvailable);

                commandStream.Read(temp, 0, 1);

                i = 0;
                do
                {
                    buffer[i] = temp[0];
                    Array.Clear(temp, 0, 1);
                    commandStream.Read(temp, 0, 1);
                    i++;
                } while (temp[0] != 10 || commandStream.DataAvailable);

                Debug.Log(Encoding.ASCII.GetString(buffer));
                code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                Array.Clear(buffer, 0, buffer.Length);
                if (code < 0)
                {
                    return code;
                }
                else return 0;
            }
        }

        public async Task<int> Move()
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes("MOVE#TRAJ\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                return 0;
                //Array.Clear(buffer, 0, buffer.Length);

                //await new WaitUntil(() => commandStream.DataAvailable);

                //commandStream.Read(temp, 0, 1);

                //i = 0;
                //do
                //{
                //    buffer[i] = temp[0];
                //    Array.Clear(temp, 0, 1);
                //    commandStream.Read(temp, 0, 1);
                //    i++;
                //} while (temp[0] != 10 || commandStream.DataAvailable);

                //Debug.Log(Encoding.ASCII.GetString(buffer));
                //code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                //Array.Clear(buffer, 0, buffer.Length);
                //if (code < 0)
                //{
                //    return code;
                //}
                //else return 0;
            }
        }

        public async Task<int> Stop()
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes("STOP\r\n");

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));

            Array.Clear(buffer, 0, buffer.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));

            Array.Clear(buffer, 0, buffer.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            return 0;
        }

        public async Task<int> ConfigureERTT(float samplingTime, float initialDynamicLimitation, uint enableDigout, uint toggleSaturation, float lag)
        {
            int code;
            byte[] buffer = new byte[64];
            byte[] temp = new byte[1];
            byte[] send = Encoding.ASCII.GetBytes(string.Format("CFG#ERTT{0},{1},{2},{3},{4}\r\n",
                samplingTime.ToString(),
                initialDynamicLimitation.ToString(),
                enableDigout.ToString(),
                toggleSaturation.ToString(),
                lag.ToString()));

            commandStream.Write(send, 0, send.Length);

            await new WaitUntil(() => commandStream.DataAvailable);

            commandStream.Read(temp, 0, 1);

            var i = 0;
            do
            {
                buffer[i] = temp[0];
                Array.Clear(temp, 0, 1);
                commandStream.Read(temp, 0, 1);
                i++;
            } while (temp[0] != 10 || commandStream.DataAvailable);

            Debug.Log(Encoding.ASCII.GetString(buffer));
            code = GetEchoCode(Encoding.ASCII.GetString(buffer));
            if (code < 0)
            {
                return code;
            }
            else
            {
                Array.Clear(buffer, 0, buffer.Length);

                await new WaitUntil(() => commandStream.DataAvailable);

                commandStream.Read(temp, 0, 1);

                i = 0;
                do
                {
                    buffer[i] = temp[0];
                    Array.Clear(temp, 0, 1);
                    commandStream.Read(temp, 0, 1);
                    i++;
                } while (temp[0] != 10 || commandStream.DataAvailable);

                Debug.Log(Encoding.ASCII.GetString(buffer));
                code = GetEchoCode(Encoding.ASCII.GetString(buffer));
                Array.Clear(buffer, 0, buffer.Length);
                if (code < 0)
                {
                    return code;
                }
                else return 0;
            }
        }

        public void ConnectERTT()
        {
            this.hexaClient = new TcpClient(hexaAddress, hexaPort);
            this.hexaStream = hexaClient.GetStream();

            if (!hexaStream.CanWrite) MonoBehaviour.print("hexaStream error");
        }

        public void Calculate(double ball_x, double ball_y, double ball_yaw, double moog_yaw, uint idx, uint output, bool isEnd)
        {
            //var tNow = Time.realtimeSinceStartup;
            // remove filtering because it adds the lag
            Tuple<double, double, double> tuple = ApplyLowPassFilter(ball_x, ball_y, ball_yaw);

            ball_x = tuple.Item1;
            ball_y = tuple.Item2;
            ball_yaw = tuple.Item3;
            //Debug.Log(string.Format("Filtered:{0},{1},{2}", ball_x, ball_y, ball_yaw));

            CallImportInputs(this.m_pNativeObject, ball_x, ball_y, ball_yaw, moog_yaw, 10.0, 10.6);

            CallCalculation(this.m_pNativeObject);

            this.frame.surge = GetSurge(this.m_pNativeObject);
            this.frame.lateral = GetLateral(this.m_pNativeObject);
            this.frame.heave = GetHeave(this.m_pNativeObject);
            this.frame.roll = GetRoll(this.m_pNativeObject);
            // need -1 to move in right direction
            this.frame.pitch = -1*GetPitch(this.m_pNativeObject);
            this.frame.yaw = GetYaw(this.m_pNativeObject);

            //print(string.Format("heave {0}, lateral {1},surge {2},roll {3},pitch {4},yaw {5}", frame.heave, frame.lateral, frame.surge, frame.roll, frame.pitch, frame.yaw));
            //print(string.Format("Tilt X {0}, Tilt Y {1}", GetMCTiltAccX(this.m_pNativeObject), GetMCTiltAccY(this.m_pNativeObject)));

            byte[] msgID = BitConverter.GetBytes(idx);
            Array.Reverse(msgID);
            byte[] Tx = BitConverter.GetBytes(this.frame.surge);
            Array.Reverse(Tx);
            byte[] Ty = BitConverter.GetBytes(this.frame.lateral);
            Array.Reverse(Ty);
            byte[] Tz = BitConverter.GetBytes(this.frame.heave);
            Array.Reverse(Tz);
            byte[] Rx = BitConverter.GetBytes(this.frame.roll * Mathf.Deg2Rad);
            Array.Reverse(Rx);
            byte[] Ry = BitConverter.GetBytes(this.frame.pitch * Mathf.Deg2Rad);
            Array.Reverse(Ry);
            byte[] Rz = BitConverter.GetBytes(this.frame.yaw * Mathf.Deg2Rad);
            Array.Reverse(Rz);
            byte[] Outputs = BitConverter.GetBytes(output);
            Array.Reverse(Outputs);
            byte[] EoT = BitConverter.GetBytes((uint)(isEnd ? 1 : 0));
            Array.Reverse(EoT);

            byte[] msg = msgID.Concat(Tx).Concat(Ty).Concat(Tz).Concat(Rx).Concat(Ry).Concat(Rz).Concat(Outputs).Concat(EoT).ToArray();

            //MonoBehaviour.print(msg.Length);

            hexaStream.Write(msg, 0, 60);

            //return Time.realtimeSinceStartup - tNow;
        }

        public void SetFrame()
        {
            this.frame.lateral = GetLateral(this.m_pNativeObject);
            this.frame.heave = GetHeave(this.m_pNativeObject);
            this.frame.surge = GetSurge(this.m_pNativeObject);
            this.frame.roll = GetRoll(this.m_pNativeObject);
            this.frame.pitch = GetPitch(this.m_pNativeObject);
            this.frame.yaw = GetYaw(this.m_pNativeObject);
        }

        public byte[] Combine(params byte[][] arrays)
        {
            byte[] msg = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;

            foreach (byte[] bytes in arrays)
            {
                Buffer.BlockCopy(bytes, 0, msg, offset, bytes.Length);
                offset += bytes.Length;
            }

            return msg;
        }

        public void Send(uint idx, uint output, bool isEnd)
        {
            byte[] msgID = BitConverter.GetBytes(idx);
            Array.Reverse(msgID);
            byte[] Tx = BitConverter.GetBytes(this.frame.lateral);
            //Debug.Log(BitConverter.ToDouble(Tx, 0));
            Array.Reverse(Tx);
            byte[] Ty = BitConverter.GetBytes(this.frame.heave);
            //Debug.Log(BitConverter.ToDouble(Ty, 0));
            Array.Reverse(Ty);
            byte[] Tz = BitConverter.GetBytes(this.frame.surge);
            //Debug.Log(BitConverter.ToDouble(Tz, 0));
            Array.Reverse(Tz);
            byte[] Rx = BitConverter.GetBytes(this.frame.roll * Mathf.Deg2Rad);
            //Debug.Log(BitConverter.ToDouble(Rx, 0));
            Array.Reverse(Rx);
            byte[] Ry = BitConverter.GetBytes(this.frame.pitch * Mathf.Deg2Rad);
            //Debug.Log(BitConverter.ToDouble(Ry, 0));
            Array.Reverse(Ry);
            byte[] Rz = BitConverter.GetBytes(this.frame.yaw * Mathf.Deg2Rad);
            //Debug.Log(BitConverter.ToDouble(Rz, 0));
            Array.Reverse(Rz);
            byte[] Outputs = BitConverter.GetBytes(output);
            Array.Reverse(Outputs);
            byte[] EoT = BitConverter.GetBytes((uint)(isEnd ? 1 : 0));
            Array.Reverse(EoT);

            byte[] message = Combine(msgID, Tx, Ty, Tz, Rx, Ry, Rz, Outputs, EoT);

            if (message.Length != 60)
            {
                Debug.LogError("Error: ERTT message no 60 bytes long");
                return;
            }

            hexaStream.Write(message, 0, 60);
        }

        public Tuple<double, double, double> ApplyLowPassFilter(double vx, double vy, double vyaw)
        {
            unfiltered[0][2] = vx;
            unfiltered[1][2] = vy;
            unfiltered[2][2] = vyaw;

            for (int i = 0; i < 3; i++)
            {
                unfiltered[i][2] = CheckLimit(unfiltered[i][2], limit[i]);
                LowPassFilter(unfiltered[i], filtered[i], 3, FA, FB);
                for (int k = 1; k < 3; k++)
                {
                    unfiltered[i][k - 1] = unfiltered[i][k];
                    filtered[i][k - 1] = filtered[i][k];
                }
            }

            vx = filtered[0][2];
            vy = filtered[1][2];
            vyaw = filtered[2][2];

            return Tuple.Create(vx, vy, vyaw);
        }

        //public void EnableMotor(int mode)
        //{
        //    byte[] send = Encoding.ASCII.GetBytes("drv.dis");

        //    motorStream.Write(send, 0, send.Length);

        //    switch (mode)
        //    {
        //        case 1: // position mode
        //            send = Encoding.ASCII.GetBytes("drv.cmdsource 0");
        //            motorStream.Write(send, 0, send.Length);

        //            send = Encoding.ASCII.GetBytes("drv.opmode 2");
        //            motorStream.Write(send, 0, send.Length);

        //            send = Encoding.ASCII.GetBytes("drv.en");
        //            motorStream.Write(send, 0, send.Length);
        //            break;

        //        case 2: // velocity mode, analog control
        //            send = Encoding.ASCII.GetBytes("drv.cmdsource 3");
        //            motorStream.Write(send, 0, send.Length);

        //            send = Encoding.ASCII.GetBytes("drv.opmode 1");
        //            motorStream.Write(send, 0, send.Length);

        //            send = Encoding.ASCII.GetBytes("ain.offset " + ainOffset.ToString("F3"));
        //            motorStream.Write(send, 0, send.Length);

        //            send = Encoding.ASCII.GetBytes("ain.vscale " + ainVScale.ToString("F3"));
        //            motorStream.Write(send, 0, send.Length);

        //            send = Encoding.ASCII.GetBytes("drv.en");
        //            motorStream.Write(send, 0, send.Length);
        //            break;
        //    }
        //}

        //public void DisableMotor()
        //{
        //    byte[] send = Encoding.ASCII.GetBytes("drv.cmdsource 0"); 

        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("drv.opmode 2");
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("drv.dis");
        //    motorStream.Write(send, 0, send.Length);
        //}

        //public void GoHome()
        //{
        //    byte[] trash = new byte[4096];
        //    byte[] send = Encoding.ASCII.GetBytes("drv.dis");

        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("drv.cmdsource 0");
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("drv.opmode 2");
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("drv.en");
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("pl.fb");
        //    motorStream.Write(send, 0, send.Length);

        //    // make sure stream is empty before reading
        //    while (motorStream.DataAvailable) motorStream.Read(trash, 0, trash.Length);

        //    byte[] data = new byte[256];
        //    int bytes = motorStream.Read(data, 0, data.Length);
        //    float yaw = float.Parse(Encoding.ASCII.GetString(data, 0, bytes));

        //    MonoBehaviour.print(yaw);

        //    float mod = yaw % 360.0f;
        //    float diff = yaw - mod;

        //    if (mod > 180.0f)
        //    {
        //        diff += 360.0f;
        //    }

        //    send = Encoding.ASCII.GetBytes("MT.p " + diff.ToString("F3"));
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("MT.v 30");
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("MT.Acc 90.0");
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("MT.Dec 90.0");
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("MT.set 0");
        //    motorStream.Write(send, 0, send.Length);

        //    send = Encoding.ASCII.GetBytes("MT.move 0");
        //    motorStream.Write(send, 0, send.Length);
        //}

        //public double GetMotorYaw()
        //{
        //    byte[] send = Encoding.ASCII.GetBytes("pl.fb");
        //    byte[] trash = new byte[4096];
        //    byte[] data = new byte[256];

        //    // make sure stream is empty before reading
        //    while (motorStream.DataAvailable) motorStream.Read(trash, 0, trash.Length);

        //    motorStream.Write(send, 0, send.Length);

        //    int bytes = motorStream.Read(data, 0, data.Length);

        //    return double.Parse(Encoding.ASCII.GetString(data, 0, bytes));
        //}

        private double CheckLimit(double x, double limit)
        {
            double ans = x;
            if (x > limit)
            {
                ans = limit;
            }
            else if (x < -limit)
            {
                ans = -limit;
            }
            return ans;
        }

        private void LowPassFilter(double[] input, double[] output, int n, double[] FA, double[] FB)
        {
            output[n - 1] = 0.0;
            for (int i = 0; i < n; i++)
            {
                output[n - 1] += FB[i] * input[n - 1 - i];
            }
            for (int i = 1; i < n; i++)
            {
                output[n - 1] -= FA[i] * output[n - 1 - i];
            }
            output[n - 1] /= FA[0];
        }

        private int GetEchoCode(string echo)
        {
            string[] split = echo.Split(':');
            return int.Parse(split[1]);
        }


        }
}