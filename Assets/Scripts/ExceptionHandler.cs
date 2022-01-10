using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExceptionHandler : MonoBehaviour
{
    public Canvas canvas;
    public TMP_Text text;

    void Awake()
    {
        Application.logMessageReceived += HandleException;
        //DontDestroyOnLoad(gameObject);
    }

    void HandleException(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            text.text = logString + "\n" + stackTrace;
            canvas.enabled = true;
        }
    }
}
