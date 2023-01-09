using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BulletStatusC))]
public class Hook : MonoBehaviour {

	public float hookDuration = 3.0f;
	public float hookSpeed = 8.0f;
	public GameObject target;
	public string shooterTag = "Player";
	private GameObject shooter;
	public bool penetrate = false;
	public Vector3 relativeDirection= Vector3.forward;
	public float duration = 1.0f;
	public float speed = 20.0f;
	public AnimationClip pullAnimation;
	private GameObject mainModel;
	
	private GameObject hitEffect;
	private int state = 0;
	
	void Start(){
		hitEffect = GetComponent<BulletStatusC>().hitEffect;
		shooterTag = GetComponent<BulletStatusC>().shooterTag;
		shooter = GetComponent<BulletStatusC>().shooter;
		mainModel = shooter.GetComponent<StatusC>().mainModel;
		Destroy (gameObject, duration);
	}
	
	void Update(){
		if(state == 0){
			Vector3 absoluteDirection = transform.rotation * relativeDirection;
			transform.position += absoluteDirection *speed* Time.deltaTime;
		}
		if(state == 1 && target && shooter){
			MoveTowardsTarget(target , shooter.transform.position);
		}
	}
	
	void OnTriggerEnter(Collider other){
		if(state == 0){
			if(other.gameObject.tag == "Wall") {
				if(hitEffect){
					Instantiate(hitEffect, transform.position , transform.rotation);
				}
				if(!penetrate){
					Destroy (gameObject);
				}
			}
			if(shooterTag == "Player" && other.tag == "Enemy"){
				target = other.gameObject;
				state = 1;
				StartCoroutine(OnHook());
			}else if(shooterTag == "Enemy" && other.tag == "Player" || shooterTag == "Enemy" && other.tag == "Ally"){
				target = other.gameObject;
				state = 1;
				StartCoroutine(OnHook());
			}
		}
		
	}
	
	void MoveTowardsTarget(GameObject obj , Vector3 target){
		CharacterController charcon = obj.GetComponent<CharacterController>();
		Vector3 offset = target - obj.transform.position;
		//offset.y = obj.transform.position.y;
		if(offset.magnitude > 1) {
			offset = offset.normalized * speed;
			//offset.y = obj.transform.position.y;
			charcon.Move(offset * Time.deltaTime);
			transform.position = Vector3.MoveTowards(transform.position, shooter.transform.position, speed * Time.deltaTime);
		}else{
			state = 2;
			Destroy (gameObject);
		}
	}
	
	IEnumerator OnHook(){
		if(mainModel && pullAnimation){
			mainModel.GetComponent<Animation>()[pullAnimation.name].layer = 17;
			mainModel.GetComponent<Animation>().Play(pullAnimation.name);
		}
		GetComponent<Collider>().enabled = false;
		yield return new WaitForSeconds(hookDuration);
		state = 2;
		Destroy(gameObject);
	}
}