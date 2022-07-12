using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static RewardArena;
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
        //message.text = string.Format("Good/Total: {0}/{1}", PlayerPrefs.GetInt("Good Trials"), PlayerPrefs.GetInt("Total Trials"));
        //timemessage.text = string.Format("Minutes Elapsed: {0}", SharedReward.minutes_elapsed);
        ttlState.text = string.Format("TTL: {0}", PlayerPrefs.GetInt("Updates Counter"));
        //headDirectionState = string.Format("bbbbbbb: {0}", PlayerPrefs.GetInt("Updates Counter"));
        headDirectionState.text = string.Format("Head Direction: {0}", PlayerPrefs.GetInt("Updates Counter"));
    }
}
