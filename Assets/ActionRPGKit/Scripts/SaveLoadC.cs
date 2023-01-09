using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveLoadC : MonoBehaviour {
	public bool autoLoad = false;
	public GameObject player;
	private bool menu = false;
	private Vector3 lastPosition;
	private Transform mainCam;
	public GameObject oldPlayer;
	private int saveSlot = 0;
	public bool useLegacyUi = false;
	public GameObject canVasUI;
	
	void Start(){
		if(!player){
			player = GameObject.FindWithTag ("Player");
		}
		saveSlot = PlayerPrefs.GetInt("SaveSlot");
		//GetComponent<StatusC>().characterName = PlayerPrefs.GetString("Name" +saveSlot.ToString());
		//If PlayerPrefs Loadgame = 10 That mean You Start with Load Game Menu.
		//If You Set Autoload = true It will LoadGame when you start.
		if(PlayerPrefs.GetInt("Loadgame") == 10 || autoLoad){
			LoadGame();
			 if(!autoLoad){
				//If You didn't Set autoLoad then reset PlayerPrefs Loadgame to 0 after LoadGame.
   		 		PlayerPrefs.SetInt("Loadgame", 0);
   			 }
		}
	}
	
	void Update(){
		if(Input.GetKeyDown(KeyCode.Escape)){
			//Open Save Load Menu
			OnOffMenu();
		}
	}
	
	public void OnOffMenu(){
		//Freeze Time Scale to 0 if Window is Showing
		if(!menu && Time.timeScale != 0.0f){
			if(useLegacyUi || !canVasUI){
				menu = true;
			}else{
				canVasUI.SetActive(true);
			}
			Time.timeScale = 0.0f;
			//Screen.lockCursor = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}else if(menu || canVasUI.activeSelf){
			if(useLegacyUi || !canVasUI){
				menu = false;
			}else{
				canVasUI.SetActive(false);
			}
			Time.timeScale = 1.0f;
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}
	
	void OnGUI(){
		if(menu){
			GUI.Box(new Rect(Screen.width / 2 - 110,230,220,200), "Menu");
			if(GUI.Button(new Rect(Screen.width / 2 - 45,285,90,40), "Save Game")){
				SaveData();
			}
			if(GUI.Button(new Rect(Screen.width / 2 - 45,365,90,40), "Load Game")){
				LoadData();
			}
			if(GUI.Button(new Rect(Screen.width / 2 + 55,235,30,30), "X")){
				OnOffMenu();
			}
		}
	}

	public void SaveData(){
		PlayerPrefs.SetInt("PreviousSave" +saveSlot.ToString(), 10);
		PlayerPrefs.SetString("Name" +saveSlot.ToString(), player.GetComponent<StatusC>().characterName);
		PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
		PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
		PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);
		PlayerPrefs.SetInt("PlayerID" +saveSlot.ToString(), player.GetComponent<StatusC>().characterId);
		PlayerPrefs.SetInt("PlayerLevel" +saveSlot.ToString(), player.GetComponent<StatusC>().level);
		PlayerPrefs.SetInt("PlayerATK" +saveSlot.ToString(), player.GetComponent<StatusC>().atk);
		PlayerPrefs.SetInt("PlayerDEF" +saveSlot.ToString(), player.GetComponent<StatusC>().def);
		PlayerPrefs.SetInt("PlayerMATK" +saveSlot.ToString(), player.GetComponent<StatusC>().matk);
		PlayerPrefs.SetInt("PlayerMDEF" +saveSlot.ToString(), player.GetComponent<StatusC>().mdef);
		PlayerPrefs.SetInt("PlayerEXP" +saveSlot.ToString(), player.GetComponent<StatusC>().exp);
		PlayerPrefs.SetInt("PlayerMaxEXP" +saveSlot.ToString(), player.GetComponent<StatusC>().maxExp);
		PlayerPrefs.SetInt("PlayerMaxHP" +saveSlot.ToString(), player.GetComponent<StatusC>().maxHealth);
		PlayerPrefs.SetInt("PlayerHP" +saveSlot.ToString(), player.GetComponent<StatusC>().health);
		PlayerPrefs.SetInt("PlayerMaxMP" +saveSlot.ToString(), player.GetComponent<StatusC>().maxMana);
		//	PlayerPrefs.SetInt("PlayerMP", player.GetComponent<StatusC>().mana);
		PlayerPrefs.SetInt("PlayerSTP" +saveSlot.ToString(), player.GetComponent<StatusC>().statusPoint);
		PlayerPrefs.SetInt("PlayerSKP" +saveSlot.ToString(), player.GetComponent<StatusC>().skillPoint);
		
		PlayerPrefs.SetInt("Cash" +saveSlot.ToString(), player.GetComponent<InventoryC>().cash);
		int itemSize = player.GetComponent<InventoryC>().itemSlot.Length;
		int a = 0;
		if(itemSize > 0){
			while(a < itemSize){
				PlayerPrefs.SetInt("Item" + a.ToString() +saveSlot.ToString(), player.GetComponent<InventoryC>().itemSlot[a]);
				PlayerPrefs.SetInt("ItemQty" + a.ToString() +saveSlot.ToString(), player.GetComponent<InventoryC>().itemQuantity[a]);
				a++;
			}
		}
		
		int equipSize = player.GetComponent<InventoryC>().equipment.Length;
		a = 0;
		if(equipSize > 0){
			while(a < equipSize){
				PlayerPrefs.SetInt("Equipm" + a.ToString() +saveSlot.ToString(), player.GetComponent<InventoryC>().equipment[a]);
				a++;
			}
		}
		PlayerPrefs.SetInt("WeaEquip" +saveSlot.ToString(), player.GetComponent<InventoryC>().weaponEquip);
		PlayerPrefs.SetInt("SubEquip" +saveSlot.ToString(), player.GetComponent<InventoryC>().subWeaponEquip);

		PlayerPrefs.SetInt("ArmoEquip" +saveSlot.ToString(), player.GetComponent<InventoryC>().armorEquip);
		PlayerPrefs.SetInt("AccEquip" +saveSlot.ToString(), player.GetComponent<InventoryC>().accessoryEquip);
		PlayerPrefs.SetInt("BootsEquip" +saveSlot.ToString(), player.GetComponent<InventoryC>().bootsEquip);
		PlayerPrefs.SetInt("GloveEquip" +saveSlot.ToString(), player.GetComponent<InventoryC>().glovesEquip);
		PlayerPrefs.SetInt("HatEquip" +saveSlot.ToString(), player.GetComponent<InventoryC>().hatEquip);

		//Save Quest
		int questSize = player.GetComponent<QuestStatC>().questProgress.Length;
		PlayerPrefs.SetInt("QuestSize" +saveSlot.ToString(), questSize);
		a = 0;
		if(questSize > 0){
			while(a < questSize){
				PlayerPrefs.SetInt("Questp" + a.ToString() +saveSlot.ToString(), player.GetComponent<QuestStatC>().questProgress[a]);
				a++;
			}
		}
		int questSlotSize = player.GetComponent<QuestStatC>().questSlot.Length;
		PlayerPrefs.SetInt("QuestSlotSize" +saveSlot.ToString(), questSlotSize);
		a = 0;
		if(questSlotSize > 0){
			while(a < questSlotSize){
				PlayerPrefs.SetInt("Questslot" + a.ToString() +saveSlot.ToString(), player.GetComponent<QuestStatC>().questSlot[a]);
				a++;
			}
		}
		//Save Skill Slot
		a = 0;
		while(a < player.GetComponent<SkillWindowC>().skill.Length){
			PlayerPrefs.SetInt("Skill" + a.ToString() +saveSlot.ToString(), player.GetComponent<SkillWindowC>().skill[a]);
			a++;
		}
		//Skill List Slot
		a = 0;
		while(a < player.GetComponent<SkillWindowC>().skillListSlot.Length){
			PlayerPrefs.SetInt("SkillList" + a.ToString() +saveSlot.ToString(), player.GetComponent<SkillWindowC>().skillListSlot[a]);
			a++;
		}

		//Global Event
		a = 0;
		while(a < GlobalConditionC.eventVar.Length){
			PlayerPrefs.SetInt("GlobalVar" + a.ToString() +saveSlot.ToString(), GlobalConditionC.eventVar[a]);
			a++;
		}
		for(int b = 0; b < EventSetting.globalInt.Length; b++){
			PlayerPrefs.SetInt("GlobalInt" + b.ToString() +saveSlot.ToString(), EventSetting.globalInt[b]);
		}
		for(int b = 0; b < EventSetting.globalBoolean.Length; b++){
			int val = 0;
			if(EventSetting.globalBoolean[b] == true){
				val = 1;
			}
			PlayerPrefs.SetInt("GlobalBool" + b.ToString() +saveSlot.ToString(), val);
		}

		//PlayerPrefs.SetInt("Scene" +saveSlot.ToString(), Application.loadedLevel);
		PlayerPrefs.SetInt("Scene" +saveSlot.ToString(), SceneManager.GetActiveScene().buildIndex);

		PlayerPrefs.SetFloat("PlayerX" +saveSlot.ToString(), player.transform.position.x);
		PlayerPrefs.SetFloat("PlayerY" +saveSlot.ToString(), player.transform.position.y);
		PlayerPrefs.SetFloat("PlayerZ" +saveSlot.ToString(), player.transform.position.z);
		print("Saved");
		OnOffMenu();
	}

	public void LoadData(){
		if(PlayerPrefs.GetInt("PlayerLevel" +saveSlot.ToString()) <= 0){
			return;
		}
		//oldPlayer = GameObject.FindWithTag ("Player");
		GameObject respawn = GameObject.FindWithTag ("Player");
		SpawnPlayerC.onLoadGame = true;

		respawn.GetComponent<StatusC>().characterName = PlayerPrefs.GetString("Name" +saveSlot.ToString());
		lastPosition.x = PlayerPrefs.GetFloat("PlayerX");
		lastPosition.y = PlayerPrefs.GetFloat("PlayerY") + 0.2f;
		lastPosition.z = PlayerPrefs.GetFloat("PlayerZ");
		respawn.transform.position = lastPosition;
		//GameObject respawn = Instantiate(player, lastPosition , transform.rotation) as GameObject;

		respawn.GetComponent<StatusC>().level = PlayerPrefs.GetInt("PlayerLevel" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().atk = PlayerPrefs.GetInt("PlayerATK" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().def = PlayerPrefs.GetInt("PlayerDEF" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().matk = PlayerPrefs.GetInt("PlayerMATK" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().mdef = PlayerPrefs.GetInt("PlayerMDEF" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().mdef = PlayerPrefs.GetInt("PlayerMDEF" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().exp = PlayerPrefs.GetInt("PlayerEXP" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().maxExp = PlayerPrefs.GetInt("PlayerMaxEXP" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().maxHealth = PlayerPrefs.GetInt("PlayerMaxHP" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().health = PlayerPrefs.GetInt("PlayerHP" +saveSlot.ToString());
		//respawn.GetComponent<StatusC>().health = PlayerPrefs.GetInt("PlayerMaxHP");
		respawn.GetComponent<StatusC>().maxMana = PlayerPrefs.GetInt("PlayerMaxMP" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().mana = PlayerPrefs.GetInt("PlayerMaxMP" +saveSlot.ToString());	
		respawn.GetComponent<StatusC>().statusPoint = PlayerPrefs.GetInt("PlayerSTP" +saveSlot.ToString());
		respawn.GetComponent<StatusC>().skillPoint = PlayerPrefs.GetInt("PlayerSKP" +saveSlot.ToString());
		mainCam = GameObject.FindWithTag ("MainCamera").transform;
		mainCam.GetComponent<ARPGcameraC>().target = respawn.transform;
		//-------------------------------
		respawn.GetComponent<InventoryC>().cash = PlayerPrefs.GetInt("Cash" +saveSlot.ToString());
		int itemSize = player.GetComponent<InventoryC>().itemSlot.Length;
		int a = 0;
		if(itemSize > 0){
			while(a < itemSize){
				respawn.GetComponent<InventoryC>().itemSlot[a] = PlayerPrefs.GetInt("Item" + a.ToString() +saveSlot.ToString());
				respawn.GetComponent<InventoryC>().itemQuantity[a] = PlayerPrefs.GetInt("ItemQty" + a.ToString() +saveSlot.ToString());
				//-------
				a++;
			}
		}
		int equipSize = player.GetComponent<InventoryC>().equipment.Length;
		a = 0;
		if(equipSize > 0){
			while(a < equipSize){
				respawn.GetComponent<InventoryC>().equipment[a] = PlayerPrefs.GetInt("Equipm" + a.ToString() +saveSlot.ToString());
				a++;
			}
		}
		respawn.GetComponent<InventoryC>().weaponEquip = 0;
		respawn.GetComponent<InventoryC>().subWeaponEquip = PlayerPrefs.GetInt("SubEquip" +saveSlot.ToString());
		respawn.GetComponent<InventoryC>().armorEquip = PlayerPrefs.GetInt("ArmoEquip" +saveSlot.ToString());
		respawn.GetComponent<InventoryC>().accessoryEquip = PlayerPrefs.GetInt("AccEquip" +saveSlot.ToString());
		respawn.GetComponent<InventoryC>().bootsEquip = PlayerPrefs.GetInt("BootsEquip" +saveSlot.ToString());
		respawn.GetComponent<InventoryC>().glovesEquip = PlayerPrefs.GetInt("GloveEquip" +saveSlot.ToString());
		respawn.GetComponent<InventoryC>().hatEquip = PlayerPrefs.GetInt("HatEquip" +saveSlot.ToString());
		if(PlayerPrefs.GetInt("WeaEquip" +saveSlot.ToString()) == 0){
			respawn.GetComponent<InventoryC>().RemoveWeaponMesh();
		}else{
			respawn.GetComponent<InventoryC>().EquipItem(PlayerPrefs.GetInt("WeaEquip" +saveSlot.ToString()) , respawn.GetComponent<InventoryC>().equipment.Length + 5);
		}
		//----------------------------------
		//Screen.lockCursor = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		
		//Load Quest
		respawn.GetComponent<QuestStatC>().questProgress = new int[PlayerPrefs.GetInt("QuestSize" +saveSlot.ToString())];
		int questSize = respawn.GetComponent<QuestStatC>().questProgress.Length;
		a = 0;
		if(questSize > 0){
			while(a < questSize){
				respawn.GetComponent<QuestStatC>().questProgress[a] = PlayerPrefs.GetInt("Questp" + a.ToString() +saveSlot.ToString());
				a++;
			}
		}
		
		respawn.GetComponent<QuestStatC>().questSlot = new int[PlayerPrefs.GetInt("QuestSlotSize" +saveSlot.ToString())];
		int questSlotSize = respawn.GetComponent<QuestStatC>().questSlot.Length;
		a = 0;
		if(questSlotSize > 0){
			while(a < questSlotSize){
				respawn.GetComponent<QuestStatC>().questSlot[a] = PlayerPrefs.GetInt("Questslot" + a.ToString() +saveSlot.ToString());
				a++;
			}
		}
		
		//Load Skill Slot
		a = 0;
		while(a < respawn.GetComponent<SkillWindowC>().skill.Length){
			respawn.GetComponent<SkillWindowC>().skill[a] = PlayerPrefs.GetInt("Skill" + a.ToString() +saveSlot.ToString());
			a++;
		}
		//Skill List Slot
		a = 0;
		while(a < respawn.GetComponent<SkillWindowC>().skillListSlot.Length){
			respawn.GetComponent<SkillWindowC>().skillListSlot[a] = PlayerPrefs.GetInt("SkillList" + a.ToString() +saveSlot.ToString());
			a++;
		}
		respawn.GetComponent<SkillWindowC>().AssignAllSkill();

		//Global Event
		a = 0;
		while(a < GlobalConditionC.eventVar.Length){
			GlobalConditionC.eventVar[a] = PlayerPrefs.GetInt("GlobalVar" + a.ToString() +saveSlot.ToString());
			a++;
		}
		for(int b = 0; b < EventSetting.globalInt.Length; b++){
			EventSetting.globalInt[b] = PlayerPrefs.GetInt("GlobalInt" + b.ToString() +saveSlot.ToString());
		}
		for(int b = 0; b < EventSetting.globalBoolean.Length; b++){
			int val = PlayerPrefs.GetInt("GlobalBool" + b.ToString() +saveSlot.ToString());
			if(val >= 1){
				EventSetting.globalBoolean[b] = true;
			}
		}

		//---------------Set Target to Minimap--------------
		GameObject minimap = GameObject.FindWithTag("Minimap");
		if(minimap){
			GameObject mapcam = minimap.GetComponent<MinimapOnOffC>().minimapCam;
			mapcam.GetComponent<MinimapCameraC>().target = respawn.transform;
		}

		if(GlobalConditionC.freezePlayer){
			respawn.transform.root.SendMessage("Deactivate" , SendMessageOptions.DontRequireReceiver);
		}
		GameObject[] gos = GameObject.FindGameObjectsWithTag("Mount");
		if(gos.Length > 0){
			foreach(GameObject go in gos){ 
				go.SendMessage("DestroySelf" , SendMessageOptions.DontRequireReceiver);
			}
		}
		
		//player = GameObject.FindWithTag ("Player");
		lastPosition.x = PlayerPrefs.GetFloat("PlayerX" +saveSlot.ToString());
		lastPosition.y = PlayerPrefs.GetFloat("PlayerY" +saveSlot.ToString()) + 0.5f;
		lastPosition.z = PlayerPrefs.GetFloat("PlayerZ" +saveSlot.ToString());
		respawn.transform.position = lastPosition;
		int sceneLoad = PlayerPrefs.GetInt("Scene" +saveSlot.ToString());
		//Application.LoadLevel(sceneLoad);
		SceneManager.LoadScene(sceneLoad, LoadSceneMode.Single);
		/*if(oldPlayer){
			Destroy(gameObject);
		}*/
		OnOffMenu();
	}
	
	//Function LoadGame is unlike the Function LoadData.
	//This Function will not spawn new Player.
	void LoadGame (){
		player.GetComponent<StatusC>().characterName = PlayerPrefs.GetString("Name" +saveSlot.ToString());
		player.GetComponent<StatusC>().level = PlayerPrefs.GetInt("PlayerLevel" +saveSlot.ToString());
		player.GetComponent<StatusC>().atk = PlayerPrefs.GetInt("PlayerATK" +saveSlot.ToString());
		player.GetComponent<StatusC>().def = PlayerPrefs.GetInt("PlayerDEF" +saveSlot.ToString());
		player.GetComponent<StatusC>().matk = PlayerPrefs.GetInt("PlayerMATK" +saveSlot.ToString());
		player.GetComponent<StatusC>().mdef = PlayerPrefs.GetInt("PlayerMDEF" +saveSlot.ToString());
		player.GetComponent<StatusC>().mdef = PlayerPrefs.GetInt("PlayerMDEF" +saveSlot.ToString());
		player.GetComponent<StatusC>().exp = PlayerPrefs.GetInt("PlayerEXP" +saveSlot.ToString());
		player.GetComponent<StatusC>().maxExp = PlayerPrefs.GetInt("PlayerMaxEXP" +saveSlot.ToString());
		player.GetComponent<StatusC>().maxHealth = PlayerPrefs.GetInt("PlayerMaxHP" +saveSlot.ToString());
		//player.GetComponent<StatusC>().health = PlayerPrefs.GetInt("PlayerMaxHP" +saveSlot.ToString());
		player.GetComponent<StatusC>().maxMana = PlayerPrefs.GetInt("PlayerMaxMP" +saveSlot.ToString());
		//player.GetComponent<StatusC>().mana = PlayerPrefs.GetInt("PlayerMaxMP" +saveSlot.ToString());	
		player.GetComponent<StatusC>().statusPoint = PlayerPrefs.GetInt("PlayerSTP" +saveSlot.ToString());
		player.GetComponent<StatusC>().skillPoint = PlayerPrefs.GetInt("PlayerSKP" +saveSlot.ToString());

		player.GetComponent<StatusC>().CalculateStatus();
		player.GetComponent<StatusC>().health = player.GetComponent<StatusC>().totalMaxHealth;
		player.GetComponent<StatusC>().mana = player.GetComponent<StatusC>().totalMaxMana;
		//mainCam = GameObject.FindWithTag ("MainCamera").transform;
		//mainCam.GetComponent<ARPGcamera>().target = respawn.transform;
		//-------------------------------
		player.GetComponent<InventoryC>().cash = PlayerPrefs.GetInt("Cash" +saveSlot.ToString());
		int itemSize = player.GetComponent<InventoryC>().itemSlot.Length;
		int a = 0;
		if(itemSize > 0){
			while(a < itemSize){
				player.GetComponent<InventoryC>().itemSlot[a] = PlayerPrefs.GetInt("Item" + a.ToString() +saveSlot.ToString());
				player.GetComponent<InventoryC>().itemQuantity[a] = PlayerPrefs.GetInt("ItemQty" + a.ToString() +saveSlot.ToString());
				//-------
				a++;
			}
		}
		int equipSize = player.GetComponent<InventoryC>().equipment.Length;
		a = 0;
		if(equipSize > 0){
			while(a < equipSize){
				player.GetComponent<InventoryC>().equipment[a] = PlayerPrefs.GetInt("Equipm" + a.ToString() +saveSlot.ToString());
				a++;
			}
		}
		player.GetComponent<InventoryC>().weaponEquip = 0;
		player.GetComponent<InventoryC>().subWeaponEquip = PlayerPrefs.GetInt("SubEquip" +saveSlot.ToString());
		player.GetComponent<InventoryC>().armorEquip = PlayerPrefs.GetInt("ArmoEquip" +saveSlot.ToString());
		player.GetComponent<InventoryC>().accessoryEquip = PlayerPrefs.GetInt("AccEquip" +saveSlot.ToString());
		player.GetComponent<InventoryC>().bootsEquip = PlayerPrefs.GetInt("BootsEquip" +saveSlot.ToString());
		player.GetComponent<InventoryC>().glovesEquip = PlayerPrefs.GetInt("GloveEquip" +saveSlot.ToString());
		player.GetComponent<InventoryC>().hatEquip = PlayerPrefs.GetInt("HatEquip" +saveSlot.ToString());

		if(PlayerPrefs.GetInt("WeaEquip" +saveSlot.ToString()) == 0){
			player.GetComponent<InventoryC>().RemoveWeaponMesh();
		}else{
			player.GetComponent<InventoryC>().EquipItem(PlayerPrefs.GetInt("WeaEquip" +saveSlot.ToString()) , player.GetComponent<InventoryC>().equipment.Length + 5);
		}
		//----------------------------------
		//Screen.lockCursor = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		//Load Quest
		player.GetComponent<QuestStatC>().questProgress = new int[PlayerPrefs.GetInt("QuestSize" +saveSlot.ToString())];
		int questSize = player.GetComponent<QuestStatC>().questProgress.Length;
		a = 0;
		if(questSize > 0){
			while(a < questSize){
				player.GetComponent<QuestStatC>().questProgress[a] = PlayerPrefs.GetInt("Questp" + a.ToString() +saveSlot.ToString());
				a++;
			}
		}
		
		player.GetComponent<QuestStatC>().questSlot = new int[PlayerPrefs.GetInt("QuestSlotSize" +saveSlot.ToString())];
		int questSlotSize = player.GetComponent<QuestStatC>().questSlot.Length;
		a = 0;
		if(questSlotSize > 0){
			while(a < questSlotSize){
				player.GetComponent<QuestStatC>().questSlot[a] = PlayerPrefs.GetInt("Questslot" + a.ToString() +saveSlot.ToString());
				a++;
			}
		}
		
		//Load Skill Slot
		a = 0;
		while(a < player.GetComponent<SkillWindowC>().skill.Length){
			player.GetComponent<SkillWindowC>().skill[a] = PlayerPrefs.GetInt("Skill" + a.ToString() +saveSlot.ToString());
			a++;
		}
		//Skill List Slot
		a = 0;
		while(a < player.GetComponent<SkillWindowC>().skillListSlot.Length){
			player.GetComponent<SkillWindowC>().skillListSlot[a] = PlayerPrefs.GetInt("SkillList" + a.ToString() +saveSlot.ToString());
			a++;
		}
		player.GetComponent<SkillWindowC>().AssignAllSkill();

		//Global Event
		a = 0;
		while(a < GlobalConditionC.eventVar.Length){
			GlobalConditionC.eventVar[a] = PlayerPrefs.GetInt("GlobalVar" + a.ToString() +saveSlot.ToString());
			a++;
		}
		for(int b = 0; b < EventSetting.globalInt.Length; b++){
			EventSetting.globalInt[b] = PlayerPrefs.GetInt("GlobalInt" + b.ToString() +saveSlot.ToString());
		}
		for(int b = 0; b < EventSetting.globalBoolean.Length; b++){
			int val = PlayerPrefs.GetInt("GlobalBool" + b.ToString() +saveSlot.ToString());
			if(val >= 1){
				EventSetting.globalBoolean[b] = true;
			}
		}

		lastPosition.x = PlayerPrefs.GetFloat("PlayerX" +saveSlot.ToString());
		lastPosition.y = PlayerPrefs.GetFloat("PlayerY" +saveSlot.ToString()) + 0.5f;
		lastPosition.z = PlayerPrefs.GetFloat("PlayerZ" +saveSlot.ToString());
		
		player.transform.position = lastPosition;
		int sceneLoad = PlayerPrefs.GetInt("Scene" +saveSlot.ToString());
		//Application.LoadLevel(sceneLoad);
		SceneManager.LoadScene(sceneLoad, LoadSceneMode.Single);
		player.transform.position = lastPosition;
		
		print("Loaded");
	}

	public void QuitGame(){
		if(GetComponent<UiMasterC>()){
			GetComponent<UiMasterC>().DestroyAllUi();
		}
		mainCam = GameObject.FindWithTag("MainCamera").transform;
		Destroy(mainCam.gameObject); //Destroy Main Camera
		//Application.LoadLevel("Title");
		SceneManager.LoadScene("Title", LoadSceneMode.Single);
		OnOffMenu();
		Destroy(gameObject);
	}
}