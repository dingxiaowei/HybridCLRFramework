using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatusWindowCanvasC : MonoBehaviour {
	public GameObject player;
	public Text charName;
	public Text lv;
	public Text atk;
	public Text def;
	public Text matk;
	public Text mdef;
	public Text exp;
	public Text nextLv;
	public Text stPoint;
	
	public Text totalAtk;
	public Text totalDef;
	public Text totalMatk;
	public Text totalMdef;
	
	public Button atkUpButton;
	public Button defUpButton;
	public Button matkUpButton;
	public Button mdefUpButton;
	
	void Start(){
		DontDestroyOnLoad(transform.gameObject);
		if(!player){
			player = GameObject.FindWithTag("Player");
		}
	}
	
	void Update(){
		if(!player){
			Destroy(gameObject);
			return;
		}
		StatusC stat = player.GetComponent<StatusC>();
		if(charName){
			charName.text = stat.characterName.ToString();
		}
		if(lv){
			lv.text = stat.level.ToString();
		}
		if(atk){
			atk.text = stat.atk.ToString();
		}
		if(def){
			def.text = stat.def.ToString();
		}
		if(matk){
			matk.text = stat.matk.ToString();
		}
		if(mdef){
			mdef.text = stat.mdef.ToString();
		}
		
		if(exp){
			exp.text = stat.exp.ToString();
		}
		if(nextLv){
			nextLv.text = (stat.maxExp - stat.exp).ToString();
		}
		if(stPoint){
			stPoint.text = stat.statusPoint.ToString();
		}
		
		if(totalAtk){
			totalAtk.text = "(" + stat.addAtk.ToString() + ")";
		}
		if(totalDef){
			totalDef.text = "(" + (stat.def + stat.addDef + stat.buffDef).ToString() + ")";
		}
		if(totalMatk){
			totalMatk.text = "(" + stat.addMatk.ToString() + ")";
		}
		if(totalMdef){
			totalMdef.text = "(" + (stat.mdef + stat.addMdef + stat.buffMdef).ToString() + ")";
		}
		
		if(stat.statusPoint > 0){
			if(atkUpButton)
				atkUpButton.gameObject.SetActive(true);
			if(defUpButton)
				defUpButton.gameObject.SetActive(true);
			if(matkUpButton)
				matkUpButton.gameObject.SetActive(true);
			if(mdefUpButton)
				mdefUpButton.gameObject.SetActive(true);
		}else{
			if(atkUpButton)
				atkUpButton.gameObject.SetActive(false);
			if(defUpButton)
				defUpButton.gameObject.SetActive(false);
			if(matkUpButton)
				matkUpButton.gameObject.SetActive(false);
			if(mdefUpButton)
				mdefUpButton.gameObject.SetActive(false);
		}
		
	}
	
	public void UpgradeStatus(int statusId){
		//0 = Atk , 1 = Def , 2 = Matk , 3 = Mdef
		if(!player){
			return;
		}
		StatusC stat = player.GetComponent<StatusC>();
		if(statusId == 0 && stat.statusPoint > 0){
			stat.atk += 1;
			stat.statusPoint -= 1;
			stat.CalculateStatus();
		}
		if(statusId == 1 && stat.statusPoint > 0){
			stat.def += 1;
			stat.maxHealth += 5;
			stat.statusPoint -= 1;
			stat.CalculateStatus();
		}
		if(statusId == 2 && stat.statusPoint > 0){
			stat.matk += 1;
			stat.maxMana += 3;
			stat.statusPoint -= 1;
			stat.CalculateStatus();
		}
		if(statusId == 3 && stat.statusPoint > 0){
			stat.mdef += 1;
			stat.statusPoint -= 1;
			stat.CalculateStatus();
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
