using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


[DisallowMultipleComponent]
public class LickDetector : MonoBehaviour
{
    //beam break
    wrmhl bb = new wrmhl();
    public static LickDetector lickDetector;
  
    public int TTL;
    public bool IsConnected = true;

    public int ReadTimeout = 5000;
    public int QueueLength = 1;
    public string portName = "COM7";
    public int baudRate = 2000000;

    // Start is called before the first frame update
    void Start()
    {
        lickDetector = this;

        bb.set(portName, baudRate, ReadTimeout, QueueLength);
        bb.connect();

    }

    // Update is called once per frame
    public async void Update()
    {
        
        try
        {
            TTL = int.Parse(bb.readQueue());
            //print(TTL);
        }
        catch (Exception e)
        {
            //Debug.LogWarning(e);
        }
        await new WaitForUpdate();

    }

    private void OnDisable()
    {
        bb.close();
    }

    private void OnApplicationQuit()
    {
        bb.close();
    }
}
