using UnityEngine;
using System.Collections;

public class SetAnimationSpeedC : MonoBehaviour {

	//For Legacy
	public GameObject model;
	public AnimationClip[] animations = new AnimationClip[1];
	public float speed = 1.5f;

	void Start(){
		if(!model){
			print("Please assign the model");
			return;
		}
		for(int i = 0; i < animations.Length; i++){
			model.GetComponent<Animation>()[animations[i].name].speed = speed;
		}
	}

}
