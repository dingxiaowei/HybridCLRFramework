using UnityEngine;
using System.Collections;

public class RespawnMonsterC : MonoBehaviour {
	
	public GameObject enemy;
	public string pointName = "SpawnPoint";
	public float delay = 3.0f;
	public float randomPoint = 10.0f;
	
	void  Start (){
		StartCoroutine(Delay());
	}

	IEnumerator Delay(){
		GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag (pointName);
		if(spawnpoints.Length > 0){
				Transform spawnpoint = spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
				yield return new WaitForSeconds(delay);
				Vector3 ranPos = spawnpoint.position; //Slightly Random x y position from respawn point.
				ranPos.x += Random.Range(0.0f,randomPoint);
				ranPos.z += Random.Range(0.0f,randomPoint);
				GameObject mon = Instantiate(enemy, ranPos , spawnpoint.rotation) as GameObject;
				mon.name = enemy.name;
				Destroy (gameObject, 1);
		}else{
				Destroy (gameObject, delay +1);
		}
	}
}