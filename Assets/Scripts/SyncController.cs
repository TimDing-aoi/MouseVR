using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


[DisallowMultipleComponent]
public class SyncController : MonoBehaviour
{
    wrmhl sync = new wrmhl();
    public static SyncController syncController;
  
    public int TTL;
    public bool IsConnected = true;

    public int ReadTimeout = 5000;
    public int QueueLength = 1;
    public string portName = "COM3";
    public int baudRate = 2000000;

    bool keyboard;

    // Start is called before the first frame update
    void Start()
    {
        keyboard = (int)PlayerPrefs.GetFloat("IsKeyboard") == 1;

        syncController = this;

        sync.set(portName, baudRate, ReadTimeout, QueueLength);
        if (!keyboard)
        {
            sync.connect();
        }

    }

    // Update is called once per frame
    public async void Update()
    {
        
        try
        {
            TTL = int.Parse(sync.readQueue());
            //Debug.Log(TTL);
        }
        catch (Exception e)
        {
            //Debug.LogWarning(e);
        }
        await new WaitForUpdate();

    }

    private void OnDisable()
    {
        sync.close();
    }

    private void OnApplicationQuit()
    {
        sync.close();
    }
}
