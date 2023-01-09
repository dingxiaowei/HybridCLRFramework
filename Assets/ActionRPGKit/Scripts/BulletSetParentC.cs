using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BulletStatusC))]

public class BulletSetParentC : MonoBehaviour {
	public float duration = 1.0f;
	public bool penetrate = false;
	private GameObject hitEffect;
	
	public bool setRotation = false;
	public Vector3 rotationSetTo = Vector3.zero;
	// Use this for initialization
	void Start(){
		hitEffect = GetComponent<BulletStatusC>().hitEffect;
		//Set this object parent of the Shooter GameObject from BulletStatus
		if(setRotation){
			transform.eulerAngles = rotationSetTo;
		}
		Transform shooter = GetComponent<BulletStatusC>().shooter.transform;
		this.transform.parent = shooter;
		this.transform.position = new Vector3(transform.position.x , transform.position.y , shooter.position.z);
		Destroy(gameObject, duration);
	}
	
	void OnTriggerEnter(Collider other){  
		if(other.gameObject.tag == "Wall"){
			if(hitEffect && !penetrate){
				Instantiate(hitEffect, transform.position , transform.rotation);
			}
			if(!penetrate){
				//Destroy this object if it not Penetrate
				Destroy (gameObject);
			}
		}
	}
}
