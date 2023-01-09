using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[AddComponentMenu("Action-RPG Kit(C#)/Create Teleporter")]

public class TeleporterC : MonoBehaviour {
	
	public string teleportToMap = "Level1";
	public string spawnPointName = "PlayerSpawn1"; //Use for Move Player to the SpawnPoint Position
	public bool allowMountEnter = true;
	//Vector3 spawnPosition;
	
	void OnTriggerEnter(Collider other){
		if(other.tag == "Player"){
			if(!allowMountEnter && GlobalConditionC.freezePlayer){
				return;
			}
			other.GetComponent<StatusC>().spawnPointName = spawnPointName;
			ChangeMap();
		}
	}
	
	void ChangeMap(){
		GameObject[] gos = GameObject.FindGameObjectsWithTag("Mount");
		if(gos.Length > 0){
			foreach(GameObject go in gos){ 
				go.SendMessage("DestroySelf" , SendMessageOptions.DontRequireReceiver);
			}
		}
		//Application.LoadLevel (teleportToMap);
		SceneManager.LoadScene(teleportToMap, LoadSceneMode.Single);
	}
}