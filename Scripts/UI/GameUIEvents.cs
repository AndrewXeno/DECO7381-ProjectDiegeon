using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameUIEvents : MonoBehaviour {

	public void ExitGame() {
		GameManager.instance.isPaused = false;
		Time.timeScale = 1;
		SceneManager.LoadScene ("Scenes/MainMenu");
	}

	public void ResetRoom(){
		GameManager.instance.isPaused = false;
		GameManager.instance.ClosePauseMenu ();
		Time.timeScale = 1;
		GameManager.instance.ResetRoom ();
	}

	public void ResumeGame(){
		GameManager.instance.isPaused = false;
		Time.timeScale = 1;
		GameManager.instance.ClosePauseMenu ();
	}

    public void RetryGame()
    {
        GameManager.instance.RetryGame();
    }

}
