using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class RingSensor : MonoBehaviour
{

    wrmhl ring = new wrmhl();
    public string portName = "COM13";
    public int baudRate = 2000000;
    public int ReadTimeout = 5000;
    public int QueueLength = 1;

    public static RingSensor ringSensor;
    //SerialPort _serialPort;
    public float dir;

    public string reading;
    public bool IsConnected = true;
    float dt = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        ringSensor = this;

        ring.set(portName, baudRate, ReadTimeout, QueueLength);
        ring.connect();
    }

    // Update is called once per frame
    public async void Update()
    {
   
        try
        {
            reading = ring.readQueue();

            //string[] line = reading.Split(',');

            dir = float.Parse(reading);
        

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
        ring.close();
    }

    private void OnApplicationQuit()
    {
        //if (_serialPort.IsOpen)
        //{
        //    //_serialPort.Close();
        //}
        ring.close();
    }
}
