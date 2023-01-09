using UnityEngine;
using System.Collections;

public class MountActivatorC : MonoBehaviour {
	
	public float checkDistance = 1.5f;
	public Transform player;
	private bool showUi = false;
	public GUIStyle textStyle;
	
	void Start (){
		if(!player){
			player = transform.root;
		}
	}
	
	void Update(){
		if(GlobalConditionC.freezePlayer){
			return;
		}
		if(Input.GetKeyDown("f")){
			ActivateMount();
		}
		
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		RaycastHit hit;
		
		if(Physics.Raycast(transform.position, fwd, out hit , checkDistance)){
			if(hit.transform.tag == "Mount"){
				showUi = true;
			}else{
				showUi = false;
			}
		}else{
			showUi = false;
		}
	}
	
	void OnGUI(){
		if(showUi && !GlobalConditionC.freezePlayer){
			GUI.Label( new Rect(Screen.width /2 - 250, Screen.height - 110, 500, 50), "Press [F] to ride." , textStyle);
		}
	}
	
	void ActivateMount(){
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		RaycastHit hit;
		
		if(Physics.Raycast(transform.position, fwd, out hit , checkDistance)){
			print(hit.transform.name);
			if(hit.transform.tag == "Mount"){
				//hit.transform.GetComponent<MountControllerC>().GetOn(player);
				hit.transform.GetComponent<MountControllerC>().GetOn();
				showUi = false;
			}
		}
	}
}
