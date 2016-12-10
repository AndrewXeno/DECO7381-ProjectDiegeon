using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerOnEntranceExit : MonoBehaviour {
	private GameManager gameScript;
	private TestManager testScript;

	// Use this for initialization
	void Start () {
		if (GameManager.instance)
			gameScript = GameManager.instance.GetComponent<GameManager> ();
		if (TestManager.instance)
			testScript = TestManager.instance.GetComponent<TestManager> ();


	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void OnTriggerEnter2D (Collider2D other) {
		if (GameManager.instance) {
			if (other.tag == "Exit") {
				gameScript.NextRoom ();

			} else if (other.tag == "LevelEndExit"){
				gameScript.NextLevel ();
			}
//			else if (other.tag == "Entrance") {
//				gameScript.PreviousRoom ();
//			}
		} else if (TestManager.instance) {
			if (other.tag == "Exit") {
				testScript.ShowClearMenu ();
			} 
		}
		return;
	}

}
