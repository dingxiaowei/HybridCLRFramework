using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterMotorC))]
public class MountControllerC : MonoBehaviour {

	public GameObject mainModel;
	public float runSpeed = 9;
	public Transform getOnPosition;
	public Transform getOffPosition;
	public Transform mountPoint;
	public bool useMecanim = false;
	private bool  playerMecanim = false;
	public AudioClip walkingSound;
	
	public MountLegacyC legacyAnimationSet;
	public Animator mecanimAnimator;
	
	private bool onRiding = false;
	private Transform player;
	private CharacterMotorC motor;
	private bool freeze = false;
	private float moveHorizontal = 0;
	private float moveVertical = 0;
	private bool onGetOff = false;
	public bool cameraChangeTarget = true;
	
	void Start(){
		DontDestroyOnLoad(transform.gameObject);
		if(!mainModel){
			mainModel = this.gameObject;
		}
		if(!mecanimAnimator && useMecanim){
			mecanimAnimator = mainModel.GetComponent<Animator>();
		}
		motor = GetComponent<CharacterMotorC>();
		
		motor.movement.maxForwardSpeed = runSpeed;
		motor.movement.maxSidewaysSpeed = runSpeed;
	}
	
	void Update(){
		if(onGetOff && getOffPosition){
			PlayerMoveTowardsTarget(getOffPosition.position);
		}
		if(onRiding && !player){
			GlobalConditionC.freezePlayer = false;
			onRiding = false;
			if(!useMecanim){
				mainModel.GetComponent<Animation>().CrossFade(legacyAnimationSet.idleAnimation.name);
			}else{
				mecanimAnimator.SetBool("run" , false);
			}
		}
		if(GlobalConditionC.freezeAll || freeze){
			motor.inputMoveDirection = Vector3.zero;
			return;
		}
		if(Time.timeScale == 0.0f){
			return;
		}
		
		if(Input.GetKeyDown("f") && onRiding || Input.GetButtonDown("Jump") && onRiding || Input.GetKeyDown("e") && onRiding){
			GetOff();
		}
		
		if(onRiding){
			if(Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0){
				moveHorizontal = Input.GetAxis("Horizontal");
				moveVertical = Input.GetAxis("Vertical");
			}else{
				moveHorizontal = 0;
				moveVertical = 0;
			}
			
			Transform cameraTransform = Camera.main.transform;
			Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
			forward.y = 0;
			forward = forward.normalized;
			Vector3 right = new Vector3(forward.z, 0, -forward.x);
			Vector3 targetDirection= moveHorizontal * right + moveVertical * forward;
			
			//----------------------------------
			if(moveHorizontal != 0 || moveVertical != 0){
				transform.rotation = Quaternion.LookRotation(targetDirection.normalized);
			}
			//-----------------------------------------------------------------------------
			if(moveVertical != 0 && walkingSound && !GetComponent<AudioSource>().isPlaying|| moveHorizontal != 0 && walkingSound && !GetComponent<AudioSource>().isPlaying){
				GetComponent<AudioSource>().clip = walkingSound;
				GetComponent<AudioSource>().Play();
			}
			
			motor.inputMoveDirection = targetDirection.normalized;
			
			if(!useMecanim){
				//Play Legacy Animation
				if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0){
					mainModel.GetComponent<Animation>().CrossFade(legacyAnimationSet.movingAnimation.name);
				}else{
					mainModel.GetComponent<Animation>().CrossFade(legacyAnimationSet.idleAnimation.name);
				}
			}else{
				//Play Mecanim Animation
				if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0){
					mecanimAnimator.SetBool("run" , true);
				}else{
					mecanimAnimator.SetBool("run" , false);
				}
			}
		}
	}
	
	public void GetOn(){
		if(onRiding){
			return;
		}
		player = GlobalConditionC.mainPlayer.transform;
		playerMecanim = player.GetComponent<AttackTriggerC>().useMecanim;
		onRiding = true;
		Physics.IgnoreCollision(GetComponent<Collider>(), player.GetComponent<Collider>());
		player.position = getOnPosition.position;
		player.rotation = getOnPosition.rotation;
		if(!mountPoint){
			mountPoint = this.transform;
		}
		player.parent = mountPoint;
		GlobalConditionC.freezePlayer = true;
		player.GetComponent<CharacterMotorC>().enabled = false;
		freeze = true;
		if(player.GetComponent<AttackTriggerC>())
			player.GetComponent<AttackTriggerC>().GetActivator(this.gameObject , "GetOff" , "Get Off");
		StartCoroutine(GetOnAnim());
	}

	IEnumerator GetOnAnim(){
		GameObject mm = player.GetComponent<AttackTriggerC>().mainModel;
		//Play Animation
		if(!playerMecanim){
			//Player Use Legacy
			mm.GetComponent<Animation>()[legacyAnimationSet.getOnAnimation.name].layer = 2;
			mm.GetComponent<Animation>().Play(legacyAnimationSet.getOnAnimation.name);
			yield return new WaitForSeconds(mm.GetComponent<Animation>()[legacyAnimationSet.getOnAnimation.name].length);
			mm.GetComponent<Animation>().Stop(legacyAnimationSet.getOnAnimation.name);
			mm.GetComponent<Animation>()[legacyAnimationSet.ridingAnimation.name].layer = 2;
			mm.GetComponent<Animation>().Play(legacyAnimationSet.ridingAnimation.name);
		}else{
			//Player Use Mecanim
			Animator anim = mm.GetComponent<Animator>();
			anim.SetTrigger("riding");
			yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);
		}
		if(cameraChangeTarget){
			SetCameraMode();
			Camera.main.GetComponent<ARPGcameraC>().target = this.transform;
		}
		freeze = false;
	}
	
	public void GetOff(){
		onRiding = false;
		//Set Animation to Idle when player get off.
		if(!useMecanim){
			mainModel.GetComponent<Animation>().CrossFade(legacyAnimationSet.idleAnimation.name);
		}else{
			mecanimAnimator.SetBool("run" , false);
		}
		
		player.GetComponent<CharacterMotorC>().enabled = true;
		freeze = true;
		if(player.GetComponent<AttackTriggerC>())
			player.GetComponent<AttackTriggerC>().RemoveActivator(this.gameObject);
		StartCoroutine(GetOffAnim());
	}

	IEnumerator GetOffAnim(){
		GameObject mm = player.GetComponent<AttackTriggerC>().mainModel;
		//Play Animation
		if(!playerMecanim){
			//Player Use Legacy
			mm.GetComponent<Animation>().Stop(legacyAnimationSet.ridingAnimation.name);
			mm.GetComponent<Animation>()[legacyAnimationSet.getOffAnimation.name].layer = 2;
			mm.GetComponent<Animation>().Play(legacyAnimationSet.getOffAnimation.name);
			yield return new WaitForSeconds(mm.GetComponent<Animation>()[legacyAnimationSet.getOffAnimation.name].length);
			mm.GetComponent<Animation>().Stop(legacyAnimationSet.getOffAnimation.name);
		}else{
			//Player Use Mecanim
			Animator anim = mm.GetComponent<Animator>();
			anim.SetTrigger("getoff");
			yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);
		}
		player.parent = null;
		//player.position = getOffPosition.position;
		onGetOff = true;
		yield return new WaitForSeconds(0.12f);
		onGetOff = false;
		Physics.IgnoreCollision(GetComponent<Collider>(), player.GetComponent<Collider>() , false);
		
		if(cameraChangeTarget){
			SetCameraMode();
			Camera.main.GetComponent<ARPGcameraC>().target = player;
		}
		freeze = false;
		GlobalConditionC.freezePlayer = false;
	}

	void Deactivate(){
		//This function use when player Load on Quit Game while riding.
		onRiding = false;
		player.GetComponent<CharacterMotorC>().enabled = true;
		player.parent = null;
		if(cameraChangeTarget){
			SetCameraMode();
			Camera.main.GetComponent<ARPGcameraC>().target = player;
		}
		
		GameObject mm = player.GetComponent<AttackTriggerC>().mainModel;
		if(!playerMecanim){
			//Player Use Legacy
			mm.GetComponent<Animation>().Stop(legacyAnimationSet.ridingAnimation.name);
		}else{
			//Player Use Mecanim
			Animator anim = mm.GetComponent<Animator>();
			anim.SetTrigger("getoff");
		}
		freeze = false;
		GlobalConditionC.freezePlayer = false;
		Destroy(gameObject);
	}
	
	void PlayerMoveTowardsTarget(Vector3 targetPoint){
		CharacterController cc = player.GetComponent<CharacterController>();
		Vector3 offset = targetPoint - player.transform.position;
		
		if(offset.magnitude > 0.1f) {
			offset = offset.normalized * 15;
			cc.Move(offset * Time.deltaTime);
		}
	}
	
	void DestroySelf(){
		if(!onRiding){
			Destroy(gameObject);
		}
	}
	
	void SetCameraMode(){
		//Set Camera Mode to Normal
		if(!player.GetComponent<SwitchAimModeC>()){
			return;
		}
		player.GetComponent<AttackTriggerC>().aimingType = AimType.Normal;
		Camera.main.GetComponent<ARPGcameraC>().target = this.transform;
		//Camera.main.GetComponent<ARPGcameraC>().targetHeight = player.GetComponent<SwitchAimModeC>().targetHeightNormal;
		Camera.main.GetComponent<ARPGcameraC>().lockOn = false;
	}
}

[System.Serializable]
public class MountLegacyC{
	public AnimationClip idleAnimation;
	public AnimationClip movingAnimation;
	
	public AnimationClip getOnAnimation;
	public AnimationClip getOffAnimation;
	public AnimationClip ridingAnimation;
}