using UnityEngine;
using System.Collections;

[AddComponentMenu("Action-RPG Kit(C#)/Create Pickup Item")]

public class AddItemC : MonoBehaviour {
	
	public int itemID = 0;
	public int itemQuantity = 1;
	public string textPopup = "";
	
	public ItType itemType = ItType.Usable; 
	
	public float duration = 30.0f;
	private Transform master;
	
	public Transform popup;
	
	void Start(){
		master = transform.root;
		GetComponent<Collider>().isTrigger = true;
		if(duration > 0){
			Destroy(master.gameObject, duration);
		}
	}
	
	void OnTriggerEnter(Collider other){
		//Pick up Item
		if(other.gameObject.tag == "Player") {
			AddItemToPlayer(other.gameObject);
		}
	}
	
	void AddItemToPlayer(GameObject other){
		bool full = false;
		if(itemType == ItType.Usable){
			full = other.GetComponent<InventoryC>().AddItem(itemID , itemQuantity);
		}else{
			full = other.GetComponent<InventoryC>().AddEquipment(itemID);
		}
		
		if(!full){
			master = transform.root;
			
			if(popup && textPopup != ""){
				Transform pop = Instantiate(popup, transform.position , transform.rotation) as Transform;
				pop.GetComponent<DamagePopupC>().damage = textPopup;
			}
			Destroy(master.gameObject);
		}
	}
}

public enum ItType {
	Usable = 0,
	Equipment = 1,
}