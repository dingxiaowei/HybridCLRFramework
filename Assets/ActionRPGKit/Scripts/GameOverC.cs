using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverC : MonoBehaviour {
	
	public float delay = 3.0f;
	public GameObject player;
	private bool menu = false;
	private Vector3 lastPosition;
	private Transform mainCam;
	GameObject oldPlayer;
	public bool useLegacyUi = false;
	public GameObject canVasUI;
	
	void Start(){
		StartCoroutine(Delay());
	}

	IEnumerator Delay(){
		yield return new WaitForSeconds(delay);
		if(useLegacyUi || !canVasUI){
			menu = true;
		}else{
			canVasUI.SetActive(true);
		}
		//Screen.lockCursor = false;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
	
	void OnGUI(){
		if(menu){
			GUI.Box(new Rect(Screen.width /2 -100,Screen.height /2 -120,200,160), "Game Over");
			if(GUI.Button(new Rect(Screen.width /2 -80,Screen.height /2 -80,160,40), "Retry")) {
				LoadData();
			}
			if(GUI.Button(new Rect(Screen.width /2 -80,Screen.height /2 -20,160,40), "Quit Game")) {
				mainCam = GameObject.FindWithTag ("MainCamera").transform;
				Destroy(mainCam.gameObject); //Destroy Main Camera
				//Application.LoadLevel("Title");
				SceneManager.LoadScene("Title", LoadSceneMode.Single);
				//Application.Quit();
			}
		}
	}

	public void QuitGame(){
		if(canVasUI){
			Destroy(canVasUI);
		}
		mainCam = GameObject.FindWithTag("MainCamera").transform;
		Destroy(mainCam.gameObject); //Destroy Main Camera
		//Application.LoadLevel("Title");
		SceneManager.LoadScene("Title", LoadSceneMode.Single);
	}
	
	public void LoadData(){
		if(canVasUI){
			Destroy(canVasUI);
		}
		oldPlayer = GameObject.FindWithTag("Player");
		if(oldPlayer){
			Destroy(gameObject);
		}else{
			lastPosition.x = PlayerPrefs.GetFloat("PlayerX");
			lastPosition.y = PlayerPrefs.GetFloat("PlayerY") + 0.2f;
			lastPosition.z = PlayerPrefs.GetFloat("PlayerZ");
			GameObject respawn = Instantiate(player, lastPosition , transform.rotation) as GameObject;
			respawn.GetComponent<StatusC>().level = PlayerPrefs.GetInt("TempPlayerLevel");
			respawn.GetComponent<StatusC>().characterId = PlayerPrefs.GetInt("TempID");
			respawn.GetComponent<StatusC>().characterName = PlayerPrefs.GetString("TempName");
			respawn.GetComponent<StatusC>().atk = PlayerPrefs.GetInt("TempPlayerATK");
			respawn.GetComponent<StatusC>().def = PlayerPrefs.GetInt("TempPlayerDEF");
			respawn.GetComponent<StatusC>().matk = PlayerPrefs.GetInt("TempPlayerMATK");
			respawn.GetComponent<StatusC>().mdef = PlayerPrefs.GetInt("TempPlayerMDEF");
			respawn.GetComponent<StatusC>().mdef = PlayerPrefs.GetInt("TempPlayerMDEF");
			respawn.GetComponent<StatusC>().exp = PlayerPrefs.GetInt("TempPlayerEXP");
			respawn.GetComponent<StatusC>().maxExp = PlayerPrefs.GetInt("TempPlayerMaxEXP");
			respawn.GetComponent<StatusC>().maxHealth = PlayerPrefs.GetInt("TempPlayerMaxHP");
			//respawn.GetComponent<StatusC>().health = PlayerPrefs.GetInt("PlayerHP");
			respawn.GetComponent<StatusC>().health = PlayerPrefs.GetInt("TempPlayerMaxHP");
			respawn.GetComponent<StatusC>().maxMana = PlayerPrefs.GetInt("TempPlayerMaxMP");
			respawn.GetComponent<StatusC>().mana = PlayerPrefs.GetInt("TempPlayerMaxMP");	
			respawn.GetComponent<StatusC>().statusPoint = PlayerPrefs.GetInt("TempPlayerSTP");
			respawn.GetComponent<StatusC>().skillPoint = PlayerPrefs.GetInt("TempPlayerSKP");
			mainCam = GameObject.FindWithTag("MainCamera").transform;
			if(mainCam.GetComponent<ARPGcameraC>())
				mainCam.GetComponent<ARPGcameraC>().target = respawn.transform;
			//-------------------------------
			respawn.GetComponent<InventoryC>().cash = PlayerPrefs.GetInt("TempCash");
			int itemSize = player.GetComponent<InventoryC>().itemSlot.Length;
			int a = 0;
			if(itemSize > 0){
				while(a < itemSize){
					respawn.GetComponent<InventoryC>().itemSlot[a] = PlayerPrefs.GetInt("TempItem" + a.ToString());
					respawn.GetComponent<InventoryC>().itemQuantity[a] = PlayerPrefs.GetInt("TempItemQty" + a.ToString());
					//-------
					a++;
				}
			}
			
			int equipSize = player.GetComponent<InventoryC>().equipment.Length;
			a = 0;
			if(equipSize > 0){
				while(a < equipSize){
					respawn.GetComponent<InventoryC>().equipment[a] = PlayerPrefs.GetInt("TempEquipm" + a.ToString());
					a++;
				}
			}
			respawn.GetComponent<InventoryC>().weaponEquip = 0;
			respawn.GetComponent<InventoryC>().subWeaponEquip = PlayerPrefs.GetInt("TempSubEquip");
			respawn.GetComponent<InventoryC>().armorEquip = PlayerPrefs.GetInt("TempArmoEquip");
			respawn.GetComponent<InventoryC>().accessoryEquip = PlayerPrefs.GetInt("TempAccEquip");
			if(PlayerPrefs.GetInt("TempWeaEquip") == 0){
				respawn.GetComponent<InventoryC>().RemoveWeaponMesh();
			}else{
				respawn.GetComponent<InventoryC>().EquipItem(PlayerPrefs.GetInt("TempWeaEquip") , respawn.GetComponent<InventoryC>().equipment.Length + 5);
			}
			//----------------------------------
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			//--------------Set Target to Monster---------------
			/*GameObject[] mon; 
			mon = GameObject.FindGameObjectsWithTag("Enemy"); 
			foreach(GameObject mo in mon) { 
				if(mo){
					mo.GetComponent<AIsetC>().followTarget = respawn.transform;
				}
			}*/
			//---------------Set Target to Minimap--------------
			GameObject minimap = GameObject.FindWithTag("Minimap");
			if(minimap){
				GameObject mapcam = minimap.GetComponent<MinimapOnOffC>().minimapCam;
				mapcam.GetComponent<MinimapCameraC>().target = respawn.transform;
			}
			
			//Load Quest
			respawn.GetComponent<QuestStatC>().questProgress = new int[PlayerPrefs.GetInt("TempQuestSize")];
			int questSize = respawn.GetComponent<QuestStatC>().questProgress.Length;
			a = 0;
			if(questSize > 0){
				while(a < questSize){
					respawn.GetComponent<QuestStatC>().questProgress[a] = PlayerPrefs.GetInt("TempQuestp" + a.ToString());
					a++;
				}
			}
			
			respawn.GetComponent<QuestStatC>().questSlot = new int[PlayerPrefs.GetInt("TempQuestSlotSize")];
			int questSlotSize = respawn.GetComponent<QuestStatC>().questSlot.Length;
			a = 0;
			if(questSlotSize > 0){
				while(a < questSlotSize){
					respawn.GetComponent<QuestStatC>().questSlot[a] = PlayerPrefs.GetInt("TempQuestslot" + a.ToString());
					a++;
				}
			}
			//Load Skill Slot
			a = 0;
			while(a < respawn.GetComponent<SkillWindowC>().skill.Length){
				respawn.GetComponent<SkillWindowC>().skill[a] = PlayerPrefs.GetInt("TempSkill" + a.ToString());
				a++;
			}
			//Skill List Slot
			a = 0;
			while(a < respawn.GetComponent<SkillWindowC>().skillListSlot.Length){
				respawn.GetComponent<SkillWindowC>().skillListSlot[a] = PlayerPrefs.GetInt("TempSkillList" + a.ToString());
				a++;
			}
			respawn.GetComponent<SkillWindowC>().AssignAllSkill();
			
			Destroy(gameObject);
		}
	}
	
}