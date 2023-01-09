using UnityEngine;
using System.Collections;

public class Unparent : MonoBehaviour {
	public GameObject player;
	// Use this for initialization
	void Start () {
		//transform.parent = null;
		transform.SetParent(null , true);
		if(!player){
			player = GameObject.FindWithTag("Player");
		}
		DontDestroyOnLoad (transform.gameObject);
	}

	void Update() {
		if(!player){
			Destroy(gameObject);
		}

	}

}
