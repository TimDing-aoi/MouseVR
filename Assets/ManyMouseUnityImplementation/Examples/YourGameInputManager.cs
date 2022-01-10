using System;
using UnityEngine;
using System.Collections;

//This class spawns characters when you click the mouse. It also hands disconnects/reconnects.
public class YourGameInputManager : MonoBehaviour {

	public MouseControlledBox YourCharacterPrefab;

	private ManyMouse[] _manyMouseMice;
    private int numSpawnedBoxes = 0;
	void Start()
	{
	    int numMice = ManyMouseWrapper.MouseCount;
        if (numMice > 0)
        {
            _manyMouseMice = new ManyMouse[numMice];
            for (int i = 0; i < numMice; i++)
            {
                _manyMouseMice[i] = ManyMouseWrapper.GetMouseByID(i);
                _manyMouseMice[i].EventButtonDown += EventButtonDownJoinGame;
                //you'll want to pay attention to things disconnecting.
                _manyMouseMice[i].EventMouseDisconnected += EventMouseDisconnected;
            }
        }


	}

    
    private void EventButtonDownJoinGame(ManyMouse mouse, int buttonId)
    {
        if (buttonId == 0)
        {
            mouse.EventButtonDown -= EventButtonDownJoinGame;
            
            SpawnACharacterControlledByMouseID((int) mouse.id);


            mouse.EventButtonDown += EventButtonDownLeaveGame;//listen for another button down
        }

    }
    private void EventButtonDownLeaveGame(ManyMouse mouse, int buttonId)
    {
         if (buttonId == 1)
         {
             mouse.EventButtonDown -= EventButtonDownLeaveGame;//listen for another button down

          
             mouse.EventButtonDown += EventButtonDownJoinGame;

             numSpawnedBoxes--;
             if(numSpawnedBoxes == 0)
             {
                 Screen.lockCursor = false;
                 Cursor.visible = true;
             }

         }
     }

    private void SpawnACharacterControlledByMouseID(int mouseID)
	{
	    MouseControlledBox newGuy =
	        (Instantiate(YourCharacterPrefab.gameObject) as GameObject).GetComponent<MouseControlledBox>();
        newGuy.Init(mouseID);//pass the mouse id that this character should listen to.
        numSpawnedBoxes++;
        Screen.lockCursor = true;
        Cursor.visible = false;

	}

    private void EventMouseDisconnected(ManyMouse mouse)
    {
        //keep details of disconnected mouse... then keep looking for it with an init?
        Debug.LogWarning("Mouse Disconnected: " + mouse.DeviceName);
    }

}
