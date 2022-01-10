using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDebugMessage : MonoBehaviour
{
    TMPro.TMP_Text textBox;
    // Start is called before the first frame update
    void Start()
    {
        textBox = GetComponent<TMPro.TMP_Text>();
        Application.logMessageReceived += OnMessageReceived;
    }

    void OnMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (type != LogType.Log) return;

        textBox.text = "status:\n" + logString;
    }
}
