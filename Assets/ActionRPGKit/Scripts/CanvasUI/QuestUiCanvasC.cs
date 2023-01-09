using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class QuestUiCanvasC : MonoBehaviour {
	public Text[] questName = new Text[5];
	public Text[] questDescription = new Text[5];
	public GameObject[] cancelButton = new GameObject[5];
	public GameObject player;

	public GameObject database;
	private QuestDataC db; 

	public void UpdateQuestDetails(){
		if(!player){
			return;
		}
		db = database.GetComponent<QuestDataC>();
		QuestStatC pq = player.GetComponent<QuestStatC>();
		for(int a = 0; a < questName.Length; a++){
			questName[a].GetComponent<Text>().text = db.questData[pq.questSlot[a]].questName;
			questDescription[a].GetComponent<Text>().text = db.questData[pq.questSlot[a]].description + " (" + pq.questProgress[pq.questSlot[a]].ToString() + " / " + db.questData[pq.questSlot[a]].finishProgress + ")";
			if(pq.questSlot[a] > 0){
				questDescription[a].gameObject.SetActive(true);
				cancelButton[a].SetActive(true);
			}else{
				questDescription[a].gameObject.SetActive(false);
				cancelButton[a].SetActive(false);
			}
		}
	}

	public void CancelQuest(int qid){
		if(!player){
			return;
		}
		QuestStatC pq = player.GetComponent<QuestStatC>();
		pq.questProgress[pq.questSlot[qid]] = 0;
		pq.questSlot[qid] = 0;
		pq.SortQuest();
		UpdateQuestDetails();
	}

	public void CloseMenu(){
		Time.timeScale = 1.0f;
		//Screen.lockCursor = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		gameObject.SetActive(false);
	}
}
