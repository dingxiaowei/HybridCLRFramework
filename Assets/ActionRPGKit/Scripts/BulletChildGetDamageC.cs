using UnityEngine;
using System.Collections;

public class BulletChildGetDamageC : MonoBehaviour {
	public GameObject master;
	
	void  Start (){
		GetComponent<BulletStatusC>().totalDamage = master.GetComponent<BulletStatusC>().totalDamage;
		GetComponent<BulletStatusC>().shooterTag = master.GetComponent<BulletStatusC>().shooterTag;
		
	}
	
}