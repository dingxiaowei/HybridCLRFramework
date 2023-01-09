using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingCanvasC : MonoBehaviour {
	public int[] craftingListId = new int[3];
	public CraftingButton[] craftMenuUi = new CraftingButton[8];
	public CraftingButton[] ingredientUi = new CraftingButton[5];
	public Text pageText;

	public GameObject menuPanel;
	public GameObject ingredientPanel;
	public Text gotItemText;

	private CraftData[] craftingData;
	public ItemDataC itemDatabase;
	public CraftingDataC craftingDatabase;
	private GameObject player;

	public GameObject tooltip;
	public Image tooltipIcon;
	public Text tooltipName;
	public Text tooltipText1;
	public Text tooltipText2;
	public Text tooltipText3;

	private int uiPage = 0;
	private int page = 0;
	private int maxPage = 1;
	private int selection = 0;
	private int cPage = 0;

	private int itemQty1;
	private int itemQty2;
	private int itemQty3;
	private int itemQty4;
	private int itemQty5;

	public Color haveAllItemColor = Color.black;
	public Color notEnoughtItemColor = Color.red;

	void Start(){
		if(!player){
			player = GameObject.FindWithTag("Player");
		}

		GetCraftingData();
		//uiPage = 1;
	}

	public void OpenCraftMenu(){
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 0.0f;
		GlobalConditionC.freezeAll = true;
		uiPage = 0;
		UpdateUi();
		menuPanel.SetActive(true);
		ingredientPanel.SetActive(false);
		gotItemText.gameObject.SetActive(false);
	}

	public void CloseIngredientPanel(){
		uiPage = 0;
		menuPanel.SetActive(true);
		ingredientPanel.SetActive(false);
		gotItemText.gameObject.SetActive(false);
	}
	
	void GetCraftingData(){
		craftingData = new CraftData[craftingListId.Length];
		int a = 0;
		while(a < craftingData.Length){
			craftingData[a] = craftingDatabase.craftingData[craftingListId[a]];
			a++;
		}
		//Set Max Page
		maxPage = craftingData.Length / craftMenuUi.Length;
		if(craftingData.Length % craftMenuUi.Length != 0){
			maxPage += 1;
		}
		print(maxPage);
	}

	public void UpdateUi(){
		if(!player){
			return;
		}
		if(uiPage == 0){
			for(int a = 0; a < craftMenuUi.Length; a++){
				if(a + cPage < craftingListId.Length){
					if(craftingDatabase.craftingData[craftingListId[a + cPage]].gotItem.itemType == ItType.Usable){
						//Usable Item Shop
						craftMenuUi[a].itemIcons.sprite = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[a + cPage]].gotItem.itemId].iconSprite;
						craftMenuUi[a].itemIcons.color = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[a + cPage]].gotItem.itemId].spriteColor;
						
						craftMenuUi[a].itemNameText.text = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[a + cPage]].gotItem.itemId].itemName;
					}else{
						//Equipment Shop
						craftMenuUi[a].itemIcons.sprite = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[a + cPage]].gotItem.itemId].iconSprite;
						craftMenuUi[a].itemIcons.color = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[a + cPage]].gotItem.itemId].spriteColor;
						
						craftMenuUi[a].itemNameText.text = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[a + cPage]].gotItem.itemId].itemName;
					}
				}else{
					//Out of Range
					craftMenuUi[a].itemIcons.sprite = itemDatabase.usableItem[0].iconSprite;
					craftMenuUi[a].itemNameText.text = "";
				}
			}
		}

		//Show Ingredient
		if(uiPage == 1){
			for(int a = 0; a < ingredientUi.Length; a++){
				if(a < craftingDatabase.craftingData[craftingListId[selection]].ingredient.Length){
					if(craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].itemType == ItType.Usable){
						string qty = craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].quantity.ToString();
						string itemQty = ShowItemQuantity(a).ToString();
						ingredientUi[a].itemNameText.text = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].itemId].itemName + " x " + qty + " (" + itemQty + ")";
						ingredientUi [a].itemIcons.sprite = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].itemId].iconSprite;

						if(int.Parse(itemQty) >= int.Parse(qty)){
							ingredientUi[a].itemNameText.color = haveAllItemColor;
						}else{
							ingredientUi[a].itemNameText.color = notEnoughtItemColor;
						}
					}else{
						string itemQty = CheckEquipment(a);
						ingredientUi[a].itemNameText.text = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].itemId].itemName + " x " + 1 + " (" + itemQty + ")";
						ingredientUi [a].itemIcons.sprite = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].itemId].iconSprite;

						if(int.Parse(itemQty) >= 1){
							ingredientUi[a].itemNameText.color = haveAllItemColor;
						}else{
							ingredientUi[a].itemNameText.color = notEnoughtItemColor;
						}
					}
				}else{
					ingredientUi[a].itemNameText.text = "";
					//ingredientUi[a].itemIcons.sprite = null;
					ingredientUi[a].itemIcons.sprite = itemDatabase.usableItem[0].iconSprite;
				}

			}
		}
	}

	int ShowItemQuantity(int b){
		int qty = 0;
		int a = player.GetComponent<InventoryC>().FindItemSlot(craftingDatabase.craftingData[craftingListId[selection]].ingredient[b].itemId);
		if(a <= player.GetComponent<InventoryC>().itemSlot.Length){
			qty = player.GetComponent<InventoryC>().itemQuantity[a];
		}
		return qty;
	}

	string CheckEquipment(int b){
		string qty = "0";
		int a = player.GetComponent<InventoryC>().FindEquipmentSlot(craftingDatabase.craftingData[craftingListId[selection]].ingredient[b].itemId);
		if(a <= player.GetComponent<InventoryC>().equipment.Length){
			qty = "1";
		}
		return qty;
	}
	
	// Update is called once per frame
	void Update(){
		if(tooltip && tooltip.activeSelf == true){
			Vector2 tooltipPos = Input.mousePosition;
			tooltipPos.x += 7;
			tooltip.transform.position = tooltipPos;
		}
	}

	public void ButtonClick(int slot){
		selection = slot + cPage;
		if(selection >= craftingListId.Length){
			return;
		}
		uiPage = 1;
		menuPanel.SetActive(false);
		ingredientPanel.SetActive(true);
		UpdateUi();
		gotItemText.gameObject.SetActive(false);
		HideTooltip();
	}

	public void NextPage(){
		if(page < maxPage -1){
			page++;
			cPage = page * craftMenuUi.Length;
		}
		if(pageText){
			int p = page + 1;
			pageText.GetComponent<Text>().text = p.ToString();
		}
		UpdateUi();
	}
	
	public void PreviousPage(){
		if(page > 0){
			page--;
			cPage = page * craftMenuUi.Length;
		}
		if(pageText){
			int p = page + 1;
			pageText.GetComponent<Text>().text = p.ToString();
		}
		UpdateUi();
	}

	public void CloseMenu(){
		Time.timeScale = 1.0f;
		//Screen.lockCursor = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		GlobalConditionC.freezeAll = false;
		menuPanel.gameObject.SetActive(false);
		ingredientPanel.gameObject.SetActive(false);
		gotItemText.gameObject.SetActive(false);
		//gameObject.SetActive(false);
	}

	public void CraftItem(){
		bool canCraft = CheckIngredients();
		if(canCraft){
			AddandRemoveItem();
		}
		//Show Items Quantity
		for(int a = 0; a < craftingDatabase.craftingData[craftingListId[selection]].ingredient.Length; a++){
			if(craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].itemType == ItType.Usable){
				string qty = craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].quantity.ToString();
				string itemQty = ShowItemQuantity(a).ToString();
				ingredientUi[a].itemNameText.text = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].itemId].itemName + " x " + qty + " (" + itemQty + ")";
				if(int.Parse(itemQty) >= int.Parse(qty)){
					ingredientUi[a].itemNameText.color = haveAllItemColor;
				}else{
					ingredientUi[a].itemNameText.color = notEnoughtItemColor;
				}
			}else{
				string itemQty = CheckEquipment(a);
				ingredientUi[a].itemNameText.text = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[selection]].ingredient[a].itemId].itemName + " x " + 1 + " (" + itemQty + ")";
				if(int.Parse(itemQty) >= 1){
					ingredientUi[a].itemNameText.color = haveAllItemColor;
				}else{
					ingredientUi[a].itemNameText.color = notEnoughtItemColor;
				}
			}
		}
	}

	bool CheckIngredients(){
		int a = 0;
		while(a < craftingData[selection].ingredient.Length){
			bool  item = player.GetComponent<InventoryC>().CheckItem(craftingData[selection].ingredient[a].itemId , (int)craftingData[selection].ingredient[a].itemType, craftingData[selection].ingredient[a].quantity);
			if(!item){
				//showError = "Not enought items";
				return false;
			}
			a++;
		}
		return true;
	}

	void AddandRemoveItem(){
		bool full = false;
		if((int)craftingData[selection].gotItem.itemType == 0){
			full = player.GetComponent<InventoryC>().AddItem(craftingData[selection].gotItem.itemId , craftingData[selection].gotItem.quantity);
		}else if((int)craftingData[selection].gotItem.itemType == 1){
			full = player.GetComponent<InventoryC>().AddEquipment(craftingData[selection].gotItem.itemId);
		}
		//Remove Ingrediant Items
		if(!full){
			int a = 0;
			while(a < craftingData[selection].ingredient.Length){
				if((int)craftingData[selection].ingredient[a].itemType == 0){
					player.GetComponent<InventoryC>().RemoveItem(craftingData[selection].ingredient[a].itemId , craftingData[selection].ingredient[a].quantity);
					//------------------
				}else if((int)craftingData[selection].ingredient[a].itemType == 1){
					player.GetComponent<InventoryC>().RemoveEquipment(craftingData[selection].ingredient[a].itemId);
					//------------------
				}
				a++;
			}
			//showError = "You Got " + craftingData[selection].itemName;
			gotItemText.gameObject.SetActive(true);
			gotItemText.text = "You Got " + craftingData[selection].itemName;
		}else{
			//showError = "Inventory Full";
			gotItemText.gameObject.SetActive(true);
			gotItemText.text = "Inventory Full";
		}
	}

	public void ShowTooltip(int slot){
		if(!tooltip || !player || slot + cPage >= craftingListId.Length) {
			return;
		}
		slot += cPage;
		if(craftingDatabase.craftingData[craftingListId [slot]].gotItem.itemType == ItType.Usable){
			if(slot >= craftingListId.Length) {
				HideTooltip();
				return;
			}
			tooltipIcon.sprite = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].iconSprite;
			tooltipName.text = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].itemName;

			tooltipText1.text = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].description;
			tooltipText2.text = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].description2;
			tooltipText3.text = itemDatabase.usableItem[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].description3;

			tooltip.SetActive (true);
		}else{
			if(slot >= craftingListId.Length) {
				HideTooltip();
				return;
			}
			tooltipIcon.sprite = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].iconSprite;
			tooltipName.text = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].itemName;

			tooltipText1.text = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].description;
			tooltipText2.text = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].description2;
			tooltipText3.text = itemDatabase.equipment[craftingDatabase.craftingData[craftingListId[slot]].gotItem.itemId].description3;

			tooltip.SetActive (true);
		}
	}
	public void HideTooltip(){
		if(!tooltip){
			return;
		}
		tooltip.SetActive(false);
	}
}

[System.Serializable]
public class CraftingButton{
	public Image itemIcons;
	public Text itemNameText;
}
