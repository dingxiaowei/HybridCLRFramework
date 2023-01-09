using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MoveImage : MonoBehaviour {
	public bool onMoving = false;
	public bool onScaling = false;
	public Vector3 destinationPosition = Vector3.zero;
	public Vector2 size = new Vector2(100 , 100);
	public float speed = 5;
	public float scalingSpeed = 5;

	// Update is called once per frame
	void Update(){
		if(onMoving){
			GetComponent<RectTransform>().anchoredPosition = Vector3.MoveTowards(GetComponent<RectTransform>().anchoredPosition, destinationPosition , speed * Time.deltaTime);
		}
		if(onScaling){
			GetComponent<RectTransform>().sizeDelta = Vector2.MoveTowards(GetComponent<RectTransform>().sizeDelta , size , scalingSpeed * Time.deltaTime);
		}
	}
}
