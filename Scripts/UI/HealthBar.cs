using UnityEngine;
using System.Collections;

/**
 * Scales the width of the lifebar based on the total amount of damage dealt out of a 100. 
 * negative numbers will heal the player
 **/
public class HealthBar : MonoBehaviour {

    public GameObject lifeBar; //Sprite that represents the current health.
    public GameObject lifBarEnd; // Sprite that is attached to the end of the health bar for asthetics.
    public float life = 100; // THe current life of the player. 0 = death, 100 = full health
 	
	private Player ps;
	void Start()
	{
		ps = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();;

	}

	void Update()
	{
		float targetWidth = (float)ps.HP / (float)ps.maxHP;
		//        float tempWidth = lifeBar.transform.localScale.x;
		//        float tempHealth = life - damage;
		if (targetWidth>=0 && targetWidth<=1 && targetWidth<lifeBar.transform.localScale.x){
			lifeBar.transform.localScale = new Vector3(lifeBar.transform.localScale.x-0.005f, lifeBar.transform.localScale.y, lifeBar.transform.localScale.z);
		}
        if (targetWidth >= 0 && targetWidth <= 1 && targetWidth > lifeBar.transform.localScale.x)
        {
            lifeBar.transform.localScale = new Vector3(lifeBar.transform.localScale.x + 0.005f, lifeBar.transform.localScale.y, lifeBar.transform.localScale.z);
        }
        float endPoint = lifeBar.GetComponent<SpriteRenderer>().bounds.size.x;
		lifBarEnd.transform.position = new Vector3(lifeBar.transform.position.x + endPoint, lifBarEnd.transform.position.y, lifBarEnd.transform.position.z);
	}

}
