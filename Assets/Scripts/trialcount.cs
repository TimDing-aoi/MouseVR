using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static RewardArena;

public class trialcount : MonoBehaviour
{
    public TMP_Text message;
    public TMP_Text timemessage;
    public TMP_Text zpos;
    public TMP_Text xpos;
    public TMP_Text rotpos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        message.text = string.Format("Good/Total: {0}/{1}", PlayerPrefs.GetInt("Good Trials"), PlayerPrefs.GetInt("Total Trials"));
        timemessage.text = string.Format("Minutes Elapsed: {0}", SharedReward.minutes_elapsed);
        zpos.text = string.Format("Z Pos: {0}", SharedReward.playerZPosition);
        xpos.text = string.Format("X Pos: {0}", SharedReward.playerXPosition);
        rotpos.text = string.Format("Rot (deg): {0}", SharedReward.playerRotatedPosition);
    }
}
