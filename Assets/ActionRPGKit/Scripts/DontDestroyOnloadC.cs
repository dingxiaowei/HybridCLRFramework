using UnityEngine;
using System.Collections;

public class DontDestroyOnloadC : MonoBehaviour {
	
	void Awake(){
		DontDestroyOnLoad (transform.gameObject);
	}
}