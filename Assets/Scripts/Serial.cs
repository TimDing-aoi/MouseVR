using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Serial : MonoBehaviour
{
    SerialPort sp;
    bool juice = true;
    // Start is called before the first frame update
    void Start()
    {
        sp = new SerialPort("COM3", 115200);
        sp.Open();
        sp.ReadTimeout = 1;
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.spaceKey.isPressed && juice) GiveJuice();
    }

    async void GiveJuice()
    {
        juice = false;
        sp.Write("j100");
        await new WaitForSeconds(0.1f);
        juice = true;
    }
}
