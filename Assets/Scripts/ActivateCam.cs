using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCam : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 50;

        for (int i = 0; i < Display.displays.Length; i++)
        {
            if (i != 1) Display.displays[i].Activate();

        }
    }
}

    // Update is called once per frame
    //void Update()
    //{
        
  