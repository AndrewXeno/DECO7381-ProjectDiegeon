using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Serialization.JsonFx;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public readonly string SERVERHOST = "https://6bit.uqcloud.net/"; 	//Constant indicating the game server host
	//public readonly string SERVERHOST = "http://localhost/"; 	//Constant indicating the game server host
	public static GameManager instance = null;              	//Static instance of GameManager which allows it to be accessed by any other script.
	private BoardManager boardScript;                       	//Store a reference to our BoardManager which will set up the level.

	private ArrayList DungeonData;
	public int levelNumber = 1;
	public int roomNumber = 0;

    public GameObject _dicePool;
    private DiceMenu _diceMenu;

	private GameObject pauseMenu;
	public bool isPaused = false;

    private GameObject gameoverMenu;
    public bool gameovered = false;

	private GameObject loadingScreen;

    private Text levelText;
	private Text roomText;

    public int rollCount = 1;



    //Awake is always called before any Start functions
    void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		//DontDestroyOnLoad(gameObject);
		boardScript = BoardManager.instance.GetComponent<BoardManager> ();
		DungeonData = new ArrayList ();

		pauseMenu = GameObject.Find ("PauseMenu");
		pauseMenu.SetActive (false);
        gameoverMenu = GameObject.Find("GameoverMenu");
        gameoverMenu.SetActive(false);
		loadingScreen = GameObject.Find("LoadingScreen");
		loadingScreen.SetActive(false);
        levelText = GameObject.Find ("LevelText").GetComponent<Text>();
		roomText = GameObject.Find ("RoomText").GetComponent<Text>();

        //InitGame();
        _dicePool = Camera.main.transform.FindChild("DicePool").gameObject;
        _diceMenu = Camera.main.transform.Find("DiceMenu").GetComponent<DiceMenu>();

		updateRoomText ();
		StartCoroutine(LoadLevel(levelNumber));

	}

	public void ResetRoom(){
		if (!isLevelEndRoom ()) {
			boardScript.UpdateRoom ();
		}
	}

	public void NextRoom(){
		roomNumber++;
		if (roomNumber >= 0 && roomNumber <= ((string[])DungeonData [levelNumber - 1]).Length) {
			boardScript.SetRoomData (((string[])DungeonData [levelNumber - 1])[roomNumber - 1]);
			boardScript.LoadNextRoom ();
		}
		if (roomNumber > ((string[])DungeonData [levelNumber - 1]).Length) {
			boardScript.SetupLevelEndRoom ();
			boardScript.UpdateRoom ();
		}
		updateRoomText ();
	}

	public bool isLevelEndRoom(){
		return (roomNumber > ((string[])DungeonData [levelNumber - 1]).Length);
	}

	public void PreviousRoom(){
		roomNumber--;
		if (roomNumber > 0 && roomNumber <= ((string[])DungeonData [levelNumber - 1]).Length) {
			boardScript.SetRoomData (((string[])DungeonData [levelNumber - 1]) [roomNumber - 1]);
			boardScript.LoadPreviousRoom ();
		}
		updateRoomText ();
	}

	public void NextLevel(){
		levelNumber++;

		StartCoroutine(LoadLevel(levelNumber));
	}

	public void PreviousLevel(){
		levelNumber--;
	}

	public void RandomRoom() {
		boardScript.SetupRandomRoom();
		string data = JsonWriter.Serialize(boardScript.getRoomData());
		Debug.Log ("Room created. Room data: " + data);
	}

	// Submit current room to the server
	public IEnumerator SubmitRoom() {
		string data = JsonWriter.Serialize(boardScript.getRoomData());
		string url = SERVERHOST + "game_server/submit_room.php";
		WWWForm form = new WWWForm();
		form.AddField("data", data);
		WWW www = new WWW(url, form);
		Debug.Log("Submitting room data: " + data);
		yield return www;

		if (www.error != null){
			Debug.LogError("error:" + www.error);
			yield break;
		}
		Debug.Log(www.text);

	}

	// Load the newest room from the server
	public IEnumerator LoadRoom() {
		string url = SERVERHOST + "game_server/load_room.php";
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null){
			Debug.LogError("error:" + www.error);
			yield break;
		}
		if (string.IsNullOrEmpty(www.text)){
			Debug.LogError("Request returned an empty string.");
			yield break;
		}

		string roomData = www.text;
		Debug.Log("Received room data: "+ roomData);
		boardScript.SetupLoadedRoom (roomData);
		Debug.Log("Room constructed.");
	}

	public IEnumerator LoadLevel(int levelNumber){
		loadingScreen.SetActive(true);
		string url = SERVERHOST + "game_server/load_level.php";
        WWWForm form = new WWWForm();
        form.AddField("level", levelNumber);
		WWW www = new WWW(url,form);
		yield return www;

		if (www.error != null){
			Debug.LogError("error:" + www.error);
			yield break;
		}
		if (string.IsNullOrEmpty(www.text)){
			Debug.LogError("Request returned an empty string.");
			yield break;
		}

		string levelString = www.text;
		string[] levelData = JsonReader.Deserialize<string[]>(levelString);
		roomNumber = 0;
		DungeonData.Add (levelData);
		updateRoomText ();
		boardScript.SetupLevelStartRoom ();
		//boardScript.SetRoomData (((string[])DungeonData [levelNumber - 1])[0]);
		boardScript.UpdateRoom ();
		loadingScreen.SetActive(false);
	}

    public void ResetDice()
    {
        _dicePool.GetComponent<DicePool>().RollDice();   //Calls the roll dice function in the dice pool. 
        _diceMenu.ResetUI();                                            //Resets the players stats and removes all the values from the dice slot. 
    }

    //NONE, ARM, STR. 
    public void AddDice(string diceType, GameObject dice)
    {
        Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        GameObject newDice = Instantiate(dice, Vector3.zero, randomRotation) as GameObject;
        newDice.transform.parent = _dicePool.transform;
        newDice.transform.position = new Vector3(_dicePool.transform.position.x, _dicePool.transform.position.y + 1, _dicePool.transform.position.z);
        newDice.transform.localPosition = Random.insideUnitCircle * 1;
        newDice.name = "Dice-" + diceType;
        newDice.GetComponent<Renderer>().enabled = false;
        _diceMenu.ResetRollBTN();
    }

    public void RemoveDice(string diceType)
    {
        foreach(Transform dice in _dicePool.transform)
        {
            if (dice.name.Contains(diceType))
            {
                Destroy(dice.gameObject);
                if(diceType == "NONE")
                {
                    break;
                }
            }
        }
    }
    public void RemoveDiceAndRoll(string diceType)
    {
        foreach (Transform dice in _dicePool.transform)
        {
            if (dice.name.Contains(diceType))
            {
                Destroy(dice.gameObject);
                if (diceType == "NONE")
                {
                    break;
                }
            }
        }
        if (_dicePool != null)
        {
            if (_dicePool.GetComponent<DicePool>().isRolling == false)
            {
                _dicePool.GetComponent<DicePool>().RollDice();
                //StartCoroutine();   //Calls the roll dice function in the dice pool. 
                _diceMenu.ResetUI();                                            //Resets the players stats and removes all the values from the dice slot. 
            }
        }
    }




	//Initializes the game.
	void InitGame() {
		StartCoroutine(LoadLevel(levelNumber));
		Debug.Log (DungeonData[0]);
	}

	public void ClosePauseMenu(){
		pauseMenu.SetActive (false);
	}
		
	//Update is called every frame.
	void Update()
	{}

	private void updateRoomText(){
		levelText.text = "LEVEL: "+levelNumber;
		if (roomNumber == 0) {
			roomText.text = "ROOM : Start Room";
		} else if (roomNumber > ((string[])DungeonData [levelNumber - 1]).Length) {
			roomText.text = "ROOM : End Room";
		} else {
			roomText.text = "ROOM : "+roomNumber+"/"+((string[])DungeonData [levelNumber - 1]).Length;
		}
        IncreaseRollCount();
        if (rollCount == 1)
        {
            if (CountDice() > 0 || CountSpecDice() > 0)
            {
                _diceMenu.ResetRollBTN();
            }   
        }
    }

	void LateUpdate () {
		
		if (Input.GetKeyDown(KeyCode.Escape) && !gameovered) {
			if (isPaused) {
				isPaused = false;
				Time.timeScale = 1;
				pauseMenu.SetActive (false);
			} else {
				Time.timeScale = 0;
				isPaused = true;
				pauseMenu.SetActive (true);
				GameObject resetButton = GameObject.Find ("ResetButton");
				if (isLevelEndRoom ()) {
					resetButton.GetComponent<Button> ().interactable = false;
				} else {
					resetButton.GetComponent<Button> ().interactable = true;
				}
			}

		}
	}

    public int CountDice()
    {
        int diceCount = 0;
        foreach (Transform child in _dicePool.transform)
        {
            if (child.gameObject.name == "Dice-NONE")
            {
                diceCount++;
            }
        }
        return diceCount;
    }

    public int CountSpecDice()
    {
        int diceCount = 0;
        foreach (Transform child in _dicePool.transform)
        {

            if (child.gameObject.name == "Dice-ARM" || child.gameObject.name == "Dice-STR" )
            {
                diceCount++;
            }
        }
        return diceCount;
    }


    public void ShowGameoverMenu() {
        gameovered = true;
        gameoverMenu.SetActive(true);

        GameObject retryButton = GameObject.Find("RetryButton");
        if (CountDice() == 0)
        {
            retryButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            retryButton.GetComponent<Button>().interactable = true;
        }

    }

    public void RetryGame() {
        gameovered = false;
        gameoverMenu.SetActive(false);

        // remove dice goes here
        RemoveDice("NONE");
        _diceMenu.ResetStats();
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Reborn();
        ResetRoom();
		SoundManager.instance.PlayBGM ();
    }

    public void IncreaseRollCount()
    {
        if (rollCount == 0)
        {
            rollCount = 1;
        }
    }

    public void DecreaseRollCount()
    {
        if (rollCount > 0)
        {
            rollCount = 0;
        }
    }

}