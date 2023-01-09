using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAddForce : MonoBehaviour {
	public float force = 3500;
	public Rigidbody target;
	public Vector3 direction = Vector3.back;

	// Use this for initialization
	void Start(){
		if(!target && GetComponent<Rigidbody>()){
			target = GetComponent<Rigidbody>();
		}
		if(!target){
			return;
		}
		Vector3 dir = transform.TransformDirection(direction);

		target.AddForce(dir * force);
	}
}
