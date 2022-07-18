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

    public int updatesCounter = 0;

    bool keyboard;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetString("Ring State", "Connected");
        keyboard = (int)PlayerPrefs.GetFloat("IsKeyboard") == 1;

        ringSensor = this;

        ring.set(portName, baudRate, ReadTimeout, QueueLength);

        

        //if (!keyboard)
        //{
            ring.connect();
        //}
    }

    // Update is called once per frame
    public async void Update()
    {

        updatesCounter++;

        if (updatesCounter % 100 == 0)
        {
            try
            {
                reading = ring.readQueue();

                //string[] line = reading.Split(',');

                dir = float.Parse(reading);

                PlayerPrefs.SetString("Ring State", "Connected");
                //Debug.Log(dt - float.Parse(line[6]));
                //Debug.Log(Time.deltaTime);
                //dt = float.Parse(line[6]);
                //print(accX);

            }
            catch (Exception e)
            {
                //UnityEngine.Debug.LogError(e);
                PlayerPrefs.SetString("Ring State", "Disonnected");
            }
        }



        await new WaitForUpdate();

    }

    private void OnDisable()
    {
        ring.close();
    }

    private void OnApplicationQuit()
    {
        ring.close();
    }
}
