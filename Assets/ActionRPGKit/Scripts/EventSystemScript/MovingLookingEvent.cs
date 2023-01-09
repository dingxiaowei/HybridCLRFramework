using UnityEngine;
using System.Collections;

public class MovingLookingEvent : MonoBehaviour {
	public Vector3 destinationPosition = Vector3.zero;
	public Transform destinationObject;
	public float moveSpeed = 5;

	public Vector3 lookToRotation = Vector3.zero;
	public Transform lookAtObject;
	public float lookSpeed = 5;

	public bool onMoving = false;
	public bool onLooking = false;
	public bool useCharacterController = false;
	public bool lockYAngle = false;
	public float reachDistance = 0.1f;
	public string sendMsgWhenReact = "";
	public bool stopWhenReach = false;
	[HideInInspector]
	public bool stopSending = false;

	public EventSetting source;

	// Update is called once per frame
	void FixedUpdate(){
		if(onLooking){
			Vector3 lookPos = lookAtObject.position - transform.position;
			if(lockYAngle){
				lookPos.y = 0;
			}
			if(lookPos != Vector3.zero){
				Quaternion rot = Quaternion.LookRotation(lookPos);
				transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * lookSpeed);
			}

		}
		if(onMoving && !useCharacterController){
			if(destinationObject){
				destinationPosition = destinationObject.position;
			}
			float distance = (transform.position - destinationPosition).magnitude;

			if(distance >= reachDistance){
				transform.position = Vector3.MoveTowards(transform.position, destinationPosition , moveSpeed * Time.deltaTime);
			}

			if(distance <= reachDistance){
				//onMoving = false;
				if(stopWhenReach){
					onMoving = false;
					onLooking = false;
				}
				if(sendMsgWhenReact != ""){
					SendMessage(sendMsgWhenReact , SendMessageOptions.DontRequireReceiver);
				}
				if(source && !stopSending){
					stopSending = true;
					source.NextEvent();
					//source = null;
				}
			}
		}
		if(onMoving && useCharacterController){
			if(destinationObject){
				destinationPosition = destinationObject.position;
			}
			MoveTowardsTarget(destinationPosition);
		}
	}

	void MoveTowardsTarget(Vector3 targeta) {
		CharacterController controller = GetComponent<CharacterController>();
		Vector3 offset = targeta - transform.position;
		if(offset.magnitude > reachDistance) {
			offset = offset.normalized * moveSpeed;
			controller.Move(offset * Time.deltaTime);
		}else{
			if(stopWhenReach){
				onMoving = false;
				onLooking = false;
			}
			if(sendMsgWhenReact != ""){
				SendMessage(sendMsgWhenReact , SendMessageOptions.DontRequireReceiver);
			}
			if(source && !stopSending){
				stopSending = true;
				source.NextEvent();
				//source = null;
			}
		}
	}
}
