using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;
using Pathfinding.Serialization.JsonFx;


public class BoardManager : MonoBehaviour {

	[Serializable]
	public class Count {
		public int minimum;             //Minimum value for our Count class.
		public int maximum;             //Maximum value for our Count class.

		public Count (int min, int max)
		{
			minimum = min;
			maximum = max;
		}
	}
		
	public static BoardManager instance = null;

	public int columns = 8;                                         //Number of columns in our game board.
	public int rows = 8;                                            //Number of rows in our game board.
	public Count wallCount = new Count (5, 9);                      //Lower and upper limit for our random number of walls per level.
	public GameObject[] floorTopPrefabs;                               //Array of floor prefabs.
	public GameObject[] wallTopPrefabs;                                //Array of wall prefabs.
	public GameObject[] floorFrontPrefabs;                               //Array of floor prefabs.
	public GameObject[] wallFrontPrefabs;                                //Array of wall prefabs.
	public GameObject wallShadowPrefab;
	public GameObject[] lowerObjectPrefabs;                                //Array of lower object prefabs.
	public GameObject[] upperObjectPrefabs;                                //Array of upper object prefabs.
	public GameObject entrancePrefab;
	public GameObject exitPrefab;
	public GameObject levelEndExitPrefab;
	public GameObject[] playerPrefabs;
	private RoomData roomData = null;									//the object representing map data
	private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
	private List <Vector3> gridPositions = new List <Vector3> ();   //A list of possible locations to place tiles.
	public readonly int FLOORZ = 999;
    
    public Transform indicatorHolder;
    private Transform cursorFollower;
    private Transform previewBoardHolder;   
    private ArrayList roomDataHistory;
    private int historyIndex = 0;
    private RoomData previewRoomData = null;
    public GameObject[] indicator;
	public GameObject[] pickupPrefabs;
	private int lastPickupIdx = 0;


    void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
        roomDataHistory = new ArrayList();
    }


	//Clears our list gridPositions and prepares it to generate a new board.
	void InitialiseList () {
		gridPositions.Clear ();
		for(int x = 0; x < columns; x++) {
			for(int y = 0; y < rows; y++) {
				if(x==0&&y==0)
					continue;
				gridPositions.Add (new Vector3(x, y, 0f));
			}
		}
	}

	// for given room data index, return true iff the position is a floor tile
	private bool isFloor(int idx){
		return roomData.floorTiles [idx] != 0 && roomData.wallTiles [idx] == 0;
	}

	// for given room coordinates, return true iff the position is a floor tile
	private bool isFloor(int x, int y){
		if (x < 0 || x >= columns || y < 0 || y >= rows) {
			return false;
		}
		int idx = Utils.coordsToIdx (x, y, columns);
		return roomData.floorTiles [idx] != 0 && roomData.wallTiles [idx] == 0;
	}

	// for given room data index, return true iff the position is occupied by an upper object
	private bool hasUpperObject(int idx){
		return roomData.upperObjects [idx] != 0;
	}

	// for given room coordinates, return true iff the position is occupied by an upper object
	private bool hasUpperObject(int x, int y){
		if (x < 0 || x >= columns || y < 0 || y >= rows) {
			return false;
		}
		int idx = Utils.coordsToIdx (x, y, columns);
		return roomData.upperObjects [idx] != 0;
	}

	public void SetWallAt(int x, int y, int prefabIdx){
		int positionIdx = Utils.coordsToIdx (x, y, columns);
		this.roomData.wallTiles [positionIdx] = prefabIdx;
		this.roomData.floorTiles [positionIdx] = 1;
		this.roomData.upperObjects [positionIdx] = 0;
		this.roomData.lowerObjects [positionIdx] = 0;
	}

	public void SetPitAt(int x, int y){
		int positionIdx = Utils.coordsToIdx (x, y, columns);
		this.roomData.floorTiles [positionIdx] = 0;
		this.roomData.wallTiles [positionIdx] = 0;
		this.roomData.upperObjects [positionIdx] = 0;
		this.roomData.lowerObjects [positionIdx] = 0;
	}

	public void SetFloorAt(int x, int y, int prefabIdx){
		int positionIdx = Utils.coordsToIdx (x, y, columns);
		this.roomData.floorTiles [positionIdx] = prefabIdx;
		this.roomData.wallTiles [positionIdx] = 0;
	}

	public void SetUpperObjectAt(int x, int y, int prefabIdx){
		int positionIdx = Utils.coordsToIdx (x, y, columns);
		if (isFloor(positionIdx)) {
			this.roomData.upperObjects [positionIdx] = prefabIdx;
		}
	}

	public void SetLowerObjectAt(int x, int y, int prefabIdx){
		int positionIdx = Utils.coordsToIdx (x, y, columns);
		if (isFloor(positionIdx)) {
			this.roomData.lowerObjects [positionIdx] = prefabIdx;
		}
	}

	// construct wall at position (x, y), and add the tiles to boardHolder
	private void ConstructWallAt(int x, int y, int idx){
		GameObject toInstantiate=null;
		GameObject instance = null;

		toInstantiate = wallFrontPrefabs[idx];
		instance = Instantiate (toInstantiate, new Vector3 (x, y, y), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);

		toInstantiate = wallTopPrefabs[idx];
//		// a translucent wall sprite above the character
//		instance = Instantiate (toInstantiate, new Vector3 (x, y+1, y), Quaternion.identity) as GameObject;
//		instance.GetComponent<SpriteRenderer>().material.color = new Color(1f,1f,1f,.8f);
//		instance.transform.SetParent (boardHolder);
//		// a solid wall sprite below the character
//		instance = Instantiate (toInstantiate, new Vector3 (x, y+1, FLOORZ), Quaternion.identity) as GameObject;
//		instance.transform.SetParent (boardHolder);
		instance = Instantiate (toInstantiate, new Vector3 (x, y+1, y), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);

		//if (isFloor (x + 1, y)) {
		toInstantiate = wallShadowPrefab;
		instance = Instantiate (toInstantiate, new Vector3 (x+1, y, FLOORZ-1), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
		//}
	}

	// construct board boundary at position (x, y), and add the tiles to boardHolder
	private void ConstructBoundaryAt(int x, int y){
		GameObject toInstantiate=null;
		GameObject instance = null;

		toInstantiate = floorFrontPrefabs[0];
		instance = Instantiate (toInstantiate, new Vector3 (x, y, FLOORZ), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
	}


	// construct floor at position (x, y), and add the tiles to boardHolder
	private void ConstructFloorAt(int x, int y, int idx){
		GameObject toInstantiate=null;
		GameObject instance = null;
        
		toInstantiate = floorTopPrefabs[idx];
		instance = Instantiate (toInstantiate, new Vector3 (x, y, FLOORZ), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
	}

	// construct pit at position (x, y), and add the tiles to boardHolder
	private void ConstructPitAt(int x, int y, int idx){
		GameObject toInstantiate=null;
		GameObject instance = null;

		toInstantiate = floorFrontPrefabs[idx];
		instance = Instantiate (toInstantiate, new Vector3 (x, y, FLOORZ), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
	}

	private void ConstructUpperObjectAt(int x, int y, int idx){
		GameObject toInstantiate=null;
		GameObject instance = null;

		toInstantiate = upperObjectPrefabs[idx];
		instance = Instantiate (toInstantiate, new Vector3 (x, y, y), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
	}

	private void ConstructLowerObjectAt(int x, int y, int idx){
		GameObject toInstantiate=null;
		GameObject instance = null;

		toInstantiate = lowerObjectPrefabs[idx];
		instance = Instantiate (toInstantiate, new Vector3 (x, y, FLOORZ-1), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
	}

	private void ConstructEntranceAt(int x, int y, string type="Normal"){
		GameObject toInstantiate=null;
		GameObject instance = null;

		toInstantiate = entrancePrefab;
		instance = Instantiate (toInstantiate, new Vector3 (x, y, FLOORZ-1), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
	}

	private void ConstructExitAt(int x, int y){
		GameObject toInstantiate=null;
		GameObject instance = null;
		toInstantiate = exitPrefab;
		if (GameManager.instance && GameManager.instance.isLevelEndRoom()) {
			toInstantiate = levelEndExitPrefab;
		}
		instance = Instantiate (toInstantiate, new Vector3 (x, y, FLOORZ-1), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
	}

	//Sets up the outer walls and floor (background) of the game board.
	void BaseSetup () {
		if (!boardHolder) {
			boardHolder = new GameObject ("Board").transform;
		}
		ClearBoard ();

		// upper side
		for (int x = -1; x < columns + 1; x++) {
			int y = rows;
			if ((roomData.entrance [1] == rows - 1 && roomData.entrance [0] == x)|| 
				(roomData.exit [1] == rows - 1 && roomData.exit [0] == x)) {
				ConstructFloorAt (x, y + 1, 1);
				ConstructFloorAt (x, y, 1);
				ConstructWallAt (x - 1, y + 1, 1);
				ConstructWallAt (x + 1, y + 1, 1);
				continue;
			}
			ConstructWallAt (x, y, 1);

		}

		// left side
		for (int y = 0; y < rows; y++) {
			int x=-1;
			if ((roomData.entrance [0] == 0 && roomData.entrance [1] == y && roomData.entrance [1] >0 && roomData.entrance [1] < rows - 1)||
				(roomData.exit [0] == 0 && roomData.exit [1] == y && roomData.exit [1] > 0 && roomData.exit [1] < rows - 1)) {
				ConstructFloorAt (x - 1, y, 1);
				ConstructFloorAt (x, y, 1);
				ConstructWallAt (x - 1, y + 1, 1);
				ConstructWallAt (x - 1, y - 1, 1);
				continue;
			}
			ConstructWallAt (x, y, 1);
		}

		// right side
		for (int y = 0; y < rows; y++) {
			int x=columns;
			if ((roomData.entrance [0] == columns - 1 && roomData.entrance [1] == y && roomData.entrance [1] >0 && roomData.entrance [1] < rows - 1)||
				(roomData.exit [0] == columns - 1 && roomData.exit [1] == y && roomData.exit [1] > 0 && roomData.exit [1] < rows - 1)) {
				ConstructFloorAt (x + 1, y, 1);
				ConstructFloorAt (x, y, 1);
				ConstructWallAt (x + 1, y + 1, 1);
				ConstructWallAt (x + 1, y - 1, 1);
				continue;
			}
			ConstructWallAt (x, y, 1);
		}

		// bottom side
		for (int x = -1; x < columns + 1; x++) {
			int y = -1;
			if ((roomData.entrance [1] == 0 && roomData.entrance [0] == x)||(roomData.exit [1] == 0 && roomData.entrance [0] == x)) {
				ConstructFloorAt (roomData.entrance [0], -1, 1);
				ConstructFloorAt (roomData.entrance [0], -2, 1);
				ConstructPitAt (roomData.entrance [0], -3, 1);
				ConstructWallAt (roomData.entrance [0]-1, -2, 1);
				ConstructWallAt (roomData.entrance [0]+1, -2, 1);
				ConstructPitAt (roomData.entrance [0]-1, -3, 1);
				ConstructPitAt (roomData.entrance [0]+1, -3, 1);
				continue;
			}
			ConstructWallAt (x, y, 1);
			ConstructPitAt (x, y-1, 1);
		}

		// the boundary
		for (int x = -3; x < columns+3; x++) {
			int y = -3;
			ConstructBoundaryAt (x, y);
			y = rows + 2;
			ConstructBoundaryAt (x, y);
		}
		for (int y = -3; y < rows + 3; y++) {
			int x = -3;
			ConstructBoundaryAt (x, y);
			x = columns + 2;
			ConstructBoundaryAt (x, y);
		}
	}
		
	//RandomPosition returns a random position from our list gridPositions.
	Vector3 RandomPosition () {
		int randomIndex = Random.Range (0, gridPositions.Count);
		Vector3 randomPosition = gridPositions[randomIndex];
		gridPositions.RemoveAt (randomIndex);
		return randomPosition;
	}

	// set random floor positions
	void RandomFloor (int minimum, int maximum) {
		int objectCount = Random.Range (minimum, maximum+1);
		for(int i = 0; i < objectCount; i++){
			Vector3 randomPosition = RandomPosition();

			int posX = (int)randomPosition.x;
			int posY = (int)randomPosition.y;

			this.roomData.floorTiles [Utils.coordsToIdx (posX, posY, columns)] = 0;
		}
	}

	// set random floor positions
	void RandomWall (int minimum, int maximum) {
		int objectCount = Random.Range (minimum, maximum+1);
		for(int i = 0; i < objectCount; i++){
			Vector3 randomPosition = RandomPosition();

			int posX = (int)randomPosition.x;
			int posY = (int)randomPosition.y;

			this.roomData.wallTiles [Utils.coordsToIdx (posX, posY, columns)] = Random.Range (1,wallTopPrefabs.Length);
		}
	}

	// construct all wall tiles of the map
	public void SetupWall(){
		for (int i = 0; i < roomData.wallTiles.Length; i++) {
			int x=0;
			int y=0;
			int tileIdx = roomData.wallTiles [i];
			if (tileIdx > 0) {
				Utils.idxToCoords (i, columns, out x, out y);
				ConstructWallAt (x, y, tileIdx);
			}
		}
	}

	// construct all floor tiles of the map
	public void SetupFloor(){
		for (int i = 0; i < roomData.floorTiles.Length; i++) {
			int x=0;
			int y=0;
			Utils.idxToCoords (i, columns, out x, out y);
			int tileIdx = roomData.floorTiles [i];
			if (tileIdx == 1) {
				ConstructFloorAt (x, y, tileIdx);
			}
            else if(tileIdx == 2){
                this.roomData.wallTiles[i] = 0;
                this.roomData.upperObjects[i] = 0;
                this.roomData.lowerObjects[i] = 0;
                this.roomData.floorTiles[i] = 1;
                ConstructFloorAt(x, y, tileIdx);
            }
            else {
				if (y == rows - 1) {
					tileIdx = 1;
				} else {
					tileIdx = roomData.floorTiles [Utils.coordsToIdx (x, y + 1, columns)];
				}
				ConstructPitAt (x, y, tileIdx);
			}
		}
		for (int x = 0; x < columns; x++) {
			if (roomData.entrance [1] == 0 && roomData.entrance [0] == x) {
				continue;
			}
			if (roomData.exit [1] == 0 && roomData.exit [0] == x) {
				continue;
			}
			int y = -1;
			int tileIdx = roomData.floorTiles [Utils.coordsToIdx (x, y + 1, columns)];
			ConstructPitAt (x, y, tileIdx);
		}

	}

	public void SetupUpperObjects(){
		for (int i = 0; i < roomData.upperObjects.Length; i++) {
			int x=0;
			int y=0;
			int objIdx = roomData.upperObjects [i];
			if (objIdx > 0) {
				Utils.idxToCoords (i, columns, out x, out y);
				ConstructUpperObjectAt (x, y, objIdx);
			}
		}
	}

	public void SetupLowerObjects(){
		for (int i = 0; i < roomData.lowerObjects.Length; i++) {
			int x=0;
			int y=0;
			int objIdx = roomData.lowerObjects [i];
			if (objIdx > 0) {
				Utils.idxToCoords (i, columns, out x, out y);
				ConstructLowerObjectAt (x, y, objIdx);
			}
		}
	}

	public void SetupEntranceAndExit(){
		int x = roomData.entrance [0];
		int y = roomData.entrance [1];
		if (y == 0) {
			ConstructEntranceAt (x, y - 2);
		} else if (y == rows - 1) {
			ConstructEntranceAt (x, y + 2);
		} else if (x == 0) {
			ConstructEntranceAt (x-2, y);
		} else if (x == columns - 1) {
			ConstructEntranceAt (x+2, y);
		}
		x = roomData.exit [0];
		y = roomData.exit [1];


		if (y == 0) {
			ConstructExitAt (x, y - 2);
		} else if (y == rows - 1) {
			ConstructExitAt (x, y + 2);
		} else if (x == 0) {
			ConstructExitAt (x-2, y);
		} else if (x == columns - 1) {
			ConstructExitAt (x+2, y);
		}

	}

	private void ConstructPlayerAt(int x, int y){
		if (GameManager.instance != null || TestManager.instance != null) {
			if (GameObject.FindWithTag ("Player")) {
				GameObject player = GameObject.FindWithTag ("Player");
				player.transform.position = new Vector3 (x, y, y);
			} else {
				GameObject toInstantiate = null;
				GameObject instance = null;

				toInstantiate = playerPrefabs [0];
				instance = Instantiate (toInstantiate, new Vector3 (x, y, y), Quaternion.identity) as GameObject;
				//instance.transform.SetParent (boardHolder);
			}
		} else {
			if (GameObject.FindWithTag ("Player")) {
				Destroy (GameObject.FindWithTag ("Player").gameObject);
			}
		}
	}

	public void SetupPlayerAtEntrance(){
		int x = roomData.entrance [0];
		int y = roomData.entrance [1];
		if (y == 0) {
			y--;
		} else if (y == rows - 1) {
			y++;
		} else if (x == 0) {
			x--;
		} else if (x == columns - 1) {
			x++;
		}
		ConstructPlayerAt(x, y);
	}

	public void SetupPlayerAtExit(){
		int x = roomData.exit [0];
		int y = roomData.exit [1];
		if (y == 0) {
			y--;
		} else if (y == rows - 1) {
			y++;
		} else if (x == 0) {
			x--;
		} else if (x == columns - 1) {
			x++;
		}
		ConstructPlayerAt(x, y);
	}


	//SetupScene initializes our level and calls the previous functions to lay out the game board
	public void SetupRandomRoom () {
		roomData = new RoomData(columns, rows);
		InitialiseList ();
		RandomFloor (wallCount.minimum, wallCount.maximum);
		RandomWall (wallCount.minimum, wallCount.maximum);
		setupRoom ();
	}

	// Construct the room map from JSON string
	public void SetupLoadedRoom (string RoomJson) {
		roomData = JsonReader.Deserialize<RoomData>(RoomJson);
		columns = roomData.columns; 
		rows = roomData.rows; 

		if (!boardHolder) {
			boardHolder = new GameObject ("Board").transform;
		}
		setupRoom ();
	}

	public void SetupLevelStartRoom () {
		columns = 9; 
		rows = 9; 
		if (!boardHolder) {
			boardHolder = new GameObject ("Board").transform;
		}
		roomData = new RoomData("LevelStart");
		setupRoom ();
	}


	private void SetupPickup(){
		GameObject toInstantiate = pickupPrefabs [lastPickupIdx];
		GameObject instance = Instantiate (toInstantiate, new Vector3 (columns/2, rows/2, 998), Quaternion.identity) as GameObject;
		instance.transform.SetParent (boardHolder);
	}

	public void SetupLevelEndRoom(){
		columns = 9; 
		rows = 9; 
		if (!boardHolder) {
			boardHolder = new GameObject ("Board").transform;
		}
		roomData = new RoomData("LevelEnd");
		ClearBoard ();
		BaseSetup ();
		SetupWall ();
		SetupFloor ();
		SetupEntranceAndExit ();

		float rand = Random.value;
        if (rand >= 0f && rand < 0.2f) {
            lastPickupIdx = 0;  //dagger
        } else if (rand>=0.2f && rand < 0.3f) {
            lastPickupIdx = 1;  //rifle
        } else if (rand >= 0.3f && rand < 0.6f) {
            lastPickupIdx = 2;  //dice
        } else if (rand >= 0.6f && rand < 0.7f)  {
            lastPickupIdx = 3;  //armor
        } else if (rand >= 0.7f && rand < 0.9f) {
            lastPickupIdx = 4;  //bow
        } else {
            lastPickupIdx = 5;  //wand
        } 
		SetupPickup ();
	}


	public void SetupEmptyRoom () {
		if (!boardHolder) {
			boardHolder = new GameObject ("Board").transform;
		}
		roomData = new RoomData(columns, rows);
		InitialiseList ();
		setupRoom ();
	}

	public void SetRoomData(string RoomJson){
		roomData = JsonReader.Deserialize<RoomData>(RoomJson);

		columns = roomData.columns; 
		rows = roomData.rows; 
	}

	private void setupRoom(){
		ClearBoard ();
		BaseSetup ();
		SetupWall ();
		SetupFloor ();
		SetupLowerObjects ();
		SetupUpperObjects ();
		SetupEntranceAndExit ();
	}

	public void UpdateRoom () {
		if (!boardHolder) {
			boardHolder = new GameObject ("Board").transform;
		}
		if (roomData == null) {
			roomData = new RoomData(columns, rows);
		}
		setupRoom ();
		SetupPlayerAtEntrance ();
		if (GameManager.instance && GameManager.instance.isLevelEndRoom ()) {
			SetupPickup ();
		}
	}

	public bool isInitialised(){
		return roomData != null;
	}

	public void ResetRoom(){
		roomData = new RoomData(columns, rows);
		setupRoom ();
		SetupPlayerAtEntrance ();
	}

	public void LoadNextRoom () {
		setupRoom ();
		SetupPlayerAtEntrance ();
	}

	public void LoadPreviousRoom () {
		setupRoom ();
		SetupPlayerAtExit ();
	}


	public RoomData getRoomData(){
		return this.roomData;
	}

	public int getBaseDifficulty(){
		int difficulty = 0;
		for (int i = 0; i < roomData.lowerObjects.Length; i++) {
			if (roomData.lowerObjects [i] > 0) {
				difficulty++;
			}
			if (roomData.upperObjects [i] > 0) {
				difficulty++;
			}
		}
		return difficulty;
	}

	// clear object on the board
	private void ClearBoard() {
		foreach (Transform obj in boardHolder) {
			Destroy (obj.gameObject);
		}
	}
    

    private bool hasLowerObject(int idx)
    {
        return roomData.lowerObjects[idx] != 0;
    }

    private bool hasWall(int idx)
    {
        return roomData.wallTiles[idx] != 0;
    }

    private bool hasFloor(int idx)
    {
        return roomData.floorTiles[idx] != 0;
    }

    public void SetPreviewWallAt(int x, int y, int prefabIdx)
    {
        int positionIdx = Utils.coordsToIdx(x, y, columns);
        this.previewRoomData.wallTiles[positionIdx] = prefabIdx;
        this.previewRoomData.floorTiles[positionIdx] = 1;
        this.previewRoomData.upperObjects[positionIdx] = 0;
        this.previewRoomData.lowerObjects[positionIdx] = 0;

        ConstructPreviewWallAt(x, y, prefabIdx);
    }

    public void SetPreviewPitAt(int x, int y)
    {
        int positionIdx = Utils.coordsToIdx(x, y, columns);
        this.previewRoomData.floorTiles[positionIdx] = 0;
        this.previewRoomData.wallTiles[positionIdx] = 0;
        this.previewRoomData.upperObjects[positionIdx] = 0;
        this.previewRoomData.lowerObjects[positionIdx] = 0;

        ConstructPreviewPitAt(x, y, 1);
    }

    public void SetPreviewFloorAt(int x, int y, int prefabIdx)
    {
        int positionIdx = Utils.coordsToIdx(x, y, columns);
        this.previewRoomData.floorTiles[positionIdx] = prefabIdx;
        this.previewRoomData.wallTiles[positionIdx] = 0;

        ConstructPreviewFloorAt(x, y, prefabIdx);
    }

    public void SetPreviewUpperObjectAt(int x, int y, int prefabIdx)
    {
        int positionIdx = Utils.coordsToIdx(x, y, columns);
        if (isFloor(positionIdx))
        {
            this.previewRoomData.upperObjects[positionIdx] = prefabIdx;

            ConstructPreviewUpperObjectAt(x, y, prefabIdx);
        }
    }

    public void SetPreviewLowerObjectAt(int x, int y, int prefabIdx)
    {
        int positionIdx = Utils.coordsToIdx(x, y, columns);
        if (isFloor(positionIdx))
        {
            this.previewRoomData.lowerObjects[positionIdx] = prefabIdx;

            ConstructPreviewLowerObjectAt(x, y, prefabIdx);
        }
    }

    // construct wall at position (x, y), and add the tiles to boardHolder
    private void ConstructPreviewWallAt(int x, int y, int idx)
    {
        GameObject toInstantiate = null;
        GameObject instance = null;

        toInstantiate = wallFrontPrefabs[idx];
        instance = Instantiate(toInstantiate, new Vector3(x, y, y), Quaternion.identity) as GameObject;
		instance.GetComponent<BoxCollider2D>().enabled = false;
		instance.transform.SetParent(previewBoardHolder);

        toInstantiate = wallTopPrefabs[idx];
        instance = Instantiate(toInstantiate, new Vector3(x, y + 1, y), Quaternion.identity) as GameObject;
        instance.transform.SetParent(previewBoardHolder);

        //toInstantiate = wallShadowPrefab;
        //instance = Instantiate(toInstantiate, new Vector3(x + 1, y, FLOORZ - 1), Quaternion.identity) as GameObject;
        //instance.transform.SetParent(boardHolder);
    }


    // construct floor at position (x, y), and add the tiles to boardHolder
    private void ConstructPreviewFloorAt(int x, int y, int idx)
    {
        GameObject toInstantiate = null;
        GameObject instance = null;


        toInstantiate = floorTopPrefabs[idx];
        if (hasWall(FindBoardObject(x, y)))
        {
            if(!hasWall(FindBoardObject(x+1, y))&& !hasUpperObject(FindBoardObject(x+1, y))
                && !hasLowerObject(FindBoardObject(x+1, y)))
            {
                instance = Instantiate(toInstantiate, new Vector3(x+1, y, 0), Quaternion.identity) as GameObject;
                instance.transform.SetParent(previewBoardHolder);
            } 
        }
        instance = Instantiate(toInstantiate, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
        instance.transform.SetParent(previewBoardHolder);
    }

    // construct pit at position (x, y), and add the tiles to boardHolder
    private void ConstructPreviewPitAt(int x, int y, int idx)
    {
        GameObject toInstantiate = null;
        GameObject instance = null;

        toInstantiate = floorFrontPrefabs[2];
        instance = Instantiate(toInstantiate, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
        instance.transform.SetParent(previewBoardHolder);
    }

    private void ConstructPreviewUpperObjectAt(int x, int y, int idx)
    {
        GameObject toInstantiate = null;
        GameObject instance = null;

        if (idx == 1)
        {
            idx = 5;
        }
        toInstantiate = upperObjectPrefabs[idx];
        instance = Instantiate(toInstantiate, new Vector3(x, y, y), Quaternion.identity) as GameObject;
        instance.transform.SetParent(previewBoardHolder);
    }

    private void ConstructPreviewLowerObjectAt(int x, int y, int idx)
    {
        GameObject toInstantiate = null;
        GameObject instance = null;

        toInstantiate = lowerObjectPrefabs[idx];
        instance = Instantiate(toInstantiate, new Vector3(x, y, FLOORZ - 1), Quaternion.identity) as GameObject;
        instance.transform.SetParent(previewBoardHolder);
    }


    // construct all wall tiles of the map
    public void SetupPreviewWall()
    {
        for (int i = 0; i < previewRoomData.wallTiles.Length; i++)
        {
            int x = 0;
            int y = 0;
            int tileIdx = previewRoomData.wallTiles[i];
            if (tileIdx > 0)
            {
                Utils.idxToCoords(i, columns, out x, out y);
            }
        }
    }
    
    public void clearTransformChild(string Holder)
    {
        Transform toClear = null;
        if (Holder == "preview")
        {
            toClear = this.previewBoardHolder;

            this.previewRoomData = new RoomData(columns, rows, true);
        }
        else if (Holder == "cursor")
        {
            toClear = this.cursorFollower;
        }
        else if (Holder == "indicator")
        {
            toClear = this.indicatorHolder;
        }

        if (toClear!=null)
        {
            for (int i = 0; i < toClear.childCount; i++)
            {
                Destroy(toClear.GetChild(i).gameObject);
            }
        }
    }


    public void SynchronizeBoard()
    {
        this.roomData.add(this.previewRoomData);
    }

    public void initPreview()
    {
        if (!previewBoardHolder)
        {
            previewBoardHolder = new GameObject("PreviewBoard").transform;
        }
        if (previewRoomData == null)
        {
            previewRoomData = new RoomData(columns, rows, true);
        }
        if (!cursorFollower)
        {
            cursorFollower = new GameObject("CursorFollower").transform;
        }
        if (!indicatorHolder)
        {
            indicatorHolder = new GameObject("indicatorHolder").transform;
        }
    }

    public void DrawRect(bool full, int type, int id, int x0, int y0, int x1, int y1)
    {
        if (type == -1 || id == -1)
        {
            return;
        }
        if (full == false)
        {
            for (int x = Math.Min(x0, x1); x < Math.Max(x0, x1) + 1; x++)
            {
                if (type == 1)
                {
                    SetPreviewFloorAt(x, y0, id);
                    SetPreviewFloorAt(x, y1, id);
                }
                else if (type == 2)
                {
                    SetPreviewWallAt(x, y0, id);
                    SetPreviewWallAt(x, y1, id);
                }
                else if (type == 3)
                {
                    SetPreviewPitAt(x, y0);
                    SetPreviewPitAt(x, y1);
                }
                else if (type == 4)
                {
                    SetPreviewLowerObjectAt(x, y0, id);
                    SetPreviewLowerObjectAt(x, y1, id);
                }
                else if (type == 5)
                {
                    SetPreviewUpperObjectAt(x, y0, id);
                    SetPreviewUpperObjectAt(x, y1, id);
                }
            }
            for (int y = Math.Min(y0, y1); y < Math.Max(y0, y1) + 1; y++)
            {
                if (type == 1)
                {
                    SetPreviewFloorAt(x0, y, id);
                    SetPreviewFloorAt(x1, y, id);
                }
                else if (type == 2)
                {
                    SetPreviewWallAt(x0, y, id);
                    SetPreviewWallAt(x1, y, id);
                }
                else if (type == 3)
                {
                    SetPreviewPitAt(x0, y);
                    SetPreviewPitAt(x1, y);
                }
                else if (type == 4)
                {
                    SetPreviewLowerObjectAt(x0, y, id);
                    SetPreviewLowerObjectAt(x1, y, id);
                }
                else if (type == 5)
                {
                    SetPreviewUpperObjectAt(x0, y, id);
                    SetPreviewUpperObjectAt(x1, y, id);
                }
            }
        }
        else
        {
            for (int x = Math.Min(x0, x1); x < Math.Max(x0, x1) + 1; x++)
            {
                for (int y = Math.Min(y0, y1); y < Math.Max(y0, y1) + 1; y++)
                {
                    if (type == 1)
                    {
                        SetPreviewFloorAt(x, y, id);
                    }
                    else if (type == 2)
                    {
                        SetPreviewWallAt(x, y, id);
                    }
                    else if (type == 3)
                    {
                        SetPreviewPitAt(x, y);
                    }
                    else if (type == 4)
                    {
                        SetPreviewLowerObjectAt(x, y, id);
                    }
                    else if (type == 5)
                    {
                        SetPreviewUpperObjectAt(x, y, id);
                    }
                }
            }
        }
    }

    public int FindBoardObject(int x, int y)
    {
        int idx = Utils.coordsToIdx(x, y, columns);
        return idx;
    }

    public void DeleteBoardObject(int idx)
    {
        if (hasUpperObject(idx))
        {
            roomData.upperObjects[idx] = 0;
        }
        else if (hasLowerObject(idx))
        {
            roomData.lowerObjects[idx] = 0;
        }
        else if (hasWall(idx))
        {
            roomData.wallTiles[idx] = 0;
        }
        else if (hasFloor(idx))
        {
            roomData.floorTiles[idx] = 0;
        }
    }

    public void ResetBoard()
    {
        this.roomData = new RoomData(columns, rows);
        backupRoomData();
        UpdateRoom();
    }

    public void backupRoomData()
    {
        if (historyIndex == 100)
        {
            historyIndex = 0;
        }
        this.roomDataHistory.Insert(historyIndex, new RoomData(this.roomData));
        Debug.Log("save at slot " + (historyIndex).ToString());
        historyIndex += 1;
        updateHistory();
    }

    private void updateHistory()
    {
        if (historyIndex < roomDataHistory.Count)
        {
            this.roomDataHistory.RemoveRange(historyIndex, roomDataHistory.Count - historyIndex);
        }
    }
    public void printHistory()
    {
        for (int i = 1; i < historyIndex; i++)
        {
            Debug.Log("at slot " + i + ":");
            ((RoomData)this.roomDataHistory[i]).toString();
        }
    }

    public void GoBackRoomData()
    {
        if (historyIndex - 1 >= 0)
        {
            historyIndex -= 1;
            this.roomData = new RoomData((RoomData)this.roomDataHistory[historyIndex]);
            UpdateRoom();
            Debug.Log("load at slot " + (historyIndex).ToString());
        }
    }

    public void GoForwardRoomData()
    {
        if (historyIndex + 1 < roomDataHistory.Count)
        {
            historyIndex += 1;
            this.roomData = new RoomData((RoomData)this.roomDataHistory[historyIndex]);
            UpdateRoom();
            Debug.Log("load at slot " + (historyIndex).ToString());
        }
    }

    public void ConstructAt(int type, int idx, float x, float y)
    {
        GameObject toInstantiate = null;
        GameObject instance = null;

        if (type == 1)
        {
            toInstantiate = floorTopPrefabs[idx];
            instance = Instantiate(toInstantiate, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
        }
        else if (type == 2)
        {
            toInstantiate = wallFrontPrefabs[idx];
            instance = Instantiate(toInstantiate, new Vector3(x, y, y), Quaternion.identity) as GameObject;
			instance.GetComponent<BoxCollider2D>().enabled = false;
			instance.transform.SetParent(cursorFollower);
            toInstantiate = wallTopPrefabs[idx];
            instance = Instantiate(toInstantiate, new Vector3(x, y + 1, y), Quaternion.identity) as GameObject;
            instance.transform.SetParent(cursorFollower);

            //toInstantiate = wallShadowPrefab;
            //instance = Instantiate(toInstantiate, new Vector3(x + 1, y, FLOORZ - 1), Quaternion.identity) as GameObject;
            //instance.transform.SetParent(cursorFollower);
        }
        else if (type == 3)
        {
            toInstantiate = floorFrontPrefabs[2];
            instance = Instantiate(toInstantiate, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
        }
        else if (type == 5)
        {
            if (idx == 1)
            {
                idx = 5;
            }
            toInstantiate = upperObjectPrefabs[idx];
            instance = Instantiate(toInstantiate, new Vector3(x, y, y), Quaternion.identity) as GameObject;
        }
        else if (type == 4)
        {
            toInstantiate = lowerObjectPrefabs[idx];
            instance = Instantiate(toInstantiate, new Vector3(x, y, FLOORZ - 1), Quaternion.identity) as GameObject;
        }
        instance.transform.SetParent(cursorFollower);
    }

    public void showIndicator(int x0, int y0, int x1, int y1)
    {
        clearTransformChild("indicator");
        GameObject toInstantiate = null;
        GameObject instance = null;
        if (x0 == x1 && y0 == y1)
        {
            toInstantiate = indicator[0];
            instance = Instantiate(toInstantiate, new Vector3(x0, y0, 0), Quaternion.identity) as GameObject;
            instance.transform.SetParent(indicatorHolder);
        }
        
    }
}
