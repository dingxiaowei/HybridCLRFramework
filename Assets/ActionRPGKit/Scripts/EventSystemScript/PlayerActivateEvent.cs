using UnityEngine;
using System.Collections;

[AddComponentMenu("Easy Event Maker/PlayerActivateEvent")]

public class PlayerActivateEvent : MonoBehaviour {
	public string triggerKey = "e";
	public float checkDistance = 1.5f;
	public Transform player;
	private bool showUi = false;
	public GUIStyle textStyle;
	public EventActivator activateEvent;

	void Start(){
		if(!player){
			player = transform.root;
		}
	}
	
	void Update(){
		if(GlobalConditionC.freezeAll){
			return;
		}
		if(Input.GetKeyDown(triggerKey)){
			ActivateEvent();
		}
		
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		RaycastHit hit;
		
		if(Physics.Raycast(transform.position, fwd, out hit , checkDistance)){
			//if(hit.transform.GetComponent<EventActivator>()){
			if(hit.transform.tag == "TriggerEvent"){
				showUi = true;
			}else{
				showUi = false;
			}
		}else{
			showUi = false;
		}
	}

	void ActivateEvent(){
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		RaycastHit hit;
		
		if(Physics.Raycast(transform.position, fwd, out hit , checkDistance)){
			if(hit.transform.tag == "TriggerEvent"){
				hit.transform.GetComponent<EventActivator>().ActivateTrigger();
			}
		}
	}
	
	void OnGUI(){
		if(showUi && !GlobalConditionC.freezeAll){
			GUI.Label( new Rect(Screen.width /2 - 50, Screen.height - 65, 100, 50), "[" + triggerKey +"] Check" , textStyle);
		}
	}
}
