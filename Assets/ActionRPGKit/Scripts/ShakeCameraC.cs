using UnityEngine;
using System.Collections;

public class ShakeCameraC : MonoBehaviour{
	public float shakeValue = 0.3f;
	public float shakeDuration = 0.3f;
	
	void Start(){
		if(Camera.main.GetComponent<ARPGcameraC>()){
			Camera.main.GetComponent<ARPGcameraC>().Shake(shakeValue , shakeDuration);
		}
	}
}