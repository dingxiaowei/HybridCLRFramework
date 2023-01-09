using UnityEngine;
using System.Collections;

public class DestroyObjectC : MonoBehaviour {
	
	public float duration = 1.5f;
	
	void  Start (){
		Destroy (gameObject, duration);
	}
}