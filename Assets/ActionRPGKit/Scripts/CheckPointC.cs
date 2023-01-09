using UnityEngine;
using System.Collections;

public class CheckPointC : MonoBehaviour {
	
	private GameObject player;
	
	void  OnTriggerEnter ( Collider other  ){
		if (other.gameObject.tag == "Player") {
			player = other.gameObject;
			SaveData();
		}
	}
	
	void  SaveData (){
		PlayerPrefs.SetInt("PreviousSave", 10);
		PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
		PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
		PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);
	/*	PlayerPrefs.SetInt("PlayerLevel", player.GetComponent<Status>().level);
		PlayerPrefs.SetInt("PlayerATK", player.GetComponent<Status>().atk);
		PlayerPrefs.SetInt("PlayerDEF", player.GetComponent<Status>().def);
		PlayerPrefs.SetInt("PlayerMATK", player.GetComponent<Status>().matk);
		PlayerPrefs.SetInt("PlayerMDEF", player.GetComponent<Status>().mdef);
		PlayerPrefs.SetInt("PlayerEXP", player.GetComponent<Status>().exp);
		PlayerPrefs.SetInt("PlayerMaxEXP", player.GetComponent<Status>().maxExp);
		PlayerPrefs.SetInt("PlayerMaxHP", player.GetComponent<Status>().maxHealth);
		//	PlayerPrefs.SetInt("PlayerHP", player.GetComponent<Status>().health);
		PlayerPrefs.SetInt("PlayerMaxMP", player.GetComponent<Status>().maxMana);
		//	PlayerPrefs.SetInt("PlayerMP", player.GetComponent<Status>().mana);
		PlayerPrefs.SetInt("PlayerSTP", player.GetComponent<Status>().statusPoint);
		
		PlayerPrefs.SetInt("Cash", player.GetComponent<Inventory>().cash);
		int itemSize = player.GetComponent<Inventory>().itemSlot.Length;
		int a = 0;
		if(itemSize > 0){
			while(a < itemSize){
				PlayerPrefs.SetInt("Item" + a.ToString(), player.GetComponent<Inventory>().itemSlot[a]);
				PlayerPrefs.SetInt("ItemQty" + a.ToString(), player.GetComponent<Inventory>().itemQuantity[a]);
				a++;
			}
		}
		
		int equipSize = player.GetComponent<Inventory>().equipment.Length;
		a = 0;
		if(equipSize > 0){
			while(a < equipSize){
				PlayerPrefs.SetInt("Equipm" + a.ToString(), player.GetComponent<Inventory>().equipment[a]);
				a++;
			}
		}
		PlayerPrefs.SetInt("WeaEquip", player.GetComponent<Inventory>().weaponEquip);
		PlayerPrefs.SetInt("ArmoEquip", player.GetComponent<Inventory>().armorEquip);*/
		print("Saved");
	}
}