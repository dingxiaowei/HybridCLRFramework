using UnityEngine;
using System.Collections;

public class SkillDataC : MonoBehaviour {
	public SkillSetting[] skill = new SkillSetting[3];
}

[System.Serializable]
public class SkillSetting{
	public string skillName = "";
	public Texture2D icon;
	public Sprite iconSprite;
	public Transform skillPrefab;
	public AnimationClip skillAnimation;
	public string mecanimTriggerName = "";
	public int manaCost = 10;
	public float castTime = 0.5f;
	public float skillDelay = 0.3f;
	public int coolDown = 1;
	public string description = "";
	public GameObject castEffect;
	public string sendMsg = "";//Send Message calling function when use this skill.
	public whileAtk whileAttack = whileAtk.Immobile;
	public BSpawnType skillSpawn = BSpawnType.FromPlayer;
	public AudioClip soundEffect;
	public bool requireWeapon = false;
	public int requireWeaponType = 0;
	public SkillAdditionHit[] multipleHit;
}

public enum BSpawnType{
	FromPlayer = 0,
	AtMouse = 1
}

[System.Serializable]
public class SkillAdditionHit{
	public Transform skillPrefab;
	public AnimationClip skillAnimation;
	public string mecanimTriggerName = "";
	public float castTime = 0.5f;
	public float skillDelay = 0.3f;
	public AudioClip soundEffect;
}
