using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BulletStatusC))]

public class AreaDamageSkillC : MonoBehaviour {
	public float radius = 5.0f;
	public float delayPerHit = 1.0f;
	public float duration = 3.1f;
	
	private Elementala element = Elementala.Normal;
	private AtkType attackType = AtkType.Physic;
	private float wait = 0;
	private BulletStatusC bl;
	private int variance = 15;
	private Transform popup;
	private GameObject hitEffect;
	
	void Start(){
		bl = GetComponent<BulletStatusC>();
		hitEffect = bl.hitEffect;
		popup = bl.Popup;
		attackType = bl.AttackType;
		element = bl.element;
		variance = bl.variance;
		ApplyDamage();
		if(duration > 0){
			Destroy (gameObject, duration);
		}
	}
	
	void Update (){
		if(wait >= delayPerHit){
			ApplyDamage();
			wait = 0;
		}else{
			wait += Time.deltaTime;
		}
	}
	
	void ApplyDamage(){
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
		
		for(int i = 0; i < hitColliders.Length; i++) {
			if(bl.shooterTag == "Enemy" && hitColliders[i].tag == "Player" && hitColliders[i].tag == "Ally" && hitColliders[i].gameObject != bl.shooter){	  
				Damage(hitColliders[i].gameObject);
				
			}else if(bl.shooterTag == "Player" && hitColliders[i].tag == "Enemy" && hitColliders[i].gameObject != bl.shooter){  	
				Damage(hitColliders[i].gameObject);
			}
		}
	}
	
	void Damage(GameObject target){
		if(hitEffect){
			Instantiate(hitEffect, target.transform.position , transform.rotation);
		}
		int varMin = 100 - variance;
		int varMax = 100 + variance;
		int total = bl.totalDamage * Random.Range(varMin ,varMax) / 100;
		string popDamage = "";
		if(attackType == AtkType.Physic){
			popDamage = target.GetComponent<StatusC>().OnDamage(total , (int)element);
		}else{
			popDamage = target.GetComponent<StatusC>().OnMagicDamage(total , (int)element);
		}
		Transform dmgPop = Instantiate(popup, target.transform.position , transform.rotation) as Transform;	
		dmgPop.GetComponent<DamagePopupC>().damage = popDamage;
		if(bl.flinch){
			Vector3 dir = (target.transform.position - transform.position).normalized;
			target.SendMessage("Flinch" , dir , SendMessageOptions.DontRequireReceiver);
		}
		if(GetComponent<AbnormalStatAttackC>() && popDamage != "Miss" && popDamage != "Evaded" && popDamage != "Guard" && popDamage != "Invulnerable"){
			GetComponent<AbnormalStatAttackC>().InflictAbnormalStats(target);
		}
		
	}
}