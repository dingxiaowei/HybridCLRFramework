using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickCanvas : MonoBehaviour , IPointerDownHandler, IPointerUpHandler , IMoveHandler{
	public Vector2 position; // [-1, 1] in x,y
	private bool press = false;
	public float limit = 50;

	void Update(){
		if(press){
			if(Input.touchCount > 0){
				for(int i = 0 ; i < Input.touchCount ; i++){
					Touch touch = Input.GetTouch(i); // only considers the first touch
					switch (touch.phase){
					case TouchPhase.Began:
						//HandleTouchBegan (touch.position);
						transform.position = touch.position;
						break;
					case TouchPhase.Moved:
						//HandleTouchMoved (touch.position);
						transform.position = touch.position;
						break;
					case TouchPhase.Ended:
						//HandleTouchEnded (touch.position);
						transform.position = touch.position;
						break;
					}
				}
			}else{
				//transform.localPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				transform.position = Input.mousePosition;
			}
			//Dead Zone
			if(transform.localPosition.x > limit){
				Vector3 pos = transform.localPosition;
				pos.x = limit;
				transform.localPosition = pos;
			}
			if(transform.localPosition.y > limit){
				Vector3 pos = transform.localPosition;
				pos.y = limit;
				transform.localPosition = pos;
			}
			if(transform.localPosition.x < -limit){
				Vector3 pos = transform.localPosition;
				pos.x = -limit;
				transform.localPosition = pos;
			}
			if(transform.localPosition.y < -limit){
				Vector3 pos = transform.localPosition;
				pos.y = -limit;
				transform.localPosition = pos;
			}
			position.x = transform.localPosition.x / limit;
			position.y = transform.localPosition.y / limit;
			//print(position);
		}
	}
	
	public void ResetJoystick(){
		// Release the finger control and set the joystick back to the default position
		position = Vector2.zero;
		transform.localPosition = Vector3.zero;
	}
	
	public void OnPointerDown(PointerEventData eventData){
		press = true;
		//Debug.Log(this.gameObject.name + " Was Clicked.");
	}
	
	public void OnPointerUp(PointerEventData eventData){
		press = false;
		ResetJoystick();
		//Debug.Log(this.gameObject.name + " Was Released.");
	}

	public void OnMove(AxisEventData eventData){
		Debug.Log("Move");
	}
}
