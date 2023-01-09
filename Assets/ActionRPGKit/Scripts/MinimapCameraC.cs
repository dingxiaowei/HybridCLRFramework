using UnityEngine;
using System.Collections;

public class MinimapCameraC : MonoBehaviour {
	
	public Transform target;

	
	void Update (){
		if(!target){
			GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
			if(gos.Length > 0){
				target = gos[0].transform;
			}
			return;
		}
		transform.position = new Vector3(target.position.x ,transform.position.y ,target.position.z);
	}
	
	void  FindTarget (){
		if(!target){
			Transform newTarget = GameObject.FindWithTag ("Player").transform;
			if(newTarget){
				target = newTarget;
			}
		}

	}
}