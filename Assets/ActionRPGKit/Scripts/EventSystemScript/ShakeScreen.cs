using UnityEngine;
using System.Collections;

public class ShakeScreen : MonoBehaviour {
	public bool onShaking = false;
	public float shakeValue = 0.3f;
	private float shakingv = 0.3f;

	public void Shake(float val , float dur){
		if(onShaking){
			return;
		}
		StartCoroutine(Shaking(val , dur));
	}
	
	IEnumerator Shaking(float val , float dur){
		onShaking = true;
		shakingv = val;
		yield return new WaitForSeconds(dur);
		shakingv = 0;
		onShaking = false;
	}
	
	// Update is called once per frame
	void LateUpdate(){
		if(onShaking){
			shakeValue = Random.Range(-shakingv , shakingv)* 0.2f;
			Vector3 s = transform.position;
			s.y += shakeValue;
			transform.position = s;
		}
	}
}
