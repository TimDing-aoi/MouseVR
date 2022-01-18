using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static RewardArena;

public class trialcount : MonoBehaviour
{
    public TMP_Text message;
    public TMP_Text timemessage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        message.text = string.Format("Previous Run\nGood/Total: {0}/{1}", PlayerPrefs.GetInt("Good Trials"), PlayerPrefs.GetInt("Total Trials"));
        timemessage.text = string.Format("Minutes Elapsed: {0}", SharedReward.minutes_elapsed);
    }
}
