using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AIsetC))]

public class MonsterSkillC : MonoBehaviour {
	
	public GameObject mainModel;
	public float skillDistance = 4.5f;
	public float delay = 2.0f;
	private bool  begin = false;
	private bool  onSkill = false;
	
	private float wait = 0;
	private GameObject eff;
	
	[System.Serializable]
	public class SkillSetting{
		public string skillName;
		public Transform skillPrefab;
		public AnimationClip skillAnimation;
		public GameObject castEffect;
		public float castTime = 0.5f;
		public float delayTime = 1.5f;
		public bool spawnAtPlayer = false;
	}
	
	public SkillSetting[] skillSet = new SkillSetting[1];
	
	void Start(){
		 if(!mainModel){
		 	mainModel = this.gameObject;
		 }
		StartCoroutine("Begin");
	}
	
	IEnumerator Begin(){
		 yield return new WaitForSeconds(1.5f);
		 begin = true;
	}
	
	void Update(){
		if(GlobalConditionC.freezeAll){
			return;
		}
		if(begin && !onSkill){
		 	if(wait >= delay){
				StartCoroutine("UseSkill");
		    	wait = 0;
		    }else{
		     	wait += Time.deltaTime;
		    }
		}
	}
	
	IEnumerator UseSkill(){
		if(!GetComponent<StatusC>().freeze){
			AIsetC ai = GetComponent<AIsetC>();
			 int c = 0;
			 if(skillSet.Length > 1){
			  	c = Random.Range(0 , skillSet.Length);
			 }
			 onSkill = true;
			  //Cast Effect
			 if(skillSet[c].castEffect){
			 	eff = Instantiate(skillSet[c].castEffect , mainModel.transform.position , mainModel.transform.rotation) as GameObject;
			 	eff.transform.parent = this.transform;
			 }
			 //Call UseSkill Function in AIsetC Script.
			ai.ActivateSkill(skillSet[c].skillPrefab ,skillSet[c].castTime, skillSet[c].delayTime , skillSet[c].skillAnimation.name , skillDistance , skillSet[c].spawnAtPlayer);
			 yield return new WaitForSeconds(skillSet[c].castTime);
			 if(eff){
			 	Destroy(eff);
			 }
			 yield return new WaitForSeconds(skillSet[c].delayTime);
			 onSkill = false;
		}
	}
}
