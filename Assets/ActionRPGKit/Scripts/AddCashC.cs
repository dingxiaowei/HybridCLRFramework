using UnityEngine;
using System.Collections;

[AddComponentMenu("Action-RPG Kit(C#)/Create Pickup Cash")]

public class AddCashC : MonoBehaviour {
	
	public int cashMin = 10;
	public int cashMax = 50;
	public float duration = 30.0f;

	private Transform master;
	
	public Transform popup;
	
	void  Start (){
		master = transform.root;
		GetComponent<Collider>().isTrigger = true;
		if(duration > 0){
			Destroy (gameObject, duration);
		}
	}
	
	void OnTriggerEnter ( Collider other  ){
		//Pick up Item
		if(other.gameObject.tag == "Player"){
			AddCashToPlayer(other.gameObject);
		}
	}
	
	void AddCashToPlayer(GameObject other){
		int gotCash = Random.Range(cashMin , cashMax);
		other.GetComponent<InventoryC>().cash += gotCash;
		master = transform.root;
		
		if(popup){
			Transform pop = Instantiate(popup, transform.position , transform.rotation) as Transform;
			pop.GetComponent<DamagePopupC>().damage = "Money " + gotCash.ToString();
		}
		
		Destroy(master.gameObject);
	}
	
}