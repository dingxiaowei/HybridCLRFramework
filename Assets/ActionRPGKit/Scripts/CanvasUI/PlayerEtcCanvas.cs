using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEtcCanvas : MonoBehaviour {
	public GameObject staminaBorder;
	public Image staminaBar;
	
	public GameObject activatorButton;
	public Text activatorText;
	public GameObject aimIcon;
	
	public GameObject monHpBorder;
	public Image monHpBar;
	public Text enemyName;
	
	public Transform player;

	void Start(){
		if(!player){
			if(transform.root.GetComponent<AttackTriggerC>()){
				player = transform.root;
			}else{
				return;
			}
		}
		//-----------------------
		if(staminaBorder && staminaBar && player.GetComponent<PlayerInputControllerC>()){
			player.GetComponent<PlayerInputControllerC>().canvasElement.staminaBorder = staminaBorder;
			player.GetComponent<PlayerInputControllerC>().canvasElement.staminaBar = staminaBar;
			player.GetComponent<PlayerInputControllerC>().canvasElement.useCanvas = true;
		}

		if(activatorButton && activatorText && aimIcon){
			player.GetComponent<AttackTriggerC>().canvasElement.activatorButton = activatorButton;
			player.GetComponent<AttackTriggerC>().canvasElement.activatorText = activatorText;
			player.GetComponent<AttackTriggerC>().canvasElement.aimIcon = aimIcon;
			player.GetComponent<AttackTriggerC>().canvasElement.useCanvas = true;
		}

		if(monHpBorder && monHpBar && enemyName && player.GetComponent<ShowEnemyHealthC>()){
			player.GetComponent<ShowEnemyHealthC>().canvasElement.border = monHpBorder;
			player.GetComponent<ShowEnemyHealthC>().canvasElement.hpBar = monHpBar;
			player.GetComponent<ShowEnemyHealthC>().canvasElement.enemyName = enemyName;
			player.GetComponent<ShowEnemyHealthC>().canvasElement.useCanvas = true;
		}
	}
}
