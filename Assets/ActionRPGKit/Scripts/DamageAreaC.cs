using UnityEngine;
using System.Collections;

public class DamageAreaC : MonoBehaviour {
	public int damage = 50;
	
	void  OnTriggerEnter ( Collider other  ){
		if (other.gameObject.tag == "Player") {
			other.GetComponent<StatusC>().OnDamage(damage , 0);
		}
	}
}