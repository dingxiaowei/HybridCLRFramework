using UnityEngine;
using System.Collections;

public class CraftingDataC : MonoBehaviour {
	public CraftData[] craftingData = new CraftData[3];
}

[System.Serializable]
public class CraftData{
	public string itemName = "";
	public Ingredients[] ingredient = new Ingredients[2];
	public Ingredients gotItem;
}

[System.Serializable]
public class Ingredients{
	public int itemId = 1;
	public ItType itemType = ItType.Usable;
	public int quantity = 1;
}
