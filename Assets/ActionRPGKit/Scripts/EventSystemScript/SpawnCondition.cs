using UnityEngine;
using System.Collections;

[AddComponentMenu("Easy Event Maker/Spawn Condition")]
public class SpawnCondition : MonoBehaviour {
	public enum EvAction{
		Spawn = 0,
		Enable = 1,
		Disable = 2,
		Delete = 3,
		None = 4
	}
	public int varId = 1;
	public int conditionValue = 1; //If EventSetting.globalInt[varId] value Greater than or Equal this value. Condition is Pass
	public GameObject targetObject;
	public EvAction eventAction = EvAction.Spawn;
	public string sendMsgPass = "";

	public enum ConditionsChecker{
		GreaterOrEqual,
		Greater,
		Equal,
		LessOrEqual,
		Less,
		NotEqual
	}
	public ConditionsChecker condition = ConditionsChecker.GreaterOrEqual;
	
	public bool keepUpdate = false;
	public bool checkFromStart = true;
	public bool stopCheckingWhenPass = true;
	public bool destroyWhenPass = false;

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
		if(condition == ConditionsChecker.GreaterOrEqual){
			if(EventSetting.globalInt[varId] >= conditionValue){
				ConditionPass();
			}
		}
		if(condition == ConditionsChecker.Greater){
			if(EventSetting.globalInt[varId] > conditionValue){
				ConditionPass();
			}
		}
		if(condition == ConditionsChecker.Equal){
			if(EventSetting.globalInt[varId] == conditionValue){
				ConditionPass();
			}
		}
		if(condition == ConditionsChecker.LessOrEqual){
			if(EventSetting.globalInt[varId] <= conditionValue){
				ConditionPass();
			}
		}
		if(condition == ConditionsChecker.Less){
			if(EventSetting.globalInt[varId] < conditionValue){
				ConditionPass();
			}
		}
		if(condition == ConditionsChecker.NotEqual){
			if(EventSetting.globalInt[varId] != conditionValue){
				ConditionPass();
			}
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
