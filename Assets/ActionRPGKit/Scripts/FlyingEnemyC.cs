using UnityEngine;
using System.Collections;

public class FlyingEnemyC : MonoBehaviour {
	
	private int flying = 0; //0 = Idle , 1 = Flying Down , 2 = Fly Up
	private bool  onGround = false;
	private Transform target;
	private float distance = 0.0f;
	public float flyDownRange = 5.5f;
	public float flyUpRange = 8.5f;
	public float flyingSpeed = 8.0f;
	public float flyUpHeight = 7.0f;
	public float landingDelay = 0.4f;
	private float currentHeight = 0.0f;
	
	public AnimationClip flyDownAnimation;
	public AnimationClip flyUpAnimation;
	public AnimationClip landingAnimation;
	private GameObject mainModel;
	
	private bool  useMecanim = false;
	private Animator animator; //For Mecanim
	private AIsetC ai;
	
	void  Start (){
		ai	=	GetComponent<AIsetC>();
		mainModel = GetComponent<AIsetC>().mainModel;
		GetComponent<CharacterMotorC>().enabled = false;
		useMecanim = GetComponent<AIsetC>().useMecanim;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		//-------Check for Mecanim Animator-----------
		if(useMecanim){
			animator = ai.animator;
			if(!animator){
				animator = mainModel.GetComponent<Animator>();
			}
		}
	}
	
	Vector3 GetDestination (){
		Vector3 destination = target.position;
		destination.y = transform.position.y;
		return destination;
	}
	
	void  Update (){
		if(!target && GetComponent<AIsetC>().followTarget){
			target = GetComponent<AIsetC>().followTarget;
		}
		if(!target){
			return;
		}
		CharacterController controller = GetComponent<CharacterController>();
		if(flying == 1){
			if(!useMecanim && flyDownAnimation){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().CrossFade(flyDownAnimation.name, 0.2f);
			}else{
				animator.SetBool("flyDown" , true);
			}
			Vector3 direction = transform.TransformDirection(Vector3.down);
			controller.Move(direction * flyingSpeed * Time.deltaTime);
			return;
		}
		if(flying == 2){
			if(!useMecanim && flyUpAnimation){
				//If using Legacy Animation
				mainModel.GetComponent<Animation>().CrossFade(flyUpAnimation.name, 0.2f);
			}else{
				animator.SetBool("flyUp" , true);
			}
			Vector3 direction = transform.TransformDirection(Vector3.up);
			controller.Move(direction * flyingSpeed * Time.deltaTime);
			
			if(transform.position.y >= currentHeight + flyUpHeight){
				ai.freeze = false;
				flying = 0;
				//onGround = false;
			}
			return;
		}
		
		distance = (transform.position - GetDestination()).magnitude;
		if (distance <= flyDownRange && !onGround) {
			FlyDown();
		}
		if (distance >= flyUpRange && onGround) {
			FlyUp();
		}
		
	}
	
	void  FlyDown (){
		ai.freeze = true;
		flying = 1;
	}
	
	void  FlyUp (){
		onGround = false;
		GetComponent<CharacterMotorC>().enabled = false;
		currentHeight = transform.position.y;
		ai.freeze = true;
		flying = 2;
	}
	
	void  OnControllerColliderHit ( ControllerColliderHit hit  ){
		if(flying == 1){
			//Landing();
			StartCoroutine(Landing());
		}
	}
	
	IEnumerator  Landing (){
		GetComponent<CharacterMotorC>().enabled = true;
		if(landingAnimation && !useMecanim){
			//For Legacy Animation
			mainModel.GetComponent<Animation>().Play(landingAnimation.name);
		}else if(useMecanim){
			//For Mecanim Animation
			animator.Play(landingAnimation.name);
		}
		yield return new WaitForSeconds(landingDelay);
		ai.freeze = false;
		flying = 0;
		onGround = true;
	}

		
}