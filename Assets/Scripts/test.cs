using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.InputSystem;
using UnityEditor;

public class test : MonoBehaviour
{
    SerialPort sp;
    // Start is called before the first frame update
    void Start()
    {
        sp = new SerialPort("COM3", 9600);
        sp.Open();
        sp.ReadTimeout = 1;
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            Keyboard kb = Keyboard.current;
            if (kb.spaceKey.isPressed) GiveJuice();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

#if UNITY_EDITOR
        if (EditorApplication.isPlaying == false)
        {
            sp.Close();
        }
#endif
    }

    private async void GiveJuice()
    {
        sp.Write("j1000");
        await new WaitForSeconds(1);
        //Debug.Log(sp.ReadLine());
        //Debug.Log(sp.ReadLine());
    }


}
