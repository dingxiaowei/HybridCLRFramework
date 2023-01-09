using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AttackTriggerC))]
[RequireComponent(typeof(PlayerInputControllerC))]
[AddComponentMenu("Action-RPG Kit(C#)/Create Player(Legacy)")]

public class PlayerAnimationC : MonoBehaviour {
	
	public float runMaxAnimationSpeed = 1.0f;
	public float backMaxAnimationSpeed = 1.0f;
	public float sprintAnimationSpeed = 1.5f;
	
	private GameObject player;
	private GameObject mainModel;
	
	//string idle = "idle";
	public AnimationClip idle;
	public AnimationClip run;
	public AnimationClip right;
	public AnimationClip left;
	public AnimationClip back;
	public AnimationClip jump;
	public AnimationClip jumpUp;
	public AnimationClip hurt;
	public AnimationClip doubleJump;

	public JoystickCanvas joyStick;// For Mobile
	private float moveHorizontal;
	private float moveVertical;
	
	void Start(){
		if(!player){
			player = this.gameObject;
		}
		mainModel = GetComponent<AttackTriggerC>().mainModel;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		GetComponent<AttackTriggerC>().useMecanim = false;
		mainModel.GetComponent<Animation>()[run.name].speed = runMaxAnimationSpeed;
		mainModel.GetComponent<Animation>()[right.name].speed = runMaxAnimationSpeed;
		mainModel.GetComponent<Animation>()[left.name].speed = runMaxAnimationSpeed;
		mainModel.GetComponent<Animation>()[back.name].speed = backMaxAnimationSpeed;
		
		//mainModel.GetComponent<Animation>()[jump.name].wrapMode  = WrapMode.ClampForever;
		
		if(hurt){
			mainModel.GetComponent<Animation>()[hurt.name].layer = 5;
		}
	}
	
	void Update (){
		if(Time.timeScale == 0.0f){
			return;
		}
		if(GlobalConditionC.freezeAll || GlobalConditionC.freezePlayer){
			mainModel.GetComponent<Animation>().CrossFade(idle.name);
			return;
		}
		CharacterController controller = player.GetComponent<CharacterController>();

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

		if ((controller.collisionFlags & CollisionFlags.Below) != 0){
			if(moveHorizontal > 0.1f)
				mainModel.GetComponent<Animation>().CrossFade(right.name);
			else if(moveHorizontal < -0.1f)
				mainModel.GetComponent<Animation>().CrossFade(left.name);
			else if(moveVertical > 0.1f)
				mainModel.GetComponent<Animation>().CrossFade(run.name);
			else if(moveVertical < -0.1f)
				mainModel.GetComponent<Animation>().CrossFade(back.name);
			else
				mainModel.GetComponent<Animation>().CrossFade(idle.name);

			if(Input.GetButtonDown("Jump") && jumpUp){
				mainModel.GetComponent<Animation>()[jumpUp.name].layer = 2;
				mainModel.GetComponent<Animation>().PlayQueued(jumpUp.name , QueueMode.PlayNow);
			}
		}else{
			mainModel.GetComponent<Animation>().CrossFade(jump.name);
		}
	}
	
	public void AnimationSpeedSet(){
		mainModel = GetComponent<AttackTriggerC>().mainModel;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		mainModel.GetComponent<Animation>()[run.name].speed = runMaxAnimationSpeed;
		mainModel.GetComponent<Animation>()[right.name].speed = runMaxAnimationSpeed;
		mainModel.GetComponent<Animation>()[left.name].speed = runMaxAnimationSpeed;
		mainModel.GetComponent<Animation>()[back.name].speed = backMaxAnimationSpeed;
	}

	public void DoubleJumpAnimation(){
		if(!doubleJump){
			return;
		}
		mainModel.GetComponent<Animation>()[doubleJump.name].layer = 2;
		mainModel.GetComponent<Animation>().PlayQueued(doubleJump.name , QueueMode.PlayNow);
	}
}