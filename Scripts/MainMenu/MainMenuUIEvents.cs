using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuUIEvents : MonoBehaviour {

	public void StartExploreMode()
	{
		SceneManager.LoadScene ("Scenes/ExploreMode");
	}

	public void StartBuildMode()
	{
		SceneManager.LoadScene ("Scenes/BuildMode");
	}

	public void ExitGame()
	{
		Application.Quit ();
	}

	public void OpenCredit()
	{
		MainMenu.creditWindow.SetActive (true);
	}

	public void CloseCredit()
	{
		MainMenu.creditWindow.SetActive (false);
	}
}
