using UnityEngine;
using System.Collections;

public class GlobalConditionC : MonoBehaviour {
	public int eventId = 1;
	public int conditionValue = 1; //If eventVar[eventId] value Greater than or Equal this value. Condition is Pass
	public GameObject targetObject;
	public EvAction eventAction = EvAction.Spawn;
	public string sendMsgTo = "";
	
	public bool keepUpdate = false;
	public bool checkFromStart = true;
	public bool destroyWhenPass = true;

	public static GameObject mainPlayer;
	public static int[] eventVar = new int[20]; //Stored all event condition variable
	public static bool freezeAll = false;
	public static bool freezePlayer = false;
	public static bool interacting = false;
	public static bool freezeCam = false;
	public static int playerId = 0;

	void Start(){
		if(checkFromStart){
			CheckCondition();
		}
	}
	
	void Update(){
		if(keepUpdate){
			CheckCondition();
		}
	}
	
	void CheckCondition(){
		if(eventVar[eventId] >= conditionValue){
			//Pass Condition
			if(eventAction == EvAction.Spawn){
				GameObject ob = Instantiate(targetObject , transform.position , transform.rotation) as GameObject;
				if(sendMsgTo != ""){
					ob.SendMessage(sendMsgTo , SendMessageOptions.DontRequireReceiver);
				}
			}
			if(eventAction == EvAction.Enable && targetObject){
				targetObject.SetActive(true);
				if(sendMsgTo != ""){
					targetObject.SendMessage(sendMsgTo , SendMessageOptions.DontRequireReceiver);
				}
			}
			if(eventAction == EvAction.Delete && targetObject){
				Destroy(targetObject);
			}
			if(eventAction == EvAction.None && targetObject){
				if(sendMsgTo != ""){
					targetObject.SendMessage(sendMsgTo , SendMessageOptions.DontRequireReceiver);
				}
			}
			if(destroyWhenPass){
				Destroy(gameObject);
			}
		}
	}
}

public enum EvAction{
	Spawn = 0,
	Enable = 1,
	Delete = 2,
	None = 3
}
