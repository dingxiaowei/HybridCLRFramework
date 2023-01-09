using UnityEngine;
using System.Collections;

public class GainEXPC : MonoBehaviour {
	public int expGain = 20;
	GameObject player;
	void Start (){
		player = GameObject.FindWithTag ("Player");
		if(!player){
			return;
		}
		player.GetComponent<StatusC>().gainEXP(expGain);
	}
}