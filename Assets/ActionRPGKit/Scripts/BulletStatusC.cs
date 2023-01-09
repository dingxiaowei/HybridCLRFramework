using UnityEngine;
using System.Collections;

public class BulletStatusC : MonoBehaviour {
	public int damage = 10;
	public int damageMax = 20;

	[HideInInspector]
	public int playerAttack = 5;
	public int totalDamage = 0;
	public int variance = 15;
	[Range(0, 100)]
	public int criticalChance = 0;
	public string shooterTag = "Player";
	[HideInInspector]
	public GameObject shooter;
	
	public Transform Popup;
	
	public GameObject hitEffect;
	public bool flinch = false;
	public bool penetrate = false;
	private string popDamage = "";
	
	public AtkType AttackType = AtkType.Physic;
	public Elementala element = Elementala.Normal;
	[Range(0 , 100)]
	public int drainHp = 0;

	public BombHit bombHitSetting;
	//-----------------------------
	void Start(){
		if(variance >= 100){
			variance = 100;
		}
		if(variance <= 1){
			variance = 1;
		}
	}
	
	public void Setting(int str , int mag , string tag , GameObject owner){
		//print ("GuSetLaew");
		if(AttackType == AtkType.Physic){
			playerAttack = str;
		}else{
			playerAttack = mag;
		}
		shooterTag = tag;
		shooter = owner;
		int varMin = 100 - variance;
		int varMax = 100 + variance;
		int randomDmg = Random.Range(damage, damageMax);
		totalDamage = (randomDmg + playerAttack) * Random.Range(varMin ,varMax) / 100;
	}

	void OnTriggerEnter(Collider other){  	
		//When Player Shoot at Enemy
		if(shooterTag == "Player" && other.tag == "Enemy"){	  
			Transform dmgPop = Instantiate(Popup, other.transform.position , transform.rotation) as Transform;

			if(criticalChance >= 1){
				if(criticalChance >= 100){
					criticalChance = 100;
				}
				int per = Random.Range(0, 100);
				if(per <= criticalChance){
					if(Camera.main.GetComponent<ARPGcameraC>())
						Camera.main.GetComponent<ARPGcameraC>().Shake(0.5f , 0.35f);
					dmgPop.GetComponent<DamagePopupC>().critical = true;
					totalDamage *= 2;
				}
			}

			if(AttackType == AtkType.Physic){
				popDamage = other.GetComponent<StatusC>().OnDamage(totalDamage , (int)element);
			}else{
				popDamage = other.GetComponent<StatusC>().OnMagicDamage(totalDamage , (int)element);
			}
			if(shooter && shooter.GetComponent<ShowEnemyHealthC>()){
	    		shooter.GetComponent<ShowEnemyHealthC>().GetHP(other.GetComponent<StatusC>().maxHealth , other.gameObject , other.name);
	    	}
			dmgPop.GetComponent<DamagePopupC>().damage = popDamage;	
			
			if(hitEffect){
				Instantiate(hitEffect, transform.position , transform.rotation);
			}
			if(flinch){
				Vector3 dir = (other.transform.position - transform.position).normalized;
				//other.GetComponent<AIsetC>().Flinch(dir);
				other.SendMessage("Flinch" , dir , SendMessageOptions.DontRequireReceiver);
			}
			if(bombHitSetting.enable){
				ExplosionDamage();
			}
			//Drain HP
			if(drainHp > 0 && shooter && popDamage != "Miss" && popDamage != "Evaded" && popDamage != "Guard" && popDamage != "Invulnerable"){
				int lf = int.Parse(popDamage) * drainHp;
				lf /= 100;
				if(lf < 1){
					lf = 1;
				}
				Vector3 hpos = shooter.transform.position;
				hpos.y += 0.75f;
				Transform hpPop = Instantiate(Popup, hpos , transform.rotation) as Transform;
				hpPop.GetComponent<DamagePopupC>().damage = lf.ToString();
				hpPop.GetComponent<DamagePopupC>().fontStyle.normal.textColor = Color.green;
				shooter.GetComponent<StatusC>().Heal(lf , 0);
			}

			//----------------------------

			if(!penetrate){
				Destroy (gameObject);
			}
			//When Enemy Shoot at Player
		}else if(shooterTag == "Enemy" && other.tag == "Player" || shooterTag == "Enemy" && other.tag == "Ally"){  	
			if(AttackType == AtkType.Physic){
				popDamage = other.GetComponent<StatusC>().OnDamage(totalDamage , (int)element);
			}else{
				popDamage = other.GetComponent<StatusC>().OnMagicDamage(totalDamage , (int)element);
			}
			Transform dmgPop = Instantiate(Popup, transform.position , transform.rotation) as Transform;	
			dmgPop.GetComponent<DamagePopupC>().damage = popDamage;
			dmgPop.GetComponent<DamagePopupC>().fontStyle.normal.textColor = Color.red;
			
			if(hitEffect){
				Instantiate(hitEffect, transform.position , transform.rotation);
			}
			if(flinch){
				Vector3 dir = (other.transform.position - transform.position).normalized;
				//other.GetComponent<AttackTriggerC>().Flinch(dir);
				other.SendMessage("Flinch" , dir , SendMessageOptions.DontRequireReceiver);
			}
			if(bombHitSetting.enable){
				ExplosionDamage();
			}
			//Drain HP
			if(drainHp > 0 && shooter && popDamage != "Miss" && popDamage != "Evaded" && popDamage != "Guard" && popDamage != "Invulnerable"){
				int lf = int.Parse(popDamage) * drainHp;
				lf /= 100;
				if(lf < 1){
					lf = 1;
				}
				Vector3 hpos = shooter.transform.position;
				hpos.y += 0.75f;
				Transform hpPop = Instantiate(Popup, hpos , transform.rotation) as Transform;
				hpPop.GetComponent<DamagePopupC>().damage = lf.ToString();
				hpPop.GetComponent<DamagePopupC>().fontStyle.normal.textColor = Color.green;
				shooter.GetComponent<StatusC>().Heal(lf , 0);
			}
			//----------------------------
			if(!penetrate){
				Destroy (gameObject);
			}
		}
	}

	public void ExplosionDamage(){
		Collider[] hitColliders= Physics.OverlapSphere(transform.position, bombHitSetting.bombRadius);
		if(bombHitSetting.bombEffect){
			Instantiate(bombHitSetting.bombEffect , transform.position , transform.rotation);
		}
		
		for(int i= 0; i < hitColliders.Length; i++) {
			if(shooterTag == "Player" && hitColliders[i].tag == "Enemy"){	  
				DealDamageOnly(hitColliders[i].transform , false);
			}else if(shooterTag == "Enemy" && hitColliders[i].tag == "Player" || shooterTag == "Enemy" && hitColliders[i].tag == "Ally"){  	
				DealDamageOnly(hitColliders[i].transform , true);
			}
		}
		bombHitSetting.enable = false;
	}

	public void DealDamageOnly(Transform other , bool red){
		if(AttackType == AtkType.Physic){
			popDamage = other.GetComponent<StatusC>().OnDamage(totalDamage , (int)element);
		}else{
			popDamage = other.GetComponent<StatusC>().OnMagicDamage(totalDamage , (int)element);
		}
		Transform dmgPop = Instantiate(Popup, other.position , other.rotation) as Transform;	
		dmgPop.GetComponent<DamagePopupC>().damage = popDamage;
		if(red){
			dmgPop.GetComponent<DamagePopupC>().fontStyle.normal.textColor = Color.red;
		}
		
		if(hitEffect){
			Instantiate(hitEffect, other.position , other.rotation);
		}
		if(flinch){
			Vector3 dir = (other.transform.position - transform.position).normalized;
			//other.GetComponent<AttackTriggerC>().Flinch(dir);
			other.SendMessage("Flinch" , dir , SendMessageOptions.DontRequireReceiver);
		}
	}
}

public enum AtkType {
	Physic = 0,
	Magic = 1,
}
public enum Elementala{
	Normal = 0,
	Fire = 1,
	Ice = 2,
	Earth = 3,
	Lightning = 4,
}

[System.Serializable]
public class BombHit{
	public bool enable = false;
	public GameObject bombEffect;
	public float bombRadius = 20;
}