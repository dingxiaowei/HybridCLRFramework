using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTriggerEV : MonoBehaviour{
	public QuestClientEV[] questClients = new QuestClientEV[2];
	public int questStep = 0;

	private GameObject player;
	private GameObject questData;
	
	public void Talking(){
		if(EventActivator.onInteracting){
			return;
		}
		bool q = questClients[questStep].ActivateQuest(player);
		if(q && questStep < questClients.Length){
			questClients[questStep].enter = false; //Reset Enter Variable of last client
			questStep++;
			if(questStep >= questClients.Length){
				questStep = questClients.Length -1;
				return;
			}
			questClients[questStep].s = 0;
			questClients[questStep].enter = true;
		}
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == "Player"){
			player = other.gameObject;
			CheckQuestSequence();
			
			questClients[questStep].s = 0;
			questClients[questStep].enter = true;
			
			if(player.GetComponent<AttackTriggerC>())
				player.GetComponent<AttackTriggerC>().GetActivator(this.gameObject , "Talking" , "Talk");
		}
	}
	
	void OnTriggerExit(Collider other){
		if(other.tag == "Player"){
			questClients[questStep].s = 0;
			questClients[questStep].enter = false;
			
			if(player.GetComponent<AttackTriggerC>())
				player.GetComponent<AttackTriggerC>().RemoveActivator(this.gameObject);
		}
	}
	
	public void CheckQuestSequence(){
		bool c = true;
		while(c == true){
			int id = questClients[questStep].questId;
			questData = questClients[questStep].questData;
			int qprogress = player.GetComponent<QuestStatC>().questProgress[id]; //Check Queststep
			int finish = questData.GetComponent<QuestDataC>().questData[id].finishProgress;
			if(qprogress >= finish + 9){ 
				questStep++;
				if(questStep >= questClients.Length){
					questStep = questClients.Length -1;
					c = false; // End Loop
				}
			}else{
				c = false; // End Loop
			}
		}
	}
}
