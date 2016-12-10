using UnityEngine;
using System.Collections;


public class Loader : MonoBehaviour 
{
	public GameObject gameManager;			//GameManager prefab to instantiate.
	public GameObject boardManager;
	public GameObject accountManager;
    public GameObject soundManager;

	void Awake() {
		if (AccountManager.instance == null)
			Instantiate(accountManager);
		if (BoardManager.instance == null)
			Instantiate(boardManager);
		if (GameManager.instance == null)
			Instantiate(gameManager);
        if (SoundManager.instance == null)
            Instantiate(soundManager);
        SoundManager.instance.GetComponent<SoundManager>().PlayBGM();
    }
}
