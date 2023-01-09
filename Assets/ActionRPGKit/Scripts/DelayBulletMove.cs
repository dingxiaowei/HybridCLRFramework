using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BulletStatusC))]
[RequireComponent(typeof(Rigidbody))]

public class DelayBulletMove : MonoBehaviour{
	public float delay = 1;
	public float speed = 20;
	public Vector3 relativeDirection= Vector3.forward;
	public float duration = 1.0f;
	public string shooterTag = "Player";
	public GameObject hitEffect;

	private bool moving = false;
	
	void Start(){
		hitEffect = GetComponent<BulletStatusC>().hitEffect;
		GetComponent<Rigidbody>().isKinematic = true;
		StartCoroutine(DelayTime());
	}

	IEnumerator DelayTime(){
		yield return new WaitForSeconds(delay);
		moving = true;
		Destroy(gameObject, duration);
	}
	
	void Update(){
		if(!moving){
			return;
		}
		Vector3 absoluteDirection = transform.rotation * relativeDirection;
		transform.position += absoluteDirection * speed* Time.deltaTime;
	}
	
	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall") {
			if(hitEffect){
				Instantiate(hitEffect, transform.position , transform.rotation);
			}
			if(GetComponent<BulletStatusC>().bombHitSetting.enable){
				GetComponent<BulletStatusC>().ExplosionDamage();
			}
			Destroy(gameObject);
		}
	}
}
