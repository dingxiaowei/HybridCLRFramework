using UnityEngine;
using System.Collections;

public class DamagePopupC : MonoBehaviour {
	
	Vector3 targetScreenPosition;
	public string damage = "";
	public GUIStyle fontStyle;
	
	public float duration = 0.5f;

	private int glide = 50;
	[HideInInspector]
	public bool critical = false;
	
	public Texture2D criticalImage;
	
	void Start(){
		Destroy(gameObject, duration);
		//Glide();
		StartCoroutine(Glide());
	}

	IEnumerator Glide(){
		int a = 0;
		while(a < 100){
			glide += 2;
			yield return new WaitForSeconds(0.03f); 
		}
	}
	
	void OnGUI(){
		if(!Camera.main){
			return;
		}
		targetScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
		targetScreenPosition.y = Screen.height - targetScreenPosition.y - glide;
		targetScreenPosition.x = targetScreenPosition.x - 6;
		if(targetScreenPosition.z > 0){
			if(critical){
				if(criticalImage){
					GUI.DrawTexture(new Rect(targetScreenPosition.x -50,targetScreenPosition.y -10,120,60), criticalImage);
				}else{
					GUI.Label (new Rect(targetScreenPosition.x,targetScreenPosition.y - 30,200,50), "Critical!!",fontStyle);
				}
			}
			GUI.Label (new Rect(targetScreenPosition.x,targetScreenPosition.y,200,50), damage,fontStyle);
		}
	}
}
