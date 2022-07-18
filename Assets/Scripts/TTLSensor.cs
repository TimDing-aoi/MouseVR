using UnityEngine;
using System;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using static RewardArena;


public class TTLSensor : MonoBehaviour
{

    wrmhl ttl = new wrmhl();
    public string portName = "COM3";
    public int baudRate = 115200;
    public int ReadTimeout = 5000;
    public static TTLSensor ttlSensor;
    public int QueueLength = 1;

    public string reading;

    public int ttlValue = -100;

    // Start is called before the first frame update
    void Start()
    {
        ttlSensor = this;
        ttl.set(portName, baudRate, ReadTimeout, QueueLength);
        ttl.connect();

        PlayerPrefs.SetString("TTL Value", "aaaaaaa");

    }

    // Update is called once per frame
    public async void Update()
    {

        try
        {
            reading = ttl.readQueue();
            ttlValue = int.Parse(reading);

        }
        catch (Exception e)
        {
            //UnityEngine.Debug.LogError(e);
        }
        await new WaitForUpdate();

    }

    private void OnDisable()
    {
        ttl.close();
    }

    private void OnApplicationQuit()
    {
        ttl.close();
    }

}
