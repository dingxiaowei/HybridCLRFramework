using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowAmmoC : MonoBehaviour {
	public Text ammoQuantity;
	public Image ammoSprite;
	public Image backGround;

	public static ShowAmmoC showAmmo;

	void Start(){
		if(ammoQuantity && ammoSprite){
			showAmmo = GetComponent<ShowAmmoC>();
		}
	}

	public void OnOffShowing(bool s){
		ammoQuantity.gameObject.SetActive(s);
		ammoSprite.gameObject.SetActive(s);
		if(backGround){
			backGround.gameObject.SetActive(s);
		}
	}

	public void UpdateSprite(Sprite s){
		ammoSprite.sprite = s;
	}

	public void UpdateAmmo(int q){
		ammoQuantity.text = q.ToString();
	}
}
