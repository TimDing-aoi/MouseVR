using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class AccelerometerController : MonoBehaviour
{

    wrmhl accel = new wrmhl();
    public string portName = "COM4";
    public int baudRate = 2000000;
    public int ReadTimeout = 5000;
    public int QueueLength = 1;

    public static AccelerometerController accelController;
    //SerialPort _serialPort;
    public float gyroX;
    public float gyroY;
    public float gyroZ;
    public float accX;
    public float accY;
    public float accZ;
    public string reading;
    public bool IsConnected = true;
    public string[] line;

    float dt = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        accelController = this;

        accel.set(portName, baudRate, ReadTimeout, QueueLength);
        accel.connect();
   
        //_serialPort = new SerialPort();
        // Change com port
        //_serialPort.PortName = "COM7";
        // Change baud rate
        //_serialPort.BaudRate = 115200;
        //_serialPort.ReadTimeout = 1;
        //_serialPort.DtrEnable = true;
        //_serialPort.RtsEnable = true;
        // Timeout after 0.5 seconds.
        //_serialPort.ReadTimeout = 500;
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
        // Debug.Log("accelController reading at   : " + Time.time);
        try
        {
            reading = accel.readQueue();

            // Debug.Log("accelController reading    : " + reading);

            //if (reading != null)
            //{
            //    line = reading.Split(',');

            //    accX = float.Parse(line[0]);
            //    accY = float.Parse(line[1]);
            //    accZ = float.Parse(line[2]);
            //    gyroX = float.Parse(line[3]);
            //    gyroY = float.Parse(line[4]);
            //    gyroZ = float.Parse(line[5]);
            //}
            //else
            //{
            //    accel.close();
            //    //accel = null;
            //    //accel = new wrmhl();
            //    accel.set(portName, baudRate, ReadTimeout, QueueLength);
            //    accel.connect();
                
            //}



        }
        catch (Exception e1)
        {

            //Debug.Log("data received: " + line[0] + "  " + line[1] + "  " + line[2] + "  " + line[3] + "  " + line[4] + "  " + line[5]);
            Debug.Log("exception in accelerometer------------------------");
            Debug.LogException(e1, this);
            // Debug.Log("accelerometer disconnected at " + Time.realtimeSinceStartup);


            //accel.close();
            //accel = new wrmhl();
            //accel.set(portName, baudRate, ReadTimeout, QueueLength);

            //try
            //{
            //    accel.connect();
            //    Debug.Log("reconnected " + Time.realtimeSinceStartup);
            //}
            //catch (Exception e2)
            //{
            //    Debug.LogException(e2, this);
            //    Debug.Log("reconnect failed " + Time.realtimeSinceStartup);
            //}

            //UnityEngine.Debug.LogError(e);
            //IsConnected = false;
        }
        await new WaitForUpdate();







    }

    private void OnDisable()
    {
        //_serialPort.Close();
        accel.close();
    }

    private void OnApplicationQuit()
    {
        //if (_serialPort.IsOpen)
        //{
        //    //_serialPort.Close();
        //}
        accel.close();
    }
}
