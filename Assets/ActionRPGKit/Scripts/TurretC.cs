using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BulletStatusC))]
public class TurretC : MonoBehaviour {
	
	public float startDelay = 1.0f;
	public float speed = 20.0f;
	public float radius = 20.0f;  //this is how far it checks for other objects
	public Transform attackPoint;
	private Transform lockTarget;
	public bool lockon = true;
	public float duration = 10.0f;
	public float attackDelay = 1.0f;
	public int continous = 3;
	public float continueDelay = 0.2f;
	public Transform bullet1;
	public GameObject destroyEffect;
	
	private int str = 0;
	private int matk = 0;
	private GameObject shooter;
	private string shooterTag = "Player";
	private string targetTag = "Enemy";
	
	private int state = 0;
	public Vector3 relativeDirection= Vector3.forward;
	private GameObject closest;
	public float rotateSpeed = 40.0f;
	
	public GameObject mainModel; 
	public string idleAnim = "";
	public string attackAnim = "";
	public string spawnAnim = "";
	public GameObject attackEffect; 
	public bool lockYAxis = false;
	public float aimUpward = 0.8f;
	
	void Start(){
		if(!attackPoint){
			attackPoint = this.transform;
		}
		if(!mainModel){
			mainModel = this.gameObject;
		}
		if(lockYAxis){
			transform.eulerAngles = new Vector3(0 , transform.eulerAngles.y , 0);
		}
		StartCoroutine(ActivateTurret());
	}

	IEnumerator ActivateTurret(){
		if(spawnAnim != ""){
			mainModel.GetComponent<Animation>().Play(spawnAnim);
			float ww = mainModel.GetComponent<Animation>()[spawnAnim].length;
			yield return new WaitForSeconds(ww);
		}
		yield return new WaitForSeconds(startDelay);
		state = 1;
		
		shooter = GetComponent<BulletStatusC>().shooter;
		shooterTag = GetComponent<BulletStatusC>().shooterTag;
		str = shooter.GetComponent<StatusC>().atk;
		matk = shooter.GetComponent<StatusC>().matk;
		
		if(shooterTag == "Player" || shooterTag == "Ally"){
			targetTag = "Enemy";
		}else{
			targetTag = "Player";
		}
		StartCoroutine(Attack());
		yield return new WaitForSeconds(duration);
		if(destroyEffect){
			Instantiate(destroyEffect, transform.position , transform.rotation);
		}
		Destroy(gameObject);
	}
	
	void Update(){
		if(!lockon){
			return;
		}
		if(state == 0){
			Vector3 absoluteDirection= transform.rotation * relativeDirection;
			transform.position += absoluteDirection *speed* Time.deltaTime;
		}else if(state == 1){
			//lockTarget = FindClosestEnemy().transform;
			FindClosestEnemy();
			if(!lockTarget){
				return;
			}
			
			if(attackPoint != mainModel && lockYAxis){
				Vector3 destiny = lockTarget.position;
				destiny.y = transform.position.y;
				//transform.LookAt(destiny);
				
				Quaternion targetRotation = Quaternion.LookRotation(destiny - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
				
			}else if(attackPoint != mainModel){
				transform.LookAt(lockTarget);
			}

			Vector3 aimTo = lockTarget.position;
			if(Mathf.Abs(attackPoint.position.y - lockTarget.position.y) > 0.3f)
				aimTo.y += aimUpward;
			attackPoint.LookAt(aimTo);
			//attackPoint.LookAt(lockTarget);
		}
	}
	
	// Find the closest enemy 
	void FindClosestEnemy(){
		// Find all game objects with tag Enemy
		float distance = Mathf.Infinity; 

		Collider[] objectsAroundMe = Physics.OverlapSphere(transform.position , radius);
		foreach(Collider obj in objectsAroundMe){
			if(obj.CompareTag(targetTag)){
				Vector3 diff = (obj.transform.position - transform.position); 
				float curDistance = diff.sqrMagnitude; 
				if (curDistance < distance) { 
					//------------
					lockTarget = obj.transform;
					distance = curDistance;
				} 
			}
		}
	}
	
	IEnumerator Attack(){
		if(GlobalConditionC.freezeAll){
			yield break;
		}
		int k = 0;
		int c = 0;
		while (k < 10){
			//transform.LookAt(target.transform);
			Transform bulletShootout;
			while (c < continous && shooter && lockTarget || c < continous && shooter && !lockon){
				if(attackAnim != ""){
					mainModel.GetComponent<Animation>().Play(attackAnim);
				}
				if(attackEffect){
					Instantiate(attackEffect, attackPoint.transform.position , attackPoint.transform.rotation);
				}
				bulletShootout = Instantiate(bullet1, attackPoint.transform.position , attackPoint.transform.rotation) as Transform;
				bulletShootout.GetComponent<BulletStatusC>().Setting(str , matk , shooterTag , shooter);
				c++;
				yield return new WaitForSeconds(continueDelay); 
			}
			if(idleAnim != ""){
				mainModel.GetComponent<Animation>().Play(idleAnim);
			}
			yield return new WaitForSeconds(attackDelay); 
			c = 0;
		}
	}
	
	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Wall" && state == 0 || other.gameObject.tag == "Enemy" && state == 0) {
			state = 1;
		}
	}
}