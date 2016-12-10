using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class EditorUIEvents : MonoBehaviour {

    private BoardManager boardScript;
    private EditorManager editorScript;
    private Button button;

    public Sprite free;
    public Sprite notFree;

    public void SetSelectingPit() {
		EditorManager.instance.setSelectingType (3);
		EditorManager.instance.setSelectingObj (0);
	}

	public void SetSelectingFloor(int objIdx) {
		EditorManager.instance.setSelectingType (1);
		EditorManager.instance.setSelectingObj (objIdx);
	}

	public void SetSelectingWall(int objIdx) {
		EditorManager.instance.setSelectingType (2);
		EditorManager.instance.setSelectingObj (objIdx);
	}

	public void SetSelectingLowerObject(int objIdx) {
		EditorManager.instance.setSelectingType (4);
		EditorManager.instance.setSelectingObj (objIdx);
	}

	public void SetSelectingUpperObject(int objIdx) {
		EditorManager.instance.setSelectingType (5);
		EditorManager.instance.setSelectingObj (objIdx);
	}

	public void TestPlay() {
		SceneManager.LoadScene ("Scenes/TestMode");
	}

	public void SubmitRoom(){
		EditorManager.instance.StartCoroutine(EditorManager.instance.SubmitRoom ());
	}

	public void ExitEditor() {
		SceneManager.LoadScene ("Scenes/MainMenu");
	}

	public void ConfirmRoomSize(){
		EditorManager.instance.ConfirmRoomSize ();
	}
    public void CleanCanvas()
    {
        BoardManager.instance.ResetBoard();
    }

    public void GoBackRoomData()
    {
        boardScript = BoardManager.instance.GetComponent<BoardManager>();
        boardScript.GoBackRoomData();
    }

    public void GoForwardRoomData()
    {
        boardScript = BoardManager.instance.GetComponent<BoardManager>();
        boardScript.GoForwardRoomData();
    }
    public void ToggleDrawMode()
    {
        
        editorScript = EditorManager.instance.GetComponent<EditorManager>();        
        editorScript.ToggleFree();

        //button = GetComponent<Button>();
        //button.image.overrideSprite = editorScript.getFreeDraw() ? free : notFree;
    }
}
