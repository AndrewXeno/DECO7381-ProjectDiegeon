using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Serialization.JsonFx;
using UnityEngine.SceneManagement;


public class TestManager : MonoBehaviour {
    //public readonly string SERVERHOST = "https://6bit.uqcloud.net/";  //Constant indicating the game server host
    //public readonly string SERVERHOST = "http://localhost/";    //Constant indicating the game server host
    public readonly string SERVERHOST = "http://diegeon.azurewebsites.net/";    //Constant indicating the game server host
    public static TestManager instance = null;                  //Static instance of GameManager which allows it to be accessed by any other script.
    private BoardManager boardScript;                           //Store a reference to our BoardManager which will set up the level.
    private AccountManager accountScript;

    private ArrayList DungeonData;
    public int levelNumber = 1;
    public int roomNumber = 1;

    private GameObject clearPopup;
    private GameObject submitPopup;
    private Player playerScript;

    //Awake is always called before any Start functions
    void Awake() {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        clearPopup = GameObject.Find("ClearPopup");
        clearPopup.SetActive (false);
        submitPopup = GameObject.Find("SubmitPopup");
        submitPopup.SetActive (false);

        playerScript = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player> ();

        boardScript = BoardManager.instance.GetComponent<BoardManager> ();
        accountScript = AccountManager.instance.GetComponent<AccountManager> ();
        DungeonData = new ArrayList ();
        InitGame();

    }

    public void NextRoom() {
    }

    public void PreviousRoom() {
    }

    public void NextLevel() {
        levelNumber++;
    }

    public void PreviousLevel() {
        levelNumber--;
    }


    //Initializes the game.
    void InitGame() {
        boardScript.UpdateRoom ();
    }

    //Update is called every frame.
    void Update()
    {}

    public void ShowClearMenu() {
        Time.timeScale = 0;
        clearPopup.SetActive (true);
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

        if (www.error != null) {
            Debug.LogError("error:" + www.error);
            yield break;
        }
        Debug.Log(www.text);
        submitPopup.SetActive (true);
    }


    void LateUpdate () {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Time.timeScale = 1;
            SceneManager.LoadScene ("Scenes/BuildMode");
        }
    }

}