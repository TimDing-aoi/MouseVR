using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BallController : MonoBehaviour
{
    //SerialPort _serialPort;
    wrmhl ball = new wrmhl();
    public static BallController Ball;

    public float pitch;
    public float roll;
    [HideInInspector] public float yaw;
    [HideInInspector] public float yawCopy;

    [HideInInspector] public float motorYaw;
    [HideInInspector] public float motorRoll;

    [HideInInspector] public float deltaX;
    [HideInInspector] public float deltaZ;
    [HideInInspector] public float deltaYaw;

    public float xVel;
    public float zVel;
    public float yawVel;

    [HideInInspector] public float vert;
    [HideInInspector] public float hor;
    private string data;
    private float initTime;
    public bool IsConnected = true;


    public string portName = "COM11";
    public int baudRate = 2000000;
    public int ReadTimeout = 5000;
    public int QueueLength = 1;
    // Start is called before the first frame update

    private System.Random rand;
    private float maxSpeed;
    private float maxAcc;
    [ShowOnly] public int seed;

    private bool keyboard;
    // replay vars
    bool isReplay;
    string replayPath;
    readonly List<float> replayX = new List<float>();
    readonly List<float> replayZ = new List<float>();
    readonly List<float> replayYaw = new List<float>();
    int replayIdx = 0;
    int replayMaxIdx;

    float Zscale;
    float Xscale;
    float Yawscale;

    IEnumerator WaitCoroutine()
    {

        Debug.Log("Ball waiting to connect at : " + Time.time);
        yield return new WaitForSeconds(15);
        Debug.Log("Ball connecting at : " + Time.time);
    }

    void Start()
    {
        keyboard = (int)PlayerPrefs.GetFloat("IsKeyboard") == 1;
        motorYaw = PlayerPrefs.GetFloat("motorYaw"); // 0 = 1D (F/B), 1 = 2D (L/R/F/B), 2 - yaw rot
        motorRoll = PlayerPrefs.GetFloat("motorRoll"); // 0 = 1D (F/B), 1 = 2D (L/R/F/B), 2 - yaw rot

        Ball = this;
        ball.set(portName, baudRate, ReadTimeout, QueueLength);

        //if MC not active, connect directly. if MC active, connect after 15 seconds
        if ((int)PlayerPrefs.GetFloat("Enable MC") == 1)
        {
            StartCoroutine(WaitCoroutine());
            ball.connect();
        } 
        else
        {
            Debug.Log("Ball without waiting connect at : " + Time.time);
            ball.connect();

        }

        




        initTime = Time.time;

        isReplay = (int)PlayerPrefs.GetFloat("IsReplay") == 1;
        print(string.Format("is replay {0}, is keyboard {1}", isReplay, keyboard));

        // calibration
        Zscale = PlayerPrefs.GetFloat("ZScale") == 0 ? 1 : PlayerPrefs.GetFloat("ZScale");
        Xscale = PlayerPrefs.GetFloat("XScale") == 0 ? 1 : PlayerPrefs.GetFloat("XScale");
        Yawscale = PlayerPrefs.GetFloat("YawScale") == 0 ? 1 : PlayerPrefs.GetFloat("YawScale");


        if (isReplay)
        {
            replayPath = PlayerPrefs.GetString("ReplayPath");
            //print(replayPath);
            StreamReader sr = new StreamReader(replayPath);

            string[] split;
            while (!sr.EndOfStream)
            {
                split = sr.ReadLine().Split(',');
                replayX.Add(float.Parse(split[0]));
                replayZ.Add(float.Parse(split[1]));
                replayYaw.Add(float.Parse(split[2]));
            }

            replayMaxIdx = replayX.Count;
        }

        maxSpeed = 0.2f/60.0f;
        maxAcc = 0.2f/3600.0f;
    }

    public async void Update()
    {
        try
        {

            //vert = Input.GetAxis("Vertical"); // ~0.5m/s
            //if (vert > 0)
            //{
            //    vert = 1 / 20000.0f;
            //}
            //else if (vert < 0)
            //{
            //    vert = -1 / 20000.0f;
            //}

            //if (vert > 0)
            //{
            //    pitch += maxAcc;

            //    if (pitch > maxSpeed)
            //    {
            //        pitch = maxSpeed;
            //    }
            //}
            //else if (vert < 0)
            //{
            //    pitch -= maxAcc;

            //    if (pitch < -maxSpeed)
            //    {
            //        pitch = -maxSpeed;
            //    }
            //}
            //else
            //{
            //    if (pitch < 0)
            //    {
            //        pitch += maxAcc;

            //        if (pitch > 0)
            //        {
            //            pitch = 0;
            //        }
            //    }
            //    else if (pitch > 0)
            //    {
            //        pitch -= maxAcc;

            //        if (pitch < 0)
            //        {
            //            pitch = 0;
            //        }
            //    }
            //}


            //print(String.Format("zVel: {0}, xVel: {1}, yawVel: {2}", zVel, xVel, yawVel));

            //if (hor > 0)
            //{
            //    hor = 1 / 20000.0f;
            //}
            //else if (hor < 0)
            //{
            //    hor = -1 / 20000.0f;
            //}

            //if (hor > 0)
            //{
            //    yaw += maxAcc;

            //    if (yaw > maxSpeed)
            //    {
            //        yaw = maxSpeed;
            //    }
            //}
            //else if (hor < 0)
            //{
            //    yaw -= maxAcc;

            //    if (yaw < -maxSpeed)
            //    {
            //        yaw = -maxSpeed;
            //    }
            //}
            //else
            //{
            //    if (yaw < 0)
            //    {
            //        yaw += maxAcc;

            //        if (yaw > 0)
            //        {
            //            yaw = 0;
            //        }
            //    }
            //    else if (yaw > 0)
            //    {
            //        yaw -= maxAcc;

            //        if (yaw < 0)
            //        {
            //            yaw = 0;
            //        }
            //    }
            //}


            if (isReplay & replayIdx < replayMaxIdx)
            {
                //print(string.Format("x {0}, yaw {1}, z {2}", replayX[replayIdx], replayYaw[replayIdx], replayZ[replayIdx]));

                deltaZ = replayZ[replayIdx];
                deltaX = replayX[replayIdx];
                deltaYaw = replayYaw[replayIdx];
                yawCopy = deltaYaw;

                zVel = deltaZ / Time.deltaTime;
                xVel = deltaX / Time.deltaTime;
                yawVel = deltaYaw / Time.deltaTime;
                //player.transform.position += new Vector3(replayX[replayIdx], p_height, replayZ[replayIdx]);
                //player.transform.rotation = Quaternion.Euler(0f, replayYaw[replayIdx], 0f);
                replayIdx++;
                
            }
            else if (keyboard)
            {
                pitch = Input.GetAxis("Vertical");

                //     1 / 0.2 second / 100 = 50 cm/s
                zVel = pitch / Time.deltaTime/100/2f;


                
                roll = Input.GetAxis("Horizontal");
                xVel = roll / Time.deltaTime/20/5;


                // 1 = 50 deg/s
                // roll / Time.deltaTime = 1/0.2 = 50 deg/s
                yawVel = roll / Time.deltaTime * 1.5f;

            }
            else
            {
                float t = Time.time;
                // if mc strat 60s after start
                if (t - initTime > 0.4f)
                {
                    string ball_input = ball.readQueue();
                    string[] line = ball_input.Split(',');
                    //print(ball_input);

                    pitch = float.Parse(line[0]);
                    roll = float.Parse(line[1]);
                    yaw = float.Parse(line[2]);
                    yawCopy = yaw;
                    // calibrate once a week
                    deltaZ = pitch * -0.0085384834f;
                    deltaX = roll * -0.0093862942f;
                    // ball/visual = 1
                    deltaYaw = yaw * -0.1713298528f*4;
                    // squeze 360 deg into 30
                    //deltaYaw = deltaYaw * 12;
              

                    if (motorYaw > 0)
                    {
                        
                        // yaw rotation is not probably detected, so disable yaw when runing at angle
                        if (Math.Abs(pitch) > 0.05)
                        {
                            deltaYaw = 0;
                        }
                        yawVel = deltaYaw / Time.deltaTime;
                    }




                    //deltaZ = pitch * -1 * Zscale;
                    //deltaX = roll * -1 * Xscale;
                    //deltaYaw = yaw * Yawscale;

                    zVel = deltaZ / Time.deltaTime * 3f * Zscale;
                    xVel = deltaX / Time.deltaTime * 2.5f * Xscale;

                    if (motorRoll > 0)
                    {
                        yawVel = 0;
                        //zVel = 0;
                        //xVel = 0;
                        // rotate when mice run at angle| tan45 = 1; tan60 = 1.73; tan75 = 3.73 (30 deg forward run area)
                        if (Math.Abs(zVel) / Math.Abs(xVel) < 2.73 & Math.Abs(xVel) > 0.015f)
                            //if (Math.Abs(roll) > 0.03f)
                        {
                            //yawVel = (float)Math.Sqrt(zVel * zVel + xVel * xVel) * 300;
                            // roll and pitch combination is not symmetric, so that running at angle might be different for various angles
                            if (xVel > 0)
                            {
                                // roll or deltaX is not symmetric, not sure why;
                                yawVel = xVel * 600;
                            } else
                            {
                                yawVel = xVel * 300;
                            }

                            
                            zVel = 0;
                            xVel = 0;
               
                        }
                    }
                    
               


                }
            }



        }
        catch (Exception e)
        {
            // It's gonna be the same exception everytime, but I'm purposely doing this.
            // It's just that this code will read serial in faster than it's actually
            // coming in so there'll be an error saying there's no object or something
            // like that.
        }
        await new WaitForUpdate();
    }

    //void OnEnable()
    //{
    //    Ball = this;

    //    _serialPort = new SerialPort();

    //    // Change com port
    //    _serialPort.PortName = "COM8";
    //    // Change baud rate
    //    _serialPort.BaudRate = 2000000;
    //    //_serialPort.ReadTimeout = 1;
    //    _serialPort.DtrEnable = true;
    //    _serialPort.RtsEnable = true;
    //    // Timeout after 0.5 seconds.
    //    _serialPort.ReadTimeout = 5;
    //    try
    //    {
    //        _serialPort.Open();
    //        _serialPort.DiscardInBuffer();
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError(e);
    //        IsConnected = false;
    //    }
    //}

    // Update is called once per frame

    //void FixedUpdate()
    // {
    //     //pitch = Input.GetAxis("Vertical") * -1;
    //     //roll = 0.0f;
    //     //yaw = 0.0f;
    //     try
    //     {
    //         float t = Time.time;
    //         if (t - initTime > 0.4f)
    //         {
    //             string[] line = _serialPort.ReadLine().Split(',');

    //             // float.Parse returns int sometimes eg 0.1 -> 1
    //             pitch = float.Parse(line[0]);
    //             roll = float.Parse(line[1]);
    //             yaw = float.Parse(line[2]);

    //             // this leads to a partial loss of msg eg. instead of 0.15 -> 15
    //             //_serialPort.DiscardInBuffer();

    //             //print(pitch);

    //         } else
    //         {
    //             _serialPort.DiscardInBuffer();
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         // It's gonna be the same exception everytime, but I'm purposely doing this.
    //         // It's just that this code will read serial in faster than it's actually
    //         // coming in so there'll be an error saying there's no object or something
    //         // like that.
    //     }

    //     //await new WaitForSeconds(0.0f);
    // }

    public async void MakeProfile(float delay, float duration, float sigma, float amplitude)
    {
        int size = Mathf.RoundToInt(duration / Time.fixedDeltaTime);
        float[] x = new float[size];

        for (int i = 0; i < size; i++)
        {
            x[i] = i * (duration / Time.fixedDeltaTime);
        }

        var a = 1.0f / (sigma * Mathf.Sqrt(2 * Mathf.PI));

        await new WaitForSeconds(delay);

        for (int i = 0; i < size; i++)
        {
            await new WaitForFixedUpdate();

            pitch += amplitude * a * Mathf.Exp(-Mathf.Pow(x[i], 2.0f) / (2.0f * Mathf.Pow(sigma, 2.0f)));
        }
    }

    public float BoxMullerGaussianSample()
    {
        float u1, u2, S;
        do
        {
            u1 = 2.0f * (float)rand.NextDouble() - 1.0f;
            u2 = 2.0f * (float)rand.NextDouble() - 1.0f;
            S = u1 * u1 + u2 * u2;
        }
        while (S >= 1.0f);
        return u1 * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
    }

    private void OnDisable()
    {
        //_serialPort.Close();
        ball.close();
    }

    private void OnApplicationQuit()
    {
        //if (_serialPort.IsOpen)
        //{
        //    _serialPort.Close();
        //}
        ball.close();
    }
}
