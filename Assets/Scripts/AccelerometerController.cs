using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class AccelerometerController : MonoBehaviour
{

    wrmhl accel = new wrmhl();
    public string portName = "COM7";
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
    float dt = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        accelController = this;

        accel.set(portName, baudRate, ReadTimeout, QueueLength);
        //accel.connect();

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
   
        try
        {
            reading = accel.readQueue();

            string[] line = reading.Split(',');

            accX = float.Parse(line[0]);
            accY = float.Parse(line[1]);
            accZ = float.Parse(line[2]);
            gyroX = float.Parse(line[3]);
            gyroY = float.Parse(line[4]);
            gyroZ = float.Parse(line[5]);

            //Debug.Log(dt - float.Parse(line[6]));
            //Debug.Log(Time.deltaTime);
            //dt = float.Parse(line[6]);
            //print(accX);

        }
        catch (Exception e)
        {
            //UnityEngine.Debug.LogError(e);
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
