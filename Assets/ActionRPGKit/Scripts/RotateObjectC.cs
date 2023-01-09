using UnityEngine;
using System.Collections;

public class RotateObjectC : MonoBehaviour {

public float rotateX = 0.0f;
public float rotateY = 5.0f;
public float rotateZ = 0.0f;

	void  Update (){
		transform.Rotate(rotateX*Time.deltaTime,rotateY*Time.deltaTime,rotateZ*Time.deltaTime);
	}
}
