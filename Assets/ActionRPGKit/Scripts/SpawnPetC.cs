using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BulletStatusC))]

public class SpawnPetC : MonoBehaviour {
	public GameObject petPrefab;
	public float additionY = 0.3f;

	public bool autoSetLvStatus = false; //Pet can get stronger by player's level
	private int currentLv = 1;
	public int maxLevel = 100;
	
	public StatusParam minStatus;
	public StatusParam maxStatus;
	
	private int min = 0;
	private int max = 0;
	
	void Start(){
		GameObject source = GetComponent<BulletStatusC>().shooter;
		if(source.GetComponent<AttackTriggerC>().pet){
			source.GetComponent<AttackTriggerC>().pet.GetComponent<StatusC>().Death();
		}
		Vector3 spawnPoint = transform.position;
		spawnPoint.y += additionY;
		GameObject pet = Instantiate(petPrefab , spawnPoint , source.transform.rotation) as GameObject;
		if(pet.GetComponent<AIfriendC>()){
			pet.GetComponent<AIfriendC>().master = source.transform;
		}
		source.GetComponent<AttackTriggerC>().pet = pet;
		if(autoSetLvStatus){
			currentLv = source.GetComponent<StatusC>().level;
			CalculateStatLv(pet);
		}
		Destroy(gameObject);
	}

	public void CalculateStatLv(GameObject pet){
		StatusC stat = pet.GetComponent<StatusC>();
		//[min_stat*(max_lv-lv)/(max_lv- 1)] + [max_stat*(lv- 1)/(max_lv- 1)]
		
		//Atk
		min = minStatus.atk * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.atk * (currentLv - 1)/(maxLevel - 1);
		stat.atk = min + max;
		//Def
		min = minStatus.def * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.def * (currentLv - 1)/(maxLevel - 1);
		stat.def = min + max;
		//Matk
		min = minStatus.matk * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.matk * (currentLv - 1)/(maxLevel - 1);
		stat.matk = min + max;
		//Mdef
		min = minStatus.mdef * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.mdef * (currentLv - 1)/(maxLevel - 1);
		stat.mdef = min + max;

		//HP
		min = minStatus.maxHealth * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.maxHealth * (currentLv - 1)/(maxLevel - 1);
		stat.maxHealth = min + max;
		stat.health = stat.maxHealth;
		//MP
		min = minStatus.maxMana * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.maxMana * (currentLv - 1)/(maxLevel - 1);
		stat.maxMana = min + max;
		stat.mana = stat.maxMana;
	}
}

[System.Serializable]
public class StatusParam{
	public int atk = 5;
	public int def = 5;
	public int matk = 5;
	public int mdef = 5;
	public int maxHealth = 100;
	public int maxMana = 100;
}
