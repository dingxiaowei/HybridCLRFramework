using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockCursor : MonoBehaviour {

	// Use this for initialization
	void Start(){
		ShowCursor();
	}
	
	// Update is called once per frame
	void ShowCursor(){
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}
