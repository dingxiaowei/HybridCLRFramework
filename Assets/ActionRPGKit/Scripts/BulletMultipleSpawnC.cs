using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BulletStatusC))]
public class BulletMultipleSpawnC : MonoBehaviour {

	public Transform spawnPrefab;
	public Transform[] randomSpawnPoint = new Transform[1];
	public int continuous = 3;
	public float continuousDelay = 0.5f;
	
	public bool setRotation = false;
	private string shooterTag = "Player";
	private GameObject shooter;
	
	private float wait = 0;
	private int sp = 0;
	private int playerAttack = 5;
	
	void Start(){
		if(setRotation){
			transform.eulerAngles = new Vector3(0 , transform.eulerAngles.y , 0);
		}
		shooterTag = GetComponent<BulletStatusC>().shooterTag;
		shooter = GetComponent<BulletStatusC>().shooter;
		playerAttack = GetComponent<BulletStatusC>().playerAttack;
	}
	
	void Update(){
		if(wait >= continuousDelay){
			SpawnBullet();
			wait = 0;
		}else{
			wait += Time.deltaTime;
		}
	}
	
	void SpawnBullet(){
		int ran = Random.Range(0 , randomSpawnPoint.Length);
		
		Transform bulletShootout = Instantiate(spawnPrefab, randomSpawnPoint[ran].position , randomSpawnPoint[ran].rotation) as Transform;
		bulletShootout.GetComponent<BulletStatusC>().Setting(playerAttack , playerAttack , shooterTag , shooter);
		
		sp++;
		if(sp >= continuous){
			Destroy(gameObject);
		}
	}
}
