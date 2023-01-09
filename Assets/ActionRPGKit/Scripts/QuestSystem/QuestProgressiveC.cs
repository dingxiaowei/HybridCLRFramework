using UnityEngine;
using System.Collections;

public class QuestProgressiveC : MonoBehaviour {
	public int questId = 1;
	private GameObject player;
	
	public enum progressType{
		Auto = 0,
		Trigger = 1
	}
	
	public progressType type = progressType.Auto;
	
	void Start(){
		if(type == progressType.Auto){
			player = GameObject.FindWithTag("Player");
			if(!player){
				return;
			}
			//Increase the progress of the Quest ID
			//The Function will automatic check If player have this quest(ID) in the Quest Slot or not.
			QuestStatC qstat = player.GetComponent<QuestStatC>();
			if(qstat){
				player.GetComponent<QuestStatC>().Progress(questId);
			}
		}
	}
	
	void OnTriggerEnter(Collider other){
		if(other.tag == "Player" && type == progressType.Trigger){
			//Increase the progress of the Quest ID
			//The Function will automatic check If player have this quest(ID) in the Quest Slot or not.
			QuestStatC qstat = other.GetComponent<QuestStatC>();
			if(qstat){
				bool c = other.GetComponent<QuestStatC>().Progress(questId);
				if(c){
					Destroy(gameObject);
				}
			}
		}
	}
}