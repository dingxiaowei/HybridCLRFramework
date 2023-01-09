using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkillTreeCanvasC : MonoBehaviour {
	public SkillSlot[] skillSlots = new SkillSlot[5];
	public Text skillPointText;
	public GameObject shortcutPanel;
	public GameObject player;
	private int buttonSelect = 0;
	public Image[] shortcutIcons;
	
	public void Start(){
		CheckUnlockSkill();
		UpdateSkillButton();
	}
	
	void Update(){
		if(!player){
			return;
		}
		if(skillPointText){
			skillPointText.text = player.GetComponent<StatusC>().skillPoint.ToString();
		}
	}
	
	public void CheckLearnedSkill(){
		if(!player){
			return;
		}
		SkillWindowC sk = player.GetComponent<SkillWindowC>();
		for(int a = 0; a < skillSlots.Length; a++){
			if(skillSlots[a].skillId > 0){
				skillSlots[a].learned = sk.HaveSkill(skillSlots[a].skillId);
			}
		}
	}
	
	public void CheckUnlockSkill(){
		if(!player){
			return;
		}
		CheckLearnedSkill();
		SkillWindowC sk = player.GetComponent<SkillWindowC>();
		int lv = player.GetComponent<StatusC>().level;
		
		for(int a = 0; a < skillSlots.Length; a++){
			//Check Player Level
			bool  lvPass = false;
			int allUnlock = 0;
			if(lv >= skillSlots[a].unlockLevel){
				lvPass = true;
			}
			
			//Check unlockConditionId
			if(skillSlots[a].unlockConditionId.Length > 0){
				allUnlock = 0;
				for(int b = 0; b < skillSlots[a].unlockConditionId.Length; b++){
					if(sk.HaveSkill(skillSlots[a].unlockConditionId[b])){
						allUnlock++;
					}
				}
				
			}
			//If Overall Pass
			if(lvPass && allUnlock >= skillSlots[a].unlockConditionId.Length){
				skillSlots[a].locked = false;
			}
		}
	}
	
	public void ButtonSkillClick(int buttonId){
		if(!player || skillSlots[buttonId].locked){
			return;
		}
		if(!skillSlots[buttonId].learned){
			LearnSkill(buttonId);
		}else{
			buttonSelect = buttonId;
			ShowShortcutSetting();
		}
	}
	
	public void ShowShortcutSetting(){
		shortcutPanel.SetActive(true);
		shortcutPanel.transform.position = Input.mousePosition;
		for(int a = 0; a < shortcutIcons.Length; a++){
			if(player && a < player.GetComponent<AttackTriggerC>().skill.Length){
				shortcutIcons[a].sprite = player.GetComponent<AttackTriggerC>().skill[a].iconSprite;
			}
		}
	}
	
	public void SetSlot(int slot){
		shortcutPanel.SetActive(false);
		SkillWindowC sk = player.GetComponent<SkillWindowC>();
		sk.AssignSkillByID(slot , skillSlots[buttonSelect].skillId);
	}
	
	public void LearnSkill(int buttonId){
		if(player.GetComponent<StatusC>().skillPoint < skillSlots[buttonId].skPointUse){
			print("Not enough Skill Point");
			return;
		}
		player.GetComponent<StatusC>().skillPoint -= skillSlots[buttonId].skPointUse;
		SkillWindowC sk = player.GetComponent<SkillWindowC>();
		
		sk.AddSkill(skillSlots[buttonId].skillId);
		CheckUnlockSkill();
		UpdateSkillButton();
	}
	
	public void UpdateSkillButton(){
		SkillButtonCanvasC[] skillButton;
		skillButton = GetComponentsInChildren<SkillButtonCanvasC>();
		if(skillButton != null) {
			foreach(SkillButtonCanvasC button in skillButton){
				button.UpdateIcon();
			}
		}
	}
	
	public void CloseMenu(){
		Time.timeScale = 1.0f;
		//Screen.lockCursor = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		gameObject.SetActive(false);
	}
	
}

[System.Serializable]
public class SkillSlot{
	public string skillName = "";
	public int skillId = 0;
	public int[] unlockConditionId;
	public int unlockLevel = 1;
	public int skPointUse = 1;
	
	public bool learned = false;
	public bool locked = false;
}