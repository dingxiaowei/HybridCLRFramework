using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeImage : MonoBehaviour {
	public bool fading = false;
	public Color startColor = Color.white;
	public Color endColor = Color.black;
	public float lerpSpeed = 1;
	private float t = 0;
	// Use this for initialization

	public void SetFirstColor(Color c){
		t = 0;
		GetComponent<Image>().color = c;
	}
	
	// Update is called once per frame
	void Update(){
		if(fading){
			GetComponent<Image>().color = Color.Lerp(startColor, endColor, t);

			if(t < 1){ // while t below the end limit...
				t += Time.deltaTime/lerpSpeed;
			}
			if(GetComponent<Image>().color == endColor){
				fading = false;
			}
		}
	}
}
