using UnityEngine;
using System.Collections;

public class UiMasterC : MonoBehaviour {
	public GameObject eventSystemPrefab;
	public GameObject healthBarPrefab;
	public GameObject statusWindowPrefab;
	public GameObject skillWindowPrefab;
	public GameObject inventoryPrefab;
	public GameObject questWindowPrefab;
	private GameObject st;
	private GameObject sk;
	private GameObject inv;
	private GameObject ev;
	private GameObject hp;
	private GameObject qu;

	public SkillShortCutUI[] mobileSkillIcon;

	void Start(){
		if(eventSystemPrefab){
			ev = Instantiate(eventSystemPrefab , eventSystemPrefab.transform.position , eventSystemPrefab.transform.rotation) as GameObject;
			DontDestroyOnLoad(ev.gameObject);
		}
		if(healthBarPrefab){
			hp = Instantiate(healthBarPrefab , healthBarPrefab.transform.position , healthBarPrefab.transform.rotation) as GameObject;
			hp.GetComponent<HealthBarCanvasC>().player = this.gameObject;
			SetSkillShortCutIcons();
		}
		if(statusWindowPrefab){
			st = Instantiate(statusWindowPrefab , statusWindowPrefab.transform.position , statusWindowPrefab.transform.rotation) as GameObject;
			st.GetComponent<StatusWindowCanvasC>().player = this.gameObject;
			DontDestroyOnLoad(st.gameObject);
			st.SetActive(false);
		}
		if(inventoryPrefab){
			inv = Instantiate(inventoryPrefab , inventoryPrefab.transform.position , inventoryPrefab.transform.rotation) as GameObject;
			inv.GetComponent<InventoryUiCanvasC>().player = this.gameObject;
			DontDestroyOnLoad(inv.gameObject);
			inv.SetActive(false);
		}
		if(skillWindowPrefab){
			sk = Instantiate(skillWindowPrefab , skillWindowPrefab.transform.position , skillWindowPrefab.transform.rotation) as GameObject;
			sk.GetComponent<SkillTreeCanvasC>().player = this.gameObject;
			DontDestroyOnLoad(sk.gameObject);
			sk.SetActive(false);
		}
		if(questWindowPrefab){
			qu = Instantiate(questWindowPrefab , questWindowPrefab.transform.position , questWindowPrefab.transform.rotation) as GameObject;
			qu.GetComponent<QuestUiCanvasC>().player = this.gameObject;
			DontDestroyOnLoad(qu.gameObject);
			qu.SetActive(false);
		}
	}
	
	void Update(){
		if(GlobalConditionC.freezeAll){
			return;
		}
		if(st && Input.GetKeyDown("c")){
			OnOffStatusMenu();
		}
		if(inv && Input.GetKeyDown("i")){
			OnOffInventoryMenu();
		}
		if(sk && Input.GetKeyDown("k")){
			OnOffSkillMenu();
		}
		if(qu && Input.GetKeyDown("q")){
			OnOffQuestMenu();
		}

		if(mobileSkillIcon.Length > 0){
			AttackTriggerC atk = GetComponent<AttackTriggerC>();
			for(int a = 0; a < mobileSkillIcon.Length; a++){
				if(atk.skillCoolDown[a] > 0){
					if(mobileSkillIcon[a].coolDownText){
						mobileSkillIcon[a].coolDownText.gameObject.SetActive(true);
						mobileSkillIcon[a].coolDownText.text = atk.skillCoolDown[a].ToString();
					}
					if(mobileSkillIcon[a].coolDownBackground){
						mobileSkillIcon[a].coolDownBackground.gameObject.SetActive(true);
					}
				}else{
					if(mobileSkillIcon[a].coolDownText){
						mobileSkillIcon[a].coolDownText.gameObject.SetActive(false);
					}
					if(mobileSkillIcon[a].coolDownBackground){
						mobileSkillIcon[a].coolDownBackground.SetActive(false);
					}
				}
			}
		}
	}
	
	public void CloseAllMenu(){
		if(st)
			st.SetActive(false);
		if(inv)
			inv.SetActive(false);
		if(sk)
			sk.SetActive(false);
		if(qu)
			qu.SetActive(false);
	}
	
	public void OnOffStatusMenu(){
		//Freeze Time Scale to 0 if Status Window is Showing
		if(st.activeSelf == false){
			Time.timeScale = 0.0f;
			//Screen.lockCursor = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			CloseAllMenu();
			st.SetActive(true);
		}else{
			Time.timeScale = 1.0f;
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			CloseAllMenu();
		}
	}
	
	public void OnOffInventoryMenu(){
		//Freeze Time Scale to 0 if Status Window is Showing
		if(inv.activeSelf == false){
			Time.timeScale = 0.0f;
			//Screen.lockCursor = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			CloseAllMenu();
			inv.SetActive(true);
		}else{
			Time.timeScale = 1.0f;
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			CloseAllMenu();
		}
	}

	public void OnOffSkillMenu(){
		//Freeze Time Scale to 0 if Status Window is Showing
		if(sk.activeSelf == false){
			Time.timeScale = 0.0f;
			//Screen.lockCursor = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			CloseAllMenu();
			sk.SetActive(true);
			sk.GetComponent<SkillTreeCanvasC>().Start();
		}else{
			Time.timeScale = 1.0f;
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			CloseAllMenu();
		}
	}

	public void OnOffQuestMenu(){
		//Freeze Time Scale to 0 if Status Window is Showing
		if(qu.activeSelf == false){
			Time.timeScale = 0.0f;
			//Screen.lockCursor = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			CloseAllMenu();
			qu.SetActive(true);
			qu.GetComponent<QuestUiCanvasC>().UpdateQuestDetails();
		}else{
			Time.timeScale = 1.0f;
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			CloseAllMenu();
		}
	}

	public void DestroyAllUi(){
		if(st)
			Destroy(st);
		if(inv)
			Destroy(inv);
		if(sk)
			Destroy(sk);
		if(ev)
			Destroy(ev);
		if(hp)
			Destroy(hp);
		if(ev)
			Destroy(ev);
		if(qu)
			Destroy(qu);
	}

	public void SetSkillShortCutIcons(){
		if(!hp){
			return;
		}
		AttackTriggerC atk = GetComponent<AttackTriggerC>();
		for(int a = 0; a < hp.GetComponent<HealthBarCanvasC>().skillShortcuts.Length; a++){
			hp.GetComponent<HealthBarCanvasC>().skillShortcuts[a].skillIcon.sprite = atk.skill[a].iconSprite;
		}

		for(int a = 0; a < mobileSkillIcon.Length; a++){
			mobileSkillIcon[a].skillIcon.sprite = atk.skill[a].iconSprite;
		}
	}
}