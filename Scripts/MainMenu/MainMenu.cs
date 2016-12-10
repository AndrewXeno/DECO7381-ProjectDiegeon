using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	public static GameObject creditWindow;      // allow UIEvents access the gameobject
    public GameObject soundManager;
    // Use this for initialization

    void Start () {
		if (GameObject.FindGameObjectWithTag ("BoardManager"))
			Destroy (GameObject.FindGameObjectWithTag ("BoardManager").gameObject);
		creditWindow = GameObject.Find ("Credit");
		creditWindow.SetActive (false);
        if (SoundManager.instance == null)
            Instantiate(soundManager);
        SoundManager.instance.GetComponent<SoundManager>().PlayBGM();
    }
		
	// Update is called once per frame
	void Update () {
	
	}

}
