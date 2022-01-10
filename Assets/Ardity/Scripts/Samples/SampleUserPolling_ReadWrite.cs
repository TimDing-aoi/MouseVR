/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using System.Collections;

/**
 * Sample for reading using polling by yourself, and writing too.
 */
public class SampleUserPolling_ReadWrite : MonoBehaviour
{
    public SerialController serialController;

    int idx = 0;
    int prevValue = -1;
    public int value = 0;
    bool flagReceived = true;
    float roll = 0.0f;
    // Initialization
    void Start()
    {
        Application.targetFrameRate = 60;
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();

        Debug.Log("Press A or Z to execute some actions");
    }

    // Executed each frame
    void Update()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------

        // If you press one of these keys send it to the serial device. A
        // sample serial device that accepts this input is given in the README.
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    Debug.Log("Sending A");
        //    serialController.SendSerialMessage("A");
        //}

        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    Debug.Log("Sending Z");
        //    serialController.SendSerialMessage("Z");
        //}
        roll = Input.GetAxis("Horizontal");
        if (roll > 0.1)
        {
            roll = 1;

        } else if (roll < -0.1)
        {
            roll = -1;
        }
        value = (int)roll * 50 + 150;

        // we don't need to send to arduino if it's the same value
        if (!(value == prevValue))
        {
            //Debug.Log(value);

            serialController.SendSerialMessage(value.ToString());
            //_serialPort.Write(value.ToString() + '\n');

            // uses writeline 
            //motor.send(value.ToString());
            prevValue = value;
            value = 150;
            //_serialPort.DiscardOutBuffer();
            //_serialPort.DiscardInBuffer();
        }

        //if (idx > 255)
        //{
        //    idx = 0;
        //}
        //serialController.SendSerialMessage(idx.ToString());
        //if (flagReceived)
        //{
            
        //    flagReceived = false;
        //}
        ////---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------

        //string message = serialController.ReadSerialMessage();

        //if (message == null)
        //    return;

        //// Check if the message is plain data or a connect/disconnect event.
        //if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
        //    Debug.Log("Connection established");
        //else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
        //    Debug.Log("Connection attempt failed or disconnection detected");
        //else
        //    Debug.Log("Message arrived: " + message);
        //    flagReceived = true;
        
        idx++;
    }
}
