using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AttackTriggerC))]
[RequireComponent(typeof(PlayerInputControllerC))]
[AddComponentMenu("Action-RPG Kit(C#)/Create Player(Mecanim)")]

public class PlayerMecanimAnimationC : MonoBehaviour {
	
	private GameObject player;
	private GameObject mainModel;
	public Animator animator;
	private CharacterController controller;
	
	public string moveHorizontalState = "horizontal";
	public string moveVerticalState = "vertical";
	public string jumpState = "jump";
	private bool jumping = false;
	private bool attacking = false;
	private bool flinch = false;

	public JoystickCanvas joyStick;// For Mobile
	private float moveHorizontal;
	private float moveVertical;
	private StatusC stat;
	
	void Start(){
		if(!player){
			player = this.gameObject;
		}
		mainModel = GetComponent<AttackTriggerC>().mainModel;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		if(!animator){
			animator = mainModel.GetComponent<Animator>();
		}
		controller = player.GetComponent<CharacterController>();
		GetComponent<AttackTriggerC>().useMecanim = true;
		stat = GetComponent<StatusC>();
	}
	
	void Update (){
		//Set attacking variable = onAttacking in AttackTrigger
		attacking = GetComponent<AttackTriggerC>().onAttacking;
		flinch = GetComponent<AttackTriggerC>().flinch;

		if(attacking || flinch || GlobalConditionC.freezeAll || GlobalConditionC.freezePlayer || stat.dodge){
			animator.SetFloat(moveHorizontalState , 0);
			animator.SetFloat(moveVerticalState , 0);
			return;
		}
		
		if((controller.collisionFlags & CollisionFlags.Below) != 0){
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
			animator.SetFloat(moveHorizontalState , moveHorizontal);
			animator.SetFloat(moveVerticalState , moveVertical);
			if(jumping){
				jumping = false;
				animator.SetBool(jumpState , jumping);
				//animator.StopPlayback(jumpState);
			}
		}else{
			jumping = true;
			animator.SetBool(jumpState , jumping);
			//animator.Play(jumpState);
		}
	}
	
	public void AttackAnimation(string anim){
		animator.SetBool(jumpState , false);
		animator.Play(anim);
	}
	
	public void PlayAnim(string anim){
		animator.Play(anim);
	}
	
	public void SetWeaponType(int val){
		mainModel = GetComponent<AttackTriggerC>().mainModel;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		if(!animator){
			animator = mainModel.GetComponent<Animator>();
		}
		animator.SetInteger("weaponType" , val);
		animator.SetTrigger("changeWeapon");
	}
}