using UnityEngine;
using System.Collections;

public class DropItemC : MonoBehaviour {

	[System.Serializable]
	public class ItemDrop{
		public GameObject itemPrefab;
		[Range (0, 100)]
		public int dropChance = 20;
	}
	public ItemDrop[] itemDropSetting = new ItemDrop[1];
	public float randomPosition = 1.0f;
	public float dropUpward = 0;
	
	void  Start (){
		for(int n = 0; n < itemDropSetting.Length ; n++){
			int ran = Random.Range(0 , 100);
			if(ran <= itemDropSetting[n].dropChance){
				Vector3 ranPos = transform.position; //Slightly Random x z position.
				ranPos.x += Random.Range(0.0f,randomPosition);
				ranPos.z += Random.Range(0.0f,randomPosition);
				ranPos.y += dropUpward;
				//Drop Item
				Instantiate(itemDropSetting[n].itemPrefab , ranPos , itemDropSetting[n].itemPrefab.transform.rotation);
			}
		}
		
	}
	
}