using UnityEngine;
using System.Collections;

public class EditorLoader : MonoBehaviour {

	public GameObject editorManager;			//GameManager prefab to instantiate.
	public GameObject boardManager;
	public GameObject accountManager;
    public GameObject soundManager;

    void Awake () {
		if (AccountManager.instance == null)
			Instantiate(accountManager);
		if (BoardManager.instance == null)
			Instantiate(boardManager);
		if (EditorManager.instance == null)
			Instantiate(editorManager);
        if (SoundManager.instance == null)
            Instantiate(soundManager);
        SoundManager.instance.GetComponent<SoundManager>().PlayBGM();
    }
}
