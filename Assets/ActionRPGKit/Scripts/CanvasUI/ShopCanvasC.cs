using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShopCanvasC : MonoBehaviour{
	public ShopBuyInfo[] shopSlot = new ShopBuyInfo[8];
	public ShopButton[] shupUi = new ShopButton[8];
	public GameObject menuPanel;
	public GameObject shopPanel;
	public GameObject buyErrorPanel;
	public Text cashText;
	public Text buyErrorText;
	public Text pageText;
	private int maxPage = 1;
	private int page = 0;
	private int cPage = 0;

	//private int playerMaxPage = 0; // For Shop Sell

	public GameObject tooltip;
	public Image tooltipIcon;
	public Text tooltipName;
	public Text tooltipText1;
	public Text tooltipText2;
	public Text tooltipText3;

	public ConfirmationUI buyConfirmation;
	public ConfirmationUI sellConfirmation;

	public ItemDataC database;
	private int mode = 0;//0 = Shop Buy , 1 = Usable Sell , 2 = Equipment Sell

	[HideInInspector]
	public GameObject player;
	// Use this for initialization
	void Start(){
		player = GlobalConditionC.mainPlayer;
		SetMaxPage();
		UpdateUi();
	}

	public void OpenShop(){
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 0.0f;
		GlobalConditionC.freezeAll = true;
		menuPanel.SetActive(true);
	}

	void SetMaxPage(){
		if(!player){
			return;
		}
		//Set Initial Page
		page = 0;
		cPage = 0;
		//Set Max Page
		if(mode == 0){
			//Shop Page
			maxPage = shopSlot.Length / shupUi.Length;
			if(shopSlot.Length % shupUi.Length != 0){
				maxPage += 1;
			}
		}
		if(mode == 1){
			//Sell Usable Item
			maxPage = player.GetComponent<InventoryC>().itemSlot.Length / shupUi.Length;
			if(player.GetComponent<InventoryC>().itemSlot.Length % shupUi.Length != 0){
				maxPage += 1;
			}
		}
		if(mode == 2){
			//Sell Equipment
			maxPage = player.GetComponent<InventoryC>().equipment.Length / shupUi.Length;
			if(player.GetComponent<InventoryC>().equipment.Length % shupUi.Length != 0){
				maxPage += 1;
			}
		}

		print(maxPage);
	}

	void Update(){
		if(tooltip && tooltip.activeSelf == true) {
			Vector2 tooltipPos = Input.mousePosition;
			tooltipPos.x += 7;
			tooltip.transform.position = tooltipPos;
		}

		if(buyConfirmation.basePanel.activeSelf || sellConfirmation.basePanel.activeSelf){
			if(Input.GetKeyDown(KeyCode.UpArrow)){
				QuantityPlus(10);
			}
			if(Input.GetKeyDown(KeyCode.RightArrow)){
				QuantityPlus(1);
			}
			if(Input.GetKeyDown(KeyCode.DownArrow)){
				QuantitySubtract(10);
			}
			if(Input.GetKeyDown(KeyCode.LeftArrow)){
				QuantitySubtract(1);
			}
		}
	}

	public void SwitchMode(int m){
		player = GlobalConditionC.mainPlayer;
		mode = m;
		SetMaxPage();
		UpdateUi();
		if(pageText){
			pageText.GetComponent<Text>().text = "1";
		}
		if(cashText){
			cashText.text = "$ : " + player.GetComponent<InventoryC>().cash.ToString();
		}
	}
	
	public void ShopBuy(){
		if(!player){
			player = GlobalConditionC.mainPlayer;
		}
		int price = 0;
		int id = shopSlot[pickupSlot].itemId;
		if(shopSlot[pickupSlot].itemType == ItType.Usable){
			price = database.usableItem[shopSlot[pickupSlot].itemId].price * pickupQuan;
		}else{
			price = database.equipment[shopSlot[pickupSlot].itemId].price;
		}

		if(player.GetComponent<InventoryC>().cash < price){
			//If not enough cash
			buyErrorPanel.SetActive(true);
			buyErrorText.text = "Not Enough Cash";
			return;
		}
		
		if(shopSlot[pickupSlot].itemType == ItType.Usable){
			//Buy Usable Item	
			bool full = player.GetComponent<InventoryC>().AddItem(id , pickupQuan);
			if(full){
				buyErrorPanel.SetActive(true);
				buyErrorText.text = "Inventory Full";
				return;
			}
		}else{
			//Buy Equipment
			bool full = player.GetComponent<InventoryC>().AddEquipment(id);
			if(full){
				buyErrorText.text = "Inventory Full";
				return;
			}
		}
		//Remove Cash
		player.GetComponent<InventoryC>().cash -= price;
		if(cashText){
			cashText.text = "$ : " + player.GetComponent<InventoryC>().cash.ToString();
		}
	}

	public void ShopSell(){
		int price = 0;
		if(mode == 1){
			//Sell Usable Item
			int id = player.GetComponent<InventoryC>().itemSlot[pickupSlot];
			price = database.usableItem[id].price * pickupQuan / 2;

			if(pickupQuan >= player.GetComponent<InventoryC>().itemQuantity[pickupSlot]){
				pickupQuan = player.GetComponent<InventoryC>().itemQuantity[pickupSlot];
			}
			player.GetComponent<InventoryC>().itemQuantity[pickupSlot] -= pickupQuan;
			if(player.GetComponent<InventoryC>().itemQuantity[pickupSlot] <= 0){
				player.GetComponent<InventoryC>().itemSlot[pickupSlot] = 0;
				player.GetComponent<InventoryC>().itemQuantity[pickupSlot] = 0;
				player.GetComponent<InventoryC>().AutoSortItem();
			}
			player.GetComponent<InventoryC>().UpdateAmmoUI();
			//Add Cash
			player.GetComponent<InventoryC>().cash += price;
			
		}else if(mode == 2){
			//Sell Equipment
			int id = player.GetComponent<InventoryC>().equipment[pickupSlot];
			price = database.equipment[id].price / 2;

			player.GetComponent<InventoryC>().equipment[pickupSlot] = 0;
			player.GetComponent<InventoryC>().AutoSortEquipment();
			
			//Add Cash
			player.GetComponent<InventoryC>().cash += price;
		}
		UpdateUi();
		if(cashText){
			cashText.text = "$ : " + player.GetComponent<InventoryC>().cash.ToString();
		}
	}

	public void UpdateUi(){
		if(!player){
			return;
		}
		if(mode == 0){
			//Shop Buy
			for(int a = 0; a < shupUi.Length; a++){
				if(a + cPage < shopSlot.Length && shopSlot[a + cPage].itemId > 0){
					if(shopSlot[a + cPage].itemType == ItType.Usable){
						//Usable Item Shop
						shupUi[a].itemIcons.sprite = database.usableItem[shopSlot[a + cPage].itemId].iconSprite;
						shupUi[a].itemIcons.color = database.usableItem[shopSlot[a + cPage].itemId].spriteColor;
						
						shupUi[a].itemNameText.text = database.usableItem[shopSlot[a + cPage].itemId].itemName;
						shupUi[a].priceText.text = ": " + (database.usableItem[shopSlot[a + cPage].itemId].price).ToString();
					}else{
						//Equipment Shop
						shupUi[a].itemIcons.sprite = database.equipment[shopSlot[a + cPage].itemId].iconSprite;
						shupUi[a].itemIcons.color = database.equipment[shopSlot[a + cPage].itemId].spriteColor;

						shupUi[a].itemNameText.text = database.equipment[shopSlot[a + cPage].itemId].itemName;
						shupUi[a].priceText.text = ": " + (database.equipment[shopSlot[a + cPage].itemId].price).ToString();
					}
				}else{
					//Out of Range
					shupUi[a].itemIcons.sprite = null;
					shupUi[a].itemNameText.text = "";
					shupUi[a].priceText.text = "";
				}
			}
		}
		if(mode == 1){
			//Sell Usable Item
			for(int a = 0; a < shupUi.Length; a++){
				if(a + cPage < player.GetComponent<InventoryC>().itemSlot.Length && player.GetComponent<InventoryC>().itemSlot[a + cPage] > 0){
					shupUi[a].itemIcons.sprite = database.usableItem[player.GetComponent<InventoryC>().itemSlot[a + cPage]].iconSprite;
					shupUi[a].itemIcons.color = database.usableItem[player.GetComponent<InventoryC>().itemSlot[a + cPage]].spriteColor;
					
					shupUi[a].itemNameText.text = database.usableItem[player.GetComponent<InventoryC>().itemSlot[a + cPage]].itemName + " x " + player.GetComponent<InventoryC>().itemQuantity[a + cPage].ToString();
					shupUi[a].priceText.text = ": " + (database.usableItem[player.GetComponent<InventoryC>().itemSlot[a + cPage]].price /2).ToString();
				}else{
					//Out of Range
					shupUi[a].itemIcons.sprite = null;
					shupUi[a].itemNameText.text = "";
					shupUi[a].priceText.text = "";
				}
			}
		}
		if(mode == 2){
			//Sell Equipment
			for(int a = 0; a < shupUi.Length; a++){
				if(a + cPage < player.GetComponent<InventoryC>().equipment.Length && player.GetComponent<InventoryC>().equipment[a + cPage] > 0){
					shupUi[a].itemIcons.sprite = database.equipment[player.GetComponent<InventoryC>().equipment[a + cPage]].iconSprite;
					shupUi[a].itemIcons.color = database.equipment[player.GetComponent<InventoryC>().equipment[a + cPage]].spriteColor;
					
					shupUi[a].itemNameText.text = database.equipment[player.GetComponent<InventoryC>().equipment[a + cPage]].itemName;
					shupUi[a].priceText.text = ": " + (database.equipment[player.GetComponent<InventoryC>().equipment[a + cPage]].price /2).ToString();
				}else{
					//Out of Range
					shupUi[a].itemIcons.sprite = null;
					shupUi[a].itemNameText.text = "";
					shupUi[a].priceText.text = "";
				}
			}
		}
	}

	public void ShowTooltip(int slot){
		if(!tooltip || !player || mode == 0 && slot + cPage >= shopSlot.Length){
			return;
		}
		slot += cPage;
		if(mode == 0 && shopSlot[slot].itemType == ItType.Usable){
			if(shopSlot[slot].itemId <= 0 || slot >= shopSlot.Length){
				HideTooltip();
				return;
			}
			tooltipIcon.sprite = database.usableItem[shopSlot[slot].itemId].iconSprite;
			tooltipName.text = database.usableItem[shopSlot[slot].itemId].itemName;
			
			tooltipText1.text = database.usableItem[shopSlot[slot].itemId].description;
			tooltipText2.text = database.usableItem[shopSlot[slot].itemId].description2;
			tooltipText3.text = database.usableItem[shopSlot[slot].itemId].description3;
			
			tooltip.SetActive(true);
		}
		if(mode == 0 && shopSlot[slot].itemType == ItType.Equipment){
			if(shopSlot[slot].itemId <= 0 || slot >= shopSlot.Length){
				HideTooltip();
				return;
			}
			tooltipIcon.sprite = database.equipment[shopSlot[slot].itemId].iconSprite;
			tooltipName.text = database.equipment[shopSlot[slot].itemId].itemName;
			
			tooltipText1.text = database.equipment[shopSlot[slot].itemId].description;
			tooltipText2.text = database.equipment[shopSlot[slot].itemId].description2;
			tooltipText3.text = database.equipment[shopSlot[slot].itemId].description3;
			
			tooltip.SetActive(true);
		}

		if(mode == 1){
			if(player.GetComponent<InventoryC>().itemSlot[slot] <= 0 || slot >= player.GetComponent<InventoryC>().itemSlot.Length){
				HideTooltip();
				return;
			}
			
			tooltipIcon.sprite = database.usableItem[player.GetComponent<InventoryC>().itemSlot[slot]].iconSprite;
			tooltipName.text = database.usableItem[player.GetComponent<InventoryC>().itemSlot[slot]].itemName;
			
			tooltipText1.text = database.usableItem[player.GetComponent<InventoryC>().itemSlot[slot]].description;
			tooltipText2.text = database.usableItem[player.GetComponent<InventoryC>().itemSlot[slot]].description2;
			tooltipText3.text = database.usableItem[player.GetComponent<InventoryC>().itemSlot[slot]].description3;
			
			tooltip.SetActive(true);
		}
		if(mode == 2){
			if(player.GetComponent<InventoryC>().equipment[slot] <= 0 || slot >= player.GetComponent<InventoryC>().equipment.Length){
				HideTooltip();
				return;
			}
			
			tooltipIcon.sprite = database.equipment[player.GetComponent<InventoryC>().equipment[slot]].iconSprite;
			tooltipName.text = database.equipment[player.GetComponent<InventoryC>().equipment[slot]].itemName;
			
			tooltipText1.text = database.equipment[player.GetComponent<InventoryC>().equipment[slot]].description;
			tooltipText2.text = database.equipment[player.GetComponent<InventoryC>().equipment[slot]].description2;
			tooltipText3.text = database.equipment[player.GetComponent<InventoryC>().equipment[slot]].description3;
			
			tooltip.SetActive(true);
		}
	}
	private int pickupSlot = 0;
	private int pickupQuan = 1;

	public void ButtonClick(int slot){
		pickupSlot = slot + cPage;
		pickupQuan = 1;
		buyErrorPanel.SetActive(false);
		if(pickupSlot >= shopSlot.Length){
			return;
		}
			
		if(mode == 0){
			shopPanel.SetActive(false);
			if(shopSlot[pickupSlot].itemType == ItType.Usable){
				buyConfirmation.basePanel.SetActive(true);
				if(buyConfirmation.inputField){
					buyConfirmation.inputField.gameObject.SetActive(true);
					buyConfirmation.inputField.text = pickupQuan.ToString();
				}
				if(buyConfirmation.priceText){
					buyConfirmation.priceText.text = database.usableItem[shopSlot[pickupSlot].itemId].price.ToString();
				}
			}
			if(shopSlot[pickupSlot].itemType == ItType.Equipment){
				buyConfirmation.basePanel.SetActive(true);
				if(buyConfirmation.inputField){
					buyConfirmation.inputField.gameObject.SetActive(false);
				}
				if(buyConfirmation.priceText){
					buyConfirmation.priceText.text = database.equipment[shopSlot[pickupSlot].itemId].price.ToString();
				}
			}

		}
		if(mode == 1){
			if(player.GetComponent<InventoryC>().itemSlot[pickupSlot] <= 0){
				return;
			}
			shopPanel.SetActive(false);
			sellConfirmation.basePanel.SetActive(true);
			if(sellConfirmation.inputField){
				sellConfirmation.inputField.gameObject.SetActive(true);
				sellConfirmation.inputField.text = pickupQuan.ToString();
			}
			if(sellConfirmation.priceText){
				sellConfirmation.priceText.text = (database.usableItem[player.GetComponent<InventoryC>().itemSlot[pickupSlot]].price / 2).ToString();
			}
		}
		if(mode == 2){
			if(player.GetComponent<InventoryC>().equipment[pickupSlot] <= 0){
				return;
			}
			shopPanel.SetActive(false);
			sellConfirmation.basePanel.SetActive(true);
			if(sellConfirmation.inputField){
				sellConfirmation.inputField.gameObject.SetActive(false);
			}
			if(sellConfirmation.priceText){
				sellConfirmation.priceText.text = (database.equipment[player.GetComponent<InventoryC>().equipment[pickupSlot]].price / 2).ToString();
			}
		}
	}

	public void HideTooltip(){
		if(!tooltip){
			return;
		}
		tooltip.SetActive(false);
	}

	public void CloseMenu(){
		Time.timeScale = 1.0f;
		//Screen.lockCursor = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		GlobalConditionC.freezeAll = false;
		//gameObject.SetActive(false);
	}

	public void NextPage(){
		if(page < maxPage -1){
			page++;
			cPage = page * shupUi.Length;
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
			cPage = page * shupUi.Length;
		}
		if(pageText){
			int p = page + 1;
			pageText.GetComponent<Text>().text = p.ToString();
		}
		UpdateUi();
	}

	public void QuantityPlus(int val){
		int currentQty = pickupQuan;
		pickupQuan += val;
		if(mode == 0){
			if(pickupQuan > 99){
				pickupQuan = 1;
			}
			if(buyConfirmation.priceText && shopSlot[pickupSlot].itemType == ItType.Usable){
				buyConfirmation.priceText.text = (database.usableItem[shopSlot[pickupSlot].itemId].price * pickupQuan).ToString();
			}
		}
		if(mode == 1){
			if(pickupQuan > player.GetComponent<InventoryC>().itemQuantity[pickupSlot] && currentQty == player.GetComponent<InventoryC>().itemQuantity[pickupSlot]){
				pickupQuan = 1;
			}else if(pickupQuan > player.GetComponent<InventoryC>().itemQuantity[pickupSlot]){
				pickupQuan = player.GetComponent<InventoryC>().itemQuantity[pickupSlot];
			}
			if(sellConfirmation.priceText){
				sellConfirmation.priceText.text = (database.usableItem[player.GetComponent<InventoryC>().itemSlot[pickupSlot]].price * pickupQuan / 2).ToString();
			}
		}

		if(buyConfirmation.inputField){
			buyConfirmation.inputField.text = pickupQuan.ToString();
		}
		if(sellConfirmation.inputField){
			sellConfirmation.inputField.text = pickupQuan.ToString();
		}
	}

	public void QuantitySubtract(int val){
		int currentQty = pickupQuan;
		pickupQuan -= val;
		if(mode == 0){
			if(pickupQuan <= 0){
				pickupQuan = 99;
			}
			if(buyConfirmation.priceText && shopSlot[pickupSlot].itemType == ItType.Usable){
				buyConfirmation.priceText.text = (database.usableItem[shopSlot[pickupSlot].itemId].price * pickupQuan).ToString();
			}
		}
		if(mode == 1){
			if(pickupQuan <= 0 && currentQty == 1){
				pickupQuan = player.GetComponent<InventoryC>().itemQuantity[pickupSlot];
			}else if(pickupQuan <= 0){
				pickupQuan = 1;
			}
			if(sellConfirmation.priceText){
				sellConfirmation.priceText.text = (database.usableItem[player.GetComponent<InventoryC>().itemSlot[pickupSlot]].price * pickupQuan / 2).ToString();
			}
		}

		if(buyConfirmation.inputField){
			buyConfirmation.inputField.text = pickupQuan.ToString();
		}
		if(sellConfirmation.inputField){
			sellConfirmation.inputField.text = pickupQuan.ToString();
		}
	}

	public void QuantityInput(string val){
		if(val == ""){
			val = "1";
		}
		int v = int.Parse(val);
		pickupQuan = v;
		if(mode == 0){
			if(pickupQuan < 1){
				pickupQuan = 1;
			}
			if(buyConfirmation.priceText && shopSlot[pickupSlot].itemType == ItType.Usable){
				buyConfirmation.priceText.text = (database.usableItem[shopSlot[pickupSlot].itemId].price * pickupQuan).ToString();
			}
		}
		if(mode == 1){
			if(pickupQuan > player.GetComponent<InventoryC>().itemQuantity[pickupSlot]){
				pickupQuan = player.GetComponent<InventoryC>().itemQuantity[pickupSlot];
			}
			if(pickupQuan < 1){
				pickupQuan = 1;
			}
			if(sellConfirmation.priceText){
				sellConfirmation.priceText.text = (database.usableItem[player.GetComponent<InventoryC>().itemSlot[pickupSlot]].price * pickupQuan / 2).ToString();
			}
		}

		if(buyConfirmation.inputField){
			buyConfirmation.inputField.text = pickupQuan.ToString();
		}
		if(sellConfirmation.inputField){
			sellConfirmation.inputField.text = pickupQuan.ToString();
		}
	}
}

[System.Serializable]
public class ShopButton{
	public Image itemIcons;
	public Text itemNameText;
	public Text priceText;
}

[System.Serializable]
public class ShopBuyInfo{
	public int itemId = 0;
	public ItType itemType = ItType.Usable; 
}

[System.Serializable]
public class ConfirmationUI{
	public GameObject basePanel;
	public Text priceText;
	public InputField inputField;
}
