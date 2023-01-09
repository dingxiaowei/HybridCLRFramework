using UnityEngine;
using System.Collections;

public class StatusC : MonoBehaviour {

	public string characterName = "";
	public int characterId = 0;
	public int level = 1;
	public int atk = 0;
	public int def = 0;
	public int matk = 0;
	public int mdef = 0;
	public int exp = 0;
	public int maxExp = 100;
	public int maxHealth = 100;
	[HideInInspector]
	public int totalMaxHealth = 100;

	public int health = 100;
	public int maxMana = 100;
	[HideInInspector]
	public int totalMaxMana = 100;

	public int mana = 100;
	public int statusPoint = 0;
	public int skillPoint = 0;
	private bool dead = false;
	public bool immortal = false;
	
	[HideInInspector]
	public GameObject mainModel;
	
	[HideInInspector]
		public int addAtk = 0;
	[HideInInspector]
		public int addDef = 0;
	[HideInInspector]
		public int addMatk = 0;
	[HideInInspector]
		public int addMdef = 0;
	[HideInInspector]
		public int addHP = 0;
	[HideInInspector]
		public int addMP = 0;

	public Transform deathBody;
	
	[HideInInspector]
		public string spawnPointName = ""; //Store the name for Spawn Point When Change Scene
	
	//---------States----------
	[HideInInspector]
		public int buffAtk = 0;
	[HideInInspector]
		public int buffDef = 0;
	[HideInInspector]
		public int buffMatk = 0;
	[HideInInspector]
		public int buffMdef = 0;
	
	[HideInInspector]
		public int weaponAtk = 0;
	[HideInInspector]
		public int weaponMatk = 0;
	
	//Negative Buffs
	[HideInInspector]
		public bool poison = false;
	[HideInInspector]
		public bool silence = false;
	[HideInInspector]
		public bool web = false;
	[HideInInspector]
		public bool stun = false;
	
	[HideInInspector]
		public bool freeze = false; // Use for Freeze Character
	[HideInInspector]
		public bool dodge = false;
	
	//Positive Buffs
	[HideInInspector]
		public bool brave = false; //Can be use for Weaken
	[HideInInspector]
		public bool barrier = false;
	[HideInInspector]
		public bool mbarrier = false;
	[HideInInspector]
		public bool faith = false; //Can be use for Clumsy
	[HideInInspector]
		public bool block = false;
	
	//Effect
	public GameObject poisonEffect;
	public GameObject silenceEffect;
	public GameObject stunEffect;
	public GameObject webbedUpEffect;
	
	public AnimationClip stunAnimation;
	public AnimationClip webbedUpAnimation;
	[System.Serializable]
	public class elem{
		public string elementName = "";
		public int effective = 100;
	}
	public elem[] elementEffective = new elem[5];
	// 0 = Normal , 1 = Fire , 2 = Ice , 3 = Earth , 4 = Wind
	public Resist statusResist;
	[HideInInspector]
	public Resist eqResist;
	[HideInInspector]
	public Resist totalResist;
	[HideInInspector]
	public HiddenStat hiddenStatus;
	[HideInInspector]
	public bool useMecanim = false;
	public string sendMsgWhenDead = "";
	[HideInInspector]
	public bool canControl = true;

	void Start(){
		if(characterName != ""){
			gameObject.name = characterName;
		}
		CalculateStatus();
	}
	
	public string OnDamage(int amount , int element){	
		if(!dead){
			if(dodge){
				return "Evaded";
			}
			if(immortal){
				return "Invulnerable";
			}
			if(block){
				return "Guard";
			}
			if(hiddenStatus.autoGuard > 0){
				int ran = Random.Range(0 , 100);
				if(ran <= hiddenStatus.autoGuard){
					return "Guard";
				}
			}
			amount -= def;
			amount -= addDef;
			amount -= buffDef;
		
			//Calculate Element Effective
			amount *= elementEffective [element].effective;
			amount /= 100;
		
			if(amount < 1){
				amount = 1;
			}
			health -= amount;
		
			if(health <= 0){
				health = 0;
				enabled = false;
				dead = true;
				Death();
			}
		}
		return amount.ToString();
	}

	public string OnMagicDamage(int amount , int element){
		if(!dead){
			if(dodge){
				return "Evaded";
			}
			if(immortal){
				return "Invulnerable";
			}
			if(block){
				return "Guard";
			}
			if(hiddenStatus.autoGuard > 0){
				int ran = Random.Range(0 , 100);
				if(ran <= hiddenStatus.autoGuard){
					return "Guard";
				}
			}
			amount -= mdef;
			amount -= addMdef;
			amount -= buffMdef;
		
			//Calculate Element Effective
			amount *= elementEffective[element].effective;
			amount /= 100;
		
			if(amount < 1){
				amount = 1;
			}
		
			health -= amount;
		
			if(health <= 0){
				health = 0;
				enabled = false;
				dead = true;
				Death();
			}
		}
		return amount.ToString();
	}
	
	public void Heal(int hp , int mp){
		health += hp;
		if(health >= totalMaxHealth){
			health = totalMaxHealth;
		}

		mana += mp;
		if(mana >= totalMaxMana){
			mana = totalMaxMana;
		}
	}
	
	public void Death(){
		if(sendMsgWhenDead != ""){
			SendMessage(sendMsgWhenDead , SendMessageOptions.DontRequireReceiver);
		}
		if(gameObject.tag == "Player"){
			SaveData();
			if(GetComponent<UiMasterC>()){
				GetComponent<UiMasterC>().DestroyAllUi();
			}
		}
		Destroy(gameObject);
		if(deathBody){
			Instantiate(deathBody, transform.position , transform.rotation);
		}else{
			print("This Object didn't assign the Death Body");
		}
	}

	public void gainEXP(int gain){
		exp += gain;
		if(exp >= maxExp){
			int remain = exp - maxExp;
			LevelUp(remain);
		}
	}
	
	public void LevelUp(int remainingEXP){
		exp = 0;
		exp += remainingEXP;
		level++;
		statusPoint += 5;
		skillPoint++;
		//Extend the Max EXP, Max Health and Max Mana
		maxExp = 125 * maxExp  / 100;
		maxHealth += 20;
		maxMana += 10;
		//Recover Health and Mana
		CalculateStatus();
		health = totalMaxHealth;
		mana = totalMaxMana;
		gainEXP(0);
		if(GetComponent<SkillWindowC>()){
			GetComponent<SkillWindowC>().LearnSkillByLevel(level);
		}
	}
	
	void SaveData(){
		PlayerPrefs.SetInt("PreviousSave", 10);
		PlayerPrefs.SetString("TempName", characterName);
		PlayerPrefs.SetInt("TempID", characterId);
		PlayerPrefs.SetInt("TempPlayerLevel", level);
		PlayerPrefs.SetInt("TempPlayerATK", atk);
		PlayerPrefs.SetInt("TempPlayerDEF", def);
		PlayerPrefs.SetInt("TempPlayerMATK", matk);
		PlayerPrefs.SetInt("TempPlayerMDEF", mdef);
		PlayerPrefs.SetInt("TempPlayerEXP", exp);
		PlayerPrefs.SetInt("TempPlayerMaxEXP", maxExp);
		PlayerPrefs.SetInt("TempPlayerMaxHP", maxHealth);
		PlayerPrefs.SetInt("TempPlayerMaxMP", maxMana);
		PlayerPrefs.SetInt("TempPlayerSTP", statusPoint);
		PlayerPrefs.SetInt("TempPlayerSKP", skillPoint);
		
		PlayerPrefs.SetInt("TempCash", GetComponent<InventoryC>().cash);
		int itemSize = GetComponent<InventoryC>().itemSlot.Length;
		int a = 0;
		if(itemSize > 0){
			while(a < itemSize){
				PlayerPrefs.SetInt("TempItem" + a.ToString(), GetComponent<InventoryC>().itemSlot[a]);
				PlayerPrefs.SetInt("TempItemQty" + a.ToString(), GetComponent<InventoryC>().itemQuantity[a]);
				a++;
			}
		}
		
		int equipSize = GetComponent<InventoryC>().equipment.Length;
		a = 0;
		if(equipSize > 0){
			while(a < equipSize){
				PlayerPrefs.SetInt("TempEquipm" + a.ToString(), GetComponent<InventoryC>().equipment[a]);
				a++;
			}
		}
		PlayerPrefs.SetInt("TempWeaEquip", GetComponent<InventoryC>().weaponEquip);
		PlayerPrefs.SetInt("TempSubEquip", GetComponent<InventoryC>().subWeaponEquip);
		PlayerPrefs.SetInt("TempArmoEquip", GetComponent<InventoryC>().armorEquip);
		PlayerPrefs.SetInt("TempAccEquip", GetComponent<InventoryC>().accessoryEquip);
		//Save Quest
		int questSize = GetComponent<QuestStatC>().questProgress.Length;
		PlayerPrefs.SetInt("TempQuestSize", questSize);
		a = 0;
		if(questSize > 0){
			while(a < questSize){
				PlayerPrefs.SetInt("TempQuestp" + a.ToString(), GetComponent<QuestStatC>().questProgress[a]);
				a++;
			}
		}
		int questSlotSize = GetComponent<QuestStatC>().questSlot.Length;
		PlayerPrefs.SetInt("TempQuestSlotSize", questSlotSize);
		a = 0;
		if(questSlotSize > 0){
			while(a < questSlotSize){
				PlayerPrefs.SetInt("TempQuestslot" + a.ToString(), GetComponent<QuestStatC>().questSlot[a]);
				a++;
			}
		}
		//Save Skill Slot
		a = 0;
		while(a < GetComponent<SkillWindowC>().skill.Length){
			PlayerPrefs.SetInt("TempSkill" + a.ToString(), GetComponent<SkillWindowC>().skill[a]);
			a++;
		}
		//Skill List Slot
		a = 0;
		while(a < GetComponent<SkillWindowC>().skillListSlot.Length){
			PlayerPrefs.SetInt("TempSkillList" + a.ToString(), GetComponent<SkillWindowC>().skillListSlot[a]);
			a++;
		}	
		print("Saved");
	}
	
	public void CalculateStatus(){
		addAtk = 0;
		addAtk += atk + buffAtk + weaponAtk;
		//addDef += def;
		addMatk = 0;
		addMatk += matk + buffMatk + weaponMatk;
		//addMdef += mdef;
		totalMaxHealth = maxHealth + addHP;
		totalMaxMana = maxMana + addMP;
		//addMdef += mdef;
		if(health >= totalMaxHealth){
			health = totalMaxHealth;
		}

		if(mana >= totalMaxMana){
			mana = totalMaxMana;
		}
		totalResist.poisonResist = statusResist.poisonResist + eqResist.poisonResist;
		totalResist.silenceResist = statusResist.silenceResist + eqResist.silenceResist;
		totalResist.stunResist = statusResist.stunResist + eqResist.stunResist;
		totalResist.webResist = statusResist.webResist + eqResist.webResist;
	}

	//----------States--------
	public IEnumerator OnPoison(int hurtTime){
		int amount = 0;
		GameObject eff = new GameObject();
		Destroy(eff.gameObject);
		if(!poison){
			int chance= 100;
			chance -= totalResist.poisonResist;
			if(chance > 0){
				int per= Random.Range(0, 100);
				if(per <= chance){
					poison = true;
					amount = maxHealth * 2 / 100; // Hurt 2% of Max HP
				}
			
			}
		//--------------------
			while(poison && hurtTime > 0){
				if(poisonEffect){ //Show Poison Effect
					eff = Instantiate(poisonEffect, transform.position, poisonEffect.transform.rotation) as GameObject;
					eff.transform.parent = transform;
				}
				yield return new WaitForSeconds(0.7f); // Reduce HP  Every 0.7f Seconds
				health -= amount;
			
				if (health <= 1){
					health = 1;
				}
				if(eff){ //Destroy Effect if it still on a map
					Destroy(eff.gameObject);
				}
				hurtTime--;
				if(hurtTime <= 0){
					poison = false;
				}
			}
		}
	}

	
	public IEnumerator OnSilence(float dur){
		GameObject eff = new GameObject();
		Destroy(eff.gameObject);
		if(!silence){
			int chance= 100;
			chance -= totalResist.silenceResist;
			if(chance > 0){
				int per= Random.Range(0, 100);
				if(per <= chance){
						silence = true;
					if(silenceEffect){
						eff = Instantiate(silenceEffect, transform.position, transform.rotation) as GameObject;
						eff.transform.parent = transform;
					}
						yield return new WaitForSeconds(dur);
						if(eff){ //Destroy Effect if it still on a map
							Destroy(eff.gameObject);
						}
						silence = false;
				}
				
			}

		}
	}

	public IEnumerator OnWebbedUp(float dur){
		GameObject eff = new GameObject();
		Destroy(eff.gameObject);
		if(!web){
			int chance= 100;
			chance -= totalResist.webResist;
			if(chance > 0){
				int per= Random.Range(0, 100);
				if(per <= chance){
					web = true;
					freeze = true; // Freeze Character On (Character cannot do anything)
					if(webbedUpEffect){
						eff = Instantiate(webbedUpEffect, transform.position, transform.rotation) as GameObject;
						eff.transform.parent = transform;
					}
					if(webbedUpAnimation){// If you Assign the Animation then play it
						if(useMecanim){
							mainModel.GetComponent<Animator>().SetBool("struggle" , true);
						}else{
							mainModel.GetComponent<Animation>()[webbedUpAnimation.name].layer = 25;
							mainModel.GetComponent<Animation>().Play(webbedUpAnimation.name);
						}
					}
					yield return new WaitForSeconds(dur);
					if(eff){ //Destroy Effect if it still on a map
						Destroy(eff.gameObject);
					}
					if(webbedUpAnimation && !useMecanim){// If you Assign the Animation then stop playing
						mainModel.GetComponent<Animation>().Stop(webbedUpAnimation.name);
					}else if(webbedUpAnimation){
						mainModel.GetComponent<Animator>().SetBool("struggle" , false);
					}
					freeze = false; // Freeze Character Off
					web = false;
				}
			}
		}
	}

	public IEnumerator OnStun(float dur){
		GameObject eff = new GameObject();
		Destroy(eff.gameObject);
		if(!stun){
			int chance= 100;
			chance -= totalResist.stunResist;
			if(chance > 0){
				int per= Random.Range(0, 100);
				if(per <= chance){
					stun = true;
					freeze = true; // Freeze Character On (Character cannot do anything)
					if(stunEffect){
						eff = Instantiate(stunEffect, transform.position, stunEffect.transform.rotation) as GameObject;
						eff.transform.parent = transform;
					}
					if(stunAnimation){// If you Assign the Animation then play it
						if(useMecanim){
							mainModel.GetComponent<Animator>().SetBool("stun" , true);
						}else{
							mainModel.GetComponent<Animation>()[stunAnimation.name].layer = 25;
							mainModel.GetComponent<Animation>().Play(stunAnimation.name);
						}
					}
					yield return new WaitForSeconds(dur);
					if(eff){ //Destroy Effect if it still on a map
						Destroy(eff.gameObject);
					}
					if(stunAnimation && !useMecanim){// If you Assign the Animation then stop playing
						mainModel.GetComponent<Animation> ().Stop (stunAnimation.name);
					}else if(stunAnimation){
						mainModel.GetComponent<Animator>().SetBool("stun" , false);
					}
					freeze = false; // Freeze Character Off
					stun = false;
				}
			}
		}
	}

	public void ApplyAbnormalStat(int statId , float dur){
		if(GlobalConditionC.freezePlayer){
			return;
		}
		if(statId == 0){
			OnPoison(Mathf.FloorToInt(dur));
			StartCoroutine(OnPoison(Mathf.FloorToInt(dur)));
		}
		if(statId == 1){
			//OnSilence(dur);
			StartCoroutine(OnSilence(dur));
		}
		if(statId == 2){
			//OnStun(dur);
			StartCoroutine(OnStun(dur));
		}
		if(statId == 3){
			//OnWebbedUp(dur);
			StartCoroutine(OnWebbedUp(dur));
		}
	}
	
	public IEnumerator OnBarrier (int amount , float dur){
		//Increase Defense
		if(!barrier){
			barrier = true;
			buffDef = 0;
			buffDef += amount;
			CalculateStatus();
			yield return new WaitForSeconds(dur);
			buffDef = 0;
			barrier = false;
			CalculateStatus();
		}
	}

	public IEnumerator OnMagicBarrier(int amount , float dur){
		//Increase Magic Defense
		if(!mbarrier){
			mbarrier = true;
			buffMdef = 0;
			buffMdef += amount;
			CalculateStatus();
			yield return new WaitForSeconds(dur);
			buffMdef = 0;
			mbarrier = false;
			CalculateStatus();
		}
	}

	public IEnumerator OnBrave(int amount , float dur){
		//Increase Attack
		if(!brave){
			brave = true;
			buffAtk = 0;
			buffAtk += amount;
			CalculateStatus();
			yield return new WaitForSeconds(dur);
			buffAtk = 0;
			brave = false;
			CalculateStatus();
		}
	}
	
	public IEnumerator OnFaith (int amount , float dur){
		//Increase Magic Attack
		if(!faith){
			faith = true;
			buffMatk = 0;
			buffMatk += amount;
			CalculateStatus();
			yield return new WaitForSeconds(dur);
			buffMatk = 0;
			faith = false;
			CalculateStatus();
		}
	}

	public void ApplyBuff(int statId , float dur , int amount){
		if(statId == 1){
			//Increase Defense
			StartCoroutine(OnBarrier(amount , dur));
		}
		if(statId == 2){
			//Increase Magic Defense
			StartCoroutine(OnMagicBarrier(amount , dur));
		}
		if(statId == 3){
			//Increase Attack
			StartCoroutine(OnBrave(amount , dur));
		}
		if(statId == 4){
			//Increase Magic Attack
			StartCoroutine(OnFaith(amount , dur));
		}
	}

	public IEnumerator GuardUp(string anim){
		if(block){
			yield break;
		}
		block = true;
		float wait = 0.75f;
		if(!useMecanim && anim != ""){
			//For Legacy Animation
			mainModel.GetComponent<Animation>()[anim].layer = 10;
			mainModel.GetComponent<Animation>().PlayQueued(anim, QueueMode.PlayNow);
			wait = mainModel.GetComponent<Animation>()[anim].length;
		}else if(anim != ""){
			//For Mecanim Animation
			GetComponent<PlayerMecanimAnimationC>().AttackAnimation(anim);
			wait = GetComponent<PlayerMecanimAnimationC>().animator.GetCurrentAnimatorClipInfo(0).Length;
		}
		yield return new WaitForSeconds(wait);
		GuardBreak();
	}
	
	public void GuardBreak(){
		block = false;
		/*if(GetComponent<LegacyAnimationSet>()){
			GetComponent<LegacyAnimationSet>().StopGuard();
		}*/
	}
	
	public void FallingDamage(int amount){
		if(GlobalConditionC.freezeAll){
			return;
		}
		int dmg = maxHealth * amount;
		dmg /= 100;
		print("Falling " + dmg);
		
		health -= dmg;
		if(health <= 0){
			health = 0;
			Death();
		}
	}
}

[System.Serializable]
public class Resist{
	public int poisonResist = 0;
	public int silenceResist = 0;
	public int webResist = 0;
	public int stunResist = 0;
}

[System.Serializable]
public class HiddenStat{
	public bool doubleJump = false;
	public int drainTouch = 0;
	public int autoGuard = 0;
	public int mpReduce = 0;
}
