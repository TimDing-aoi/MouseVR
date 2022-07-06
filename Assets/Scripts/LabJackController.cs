using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LabJack.LabJackUD;

public class LabJackController : MonoBehaviour
{
    public static LabJackController labJackController;

    U3 u3;

    LJUD.IO ioType = LJUD.IO.GET_AIN;
    LJUD.CHANNEL channel = 0;
    double dblVal = 0;
    readonly double ValueDIPort = 0;
    int intVal = 0;
    double val = 0;
    public readonly double[] ValueAIN = new double[8];


    readonly long time = 0;
    readonly long numIterations = 1;
    readonly int numChannels = 8;  //Number of AIN channels, 0-16.
    readonly long quickSample = 0;  //Set to TRUE for quick AIN sampling. See section 2.6 / 3.1 of the User's Guide
    readonly long longSettling = 1;  //Set to TRUE for extra AIN settling time.

    // Start is called before the first frame update
    void Start()
    {
        labJackController = this;

        try
        {
            //Open the first found LabJack.
            u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB

            //Start by using the pin_configuration_reset IOType so that all
            //pin assignments are in the factory default condition.
            LJUD.ePut(u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);

            //Configure quickSample.
            LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.AIN_RESOLUTION, quickSample, 0);

            //Configure longSettling.
            LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.AIN_SETTLING_TIME, longSettling, 0);

            //Configure the necessary lines as analog.
            LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_PORT, 0, Mathf.Pow(2, numChannels) - 1, numChannels);

            //Set the timer/counter pin offset to 8, which will put the first
            //timer/counter on EIO0.
            LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_COUNTER_PIN_OFFSET, 8, 0, 0);

            ////Add analog input requests.
            //for (int j = 0; j < numChannels; j++)
            //{
            //    LJUD.AddRequest(u3.ljhandle, LJUD.IO.GET_AIN, j, 0, 0, 0);
            //}

            //Set DAC0 to 2.5 volts.
            LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_DAC, 0, 2.5, 0, 0);

            LJUD.GoOne(u3.ljhandle);
        }
        catch (LabJackUDException e)
        {
            Debug.LogError(e.ToString());
        }
    }

    // Update is called once per frame
    //async void Update()
    //{
        //try
        //{
        //    for (int i = 0; i < numChannels - 4; i++)
        //    {
        //        LJUD.eAIN(u3.ljhandle, i, 31, ref val, -1, -1, -1, 0);
        //        ValueAIN[i] = val;
        //        //Debug.Log(string.Format("Ch{0}: {1}", i, ValueAIN[i]));
        //    }
        //}
        //catch (LabJackUDException e)
        //{
        //    //Debug.LogError(e.ToString());
        //}

        //await new WaitForSeconds(0.0f);
    //}

    public async void ExecuteDACRequest(double volts)
    {
        LJUD.eDAC(u3.ljhandle, 0, volts, 0, 0, 0);

        await new WaitForSeconds(0.0f);
    }
}
