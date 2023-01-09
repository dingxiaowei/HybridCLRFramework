using UnityEngine;
using System.Collections;

public class MonGravity : MonoBehaviour{
	public float gravity = 7;
	private CharacterController controller;

	[HideInInspector]
	public bool bounce = false;
	private float boucingForce = 5.0f;
	private float backForce = 5.0f;
	public bool freezeGravity = false;
	
	public bool stability = false;
	// Use this for initialization
	void Start(){
		controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update(){
		if(bounce){
			Vector3 moveDirection = Vector3.zero;
			
			moveDirection = transform.TransformDirection(0 , boucingForce , -backForce);
			//moveDirection.y = boucingForce;
			moveDirection.y -= 10 * Time.deltaTime;
			moveDirection.z -= 10 * Time.deltaTime;
			controller.Move(moveDirection * Time.deltaTime);
			return;
		}
		if(freezeGravity){
			return;
		}
		controller.Move(Vector3.down * gravity * Time.deltaTime);
	}

	public void BounceUp(float force , float jtime , float bforce){
		if(bounce || stability){
			return;
		}
		boucingForce = force;
		backForce = bforce;
		//---------
		bounce = true;
		if(gameObject.activeSelf){
			StartCoroutine(BounceDown(jtime));
		}
	}
	
	IEnumerator BounceDown(float jtime){
		yield return new WaitForSeconds(jtime);
		bounce = false;
	}
}
