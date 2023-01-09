using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthBarCanvasC : MonoBehaviour {
	public Image hpBar;
	public Image mpBar;
	public Image expBar;
	public Text hpText;
	public Text mpText;
	public Text lvText;
	public GameObject player;

	public SkillShortCutUI[] skillShortcuts = new SkillShortCutUI[8];

	//public Sprite hp2;
	
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
		
		int maxHp = stat.totalMaxHealth;
		float hp = stat.health;
		int maxMp = stat.totalMaxMana;
		float mp = stat.mana;
		int exp = stat.exp;
		float maxExp = stat.maxExp;
		//float target = (float)cur_hp / (float)cur_mhp;
		float curHp = hp/maxHp;
		float curMp = mp/maxMp;
		float curExp = exp/maxExp;

		/*if(curHp >= 0.75){
			hpBar.color = Color.green;
			hpBar.sprite = hp2;
		}else{
			hpBar.color = Color.red;
		}*/
		
		//HP Gauge
		if(curHp > hpBar.fillAmount){
			hpBar.fillAmount += 1 / 1 * Time.unscaledDeltaTime;
			if(hpBar.fillAmount > curHp){
				hpBar.fillAmount = curHp;
			}
		}	
		if(curHp < hpBar.fillAmount){
			hpBar.fillAmount -= 1 / 1 * Time.unscaledDeltaTime;
			if(hpBar.fillAmount < curHp){
				hpBar.fillAmount = curHp;
			}
		}
		
		//MP Gauge
		if(curMp > mpBar.fillAmount){
			mpBar.fillAmount += 1 / 1 * Time.unscaledDeltaTime;
			if(mpBar.fillAmount > curMp){
				mpBar.fillAmount = curMp;
			}
		}	
		if(curMp < mpBar.fillAmount){
			mpBar.fillAmount -= 1 / 1 * Time.unscaledDeltaTime;
			if(mpBar.fillAmount < curMp){
				mpBar.fillAmount = curMp;
			}
		}
		
		//EXP Gauge
		if(expBar){
			expBar.fillAmount = curExp;
		}
		if(lvText){
			lvText.text = stat.level.ToString();
		}
		if(hpText){
			hpText.text = hp.ToString() + "/" + maxHp.ToString();
		}
		if(mpText){
			mpText.text = mp.ToString() + "/" + maxMp.ToString();
		}

		//Cooldown
		AttackTriggerC atk = player.GetComponent<AttackTriggerC>();
		for(int a = 0; a < skillShortcuts.Length; a++){
			if(atk.skillCoolDown[a] > 0){
				if(skillShortcuts[a].coolDownText){
					skillShortcuts[a].coolDownText.gameObject.SetActive(true);
					skillShortcuts[a].coolDownText.text = atk.skillCoolDown[a].ToString();
				}
				if(skillShortcuts[a].coolDownBackground){
					skillShortcuts[a].coolDownBackground.gameObject.SetActive(true);
				}
			}else{
				if(skillShortcuts[a].coolDownText){
					skillShortcuts[a].coolDownText.gameObject.SetActive(false);
				}
				if(skillShortcuts[a].coolDownBackground){
					skillShortcuts[a].coolDownBackground.SetActive(false);
				}
			}
		}
	}
}

[System.Serializable]
public class SkillShortCutUI{
	public Image skillIcon;
	public GameObject coolDownBackground;
	public Text coolDownText;
}