using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SensorTracker : MonoBehaviour
{
    public TMP_Text ttlState;
    public TMP_Text headDirectionState;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        ttlState.text = string.Format("TTL: {0}", PlayerPrefs.GetString("TTL State"));
        headDirectionState.text = string.Format("Head Direction: {0}", PlayerPrefs.GetString("Ring State"));

        

    }
}
