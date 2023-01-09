using UnityEngine;
using System.Collections;

public class UpDownObjectC : MonoBehaviour {

public float moveX = 0.0f;
public float moveY = 5.0f;
public float moveZ = 0.0f;
private float wait = 0;
public float duration = 1.0f;

void  Update (){
	
	transform.Translate(moveX*Time.deltaTime, moveY*Time.deltaTime, moveZ*Time.deltaTime);
	if(wait >= duration){
     moveX *= -1;
     moveY *= -1;
     moveZ *= -1;
      wait = 0;
      
   }else wait += Time.deltaTime;

}
}
