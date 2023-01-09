using UnityEngine;
using System.Collections;

[RequireComponent(typeof(StatusC))]
public class AutoCalculateStatus : MonoBehaviour {
	public int currentLv = 1;
	public int maxLevel = 100;

	public StatusParam minStatus;
	public StatusParam maxStatus;
	
	private int min = 0;
	private int max = 0;

	void Start(){
		CalculateStatLv();
	}
	
	public void CalculateStatLv(){
		StatusC stat = GetComponent<StatusC>();
		currentLv = stat.level;
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
		
		stat.CalculateStatus();
	}
}
