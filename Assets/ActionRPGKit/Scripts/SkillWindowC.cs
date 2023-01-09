using UnityEngine;
using System.Collections;

public class SkillWindowC : MonoBehaviour {
	public GameObject database;
	
	public int[] skill = new int[8];
	public int[] skillListSlot = new int[30];

	[System.Serializable]
	public class LearnSkillLV {
		public int level = 1;
		public int skillId = 1;
	}
	public LearnSkillLV[] learnSkill = new LearnSkillLV[2];
	
	private bool menu = false;
	private bool shortcutPage = true;
	private bool skillListPage = false;
	private int skillSelect = 0;

	public GUISkin skin1;
	public Rect windowRect = new Rect (360 ,80 ,360 ,185);
	private Rect originalRect;
	//private Vector2 selectedPos = new Vector2(27 , 97);
	public GUIStyle textStyle;
	public GUIStyle textStyle2;
	private bool showSkillLearned = false;
	private string showSkillName = "";
	public int pageMultiply = 8;
	private int page = 0;
	public bool autoAssignSkill = true;
	public bool useLegacyUi = false;
	
	void Start(){
		originalRect = windowRect;
		if(autoAssignSkill){
			AssignAllSkill();
		}
	}
	
	void Update(){
		if(Input.GetKeyDown("k") && useLegacyUi) {
			OnOffMenu();
		}
	}
	
	public void OnOffMenu (){
		//Freeze Time Scale to 0 if Window is Showing
		if(!menu && Time.timeScale != 0.0f){
			menu = true;
			skillListPage = false;
			shortcutPage = true;
			Time.timeScale = 0.0f;
			//Screen.lockCursor = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}else if(menu){
			menu = false;
			Time.timeScale = 1.0f;
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	void OnGUI(){
		//SkillDataC dataItem = database.GetComponent<SkillDataC>();
		if(showSkillLearned){
			GUI.Label (new Rect (Screen.width /2 -50, 85, 400, 50), "You Learned  " + showSkillName , textStyle2);
		}
		if(menu && shortcutPage){
			windowRect = GUI.Window (3, windowRect, SkillShortcut, "Skill");
		}
		//---------------Skill List----------------------------
		if(menu && skillListPage){
			windowRect = GUI.Window (3, windowRect, AllSkill, "Skill");
		}
	}

	void SkillShortcut(int windowID){
		SkillDataC dataSkill = database.GetComponent<SkillDataC>();
		windowRect.width = 360;
		windowRect.height = 185;
		//Close Window Button
		if(GUI.Button(new Rect (310,2,30,30), "X")){
			OnOffMenu();
		}
		//Skill Shortcut
		if(GUI.Button(new Rect (30,45,80,80), dataSkill.skill[skill[0]].icon)){
			skillSelect = 0;
			skillListPage = true;
			shortcutPage = false;
		}
		GUI.Label(new Rect (70, 145, 20, 20), "1");
		if (GUI.Button (new Rect (130,45,80,80), dataSkill.skill[skill[1]].icon)) {
			skillSelect = 1;
			skillListPage = true;
			shortcutPage = false;
		}
		GUI.Label(new Rect (170, 145, 20, 20), "2");
		if (GUI.Button (new Rect (230,45,80,80), dataSkill.skill[skill[2]].icon)) {
			skillSelect = 2;
			skillListPage = true;
			shortcutPage = false;
		}
		GUI.Label(new Rect (270, 145, 20, 20), "3");
		
		GUI.DragWindow(new Rect (0,0,10000,10000));
	}
	
	void AllSkill(int windowID){
		SkillDataC dataSkill = database.GetComponent<SkillDataC>();
		windowRect.width = 300;
		windowRect.height = 555;
		//Close Window Button
		if(GUI.Button (new Rect (260,2,30,30), "X")) {
			OnOffMenu();
		}
		if(GUI.Button (new Rect (30,60,50,50), new GUIContent (dataSkill.skill[skillListSlot[0 + page]].icon, dataSkill.skill[skillListSlot[0 + page]].description ))) {
			AssignSkill(skillSelect , 0 + page);
			shortcutPage = true;
			skillListPage = false;
		}
		GUI.Label (new Rect (95, 75, 140, 40), dataSkill.skill[skillListSlot[0 + page]].skillName , textStyle); //Show Skill's Name
		GUI.Label (new Rect (220, 75, 140, 40), "MP : " + dataSkill.skill[skillListSlot[0 + page]].manaCost , textStyle); //Show Skill's MP Cost
		//-----------------------------
		
		if(GUI.Button (new Rect (30,120,50,50), new GUIContent (dataSkill.skill[skillListSlot[1 + page]].icon, dataSkill.skill[skillListSlot[1 + page]].description ))) {
			AssignSkill(skillSelect , 1 + page);
			shortcutPage = true;
			skillListPage = false;
		}
		GUI.Label (new Rect (95, 135, 140, 40), dataSkill.skill[skillListSlot[1 + page]].skillName , textStyle); //Show Skill's Name
		GUI.Label (new Rect (220, 135, 140, 40), "MP : " + dataSkill.skill[skillListSlot[1 + page]].manaCost , textStyle); //Show Skill's MP Cost
		//-----------------------------
		
		if(GUI.Button (new Rect (30,180,50,50), new GUIContent (dataSkill.skill[skillListSlot[2 + page]].icon, dataSkill.skill[skillListSlot[2 + page]].description ))) {
			AssignSkill(skillSelect , 2 + page);
			shortcutPage = true;
			skillListPage = false;
		}
		GUI.Label (new Rect (95, 195, 140, 40), dataSkill.skill[skillListSlot[2 + page]].skillName , textStyle); //Show Skill's Name
		GUI.Label (new Rect (220, 195, 140, 40), "MP : " + dataSkill.skill[skillListSlot[2 + page]].manaCost , textStyle); //Show Skill's MP Cost
		//-----------------------------
		
		if(GUI.Button (new Rect (30,240,50,50), new GUIContent (dataSkill.skill[skillListSlot[3 + page]].icon, dataSkill.skill[skillListSlot[3 + page]].description ))) {
			AssignSkill(skillSelect , 3 + page);
			shortcutPage = true;
			skillListPage = false;
		}
		GUI.Label (new Rect (95, 255, 140, 40), dataSkill.skill[skillListSlot[3 + page]].skillName , textStyle); //Show Skill's Name
		GUI.Label (new Rect (220, 255, 140, 40), "MP : " + dataSkill.skill[skillListSlot[3 + page]].manaCost , textStyle); //Show Skill's MP Cost
		//-----------------------------
		
		if(GUI.Button (new Rect (30,300,50,50), new GUIContent (dataSkill.skill[skillListSlot[4 + page]].icon, dataSkill.skill[skillListSlot[4 + page]].description ))) {
			AssignSkill(skillSelect , 4 + page);
			shortcutPage = true;
			skillListPage = false;
		}
		GUI.Label (new Rect (95, 315, 140, 40), dataSkill.skill[skillListSlot[4 + page]].skillName , textStyle); //Show Skill's Name
		GUI.Label (new Rect (220, 315, 140, 40), "MP : " + dataSkill.skill[skillListSlot[4 + page]].manaCost , textStyle); //Show Skill's MP Cost
		//-----------------------------
		
		if(GUI.Button (new Rect (30,360,50,50), new GUIContent (dataSkill.skill[skillListSlot[5 + page]].icon, dataSkill.skill[skillListSlot[5 + page]].description ))) {
			AssignSkill(skillSelect , 5 + page);
			shortcutPage = true;
			skillListPage = false;
		}
		GUI.Label (new Rect (95, 375, 140, 40), dataSkill.skill[skillListSlot[5 + page]].skillName , textStyle); //Show Skill's Name
		GUI.Label (new Rect (220, 375, 140, 40), "MP : " + dataSkill.skill[skillListSlot[5 + page]].manaCost , textStyle); //Show Skill's MP Cost
		//-----------------------------
		
		if(GUI.Button (new Rect (30,420,50,50), new GUIContent (dataSkill.skill[skillListSlot[6 + page]].icon, dataSkill.skill[skillListSlot[6 + page]].description ))) {
			AssignSkill(skillSelect , 6 + page);
			shortcutPage = true;
			skillListPage = false;
		}
		GUI.Label (new Rect (95, 435, 140, 40), dataSkill.skill[skillListSlot[6 + page]].skillName , textStyle); //Show Skill's Name
		GUI.Label (new Rect (220, 435, 140, 40), "MP : " + dataSkill.skill[skillListSlot[6 + page]].manaCost , textStyle); //Show Skill's MP Cost
		//-----------------------------
		
		if(GUI.Button (new Rect (30,480,50,50), new GUIContent (dataSkill.skill[skillListSlot[7 + page]].icon, dataSkill.skill[skillListSlot[7 + page]].description ))) {
			AssignSkill(skillSelect , 7 + page);
			shortcutPage = true;
			skillListPage = false;
		}
		GUI.Label (new Rect (95, 495, 140, 40), dataSkill.skill[skillListSlot[7 + page]].skillName , textStyle); //Show Skill's Name
		GUI.Label (new Rect (220, 495, 140, 40), "MP : " + dataSkill.skill[skillListSlot[7 + page]].manaCost , textStyle); //Show Skill's MP Cost
		
		if(GUI.Button (new Rect (220,514,25,30), "1")){
			page = 0;
		}
		if(GUI.Button (new Rect (250,514,25,30), "2")){
			page = pageMultiply;
		}
		
		GUI.Box (new Rect (20,20,240,26), GUI.tooltip);
		GUI.DragWindow (new Rect (0,0,10000,10000));
	}

	//-----------------------
	
	public void AssignSkill(int id , int sk){
		if(GetComponent<AttackTriggerC>().skillCoolDown[id] > 0 || GetComponent<AttackTriggerC>().isCasting){
			print("This Skill is not Ready");
			return;
		}
		SkillDataC dataSkill = database.GetComponent<SkillDataC>();
		GetComponent<AttackTriggerC>().skill[id].manaCost = dataSkill.skill[skillListSlot[sk]].manaCost;
		GetComponent<AttackTriggerC>().skill[id].skillPrefab = dataSkill.skill[skillListSlot[sk]].skillPrefab;
		GetComponent<AttackTriggerC>().skill[id].skillAnimation = dataSkill.skill[skillListSlot[sk]].skillAnimation;
		GetComponent<AttackTriggerC>().skill[id].mecanimTriggerName = dataSkill.skill[skillListSlot[sk]].mecanimTriggerName;

		GetComponent<AttackTriggerC>().skill[id].icon = dataSkill.skill[skillListSlot[sk]].icon;
		GetComponent<AttackTriggerC>().skill[id].iconSprite = dataSkill.skill[skillListSlot[sk]].iconSprite;
		GetComponent<AttackTriggerC>().skill[id].sendMsg = dataSkill.skill[skillListSlot[sk]].sendMsg;
		GetComponent<AttackTriggerC>().skill[id].castEffect = dataSkill.skill[skillListSlot[sk]].castEffect;

		GetComponent<AttackTriggerC>().skill[id].castTime = dataSkill.skill[skillListSlot[sk]].castTime;
		GetComponent<AttackTriggerC>().skill[id].skillDelay = dataSkill.skill[skillListSlot[sk]].skillDelay;
		GetComponent<AttackTriggerC>().skill[id].whileAttack = dataSkill.skill[skillListSlot[sk]].whileAttack;
		GetComponent<AttackTriggerC>().skill[id].coolDown = dataSkill.skill[skillListSlot[sk]].coolDown;
		GetComponent<AttackTriggerC>().skill[id].skillSpawn = dataSkill.skill[skillListSlot[sk]].skillSpawn;

		GetComponent<AttackTriggerC>().skill[id].requireWeapon = dataSkill.skill[skillListSlot[sk]].requireWeapon;
		GetComponent<AttackTriggerC>().skill[id].requireWeaponType = dataSkill.skill[skillListSlot[sk]].requireWeaponType;

		GetComponent<AttackTriggerC>().skill[id].soundEffect = dataSkill.skill[skillListSlot[sk]].soundEffect;

		int mh = dataSkill.skill[skillListSlot[sk]].multipleHit.Length;
		GetComponent<AttackTriggerC>().skill[id].multipleHit = new SkillAdditionHit[mh];
		for(int m = 0; m < mh; m++){
			GetComponent<AttackTriggerC>().skill[id].multipleHit[m] = new SkillAdditionHit();

			GetComponent<AttackTriggerC>().skill[id].multipleHit[m].skillPrefab = dataSkill.skill[skillListSlot[sk]].multipleHit[m].skillPrefab;
			GetComponent<AttackTriggerC>().skill[id].multipleHit[m].skillAnimation = dataSkill.skill[skillListSlot[sk]].multipleHit[m].skillAnimation;
			GetComponent<AttackTriggerC>().skill[id].multipleHit[m].mecanimTriggerName = dataSkill.skill[skillListSlot[sk]].multipleHit[m].mecanimTriggerName;

			GetComponent<AttackTriggerC>().skill[id].multipleHit[m].castTime = dataSkill.skill[skillListSlot[sk]].multipleHit[m].castTime;
			GetComponent<AttackTriggerC>().skill[id].multipleHit[m].skillDelay = dataSkill.skill[skillListSlot[sk]].multipleHit[m].skillDelay;

			GetComponent<AttackTriggerC>().skill[id].multipleHit[m].soundEffect = dataSkill.skill[skillListSlot[sk]].multipleHit[m].soundEffect;
		}

		skill[id] = skillListSlot[sk];
		CheckSameSkill(skill[id] , id);
		print(sk);
	}

	public void AssignSkillByID(int slot , int skillId){
		//Use With Canvas UI
		if(slot > GetComponent<AttackTriggerC>().skill.Length){
			return;
		}
		if(GetComponent<AttackTriggerC>().skillCoolDown[slot] > 0 || GetComponent<AttackTriggerC>().isCasting){
			print("This Skill is not Ready");
			return;
		}

		SkillDataC dataSkill = database.GetComponent<SkillDataC>();
		GetComponent<AttackTriggerC>().skill[slot].manaCost = dataSkill.skill[skillId].manaCost;
		GetComponent<AttackTriggerC>().skill[slot].skillPrefab = dataSkill.skill[skillId].skillPrefab;
		GetComponent<AttackTriggerC>().skill[slot].skillAnimation = dataSkill.skill[skillId].skillAnimation;
		GetComponent<AttackTriggerC>().skill[slot].mecanimTriggerName = dataSkill.skill[skillId].mecanimTriggerName;
		
		GetComponent<AttackTriggerC>().skill[slot].icon = dataSkill.skill[skillId].icon;
		GetComponent<AttackTriggerC>().skill[slot].iconSprite = dataSkill.skill[skillId].iconSprite;
		GetComponent<AttackTriggerC>().skill[slot].sendMsg = dataSkill.skill[skillId].sendMsg;
		GetComponent<AttackTriggerC>().skill[slot].castEffect = dataSkill.skill[skillId].castEffect;
		
		GetComponent<AttackTriggerC>().skill[slot].castTime = dataSkill.skill[skillId].castTime;
		GetComponent<AttackTriggerC>().skill[slot].skillDelay = dataSkill.skill[skillId].skillDelay;
		GetComponent<AttackTriggerC>().skill[slot].whileAttack = dataSkill.skill[skillId].whileAttack;
		GetComponent<AttackTriggerC>().skill[slot].coolDown = dataSkill.skill[skillId].coolDown;
		GetComponent<AttackTriggerC>().skill[slot].skillSpawn = dataSkill.skill[skillId].skillSpawn;

		GetComponent<AttackTriggerC>().skill[slot].requireWeapon = dataSkill.skill[skillId].requireWeapon;
		GetComponent<AttackTriggerC>().skill[slot].requireWeaponType = dataSkill.skill[skillId].requireWeaponType;

		GetComponent<AttackTriggerC>().skill[slot].soundEffect = dataSkill.skill[skillId].soundEffect;

		int mh = dataSkill.skill[skillId].multipleHit.Length;
		GetComponent<AttackTriggerC>().skill[slot].multipleHit = new SkillAdditionHit[mh];
		for(int m = 0; m < mh; m++){
			GetComponent<AttackTriggerC>().skill[slot].multipleHit[m] = new SkillAdditionHit();

			GetComponent<AttackTriggerC>().skill[slot].multipleHit[m].skillPrefab = dataSkill.skill[skillId].multipleHit[m].skillPrefab;
			GetComponent<AttackTriggerC>().skill[slot].multipleHit[m].skillAnimation = dataSkill.skill[skillId].multipleHit[m].skillAnimation;
			GetComponent<AttackTriggerC>().skill[slot].multipleHit[m].mecanimTriggerName = dataSkill.skill[skillId].multipleHit[m].mecanimTriggerName;

			GetComponent<AttackTriggerC>().skill[slot].multipleHit[m].castTime = dataSkill.skill[skillId].multipleHit[m].castTime;
			GetComponent<AttackTriggerC>().skill[slot].multipleHit[m].skillDelay = dataSkill.skill[skillId].multipleHit[m].skillDelay;

			GetComponent<AttackTriggerC>().skill[slot].multipleHit[m].soundEffect = dataSkill.skill[skillId].multipleHit[m].soundEffect;
		}

		skill[slot] = skillId;
		CheckSameSkill(skill[slot] , slot);
	}
	
	public void AssignAllSkill(){
		int n = 0;
		SkillDataC dataSkill = database.GetComponent<SkillDataC>();
		while(n < skill.Length){
			GetComponent<AttackTriggerC>().skill[n].manaCost = dataSkill.skill[skill[n]].manaCost;
			GetComponent<AttackTriggerC>().skill[n].skillPrefab = dataSkill.skill[skill[n]].skillPrefab;
			GetComponent<AttackTriggerC>().skill[n].skillAnimation = dataSkill.skill[skill[n]].skillAnimation;
			GetComponent<AttackTriggerC>().skill[n].mecanimTriggerName = dataSkill.skill[skill[n]].mecanimTriggerName;

			GetComponent<AttackTriggerC>().skill[n].icon = dataSkill.skill[skill[n]].icon;
			GetComponent<AttackTriggerC>().skill[n].iconSprite = dataSkill.skill[skill[n]].iconSprite;
			GetComponent<AttackTriggerC>().skill[n].sendMsg = dataSkill.skill[skill[n]].sendMsg;
			GetComponent<AttackTriggerC>().skill[n].castEffect = dataSkill.skill[skill[n]].castEffect;

			GetComponent<AttackTriggerC>().skill[n].castTime = dataSkill.skill[skill[n]].castTime;
			GetComponent<AttackTriggerC>().skill[n].skillDelay = dataSkill.skill[skill[n]].skillDelay;
			GetComponent<AttackTriggerC>().skill[n].whileAttack = dataSkill.skill[skill[n]].whileAttack;
			GetComponent<AttackTriggerC>().skill[n].coolDown = dataSkill.skill[skill[n]].coolDown;
			GetComponent<AttackTriggerC>().skill[n].skillSpawn = dataSkill.skill[skill[n]].skillSpawn;

			GetComponent<AttackTriggerC>().skill[n].requireWeapon = dataSkill.skill[skill[n]].requireWeapon;
			GetComponent<AttackTriggerC>().skill[n].requireWeaponType = dataSkill.skill[skill[n]].requireWeaponType;

			GetComponent<AttackTriggerC>().skill[n].soundEffect = dataSkill.skill[skill[n]].soundEffect;

			int mh = dataSkill.skill[skill[n]].multipleHit.Length;
			GetComponent<AttackTriggerC>().skill[n].multipleHit = new SkillAdditionHit[mh];
			for(int m = 0; m < mh; m++){
				GetComponent<AttackTriggerC>().skill[n].multipleHit[m] = new SkillAdditionHit();

				GetComponent<AttackTriggerC>().skill[n].multipleHit[m].skillPrefab = dataSkill.skill[skill[n]].multipleHit[m].skillPrefab;
				GetComponent<AttackTriggerC>().skill[n].multipleHit[m].skillAnimation = dataSkill.skill[skill[n]].multipleHit[m].skillAnimation;
				GetComponent<AttackTriggerC>().skill[n].multipleHit[m].mecanimTriggerName = dataSkill.skill[skill[n]].multipleHit[m].mecanimTriggerName;

				GetComponent<AttackTriggerC>().skill[n].multipleHit[m].castTime = dataSkill.skill[skill[n]].multipleHit[m].castTime;
				GetComponent<AttackTriggerC>().skill[n].multipleHit[m].skillDelay = dataSkill.skill[skill[n]].multipleHit[m].skillDelay;

				GetComponent<AttackTriggerC>().skill[n].multipleHit[m].soundEffect = dataSkill.skill[skill[n]].multipleHit[m].soundEffect;
			}
			n++;
		}
		if(GetComponent<UiMasterC>()){
			GetComponent<UiMasterC>().SetSkillShortCutIcons();
		}
	}

	void CheckSameSkill(int id , int slot){
		//print (id + " + " + slot);
		SkillDataC dataSkill = database.GetComponent<SkillDataC>();
		int n = 0;
		while(n < skill.Length){
			if(skill[n] == id && n != slot){
				GetComponent<AttackTriggerC>().skill[n].manaCost = 0;
				GetComponent<AttackTriggerC>().skill[n].skillPrefab = null;
				GetComponent<AttackTriggerC>().skill[n].skillAnimation = dataSkill.skill[skill[0]].skillAnimation;
				GetComponent<AttackTriggerC>().skill[n].mecanimTriggerName = dataSkill.skill[skill[0]].mecanimTriggerName;
				
				GetComponent<AttackTriggerC>().skill[n].icon = null;
				GetComponent<AttackTriggerC>().skill[n].iconSprite = dataSkill.skill[0].iconSprite;
				GetComponent<AttackTriggerC>().skill[n].sendMsg = "";
				GetComponent<AttackTriggerC>().skill[n].castEffect = null;
				
				GetComponent<AttackTriggerC>().skill[n].castTime = 0;
				GetComponent<AttackTriggerC>().skill[n].skillDelay = 0;
				GetComponent<AttackTriggerC>().skill[n].whileAttack = dataSkill.skill[skill[0]].whileAttack;
				GetComponent<AttackTriggerC>().skill[n].coolDown = 0;
				GetComponent<AttackTriggerC>().skill[n].skillSpawn = dataSkill.skill[skill[0]].skillSpawn;

				GetComponent<AttackTriggerC>().skill[n].requireWeapon = false;
				GetComponent<AttackTriggerC>().skill[n].requireWeaponType = 0;

				GetComponent<AttackTriggerC>().skill[n].soundEffect = null;
				
				if(GetComponent<AttackTriggerC>().skillCoolDown[n] > 0){
					GetComponent<AttackTriggerC>().skillCoolDown[slot] = GetComponent<AttackTriggerC>().skillCoolDown[n];
				}
				GetComponent<AttackTriggerC>().skillCoolDown[n] = 0;
				skill[n] = 0;
			}
			n++;
		}
		if(GetComponent<UiMasterC>()){
			GetComponent<UiMasterC>().SetSkillShortCutIcons();
		}
	}

	public void LearnSkillByLevel(int lv){
		int c = 0;
		while(c < learnSkill.Length){
			if(lv >= learnSkill[c].level){
				AddSkill(learnSkill[c].skillId);
			}
			c++;
		}
		
	}
	public void AddSkill(int id){
		bool geta= false;
		int pt = 0;
		while(pt < skillListSlot.Length && !geta){
			if(skillListSlot[pt] == id){
				// Check if you already have this skill.
				geta = true;
			}else if(skillListSlot[pt] == 0){
				// Add Skill to empty slot.
				skillListSlot[pt] = id;
				StartCoroutine(ShowLearnedSkill(id));
				geta = true;
			}else{
				pt++;
			}
			
		}
		
	}

	IEnumerator ShowLearnedSkill(int id){
		SkillDataC dataSkill = database.GetComponent<SkillDataC>();
		showSkillLearned = true;
		showSkillName = dataSkill.skill[id].skillName;
		yield return new WaitForSeconds(10.5f);
		showSkillLearned = false;
		
	}
	
	void ResetPosition(){
		//Reset GUI Position when it out of Screen.
		if(windowRect.x >= Screen.width -30 || windowRect.y >= Screen.height -30 || windowRect.x <= -70 || windowRect.y <= -70 ){
			windowRect = originalRect;
		}
	}

	public bool HaveSkill(int id){
		bool have = false;
		for(int a = 0; a < skillListSlot.Length; a++){
			if(skillListSlot[a] == id){
				have = true;
			}
		}
		return have;
	}
}