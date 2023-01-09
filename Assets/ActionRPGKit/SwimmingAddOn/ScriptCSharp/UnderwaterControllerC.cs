using UnityEngine;
using System.Collections;

[RequireComponent (typeof (PlayerInputControllerC))]

public class UnderwaterControllerC : MonoBehaviour {
	private CharacterMotorC motor;
	public float swimSpeed = 5.0f;
	private GameObject mainModel;
	private GameObject mainCam;
	
	//Animation
	public AnimationClip swimIdle;
	public AnimationClip swimForward;
	public AnimationClip swimRight;
	public AnimationClip swimLeft;
	public AnimationClip swimBack;
	
	public float animationSpeed = 1.0f;
	[HideInInspector]
	public float surfaceExit = 0.0f;

	private bool useMecanim = false;
	private Animator animator;
	private string moveHorizontalState = "horizontal";
	private string moveVerticalState = "vertical";
	private string jumpState = "jump";

	public JoystickCanvas joyStick;// For Mobile
	private float moveHorizontal;
	private float moveVertical;
	private bool mobileJump = false;
	
	void Start(){
		motor = GetComponent<CharacterMotorC>();
		useMecanim = GetComponent<AttackTriggerC>().useMecanim;
		
		mainModel = GetComponent<AttackTriggerC>().mainModel;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		if(GetComponent<PlayerInputControllerC>()){
			if(GetComponent<PlayerInputControllerC>().joyStick){
				joyStick = GetComponent<PlayerInputControllerC>().joyStick;
			}
		}

		mainCam = AttackTriggerC.Maincam.gameObject;
		if(!useMecanim){
			//If using Legacy Animation
			mainModel.GetComponent<Animation>()[swimForward.name].speed = animationSpeed;
			mainModel.GetComponent<Animation>()[swimRight.name].speed = animationSpeed;
			mainModel.GetComponent<Animation>()[swimLeft.name].speed = animationSpeed;
			mainModel.GetComponent<Animation>()[swimBack.name].speed = animationSpeed;
		}else{
			//If using Mecanim Animation
			animator = GetComponent<PlayerMecanimAnimationC>().animator;
			moveHorizontalState = GetComponent<PlayerMecanimAnimationC>().moveHorizontalState;
			moveVerticalState = GetComponent<PlayerMecanimAnimationC>().moveVerticalState;
			jumpState = GetComponent<PlayerMecanimAnimationC>().jumpState;
		}
	}
	
	void Update(){
		StatusC stat = GetComponent<StatusC>();
		if(stat.freeze){
			motor.inputMoveDirection = new Vector3(0,0,0);
			return;
		}
		if(Time.timeScale == 0.0f){
	        	return;
	    }
	    
	    CharacterController controller = GetComponent<CharacterController>();
		float swimUp;

		if(GetComponent<PlayerInputControllerC>()){
			mobileJump = GetComponent<PlayerInputControllerC>().mobileJumping;
		}
		// Get the input vector from kayboard or analog stick
		if(Input.GetButton("Jump") || mobileJump){
			swimUp = 2.0f;
		}else{
			swimUp = 0.0f;
		}
		if(joyStick){
			if(Input.GetButton("Horizontal") || Input.GetButton("Vertical")){
				moveHorizontal = Input.GetAxis("Horizontal");
				moveVertical = Input.GetAxis("Vertical");
			}else{
				moveHorizontal = joyStick.position.x;
				moveVertical = joyStick.position.y;
			}
		}else{
			moveHorizontal = Input.GetAxis("Horizontal");
			moveVertical = Input.GetAxis("Vertical");
		}

		Vector3 directionVector = new Vector3(moveHorizontal, swimUp, moveVertical);
		
		if (directionVector != Vector3.zero) {
			float directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
	
			directionLength = Mathf.Min(1, directionLength);
			directionLength = directionLength * directionLength;
			directionVector = directionVector * directionLength;
		}
		
		if(moveVertical != 0 && transform.position.y < surfaceExit ||  transform.position.y >= surfaceExit && moveVertical > 0 && mainCam.transform.eulerAngles.x >= 25 && mainCam.transform.eulerAngles.x <= 180){
       		transform.rotation = mainCam.transform.rotation;
       }
		//motor.inputMoveDirection = transform.rotation * directionVector;
		controller.Move(transform.rotation * directionVector * swimSpeed * Time.deltaTime);
		
		    //-------------Animation----------------
		if(!useMecanim){
			//If using Legacy Animation
			if(moveHorizontal > 0.1)
				mainModel.GetComponent<Animation>().CrossFade(swimRight.name);
			else if (moveHorizontal < -0.1)
				mainModel.GetComponent<Animation>().CrossFade(swimLeft.name);
			else if (moveVertical > 0.1)
				mainModel.GetComponent<Animation>().CrossFade(swimForward.name);
			else if (moveVertical < -0.1)
				mainModel.GetComponent<Animation>().CrossFade(swimBack.name);
			else
				mainModel.GetComponent<Animation>().CrossFade(swimIdle.name);
			//----------------------------------------
		}else{
			//If using Mecanim Animation
			float h = moveHorizontal;
			float v = moveVertical;
			animator.SetFloat(moveHorizontalState , h);
			animator.SetFloat(moveVerticalState , v);
		}
	    //-------------------------------------------
	}

	public void MecanimEnterWater(){
		useMecanim = GetComponent<AttackTriggerC>().useMecanim;
		animator = GetComponent<PlayerMecanimAnimationC>().animator;
		animator.SetBool(jumpState , false);
		animator.SetBool("swimming" , true);
		//animator.Play(swimIdle.name);
		animator.SetTrigger("enterWater");
	}
	public void MecanimExitWater(){
		animator.SetBool("swimming" , false);
		animator.SetBool(jumpState , true);
		animator.Play(jumpState);
	}

}
