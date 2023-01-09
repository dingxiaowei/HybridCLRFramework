using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ItemDataC : MonoBehaviour {
	[System.Serializable]
	public class Usable {
		public string itemName = "";
		public Texture2D icon;
		public Sprite iconSprite;
		public Color spriteColor = Color.white;
		public GameObject model;
		public string description = "";
		public string description2 = "";
		public string description3 = "";
		public int price = 10;
		public int hpRecover = 0;
		public int mpRecover = 0;
		public int atkPlus = 0;
		public int defPlus = 0;
		public int matkPlus = 0;
		public int mdefPlus = 0;
		public bool unusable = false;
		public string sendMsg = "";
	} 
	[System.Serializable]
	public class Equip {
		public string itemName = "";
		public Texture2D icon;
		public Sprite iconSprite;
		public Color spriteColor = Color.white;
		public GameObject model;
		public bool assignAllWeapon = true;
		public bool canBlock = false;
		public string description = "";
		public string description2 = "";
		public string description3 = "";
		public int price = 10;
		public int weaponType = 0; //Use for Mecanim Weapon ID
		public int attack = 5;
		public int defense = 0;
		public int magicAttack = 0;
		public int magicDefense = 0;
		public int hpBonus = 0;
		public int mpBonus = 0;
		
		public enum EqType {
			Weapon = 0,
			Armor = 1,
			Accessory = 2,
			Headgear = 3,
			Gloves = 4,
			Boots = 5
		}
		public EqType EquipmentType = EqType.Weapon; 
		
		//Ignore if the equipment type is not weapons
		public GameObject attackPrefab;
		public AnimationClip[] attackCombo = new AnimationClip[3];
		public AnimationClip idleAnimation;
		public AnimationClip runAnimation;
		public AnimationClip rightAnimation;
		public AnimationClip leftAnimation;
		public AnimationClip backAnimation;
		public AnimationClip jumpAnimation;
		public AnimationClip jumpUpAnimation;
		public AnimationClip blockingAnimation;
		public enum whileAtk{
			MeleeFwd = 0,
			Immobile = 1,
			WalkFree = 2
		}
		public whileAtk whileAttack = whileAtk.MeleeFwd;
		public float attackSpeed = 0.18f;
		public float attackDelay = 0.12f;
		public AudioClip soundEffect;

		public Resist statusResist;
		public bool canDoubleJump = false;
		[Range(0 , 100)]
		public int autoGuard = 0;
		[Range(0 , 100)]
		public int drainTouch = 0;
		[Range(0 , 100)]
		public int mpReduce = 0;
		public int requireItemId = 0; // Set to 0 if not require any item when attack.
	} 
	
	public Usable[] usableItem = new Usable[3];
	public Equip[] equipment = new Equip[3];
}



