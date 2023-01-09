using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AIsetAdvanceC))]
public class AiAdvanceSwitchModeC : MonoBehaviour {
	private AiModeSet[] modeSetting = new AiModeSet[2];

	public AiModeSet closeRangeSet;
	public AiModeSet longRangeSet;
	public float switchInRange = 5.5f;
	private int m = 0;

	public GameObject closeRangeweaponMesh;
	public GameObject longRangeweaponMesh;
	// Use this for initialization
	void Start(){
		modeSetting[0] = closeRangeSet;
		modeSetting[1] = longRangeSet;
	}
	
	// Update is called once per frame
	void Update(){
		AIsetAdvanceC ai = GetComponent<AIsetAdvanceC>();
		if(ai.followTarget){
			float distance = (transform.position - ai.GetDestination()).magnitude;
			if(m == 0 && distance >= switchInRange){
				SwitchMode(1);
				m = 1;
			}else if(m == 1 && distance < switchInRange){
				SwitchMode(0);
				m = 0;
			}
		}
	}

	void SwitchMode(int mode){
		AIsetAdvanceC ai = GetComponent<AIsetAdvanceC>();
		ai.c = 0;
		ai.approachDistance = modeSetting[mode].approachDistance;
		ai.speed = modeSetting[mode].speed;
		ai.bulletPrefab = modeSetting[mode].bulletPrefab;
		ai.attackPoint = modeSetting[mode].attackPoint;
		ai.attackCast = modeSetting[mode].attackCast;
		ai.comboDelay = modeSetting[mode].comboDelay;
		ai.attackDelay = modeSetting[mode].attackDelay;
		ai.attackAnimation = modeSetting[mode].attackAnimation;
		ai.attackVoice = modeSetting[mode].attackVoice;

		if(mode == 0){
			if(closeRangeweaponMesh){
				closeRangeweaponMesh.SetActive(true);
			}
			if(longRangeweaponMesh){
				longRangeweaponMesh.SetActive(false);
			}
		}
		if(mode == 1){
			if(closeRangeweaponMesh){
				closeRangeweaponMesh.SetActive(false);
			}
			if(longRangeweaponMesh){
				longRangeweaponMesh.SetActive(true);
			}
		}
	}
}

[System.Serializable]
public class AiModeSet{
	public float approachDistance = 2.0f;
	public float speed = 4.0f;

	public AnimationClip[] attackAnimation = new AnimationClip[1];
	public Transform bulletPrefab;
	public Transform attackPoint;

	public float attackCast = 0.3f;
	public float comboDelay = 0.3f;
	public float attackDelay = 0.5f;
	public AudioClip attackVoice;
}
