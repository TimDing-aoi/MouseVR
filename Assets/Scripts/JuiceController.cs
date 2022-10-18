using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class JuiceController : MonoBehaviour
{
    public static JuiceController juiceController;

    public SerialPort _serialPort;

    private bool giveJuice = true;
    public bool IsConnected = true;
    private float juiceTime;
    bool keyboardmode;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        keyboardmode = (int)PlayerPrefs.GetFloat("IsKeyboard") == 1;
        juiceController = this;
        juiceTime = PlayerPrefs.GetInt("Juice Time");
        _serialPort = new SerialPort();

        // Change com port
        _serialPort.PortName = "COM6";
        // Change baud rate
        _serialPort.BaudRate = 115200;
        _serialPort.ReadTimeout = 1;
        _serialPort.DtrEnable = true;
        _serialPort.RtsEnable = true;
        // Timeout after 0.5 seconds.
        _serialPort.ReadTimeout = 500;
        if (!keyboardmode)
        {
            try
            {
                _serialPort.Open();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                IsConnected = false;
            }
        }
    }

    private void OnDisable()
    {
        _serialPort.Close();
    }

    private void OnApplicationQuit()
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Keyboard keyboard = Keyboard.current;


        if (keyboard.spaceKey.wasReleasedThisFrame && giveJuice)
        {
            giveJuice = false;
            GiveJuice(60);

        }
    }

    public async void GiveJuice(float time)
    {

        

        string toSend;

        if (time < 10.0f)
        {
            toSend = string.Format("j000{0}\n", time);
        }
        else if (time >= 10.0f && time < 100.0f)
        {
            toSend = string.Format("j00{0}\n", time);
        }
        else if (time >= 100.0f && time < 1000.0f)
        {
            toSend = string.Format("j0{0}\n", time);
        }
        else
        {
            toSend = string.Format("{0}\n", time);
        }

        if (!keyboardmode)
        {
            _serialPort.Write(toSend);
        }



        await new WaitForFixedUpdate();
        giveJuice = true;
    }
}
