using UnityEngine;
using System.Collections;

public class UpperBodyAnimationMixC : MonoBehaviour {
	public GameObject mainModel;
	public Transform upperBody;
	public AnimationClip[] animationFile = new AnimationClip[1];
	
	void Start(){
		if(!mainModel){
			mainModel = GetComponent<StatusC>().mainModel;
		}
		int c = 0;
		if(animationFile.Length > 0){
			while(c < animationFile.Length && animationFile[c]){
				mainModel.GetComponent<Animation>()[animationFile[c].name].AddMixingTransform(upperBody);
				c++;
			}
		}
	}
}