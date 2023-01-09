using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestClientEV : MonoBehaviour
{
	public int questId = 1;
	public GameObject questData;
	[HideInInspector]
	public bool enter = false;
	private bool showError = false;
	[HideInInspector]
	public int s = 0;
	
	private GameObject player;

	public EventActivator talkingEvent;
	public EventActivator ongoingQuestEvent;
	public EventActivator finishQuestEvent;
	public EventActivator alreadyFinishQuestEvent;
	
	private bool acceptQuest = false;
	public bool trigger = true;
	public string showText = "";
	private bool thisActive = false;
	private bool questFinish = false;
	public string sendMsgWhenTakeQuest = "";
	public string sendMsgWhenQuestComplete = "";
	public bool repeatable = false;
	
	void Update(){
		if(Input.GetKeyDown("e") && enter && thisActive && !showError){
			SetDialogue();
		}
	}
	
	public void SetDialogue(){
		if(!player){
			player = GameObject.FindWithTag("Player");
		}

		int ongoing = player.GetComponent<QuestStatC>().CheckQuestProgress(questId);
		int finish = questData.GetComponent<QuestDataC>().questData[questId].finishProgress;
		int qprogress = player.GetComponent<QuestStatC>().questProgress[questId];
		if(qprogress >= finish + 9){
			if(finishQuestEvent.runEvent > 0 || finishQuestEvent.eventRunning){
				return;
			}
			alreadyFinishQuestEvent.player = player;
			alreadyFinishQuestEvent.ActivateEvent();
			print("Already Clear");
			return;
		}
		if(acceptQuest){
			if(ongoing >= finish){ //Quest Complete
				finishQuestEvent.player = player;
				finishQuestEvent.ActivateEvent();
				FinishQuest();
			}else{
				//Ongoing
				if(talkingEvent.runEvent > 0 || talkingEvent.eventRunning){
					return;
				}
				ongoingQuestEvent.player = player;
				ongoingQuestEvent.ActivateEvent();
			}
		}else{
			//Before Take the quest
			talkingEvent.player = player;
			talkingEvent.ActivateEvent();
			TakeQuest();
		}
	}
	
	public void TakeQuest(){
		StartCoroutine(AcceptQuest());
		CloseTalk();	
	}
	
	public void FinishQuest(){
		questData.GetComponent<QuestDataC>().QuestClear(questId , player);
		player.GetComponent<QuestStatC>().Clear(questId);
		print("Clear");
		questFinish = true;
		if(sendMsgWhenQuestComplete != ""){
			SendMessage(sendMsgWhenQuestComplete);
		}
		CloseTalk();
		if(repeatable){
			player.GetComponent<QuestStatC>().questProgress[questId] = 0;
			questFinish = false;
		}
	}
	
	public IEnumerator AcceptQuest(){
		bool full = player.GetComponent<QuestStatC>().AddQuest(questId);
		if(full){
			//Quest Full
			showError = true; //Show Quest Full Window
			yield return new WaitForSeconds(1);
			showError = false;
		}else{
			acceptQuest = player.GetComponent<QuestStatC>().CheckQuestSlot(questId);
			if(sendMsgWhenTakeQuest != ""){
				SendMessage(sendMsgWhenTakeQuest);
			}
		}
	}
	
	public void CheckQuestCondition(){
		QuestDataC quest = questData.GetComponent<QuestDataC>();
		int progress = player.GetComponent<QuestStatC>().CheckQuestProgress(questId);
		
		if(progress >= quest.questData[questId].finishProgress){
			//Quest Clear
			quest.QuestClear(questId , player);
		}
	}
	
	void OnTriggerEnter(Collider other){
		if(!trigger){
			return;
		}
		if(other.tag == "Player"){
			s = 0;
			player = other.gameObject;
			acceptQuest = player.GetComponent<QuestStatC>().CheckQuestSlot(questId);
			enter = true;
			thisActive = true;
		}
	}
	
	void OnTriggerExit(Collider other){
		if(!trigger){
			return;
		}
		if(other.tag == "Player"){
			s = 0;
			enter = false;
			CloseTalk();
		}
		thisActive = false;
		showError = false;
	}
	
	void CloseTalk(){
		//Time.timeScale = 1.0f;
		//Screen.lockCursor = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		s = 0;
	}
	
	public bool ActivateQuest(GameObject p){
		player = p;
		acceptQuest = player.GetComponent<QuestStatC>().CheckQuestSlot(questId);
		thisActive = false;
		trigger = false;
		SetDialogue();
		return questFinish;
	}
}
