using UnityEngine;
using System.Collections;

public class ShowTip : MonoBehaviour {
	public Texture2D tip;
	private bool show = true;

	void Update(){
		if(Input.GetKeyDown("h")){
			if(show){
				show = false;
			}else{
				show = true;
			}
		}
	}

	void OnGUI(){
		if(show){
			GUI.DrawTexture( new Rect(Screen.width -315,Screen.height - 435,320,282), tip);
		}
	}
}