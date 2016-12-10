using UnityEngine;
using System.Collections;

public class AccountManager : MonoBehaviour {

	public static AccountManager instance = null;
	public string username = "6bit";
	public string password = "6bitTest";

	void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
	}

	public string getUsername(){
		return username;
	}

	public string getPassword(){
		return password;
	}

	public void setUsername(string username){
		this.username = username;
	}

	public void setPassword(string password){
		this.password = password;
	}
		
}
