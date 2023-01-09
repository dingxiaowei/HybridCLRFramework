using UnityEngine;
using System.Collections;

public class AutoSetRotation : MonoBehaviour {
	public Vector3 rotationSet = Vector3.zero;
	// Use this for initialization
	void Start(){
		transform.eulerAngles = rotationSet;
	}
}
