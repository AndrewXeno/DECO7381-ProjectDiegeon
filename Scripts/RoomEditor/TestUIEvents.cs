using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TestUIEvents : MonoBehaviour {

	public void SubmitRoom(){
		StartCoroutine(TestManager.instance.SubmitRoom ());
	}

	public void ExitTest() {
		Time.timeScale = 1;
		SceneManager.LoadScene ("Scenes/BuildMode");
	}
}
