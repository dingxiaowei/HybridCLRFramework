using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof (CharacterMotorC))]

public class PlayerInputControllerC : MonoBehaviour {

	public bool unableToMove = false;
	private GameObject mainModel;
	public float walkSpeed = 6.0f;
	public float sprintSpeed = 12.0f;
	public bool canSprint = true;
	private bool sprint = false;
	[HideInInspector]
	public bool recover = false;
	private float staminaRecover = 1.4f;
	private float useStamina = 0.04f;
	[HideInInspector]
		public bool dodging = false;
	
	public Texture2D staminaGauge;
	public Texture2D staminaBorder;
	
	public float maxStamina = 100.0f;
	public float stamina = 100.0f;
	
	private float lastTime = 0.0f;
	[HideInInspector]
	public float recoverStamina = 0.0f;
	private Vector3 dir = Vector3.forward;

	public bool doubleJump = false;
	private bool airJump = false;
	private bool airMove = false;
	
	private bool useMecanim = true;
	private bool mobileMode = false;
	[HideInInspector]
	public bool mobileJumping = false;

	[System.Serializable]
	public class DodgeSetting{
		public bool canDodgeRoll = false;
		public int staminaUse = 25;
		
		public AnimationClip dodgeForward;
		public AnimationClip dodgeLeft;
		public AnimationClip dodgeRight;
		public AnimationClip dodgeBack;
	}
	public DodgeSetting dodgeRollSetting;
	public FallDamage fallingDamage;
	
	private CharacterMotorC motor;
	private CharacterController controller;

	public JoystickCanvas joyStick;// For Mobile
	private float moveHorizontal;
	private float moveVertical;

	[System.Serializable]
	public class CanvasObj{
		public bool useCanvas = false;
		public GameObject staminaBorder;
		public Image staminaBar;
	}
	public CanvasObj canvasElement;

	// Use this for initialization
	void Start(){
		motor = GetComponent<CharacterMotorC>();
		controller = GetComponent<CharacterController>();
		stamina = maxStamina;
		if(!mainModel){
			mainModel = GetComponent<StatusC>().mainModel;
		}
		useMecanim = GetComponent<AttackTriggerC>().useMecanim;
		mobileMode = GetComponent<AttackTriggerC>().mobileMode;
	}
	
	// Update is called once per frame
	void Update(){
		StatusC stat = GetComponent<StatusC>();
		if(recover && !sprint && !dodging){
			if(recoverStamina >= staminaRecover){
				StaminaRecovery();
			}else{
				recoverStamina += Time.deltaTime;
			}
		}
		if(sprint || recover || dodging){
			if(canvasElement.useCanvas && canvasElement.staminaBar){
				if(!canvasElement.staminaBorder.activeSelf){
					canvasElement.staminaBorder.SetActive(true);
				}
				float curSt = stamina/maxStamina;
				canvasElement.staminaBar.fillAmount = curSt;
			}
		}
		if(stamina >= maxStamina && canvasElement.useCanvas && canvasElement.staminaBar || GlobalConditionC.freezeAll && canvasElement.useCanvas){
			if(canvasElement.staminaBorder.activeSelf){
				canvasElement.staminaBorder.SetActive(false);
			}
		}

		if(stat.freeze || GlobalConditionC.freezeAll || GlobalConditionC.freezePlayer || !stat.canControl){
			motor.inputMoveDirection = new Vector3(0,0,0);
			if(sprint){
				sprint = false;
				recover = true;
				motor.movement.maxForwardSpeed = walkSpeed;
				motor.movement.maxSidewaysSpeed = walkSpeed;
				recoverStamina = 0.0f;
			}
			return;
		}
		if(Time.timeScale == 0.0f){
			return;
		}
		if(dodging && !unableToMove){
			Vector3 fwd = transform.TransformDirection(dir);
			controller.Move(fwd * 8 * Time.deltaTime);
			return;
		}
		
		if(dodgeRollSetting.canDodgeRoll){
			//Dodge Forward
			if(Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") > 0 && (controller.collisionFlags & CollisionFlags.Below) != 0 && Input.GetAxis("Horizontal") == 0){
				if(Input.GetButtonDown ("Vertical") && (Time.time - lastTime) < 0.4f && Input.GetButtonDown ("Vertical") && (Time.time - lastTime) > 0.1f && Input.GetAxis("Vertical") > 0.03f){
					lastTime = Time.time;
					dir = Vector3.forward;
					StartCoroutine(DodgeRoll(dodgeRollSetting.dodgeForward));
				}else
					lastTime = Time.time;
			}
			//Dodge Backward
			if(Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") < 0 && (controller.collisionFlags & CollisionFlags.Below) != 0 && Input.GetAxis("Horizontal") == 0){
				if(Input.GetButtonDown ("Vertical") && (Time.time - lastTime) < 0.4f && Input.GetButtonDown ("Vertical") && (Time.time - lastTime) > 0.1f && Input.GetAxis("Vertical") < -0.03f){
					lastTime = Time.time;
					dir = Vector3.back;
					StartCoroutine(DodgeRoll(dodgeRollSetting.dodgeBack));
				}else
					lastTime = Time.time;
			}
			//Dodge Left
			if(Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") < 0 && (controller.collisionFlags & CollisionFlags.Below) != 0 && !Input.GetButton("Vertical")){
				if(Input.GetButtonDown ("Horizontal") && (Time.time - lastTime) < 0.3f && Input.GetButtonDown ("Horizontal") && (Time.time - lastTime) > 0.15f && Input.GetAxis("Horizontal") < -0.03f){
					lastTime = Time.time;
					dir = Vector3.left;
					StartCoroutine(DodgeRoll(dodgeRollSetting.dodgeLeft));
				}else
					lastTime = Time.time;
			}
			//Dodge Right
			if(Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") > 0 && (controller.collisionFlags & CollisionFlags.Below) != 0 && !Input.GetButton("Vertical")){
				if(Input.GetButtonDown ("Horizontal") && (Time.time - lastTime) < 0.3f && Input.GetButtonDown ("Horizontal") && (Time.time - lastTime) > 0.15f && Input.GetAxis("Horizontal") > 0.03f){
					lastTime = Time.time;
					dir = Vector3.right;
					StartCoroutine(DodgeRoll(dodgeRollSetting.dodgeRight));
				}else
					lastTime = Time.time;
			}
		}
		
		//Cancel Sprint
		if(sprint && Input.GetAxis("Vertical") < 0.02f || sprint && stamina <= 0 || sprint && Input.GetButtonDown("Fire1") || sprint && Input.GetKeyUp(KeyCode.LeftShift)){
			sprint = false;
			recover = true;
			motor.movement.maxForwardSpeed = walkSpeed;
			motor.movement.maxSidewaysSpeed = walkSpeed;
			recoverStamina = 0.0f;
		}
		if(fallingDamage.enable){
			//if(!controller.isGrounded){
			if(!motor.grounded){
				airTime += Time.deltaTime;
			}else{
				yPos = transform.position.y;
			}
		}

		if(airJump){
			//Double Jump
			Vector3 aj = transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal") , 2 , Input.GetAxis("Vertical")));
			controller.Move(aj * 4 * Time.deltaTime);
			return;
		}
		//Double Jump
		if(Input.GetButtonDown("Jump") && !motor.grounded && doubleJump || Input.GetButtonDown("Jump") && !motor.grounded && stat.hiddenStatus.doubleJump){
			StartCoroutine(DoubleJumping());
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

		if(motor.grounded){
			if(Input.GetButton("Jump") && !stat.freeze){
				if(fallingDamage.enable){
					airTime = -0.75f;
				}
			}
		}

		//Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		if(!unableToMove){
			Vector3 directionVector = new Vector3(moveHorizontal, 0, moveVertical);

			if(directionVector != Vector3.zero) {
				float directionLength = directionVector.magnitude;
				directionVector = directionVector / directionLength;

				directionLength = Mathf.Min(1, directionLength);

				directionLength = directionLength * directionLength;
				directionVector = directionVector * directionLength;
			}

			// Apply the direction to the CharacterMotor
			motor.inputMoveDirection = transform.rotation * directionVector;
		}

		if(!mobileMode){
			motor.inputJump = Input.GetButton("Jump");
		}else{
			motor.inputJump = mobileJumping;
		}
		
		if(sprint){
			motor.movement.maxForwardSpeed = sprintSpeed;
			motor.movement.maxSidewaysSpeed = sprintSpeed;
			return;
		}
		//Activate Sprint
		if(Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0 && (controller.collisionFlags & CollisionFlags.Below) != 0 && canSprint && stamina > 0){
			sprint = true;
			StartCoroutine(Dasher());
		}
	}
	
	void OnGUI(){
		if(canvasElement.useCanvas){
			return;
		}
		if(sprint || recover || dodging){
			float staminaPercent = stamina * 100 / maxStamina *3;
			//GUI.DrawTexture ( new Rect((Screen.width /2) -150,Screen.height - 120,stamina *3,10), staminaGauge);
			GUI.DrawTexture ( new Rect((Screen.width /2) -150,Screen.height - 120, staminaPercent ,10), staminaGauge);
			GUI.DrawTexture ( new Rect((Screen.width /2) -153,Screen.height - 123, 306 ,16), staminaBorder);
		}
		
	}

	public void MobileJump(){
		mobileJumping = true;
	}
	public void MobileJumpRelease(){
		mobileJumping = false;
	}


	IEnumerator DoubleJumping(){
		if(!airMove){
			airMove = true;
			airJump = true;
			//Double Jump Animation
			if(!useMecanim){
				GetComponent<PlayerAnimationC>().DoubleJumpAnimation();
			}
			motor.freezeGravity = true;
			yield return new WaitForSeconds(0.25f);
			motor.freezeGravity = false;
			airJump = false;
		}
	}
	
	void OnControllerColliderHit(ControllerColliderHit col){
		CharacterController controller = GetComponent<CharacterController>();
		if(airMove && (controller.collisionFlags & CollisionFlags.Below) != 0){
			airMove = false;
			motor.freezeGravity = false;
		}
	}
	
	IEnumerator Dasher(){
		while (sprint){
			yield return new WaitForSeconds(useStamina);
			if(stamina > 0){
				stamina -= 1;
			}else{
				stamina = 0;
			}
		}
	}
	
	void StaminaRecovery(){
		stamina += 1;
		if(stamina >= maxStamina){
			stamina = maxStamina;
			recoverStamina = 0.0f;
			recover = false;
		}else{
			recoverStamina = staminaRecover - 0.02f;
		}
	}
	
	IEnumerator DodgeRoll(AnimationClip anim){
		if(stamina >= 25 && !dodging && motor.canControl){
			if(!useMecanim){
				//For Legacy Animation
				mainModel.GetComponent<Animation>()[anim.name].layer = 18;
				mainModel.GetComponent<Animation>().PlayQueued(anim.name, QueueMode.PlayNow);
			}else{
				//For Mecanim Animation
				if(GetComponent<PlayerMecanimAnimationC>()){
					GetComponent<PlayerMecanimAnimationC>().AttackAnimation(anim.name);
				}
			}
			dodging = true;
			stamina -= dodgeRollSetting.staminaUse;
			GetComponent<StatusC>().dodge = true;
			motor.canControl = false;
			yield return new WaitForSeconds(0.5f);
			GetComponent<StatusC>().dodge = false;
			recover = true;
			motor.canControl = true;
			dodging = false;
			recoverStamina = 0.0f;
		}
	}

	private float airTime = 0;
	private float yPos = 0;
	public void OnLand(){
		if(!fallingDamage.enable){
			return;
		}
		//print(airTime);
		StatusC stat = GetComponent<StatusC>();
		if(airTime > fallingDamage.minSurviveFall && transform.position.y < yPos - fallingDamage.surviveHeight){
			float df = fallingDamage.minSurviveFall / 2;
			float aa = airTime - df;
			float dmg = (float)fallingDamage.damageForSeconds * aa;
			stat.FallingDamage((int)dmg);
			if(fallingDamage.hitEffect){
				Instantiate(fallingDamage.hitEffect , transform.position , fallingDamage.hitEffect.rotation);
			}
		}
		airTime = 0;
	}
}

[System.Serializable]
public class FallDamage{
	public bool enable = false;
	public float minSurviveFall = 0.45f;
	public int damageForSeconds = 30;
	private CharacterController controller;
	public float surviveHeight = 0.3f;
	public Transform hitEffect;
}
