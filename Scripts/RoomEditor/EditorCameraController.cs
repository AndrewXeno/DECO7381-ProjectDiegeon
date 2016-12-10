using UnityEngine;
using System.Collections;

public class EditorCameraController : MonoBehaviour {
	BoardManager boardScript;					// the board manager


	void Start () {
		boardScript=BoardManager.instance.GetComponent<BoardManager>();
		float columns = boardScript.columns;
		float rows = boardScript.rows;
		Camera.main.transform.position = new Vector3 ((columns-1)/2,(rows-1)/2,-10);
	}

	public void ResetCamera (){
		float columns = boardScript.columns;
		float rows = boardScript.rows;
		Camera.main.transform.position = new Vector3 ((columns-1)/2,(rows-1)/2,-10);
	}

	// Change the camera size with given delta value
	private void UpdateCameraSize(float deltaSize){
		float size = Camera.main.orthographicSize + deltaSize;
		size = Mathf.Max (4,size);
		size = Mathf.Min (32,size);
		Camera.main.orthographicSize=size;
	}

	// Change the camera position with given delta value
	private void UpdateCameraPosition(float deltaX, float deltaY){
		float columns = boardScript.columns;
		float rows = boardScript.rows;
		float xDestination = Camera.main.transform.position.x + deltaX;
		float yDestination = Camera.main.transform.position.y + deltaY;
		xDestination = Mathf.Max (0,xDestination);
		yDestination = Mathf.Max (0,yDestination);
		xDestination = Mathf.Min (columns,xDestination);
		yDestination = Mathf.Min (rows,yDestination);
		Camera.main.transform.position = new Vector3 (xDestination,yDestination,-10);
	}


	// Update the camera position and size when the game updates
	void LateUpdate () {
		float columns = boardScript.columns;
		float rows = boardScript.rows;
		float size = Camera.main.orthographicSize;
		float moveInterval = size / 30;
		float zoomInterval = size / 30;
		if (Input.GetKey (KeyCode.W)) {
			UpdateCameraPosition (0, moveInterval);
		}
		if (Input.GetKey (KeyCode.S)) {
			UpdateCameraPosition (0, -moveInterval);
		}
		if (Input.GetKey (KeyCode.A)) {
			UpdateCameraPosition (-moveInterval, 0);
		}
		if (Input.GetKey (KeyCode.D)) {
			UpdateCameraPosition (moveInterval, 0);
		}
		if (Input.GetKey (KeyCode.Q)) {
			UpdateCameraSize(zoomInterval);
		}
		if (Input.GetKey (KeyCode.E)) {
			UpdateCameraSize(-zoomInterval);
		}
		if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			UpdateCameraSize (zoomInterval);
		}
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			UpdateCameraSize (-zoomInterval);
		}

		if (Input.GetMouseButton (1)) {
			float mouseX = Input.GetAxis ("Mouse X");
			float mouseY = Input.GetAxis ("Mouse Y");
			float cameraX = Camera.main.transform.position.x;
			float cameraY = Camera.main.transform.position.y;

			if ((cameraX <= 0 && mouseX>0 ) || (cameraX >= columns&& mouseX<0 )) {
				mouseX = 0f;
			}
			if ((cameraY <= 0 && mouseY>0 ) || (cameraY >= rows && mouseY<0 )){
				mouseY = 0f;
			}
			Camera.main.transform.Translate(new Vector3 (-mouseX,-mouseY,0)*Time.deltaTime*size*6.5f);
		}
	}
}
