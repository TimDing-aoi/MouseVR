using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using static BallController;
using static LabJackController;
using static MotionCueingController;

[DisallowMultipleComponent]
public class MotorController : MonoBehaviour
{
    wrmhl motor = new wrmhl();
    public static MotorController motorController;

    //public SerialController serialController;

    float prevValue = 0;
    public float value = 0;
    public bool IsConnected = true;
    public float deltaYaw = 0.0f;
    public float yawVel = 0.0f;
    public float yawVelMax;

    public SerialPort _serialPort;
    public int ReadTimeout = 5000;
    public int QueueLength = 1;
    public string portName = "COM999";
    public int baudRate = 2000000;
    public List<Dictionary<string, object>> data;


    float roll = 0.0f;
    float yaw = 0.0f;
    int idx = 0;
    //bool flagReceived = true;
    // Start is called before the first frame update




    void Start()
    {
        motorController = this;
        // this is needed to send normalized values to yawmotor (-1, 1)
        // then set analog gain in kollmorgen as yawvelmax/1.9
        // I couldnt use -2.5 and 2.5 V because of hardware
        yawVelMax = 200.0f;


        //serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
        //_serialPort = new SerialPort();
        //motor.set(portName, baudRate, ReadTimeout, QueueLength);
        //motor.connect();

  
        //// Change com port
        //_serialPort.PortName = "COM10";
        //// Change baud rate
        //_serialPort.BaudRate = 2000000;
        //_serialPort.DtrEnable = true;
        //_serialPort.RtsEnable = true;
        //// Timeout after 0.5 seconds.
        //_serialPort.ReadTimeout = 2;
        //_serialPort.WriteTimeout = 2;

        //try
        //{
        //    _serialPort.Open();
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError(e);
        //    IsConnected = false;
        //}

    }

    // Update is called once per frame
    public async void Update()
    {
        if ((int)PlayerPrefs.GetFloat("Enable MC") == 1)
        {
            yawVel = (float)motionCueingController.motionCueing.filtered[2][2];
            //Debug.Log("using yawVel from MC ----------------------------------------");
            //yawVel = Ball.yawVel;
        }
        else
        {
            yawVel = Ball.yawVel;
        }


        // 1.9 = 0 V

        //print(yawVel);
        //yawVel = deltaYaw / Time.deltaTime;
        yawVel /= yawVelMax;




        //print(yaw);

        //if (yaw > 0)
        //{
        //    yaw += 0.01f;

        //    if (yaw > 0.99)
        //    {
        //        yaw = 0.99f;
        //    }
        //}
        //else if (yaw < 0)
        //{
        //    yaw -= 0.01f;

        //    if (yaw < -0.99)
        //    {
        //        yaw = -0.99f;
        //    }
        //}
        //else
        //{
        //    if (yaw < 0)
        //    {
        //        yaw += 0.01f;

        //        if (yaw > 0)
        //        {
        //            yaw = 0;
        //        }
        //    }
        //    else if (yaw > 0)
        //    {
        //        yaw -= 0.01f;

        //        if (yaw < 0)
        //        {
        //            yaw = 0;
        //        }
        //    }
        //}

 

        // -1, 1 min, max ang vel
        if (yawVel > 1)
        {
            yawVel = 1.0f;

        }
        else if (yawVel < -1)
        {
            yawVel = -1.0f;
        }

   

        value = yawVel * 2.5f + 2.5f;
        //serialController.SendSerialMessage(idx.ToString());
        // we don't need to send to arduino if it's the same value
        if (!(value == prevValue))
        {
       
            labJackController.ExecuteDACRequest(value);


            // print("motor yaw ----------------------- " + value);

            //Debug.Log(value);

            //serialController.SendSerialMessage(value.ToString());
            //_serialPort.Write(value.ToString() + '\n');

            // uses writeline 
            //motor.send(value.ToString());
            prevValue = value;
            //value = 150;
            //_serialPort.DiscardOutBuffer();
            //_serialPort.DiscardInBuffer();
        }




        //string message = serialController.ReadSerialMessage();

        //if (message == null)
        //    return;
        //else
        //    Debug.Log("Message arrived: " + message);

        idx++;

        await new WaitForUpdate();

    }

    //public void SetValue(float input)
    //{
    //    if (!(input.GetType() != typeof(float)))
    //    {
    //        Debug.LogWarning("MotorController: input type not int or float.");
    //        return;
    //    }

    //    var lerp = Mathf.RoundToInt(Mathf.Lerp(0f, 255f, Mathf.InverseLerp(0f, 90f, input)));

    //    if (value == lerp)
    //    {
    //        // we don't need to reset value if it's the same as before
    //        return;
    //    }

    //    if (lerp > 255)
    //    {
    //        value = 255;
    //    }
    //    else if (lerp < 0)
    //    {
    //        value = 0;
    //    }
    //    else
    //    {
    //        value = lerp;
    //    }
    //}

    //public void SetValue(int input)
    //{
    //    if (!(input.GetType() != typeof(int))) 
    //    {
    //        Debug.LogWarning("MotorController: input type not int or float.");
    //        return;
    //    }

    //    if (value == input)
    //    {
    //        // we don't need to reset value if it's the same as before
    //        return;
    //    }

    //    if (input > 255)
    //    {
    //        value = 255;
    //    }
    //    else if (input < 0)
    //    {
    //        value = 0;
    //    }
    //    else
    //    {
    //        value = input;
    //    }
    //}

    private void OnDisable()
    {
        labJackController.ExecuteDACRequest(2.5f);
        //motor.close();
        //if (_serialPort.IsOpen)
        //{
        //    _serialPort.Close();
        //}
    }

    private void OnApplicationQuit()
    {
        labJackController.ExecuteDACRequest(2.5f);
        //if (_serialPort.IsOpen)
        //{
        //    _serialPort.Close();
        //}
        //motor.close();
    }
}
