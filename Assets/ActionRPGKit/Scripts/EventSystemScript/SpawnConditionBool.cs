using UnityEngine;
using System.Collections;

[AddComponentMenu("Easy Event Maker/Spawn Condition Bool")]
public class SpawnConditionBool : MonoBehaviour {
	public enum EvAction{
		Spawn = 0,
		Enable = 1,
		Disable = 2,
		Delete = 3,
		None = 4
	}
	public int boolId = 1;
	public bool condition = true; //If EventSetting.globalBoolean[boolId] value = this value. Condition is Pass
	public GameObject targetObject;
	public EvAction eventAction = EvAction.Spawn;
	public string sendMsgPass = "";

	public bool keepUpdate = false;
	public bool checkFromStart = true;
	public bool stopCheckingWhenPass = true;
	public bool destroyWhenPass = false;
	public bool disableWhenFail = false;
	
	private bool stop = false;
	public string note = "";

	void Start(){
		if(checkFromStart){
			CheckCondition();
		}
	}
	
	void Update(){
		if(keepUpdate && !stop){
			CheckCondition();
		}
	}
	
	public void CheckCondition(){
		if(EventSetting.globalBoolean[boolId] == condition){
			ConditionPass();
		}else if(disableWhenFail && targetObject){
			targetObject.SetActive(false);
		}
	}
	
	void ConditionPass(){
		if(eventAction == EvAction.Spawn){
			GameObject ob = Instantiate(targetObject , transform.position , transform.rotation) as GameObject;
			if(sendMsgPass != ""){
				ob.SendMessage(sendMsgPass , SendMessageOptions.DontRequireReceiver);
			}
		}
		if(eventAction == EvAction.Enable && targetObject){
			targetObject.SetActive(true);
			if(sendMsgPass != ""){
				targetObject.SendMessage(sendMsgPass , SendMessageOptions.DontRequireReceiver);
			}
		}
		if(eventAction == EvAction.Disable && targetObject){
			if(sendMsgPass != ""){
				targetObject.SendMessage(sendMsgPass , SendMessageOptions.DontRequireReceiver);
			}
			targetObject.SetActive(false);
		}
		if(eventAction == EvAction.Delete && targetObject){
			Destroy(targetObject);
		}
		if(eventAction == EvAction.None && targetObject){
			if(sendMsgPass != ""){
				targetObject.SendMessage(sendMsgPass , SendMessageOptions.DontRequireReceiver);
			}
		}
		if(destroyWhenPass){
			Destroy(gameObject);
		}
		if(stopCheckingWhenPass){
			stop = true;
		}
	}
}
