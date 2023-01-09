using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AIsetC))]

public class PatrollingAiWaypointC : MonoBehaviour {

	public Transform[] waypoints = new Transform[3];
	public bool randomWaypoints = true;
	public float speed = 4.0f;
	private AIsetC ai;
	private int state = 0; //0 = Idle , 1 = Moving.
	private AnimationClip movingAnimation;
	private AnimationClip idleAnimation;
	private GameObject mainModel;
	
	public float idleDuration = 2.0f;
	public float moveDuration = 3.0f;

	private Transform headToPoint;
	private float distance = 0.0f;
	private int step = 0;
	
	private bool useMecanim = false;
	private Animator animator;
	
	private float wait = 0;
	
	void Start(){
		if(waypoints.Length > 1){
			foreach (Transform go in waypoints) {
				go.parent = null;
			}
		}
		ai	=	GetComponent<AIsetC>();
		mainModel = GetComponent<AIsetC>().mainModel;
		useMecanim = GetComponent<AIsetC>().useMecanim;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		movingAnimation = ai.movingAnimation;
		idleAnimation = ai.idleAnimation;
		if(useMecanim){
			animator = ai.animator;
			if(!animator){
				animator = mainModel.GetComponent<Animator>();
			}
		}
	}
	
	void Update (){
		if(GlobalConditionC.freezeAll){
			return;
		}
		if(ai.followState == AIsetC.AIState.Idle){
			if(state >= 1){//Moving
				CharacterController controller = GetComponent<CharacterController>();
				Vector3 forward = transform.TransformDirection(Vector3.forward);
				controller.Move(forward * speed * Time.deltaTime);
			}
			//----------------------------
			if(wait >= idleDuration && state == 0){
				//Set to Moving Mode.
				if(waypoints.Length > 1){
					//Set to Moving Mode.
					if(randomWaypoints){
						RandomWaypoint();
					}else{
						WaypointStep();
					}
				}else{
					//Set to Moving Mode.
					RandomTurning();
				}
			}
			if(wait >= moveDuration && state == 1){
				//Set to Idle Mode.
				if(idleAnimation && !useMecanim){
					//For Legacy Animation
					mainModel.GetComponent<Animation>().CrossFade(idleAnimation.name, 0.2f);
				}else if(useMecanim){
					//For Mecanim Animation
					animator.SetBool("run" , false);
				}
				wait = 0;
				state = 0;
			}
			//----------------------------------------
			if(state == 2){
				Vector3 destination = headToPoint.position;
				destination.y = transform.position.y;
				transform.LookAt(destination);
				
				distance = (transform.position - GetDestination()).magnitude;
				if (distance <= 0.2) {
					//Set to Idle Mode.
					if(idleAnimation){
						mainModel.GetComponent<Animation>().CrossFade(idleAnimation.name, 0.2f);
					}
					wait = 0;
					state = 0;
				}
				
			}
			wait += Time.deltaTime;
			//-----------------------------
		}
		
	}
	
	void RandomTurning(){
		float dir = Random.Range(0 , 360);
		transform.eulerAngles = new Vector3(transform.eulerAngles.x , dir , transform.eulerAngles.z);
		if(movingAnimation && !useMecanim){
			//For Legacy Animation
			mainModel.GetComponent<Animation>().CrossFade(movingAnimation.name, 0.2f);
		}else if(useMecanim){
			//For Mecanim Animation
			animator.SetBool("run" , true);
		}
		wait = 0; // Reset wait time.
		state = 1; // Change State to Move.
		
	}

	void RandomWaypoint(){
		headToPoint = waypoints[Random.Range(0, waypoints.Length)];
		if(movingAnimation && !useMecanim){
			//For Legacy Animation
			mainModel.GetComponent<Animation>().CrossFade(movingAnimation.name, 0.2f);
		}else if(useMecanim){
			//For Mecanim Animation
			animator.SetBool("run" , true);
		}
		
		wait = 0; // Reset wait time.
		state = 2; // Change State to Move.
	}
	
	void WaypointStep(){
		headToPoint = waypoints[step];
		if(movingAnimation && !useMecanim){
			//For Legacy Animation
			mainModel.GetComponent<Animation>().CrossFade(movingAnimation.name, 0.2f);
		}else if(useMecanim){
			//For Mecanim Animation
			animator.SetBool("run" , true);
		}
		
		wait = 0; // Reset wait time.
		state = 2; // Change State to Move.
		
		if(step >= waypoints.Length -1){
			step = 0;
		}else{
			step++;
		}
	}
	
	Vector3 GetDestination(){
		Vector3 destination = headToPoint.position;
		destination.y = transform.position.y;
		return destination;
	}
}
