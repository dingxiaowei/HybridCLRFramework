using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnorePlayerCollisionC : MonoBehaviour {

	// Use this for initialization
	void Start(){
		StartCoroutine(Delay());
	}
	
	IEnumerator Delay(){
		yield return new WaitForSeconds(0.2f);
		if(GlobalConditionC.mainPlayer){
			Physics.IgnoreCollision(GlobalConditionC.mainPlayer.GetComponent<Collider>(), GetComponent<Collider>());
		}
	}
}
