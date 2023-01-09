using UnityEngine;
using System.Collections;

public class CharacterDataC : MonoBehaviour {
	public PlayerData[] player = new PlayerData[3];
}

[System.Serializable]
public class PlayerData{
	public string playerName = "";
	public GameObject playerPrefab;
	public GameObject characterSelectModel;
	public TextDialogue description;
	public Texture2D guiDescription;
}
