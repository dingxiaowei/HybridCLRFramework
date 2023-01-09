using UnityEngine;
using System.Collections;

public class MinimapOnOffC : MonoBehaviour {
	
	public GameObject minimapCam;
	//private bool  state = true;
	
	void  Update (){
		if(Input.GetKeyDown("m") && minimapCam){
			OnOffCamera();
		}
		
	}

	void  OnOffCamera (){
		if(minimapCam.activeSelf == true){
			minimapCam.SetActive(false);
		}else{
			minimapCam.SetActive(true);
		}
	}
}