using UnityEngine;
using System.Collections;

public class LockMouseC : MonoBehaviour {
	void  Start (){
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}
