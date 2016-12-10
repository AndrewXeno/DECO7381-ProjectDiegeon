using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Serialization.JsonFx;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour {
	public readonly string SERVERHOST = "https://6bit.uqcloud.net/"; 	//Constant indicating the game server host
//	public readonly string SERVERHOST = "http://localhost/"; 	//Constant indicating the game server host
	public static EditorManager instance = null;
	private BoardManager boardScript;
	private AccountManager accountScript;

	private int selectingType = -1;
	private int selectingObj = -1;
    private string mouseState;

    private int initx = -1;//record the x position when the mouse left button is clicked
    private int inity = -1;//record the y position when the mouse left button is clicked
    private int x = -1;//record the x position when the left button is released
    private int y = -1;//record the y position when the left button is released
    private int selectingIDx = -1;
    private bool freedraw = true;
    private bool full = true;
    void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		//DontDestroyOnLoad(gameObject);
		boardScript = BoardManager.instance.GetComponent<BoardManager> ();
		accountScript = AccountManager.instance.GetComponent<AccountManager> ();
		// if the room is already created, hide the size selection menu
		if (boardScript.isInitialised ()) {
			GameObject.Find("RoomSizeMenu").SetActive (false);
			boardScript.UpdateRoom();
			boardScript.initPreview();
		}
	}
    void InitEditor()
    {
        boardScript.UpdateRoom();
        boardScript.backupRoomData();
        boardScript.initPreview();
    }
    public void ConfirmRoomSize(){
		GameObject widthDropdown = GameObject.Find("WidthDropdown");
		GameObject heightDropdown = GameObject.Find("HeightDropdown");
		int width = 15;
		int height = 15;
		if (widthDropdown.GetComponent<Dropdown> ().value == 1)
			width = 25;
		if (widthDropdown.GetComponent<Dropdown> ().value == 2)
			width = 35;
		if (heightDropdown.GetComponent<Dropdown> ().value == 1)
			height = 25;
		if (heightDropdown.GetComponent<Dropdown> ().value == 2)
			height = 35;
		boardScript.columns = width;
		boardScript.rows = height;
        
        EditorManager.instance.GetComponent<EditorCameraController>().ResetCamera();
		GameObject.Find("RoomSizeMenu").SetActive (false);
        InitEditor();
	}

	public void setSelectingType(int typeIdx) {
		selectingType = typeIdx;
	}

	public void setSelectingObj(int objIdx) {
		selectingObj = objIdx;
	}

	public int getSelectingObj() {
		return selectingObj;
	}

	// Submit current room to the server
	public IEnumerator SubmitRoom() {
		string data = JsonWriter.Serialize(boardScript.getRoomData());
		string url = SERVERHOST + "game_server/submit_room.php";
		WWWForm form = new WWWForm();
		form.AddField("username", accountScript.getUsername());
		int difficulty = boardScript.getBaseDifficulty ();
		form.AddField("difficulty", difficulty.ToString());
		form.AddField("data", data);
		Debug.Log(form);

		WWW www = new WWW(url, form);
		Debug.Log("Submitting room data: " + data);
		yield return www;

		if (www.error != null){
			Debug.LogError("error:" + www.error);
			yield break;
		}

		Debug.Log(www.text);
	}
    
    void LateUpdate()
    {
        Vector3 mousePositionOnScreen = Input.mousePosition;
        Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);
        boardScript.clearTransformChild("cursor");
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            full = !full;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))//switch from build to select
        {
            selectingType = -1;
            selectingObj = -1;
        }
        else if (Input.GetKeyDown(KeyCode.F))//free draw or rect draw
        {
            freedraw = !freedraw;
        }

        if (selectingIDx != -1)
        {
            if (Input.GetKeyDown(KeyCode.Delete))//delete the building block
            {
                {
                    boardScript.DeleteBoardObject(selectingIDx);
                    boardScript.UpdateRoom();
                    boardScript.backupRoomData();
                }
            }
        }

		if (mouseState != "building" && selectingObj != -1 && selectingType != -1)
		{
			boardScript.clearTransformChild("indicator");
		}


        if (mouseState != "building" && selectingObj != -1 && selectingType != -1 &&
            mousePositionInWorld.x >= -0.5f && mousePositionInWorld.x < boardScript.columns - 0.5f &&
            mousePositionInWorld.y >= -0.5f && mousePositionInWorld.y < boardScript.rows - 0.5f)
        {
            boardScript.ConstructAt(selectingType, selectingObj, mousePositionInWorld.x, mousePositionInWorld.y);
            //selecting object follows the cursor
        }

        if (Input.GetMouseButton(0) && //when left button is pressed down
            mousePositionInWorld.x >= -0.5f && mousePositionInWorld.x < boardScript.columns - 0.5f &&
            mousePositionInWorld.y >= -0.5f && mousePositionInWorld.y < boardScript.rows - 0.5f)
        {
            x = (int)(mousePositionInWorld.x + 0.5f);
            y = (int)(mousePositionInWorld.y + 0.5f);
            if (!freedraw)
            {
                boardScript.clearTransformChild("preview");
            }
            if (initx == -1 && inity == -1) //update initx and inity to be the x, y the moment when button is pressed
            {
                initx = x;
                inity = y;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (selectingObj == -1 && selectingType == -1)//enter selecting mode
            {
                boardScript.showIndicator(initx, inity, x, y);
                mouseState = "selecting";
                selectingIDx = boardScript.FindBoardObject(x, y);
                if (Input.GetMouseButton(1) &&
            mousePositionInWorld.x >= -0.5f && mousePositionInWorld.x < boardScript.columns - 0.5f &&
            mousePositionInWorld.y >= -0.5f && mousePositionInWorld.y < boardScript.rows - 0.5f)
                {
                    mouseState = "canceled";
                    boardScript.clearTransformChild("indicator");
                    initx = -1;
                    inity = -1;
                }
            }
            else
            {
                mouseState = "building";
                //boardScript.clearTransformChild("indicator");
                if (freedraw)
                {
                    if (selectingType == 0)
                    {
                        // used for object selection
                    }
                    else if (selectingType == 1)
                    {
                        boardScript.SetPreviewFloorAt(x, y, selectingObj);
                    }
                    else if (selectingType == 2)
                    {
                        boardScript.SetPreviewWallAt(x, y, selectingObj);
                    }
                    else if (selectingType == 3)
                    {
                        boardScript.SetPreviewPitAt(x, y);
                    }
                    else if (selectingType == 4)
                    {
                        boardScript.SetPreviewLowerObjectAt(x, y, selectingObj);
                    }
                    else if (selectingType == 5)
                    {
                        boardScript.SetPreviewUpperObjectAt(x, y, selectingObj);
                    }
                }
                else
                {
                    boardScript.DrawRect(full, selectingType, selectingObj, initx, inity, x, y);
                }
            }

            if (Input.GetMouseButton(1) && //when right button is pressed before left button is released
            mousePositionInWorld.x >= -0.5f && mousePositionInWorld.x < boardScript.columns - 0.5f &&
            mousePositionInWorld.y >= -0.5f && mousePositionInWorld.y < boardScript.rows - 0.5f)
            {
                mouseState = "canceled";
                boardScript.clearTransformChild("preview");
                initx = -1;
                inity = -1;
            }

        }
        else if (Input.GetMouseButtonUp(0) && //when left button is released
         mousePositionInWorld.x >= -0.5f && mousePositionInWorld.x < boardScript.columns - 0.5f &&
         mousePositionInWorld.y >= -0.5f && mousePositionInWorld.y < boardScript.rows - 0.5f)
        {
            if (mouseState == "building")
            {
                mouseState = "builded";

                boardScript.SynchronizeBoard();
                boardScript.UpdateRoom();
                boardScript.backupRoomData();
                boardScript.clearTransformChild("preview");

            }
            else if (mouseState == "selecting")
            {
                mouseState = "selected";
            }
            initx = -1;
            inity = -1;
        }

    }

    public void ToggleFree()
    {
        freedraw = !freedraw;
    }

    public bool getFreeDraw()
    {
        return freedraw;
    }

}
