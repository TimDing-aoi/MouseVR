using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using System.Collections;

public enum ManyMouseEventType
{
    MANYMOUSE_EVENT_ABSMOTION = 0,
    MANYMOUSE_EVENT_RELMOTION,
    MANYMOUSE_EVENT_BUTTON,
    MANYMOUSE_EVENT_SCROLL,
    MANYMOUSE_EVENT_DISCONNECT,
    MANYMOUSE_EVENT_MAX
}

public struct ManyMouseEvent
{
    public ManyMouseEventType type;
    public uint device;
    public uint item;
    public int value;
    public int minval;
    public int maxval;
} 

 [Serializable]
 public class ManyMouse
 {
    //todo: Events. Disconnect. Scroll. Move

     public delegate void ManyMouseConnectionEvent(ManyMouse mouse);
     public delegate void ManyMouseButtonEvent(ManyMouse mouse, int buttonID);
     public delegate void ManyMouseAnyButtonEvent(ManyMouse mouse, int buttonID, bool down);
     public delegate void ManyMouseVectorEvent(ManyMouse mouse, Vector2 value);
     public delegate void ManyMouseWheelEvent(ManyMouse mouse, int delta);

    public ManyMouseButtonEvent EventButtonDown = delegate { };
    public ManyMouseButtonEvent EventButtonUp = delegate { };
     public ManyMouseAnyButtonEvent EventButtonChange = delegate { };
     public ManyMouseVectorEvent EventMouseReposition = delegate { };

     public ManyMouseVectorEvent EventMouseDelta = delegate { };
     public ManyMouseWheelEvent EventWheelScroll = delegate { };
     public ManyMouseConnectionEvent EventMouseDisconnected = delegate { };

    public int id { get; private set; }
    public string DeviceName { get; private set; }
   
    public Vector2 Delta;
    public Vector2 Pos;//accumulated since last update
   
    //ManyMouse driver only (only!) supports 5 mouse buttons
    private const int NUM_MOUSE_BUTTONS = 5;
    public bool[] MouseButtons { get; private set; }
    public bool MouseButton1 { get { return MouseButtons[0]; } }
    public bool MouseButton2 { get { return MouseButtons[1]; } }
    public bool MouseButton3 { get { return MouseButtons[2]; } }
    public bool MouseButton4 { get { return MouseButtons[3]; } }
    public bool MouseButton5 { get { return MouseButtons[4]; } }

    //Positive edges this frame
    public bool MouseButton1Down { get { return MouseButtons[0] && !_lastMouseButtons[0]; } }
    public bool MouseButton2Down { get { return MouseButtons[1] && !_lastMouseButtons[1]; } }
    public bool MouseButton3Down { get { return MouseButtons[2] && !_lastMouseButtons[2]; } }
    public bool MouseButton4Down { get { return MouseButtons[3] && !_lastMouseButtons[3]; } }
    public bool MouseButton5Down { get { return MouseButtons[4] && !_lastMouseButtons[4]; } }

    //Negative edges this frame
    public bool MouseButton1Up { get { return !MouseButtons[0] && _lastMouseButtons[0]; } }
    public bool MouseButton2Up { get { return !MouseButtons[1] && _lastMouseButtons[1]; } }
    public bool MouseButton3Up { get { return !MouseButtons[2] && _lastMouseButtons[2]; } }
    public bool MouseButton4Up { get { return !MouseButtons[3] && _lastMouseButtons[3]; } }
    public bool MouseButton5Up { get { return !MouseButtons[4] && _lastMouseButtons[4]; } }

    //Currently no support for horizontal wheel movements
    public int MouseWheel { get; private set; }//this could cancel out without someone knowing if the wheel went up then down between polling. But the event should still fire for both.

    private bool[] _lastMouseButtons;
    private int _lastScrollWheel;

    public ManyMouse(int id)
    {
        this.id = id;
        MouseButtons = new bool[NUM_MOUSE_BUTTONS];
        _lastMouseButtons = new bool[NUM_MOUSE_BUTTONS];
        DeviceName = ManyMouseWrapper.MouseDeviceName(id);
    }

    internal void PollingReset()
    {
        Pos += Delta;
        Delta = Vector2.zero;
        MouseWheel = 0;

        for (int i = 0; i < NUM_MOUSE_BUTTONS; i++)
        {
            _lastMouseButtons[i] = MouseButtons[i];
        }

    }

    private Vector2 collateVector = Vector2.zero;
    internal void ProcessEvent(ManyMouseEvent mouseEvent)
    {
        //I do not trigger events in here so that all changes since last polling are fully cached.
        switch (mouseEvent.type)
        {
            case ManyMouseEventType.MANYMOUSE_EVENT_ABSMOTION:
                //Absolute motion, set at the beginning maybe?
                Pos.x += mouseEvent.item == 0 ? mouseEvent.value : 0;
                Pos.y += mouseEvent.item == 1 ? mouseEvent.value : 0;

                if(mouseEvent.item == 0)
                {
                    collateVector.x = mouseEvent.value;
                }
                if (mouseEvent.item == 1)
                {
                    collateVector.y = mouseEvent.value;
                }
                
                break;
            case ManyMouseEventType.MANYMOUSE_EVENT_RELMOTION:
                //Relative motion: Movement since last polling
                Delta.x += mouseEvent.item == 0 ? mouseEvent.value : 0;
                Delta.y += mouseEvent.item == 1 ? mouseEvent.value : 0;
                if (mouseEvent.item == 0)
                {
                    collateVector.x = mouseEvent.value;
                }
                if (mouseEvent.item == 1)
                {
                    collateVector.y = mouseEvent.value;
                }
                break;
            case ManyMouseEventType.MANYMOUSE_EVENT_BUTTON:
                MouseButtons[mouseEvent.item] = mouseEvent.value == 1 ;
                EventButtonChange(this, (int) mouseEvent.item, mouseEvent.value == 1);

                if (mouseEvent.value == 1)
                {
                    EventButtonDown(this, (int)mouseEvent.item);
                }
                else
                {
                    EventButtonUp(this, (int)mouseEvent.item);
                }


                break;
            case ManyMouseEventType.MANYMOUSE_EVENT_SCROLL:
               
                MouseWheel = mouseEvent.value;

                EventWheelScroll(this, mouseEvent.value);

                break;
            case ManyMouseEventType.MANYMOUSE_EVENT_DISCONNECT:

                EventMouseDisconnected(this);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (mouseEvent.type == ManyMouseEventType.MANYMOUSE_EVENT_ABSMOTION && mouseEvent.item == 1)
        {
            EventMouseReposition(this, new Vector2());
        }

        if (mouseEvent.type == ManyMouseEventType.MANYMOUSE_EVENT_RELMOTION && mouseEvent.item == 1)
        {
            EventMouseDelta(this, new Vector2());
        }
    }



   

 }