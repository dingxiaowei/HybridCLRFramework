using UnityEngine;
using System.Collections;

public class UnderwaterC : MonoBehaviour {
	//This script enables underwater effects. Attach to your Water GameObject
	//The player must have UnderwaterController script attached
	 
	//Define variables
	public GameObject surface;
	public float surfaceEnterPlus = 0.4f;
	public float surfaceExitPlus = 0.95f;
	public float fogDensity = 0.04f;
	public Color fogColor = new Color (0, 0.4f, 0.7f, 0.6f);
	public float divingTime = 1.0f;
	public GameObject hitEffect;
	 
	//Underwater Fog Setting
	private bool defaultFog;
	private Color defaultFogColor;
	private float defaultFogDensity;
	private Color defaultLightColor;
	private float defaultLightIntensity;
	
	//Underwater Light Setting
	public GameObject mainLight;
	public Color underWaterLightColor = new Color (0, 0.4f, 0.7f, 0.6f);
	public float underWaterIntensity = 0.5f;
	public bool cannotAttack = true;
	
	private bool onEnter = false;
	private bool onUnderwater = false;
	
	private GameObject mainCam;
	private GameObject player;
	private bool jumping = false;
	 
	void Start(){
		if(!mainCam){
			mainCam = GameObject.FindWithTag ("MainCamera");	//Finding your Main Camera
		}
		if(!surface){
			surface = this.gameObject;
		}
		defaultFog = RenderSettings.fog;
		defaultFogColor = RenderSettings.fogColor;
		defaultFogDensity = RenderSettings.fogDensity;
		if(mainLight){
			defaultLightColor = mainLight.GetComponent<Light>().color;
			defaultLightIntensity = mainLight.GetComponent<Light>().intensity;
		}
	}
	 
	void Update(){
		if(!mainCam){
			mainCam = GameObject.FindWithTag ("MainCamera");
		}
		//Check if Main Camera is lower than water surface.
		if(mainCam.transform.position.y < surface.transform.position.y) {
			RenderSettings.fog = true;
			RenderSettings.fogColor = fogColor;
			RenderSettings.fogDensity = fogDensity;
			if(mainLight){
				mainLight.GetComponent<Light>().color = underWaterLightColor;
				mainLight.GetComponent<Light>().intensity = underWaterIntensity;
			}
		}else{
			RenderSettings.fog = defaultFog;
			RenderSettings.fogColor = defaultFogColor;
			RenderSettings.fogDensity = defaultFogDensity;
			if(mainLight){
				mainLight.GetComponent<Light>().color = defaultLightColor;
				mainLight.GetComponent<Light>().intensity = defaultLightIntensity;
			}
		}
		//------------------------------------------------------------------
		if(!player){
			player = GameObject.FindWithTag ("Player");
			return;
		}
		if(jumping && player){
			player.GetComponent<CharacterController>().Move(Vector3.up * 6 * Time.deltaTime);
		}
		//Check if Player is lower than water surface.
		if (player.transform.position.y < surface.transform.position.y - surfaceEnterPlus && !onUnderwater) {
			if(hitEffect){
				Instantiate(hitEffect, player.transform.position , player.transform.rotation);
			}
			//ActivateWaterController();
			StartCoroutine("ActivateWaterController");
		}else if(onUnderwater && player.transform.position.y > surface.transform.position.y - surfaceExitPlus){
			//ActivateGroundController();
			StartCoroutine("ActivateGroundController");
		}
	}
	 
	 IEnumerator ActivateWaterController(){
	 		if(!onEnter){
		 			if(player.GetComponent<UnderwaterControllerC>()){
		 				onEnter = true;
		 				yield return new WaitForSeconds(divingTime);
			 			player.GetComponent<PlayerInputControllerC>().enabled = false;
						player.GetComponent<CharacterMotorC>().enabled = false;
						player.GetComponent<UnderwaterControllerC>().enabled = true;
						
						if(player.GetComponent<PlayerAnimationC>()){
							//If using Legacy Animation
							player.GetComponent<PlayerAnimationC>().enabled = false;
						}else{
							//If using Mecanim Animation
							player.GetComponent<PlayerMecanimAnimationC>().enabled = false;
							player.GetComponent<UnderwaterControllerC>().MecanimEnterWater();
						}

						player.GetComponent<UnderwaterControllerC>().surfaceExit = surface.transform.position.y - surfaceExitPlus - 0.7f;
						if(cannotAttack){
							player.GetComponent<AttackTriggerC>().freeze = true;
						}
						onEnter = false;
		 		}
				onUnderwater = true;
	 		}
	}
	
	IEnumerator ActivateGroundController(){
			if(!onEnter){
		 			if(player.GetComponent<UnderwaterControllerC>() && player.GetComponent<UnderwaterControllerC>().enabled == true){
		 				onEnter = true;
		 				jumping = true;
		 				yield return new WaitForSeconds(0.2f);
		 				jumping = false;
			 			player.GetComponent<PlayerInputControllerC>().enabled = true;
						if(player.GetComponent<PlayerAnimationC>()){
							//If using Legacy Animation
							player.GetComponent<PlayerAnimationC>().enabled = true;
						}else{
							//If using Mecanim Animation
							player.GetComponent<PlayerMecanimAnimationC>().enabled = true;
							player.GetComponent<UnderwaterControllerC>().MecanimExitWater();
						}

						player.GetComponent<CharacterMotorC>().enabled = true;
						player.GetComponent<UnderwaterControllerC>().enabled = false;

						player.GetComponent<AttackTriggerC>().freeze = false;
						onEnter = false;
						
		 		}
				onUnderwater = false;
	 		}
	}
}
