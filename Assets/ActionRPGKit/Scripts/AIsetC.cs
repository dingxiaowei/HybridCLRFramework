using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(StatusC))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MonGravity))]
[AddComponentMenu("Action-RPG Kit(C#)/Create Enemy")]

public class AIsetC : MonoBehaviour {
	public enum AIState { Moving = 0, Pausing = 1 , Idle = 2 , Patrol = 3}
	
	public GameObject mainModel;
	[HideInInspector]
	public Transform followTarget;
	public float approachDistance = 2.0f;
	public float detectRange = 15.0f;
	public float lostSight = 100.0f;
	public float speed = 4.0f;
	public bool useMecanim = false;
	public Animator animator;
	public AnimationClip movingAnimation;
	public AnimationClip idleAnimation;
	public AnimationClip attackAnimation;
	public AnimationClip hurtAnimation;
	
	[HideInInspector]
	public bool flinch = false;

	public bool stability = false;
	
	public bool freeze = false;
	
	public Transform bulletPrefab;
	public Transform attackPoint;

	public float attackCast = 0.3f;
	public float attackDelay = 0.5f;
	[HideInInspector]
	public AIState followState;
	private float distance = 0.0f;
	private int atk = 0;
	private int matk = 0;
	private Vector3 knock = Vector3.zero;
	[HideInInspector]
	public bool cancelAttack = false;
	private bool attacking = false;
	private bool castSkill = false;
	private GameObject[] gos;

	public AudioClip attackVoice;
	public AudioClip hurtVoice;

	public bool aimAtTarget = true;
	public float aimUpward = 0.8f;

	void Start(){
		gameObject.tag = "Enemy"; 
		
		if(!attackPoint){
			attackPoint = this.transform;
		}
		
		if(!mainModel){
			mainModel = this.gameObject;
		}
		GetComponent<StatusC>().useMecanim = useMecanim;
		//Assign MainModel in Status Script
		GetComponent<StatusC>().mainModel = mainModel;
		//Set ATK = Monster's Status
		atk = GetComponent<StatusC>().atk;
		matk = GetComponent<StatusC>().matk;
		
		followState = AIState.Idle;
		if(!useMecanim){
			//If using Legacy Animation
			mainModel.GetComponent<Animation>().Play(idleAnimation.name);
			mainModel.GetComponent<Animation>()[hurtAnimation.name].layer = 10;
		}else{
			//If using Mecanim Animation
			if(!animator){
				animator = mainModel.GetComponent<Animator>();
			}
		}
		
	}
	
	Vector3 GetDestination(){
		Vector3 destination = followTarget.position;
		destination.y = transform.position.y;
		return destination;
	}
	
	void Update(){
		StatusC stat = GetComponent<StatusC>();
		CharacterController controller = GetComponent<CharacterController>();

		FindClosestEnemy();

		if(useMecanim){
			animator.SetBool("hurt" , flinch);
		}
		
		if(flinch){
			controller.Move(knock * 6* Time.deltaTime);
			return;
		}
		if(stat.stun){
			followState = AIState.Moving;
		}
		
		if(freeze || stat.freeze){
			return;
		}

		if(GlobalConditionC.freezeAll){
			followState = AIState.Idle;
			if(!useMecanim){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().CrossFade(idleAnimation.name, 0.2f); 
			}else{
				animator.SetBool("run" , false);
			}
			return;
		}
		
		if(!followTarget){
			if(followState == AIState.Moving || followState == AIState.Pausing){
				followState = AIState.Idle;
				if(!useMecanim){
					//If using Legacy Animation
					mainModel.GetComponent<Animation>().CrossFade(idleAnimation.name, 0.2f); 
				}else{
					animator.SetBool("run" , false);
				}
			}
			return;
		}
		//-----------------------------------
		distance = (transform.position - GetDestination()).magnitude;

		if(followState == AIState.Moving){
			if(!useMecanim){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().CrossFade(movingAnimation.name, 0.2f);
			}else{
				animator.SetBool("run" , true);
			}

			if(distance <= approachDistance) {
				followState = AIState.Pausing;
				//----Attack----
				//Attack();
				StartCoroutine(Attack());
			}else if(distance >= lostSight){
				//Lost Sight
				GetComponent<StatusC>().health = GetComponent<StatusC>().maxHealth;
				followState = AIState.Idle;
				if(!useMecanim){
					//If using Legacy Animation
					mainModel.GetComponent<Animation>().CrossFade(idleAnimation.name, 0.2f);
				}else{
					animator.SetBool("run" , false);
				}
			}else {
				Vector3 forward = transform.TransformDirection(Vector3.forward);
				controller.Move(forward * speed * Time.deltaTime);
				
				Vector3 destinationy = followTarget.position;
				destinationy.y = transform.position.y;
				transform.LookAt(destinationy);
			}
		}else if(followState == AIState.Pausing){
			if(!useMecanim){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().CrossFade(idleAnimation.name, 0.2f);
			}else{
				animator.SetBool("run" , false);
			}

			Vector3 destinya = followTarget.position;
			destinya.y = transform.position.y;
			transform.LookAt(destinya);

			if(distance > approachDistance){
				followState = AIState.Moving;
			}
		}else if(followState == AIState.Idle){
			/*if(!useMecanim){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().CrossFade(idleAnimation.name, 0.2f);
			}else{
				animator.SetBool("run" , false);
			}*/
			//----------------Idle Mode--------------
			Vector3 destinyheight = followTarget.position;
			destinyheight.y = transform.position.y - destinyheight.y;
			int getHealth = GetComponent<StatusC>().maxHealth - GetComponent<StatusC>().health;

			if(distance < detectRange && Mathf.Abs(destinyheight.y) <= 4 || getHealth > 0){
				followState = AIState.Moving;
			}
		}
		//-----------------------------------
	}
	
	public void Flinch(Vector3 dir){
		if(stability){
			return;
		}
		if(hurtVoice && GetComponent<StatusC>().health >= 1){
			GetComponent<AudioSource>().clip = hurtVoice;
			GetComponent<AudioSource>().Play();
		}
		cancelAttack = true;
		if(followTarget){
			Vector3 look = followTarget.position;
			look.y = transform.position.y;
			transform.LookAt(look);
		}
		knock = transform.TransformDirection(Vector3.back);
		//knock = dir;
		//KnockBack();
		StartCoroutine(KnockBack());
		if(!useMecanim){
			//If using Legacy Animation
			mainModel.GetComponent<Animation>().PlayQueued(hurtAnimation.name, QueueMode.PlayNow);
			mainModel.GetComponent<Animation>().CrossFade(movingAnimation.name, 0.2f);
		}
		followState = AIState.Moving;
	}
	
	IEnumerator KnockBack(){
		flinch = true;
		yield return new WaitForSeconds(0.2f);
		flinch = false;
	}
	
	IEnumerator Attack(){
		cancelAttack = false;
		Transform bulletShootout;
		if(!flinch && !GetComponent<StatusC>().freeze && !freeze && !attacking){
			freeze = true;
			attacking = true;
			if(!useMecanim){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().PlayQueued(attackAnimation.name , QueueMode.PlayNow);
			}else{
				animator.Play(attackAnimation.name);
			}
			if(aimAtTarget && followTarget){
				Vector3 aimTo = followTarget.position;
				if(Mathf.Abs(attackPoint.position.y - followTarget.position.y) > 0.3f)
					aimTo.y += aimUpward;
				attackPoint.LookAt(aimTo);
			}
			yield return new WaitForSeconds(attackCast);

			if(!cancelAttack){
				if(attackVoice && !flinch){
					GetComponent<AudioSource>().PlayOneShot(attackVoice);
				}
				bulletShootout = Instantiate(bulletPrefab, attackPoint.position , attackPoint.rotation) as Transform;
				bulletShootout.GetComponent<BulletStatusC>().Setting(atk , matk , "Enemy" , this.gameObject);
				yield return new WaitForSeconds(attackDelay);
				freeze = false;
				attacking = false;
				CheckDistance();
			}else{
				freeze = false;
				attacking = false;
			}
		}
	}

	void CheckDistance(){
		if(!followTarget || GlobalConditionC.freezeAll){
			followState = AIState.Idle;
			if(!useMecanim){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().CrossFade(idleAnimation.name, 0.2f);
			}else{
				animator.SetBool("run" , false);
			}
			return;
		}
		float distancea = (followTarget.position - transform.position).magnitude;
		if (distancea <= approachDistance){
			Vector3 destinya = followTarget.position;
			destinya.y = transform.position.y;
			transform.LookAt(destinya);
			StartCoroutine(Attack());
			//Attack();
		}else{
			followState = AIState.Moving;
		}
	}
	
	
	void FindClosest(){ 
		// Find Closest Player   
		//gos = GameObject.FindGameObjectsWithTag("Player"); 
		//gos += GameObject.FindGameObjectsWithTag("Ally");

		List<GameObject> gosList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
		gosList.AddRange(GameObject.FindGameObjectsWithTag("Ally"));
		
		gos = gosList.ToArray() as GameObject[];

		if(gos.Length > 0){
			float distance = Mathf.Infinity; 
			Vector3 position = transform.position; 
			
			foreach(GameObject go in gos) { 
				Vector3 diff = (go.transform.position - position); 
				float curDistance = diff.sqrMagnitude; 
				if (curDistance < distance) { 
					//------------
					followTarget = go.transform; 
					distance = curDistance;
				} 
			} 
		}
	}

	void FindClosestEnemy(){ 
		// Find all game objects with tag Enemy
		float distance = Mathf.Infinity;
		float findingradius = detectRange;

		if(GetComponent<StatusC>().health < GetComponent<StatusC>().maxHealth){
			findingradius += lostSight + 3.0f;
		}
		
		Collider[] objectsAroundMe = Physics.OverlapSphere(transform.position , findingradius);
		foreach(Collider obj in objectsAroundMe){
			if(obj.CompareTag("Player") || obj.CompareTag("Ally")){
				Vector3 diff = (obj.transform.position - transform.position); 
				float curDistance = diff.sqrMagnitude; 
				if(curDistance < distance) { 
					//------------
					followTarget = obj.transform;
					distance = curDistance;
				} 
			}
		}
	}

	public void ActivateSkill(Transform skill , float castTime , float delay , string anim , float dist , bool atTarget){
		StartCoroutine(UseSkill(skill ,castTime, delay , anim , dist , atTarget));
	}

	public IEnumerator UseSkill(Transform skill , float castTime , float delay , string anim , float dist , bool atTarget){
		cancelAttack = false;
		if(!flinch && followTarget && (followTarget.position - transform.position).magnitude < dist && !GetComponent<StatusC>().silence && !GetComponent<StatusC>().freeze  && !castSkill){
			freeze = true;
			castSkill = true;
			if(!useMecanim){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().Play(anim);
			}else{
				animator.Play(anim);
			}
			if(aimAtTarget && followTarget){
				Vector3 aimTo = followTarget.position;
				if(Mathf.Abs(attackPoint.position.y - followTarget.position.y) > 0.3f)
					aimTo.y += aimUpward;
				attackPoint.LookAt(aimTo);
			}
			//Transform bulletShootout;
			yield return new WaitForSeconds(castTime);
			if(!cancelAttack){
				Transform bulletShootout = Instantiate(skill, attackPoint.position , attackPoint.rotation) as Transform;
				bulletShootout.GetComponent<BulletStatusC>().Setting(atk , matk , "Enemy" , this.gameObject);
				if(atTarget && followTarget){
					bulletShootout.position = followTarget.position;
				}
				yield return new WaitForSeconds(delay);
				freeze = false;
				castSkill = false;
			}else{
				freeze = false;
				castSkill = false;
			}
		}
	}
}