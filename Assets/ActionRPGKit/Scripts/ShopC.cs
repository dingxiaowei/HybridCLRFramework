using UnityEngine;
using System.Collections;

[AddComponentMenu("Action-RPG Kit(C#)/Shop")]

public class ShopC : MonoBehaviour {
	
	public int[] itemShopSlot = new int[8];
	public int[] equipmentShopSlot = new int[8];
	public Texture2D button;
	public GameObject database;
	private GameObject player;
	
	private bool menu = false;
	private bool shopMain = false;
	private bool shopItem = false;
	private bool shopEquip = false;
	private bool itemInven = false;
	private bool equipInven = false;
	private bool sellwindow = false;
	private bool buywindow = false;
	private bool buyerror = false;
	//private bool  inputQty = false;
	private string buyErrorLog = "Not Enough Cash";
	
	private bool enter = false;
	private int select = 0;
	bool full = false;
	private int num = 1;
	private string text = "1";
	public bool activateSelf = true;
	
	void Update(){
		if(Input.GetKeyDown("e") && enter && activateSelf){
			OpenShop();
		}
	}

	public void OpenShop(){
		shopMain = true;
		OnOffMenu();
	}
	
	void ShopBuy(int id , int slot , int price , int quan){
		//ItemData dataItem = database.GetComponent<ItemData>();
		if(player.GetComponent<InventoryC>().cash < price){
			//If not enough cash
			print(price);
			buyErrorLog = "Not Enough Cash";
			buyerror = true;
			return;
		}
		
		if(shopItem){
			//Buy Usable Item	
			full = player.GetComponent<InventoryC>().AddItem(id , quan);
			if(full){
				buyErrorLog = "Inventory Full";
				buyerror = true;
				return;
			}
		}else{
			//Buy Equipment
			full = player.GetComponent<InventoryC>().AddEquipment(id);
			if(full){
				buyErrorLog = "Inventory Full";
				buyerror = true;
				return;
			}
		}
		//Remove Cash
		player.GetComponent<InventoryC>().cash -= price;
	}
	
	void ShopSell(int id , int slot , int price , int quan){
		//ItemData dataItem = database.GetComponent<ItemData>();
		if(itemInven){
			//Sell Usable Item	
			if(quan >= player.GetComponent<InventoryC>().itemQuantity[slot]){
				quan = player.GetComponent<InventoryC>().itemQuantity[slot];
			}
			player.GetComponent<InventoryC>().itemQuantity[slot] -= quan;
			if(player.GetComponent<InventoryC>().itemQuantity[slot] <= 0){
				player.GetComponent<InventoryC>().itemSlot[slot] = 0;
				player.GetComponent<InventoryC>().itemQuantity[slot] = 0;
				player.GetComponent<InventoryC>().AutoSortItem();
			}
			player.GetComponent<InventoryC>().UpdateAmmoUI();
			//Add Cash
			player.GetComponent<InventoryC>().cash += price * quan;
			
		}else{
			//Sell Equipment
			player.GetComponent<InventoryC>().equipment[slot] = 0;
			player.GetComponent<InventoryC>().AutoSortEquipment();
			
			//Add Cash
			player.GetComponent<InventoryC>().cash += price * quan;
		}
	}
	
	void OnGUI(){
		if(!player){
			return;
		}
		ItemDataC dataItem = database.GetComponent<ItemDataC>();
		int[] itemQuantity = player.GetComponent<InventoryC>().itemQuantity;
		int cash = player.GetComponent<InventoryC>().cash;
		int[] itemSlot = player.GetComponent<InventoryC>().itemSlot;
		int[] equipment = player.GetComponent<InventoryC>().equipment;
		
		if(enter && !menu && activateSelf){
			GUI.DrawTexture( new Rect(Screen.width / 2 - 130, Screen.height - 120, 260, 80), button);
		}
				
		//Shop Main Menu
		if(menu && shopMain){
			GUI.Box ( new Rect(Screen.width / 2 - 140,240,280,120), "Shop");
			if (GUI.Button ( new Rect(Screen.width / 2 - 100,305,80,30), "Buy")) {
				shopItem = true;
				shopMain = false;
			}
			if (GUI.Button ( new Rect(Screen.width / 2 + 35,305,80,30), "Sell")) {
				itemInven = true;
				shopMain = false;
			}
			if (GUI.Button ( new Rect(Screen.width / 2 + 90,245,30,30), "X")) {
				OnOffMenu();
			}
		}
		
		if(menu && itemInven && !sellwindow){
			GUI.Box ( new Rect(260,140,280,385), "Items");
			//Close Window Button
			if (GUI.Button ( new Rect(490,142,30,30), "X")) {
				OnOffMenu();
			}
			if (GUI.Button ( new Rect(290,255,50,50),new GUIContent (dataItem.usableItem[itemSlot[0]].icon, dataItem.usableItem[itemSlot[0]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[0]].description ))){
				select = 0;
				sellwindow = true;
			}
			if(itemQuantity[0] > 0){
				GUI.Label ( new Rect(330, 290, 20, 20), itemQuantity[0].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(350,255,50,50),new GUIContent (dataItem.usableItem[itemSlot[1]].icon, dataItem.usableItem[itemSlot[1]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[1]].description ))){
				select = 1;
				sellwindow = true;
			}
			if(itemQuantity[1] > 0){
				GUI.Label ( new Rect(390, 290, 20, 20), itemQuantity[1].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(410,255,50,50),new GUIContent (dataItem.usableItem[itemSlot[2]].icon, dataItem.usableItem[itemSlot[2]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[2]].description ))){
				select = 2;
				sellwindow = true;
			}
			if(itemQuantity[2] > 0){
				GUI.Label ( new Rect(450, 290, 20, 20), itemQuantity[2].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(470,255,50,50),new GUIContent (dataItem.usableItem[itemSlot[3]].icon, dataItem.usableItem[itemSlot[3]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[3]].description ))){
				select = 3;
				sellwindow = true;
			}
			if(itemQuantity[3] > 0){
				GUI.Label ( new Rect(510, 290, 20, 20), itemQuantity[3].ToString()); //Quantity
			}
			
			//-----------------------------
			if (GUI.Button ( new Rect(290,315,50,50),new GUIContent (dataItem.usableItem[itemSlot[4]].icon, dataItem.usableItem[itemSlot[4]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[4]].description ))){
				select = 4;
				sellwindow = true;
			}
			if(itemQuantity[4] > 0){
				GUI.Label ( new Rect(330, 350, 20, 20), itemQuantity[4].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(350,315,50,50),new GUIContent (dataItem.usableItem[itemSlot[5]].icon, dataItem.usableItem[itemSlot[5]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[5]].description ))){
				select = 5;
				sellwindow = true;
			}
			if(itemQuantity[5] > 0){
				GUI.Label ( new Rect(390, 350, 20, 20), itemQuantity[5].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(410,315,50,50),new GUIContent (dataItem.usableItem[itemSlot[6]].icon, dataItem.usableItem[itemSlot[6]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[6]].description ))){
				select = 6;
				sellwindow = true;
			}
			if(itemQuantity[6] > 0){
				GUI.Label ( new Rect(450, 350, 20, 20), itemQuantity[6].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(470,315,50,50),new GUIContent (dataItem.usableItem[itemSlot[7]].icon, dataItem.usableItem[itemSlot[7]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[7]].description ))){
				select = 7;
				sellwindow = true;
			}
			if(itemQuantity[7] > 0){
				GUI.Label ( new Rect(510, 350, 20, 20), itemQuantity[7].ToString()); //Quantity
			}
			//-----------------------------
			if (GUI.Button ( new Rect(290,375,50,50),new GUIContent (dataItem.usableItem[itemSlot[8]].icon, dataItem.usableItem[itemSlot[8]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[8]].description ))){
				select = 8;
				sellwindow = true;
			}
			if(itemQuantity[8] > 0){
				GUI.Label ( new Rect(330, 410, 20, 20), itemQuantity[8].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(350,375,50,50),new GUIContent (dataItem.usableItem[itemSlot[9]].icon, dataItem.usableItem[itemSlot[9]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[9]].description ))){
				select = 9;
				sellwindow = true;
			}
			if(itemQuantity[9] > 0){
				GUI.Label ( new Rect(390, 410, 20, 20), itemQuantity[9].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(410,375,50,50),new GUIContent (dataItem.usableItem[itemSlot[10]].icon, dataItem.usableItem[itemSlot[10]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[10]].description ))){
				select = 10;
				sellwindow = true;
			}
			if(itemQuantity[10] > 0){
				GUI.Label ( new Rect(450, 410, 20, 20), itemQuantity[10].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(470,375,50,50),new GUIContent (dataItem.usableItem[itemSlot[11]].icon, dataItem.usableItem[itemSlot[11]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[11]].description ))){
				select = 11;
				sellwindow = true;
			}
			if(itemQuantity[11] > 0){
				GUI.Label ( new Rect(510, 410, 20, 20), itemQuantity[11].ToString()); //Quantity
			}
			//-----------------------------
			if (GUI.Button ( new Rect(290,435,50,50),new GUIContent (dataItem.usableItem[itemSlot[12]].icon, dataItem.usableItem[itemSlot[12]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[12]].description ))){
				select = 12;
				sellwindow = true;
			}
			if(itemQuantity[12] > 0){
				GUI.Label ( new Rect(330, 470, 20, 20), itemQuantity[12].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(350,435,50,50),new GUIContent (dataItem.usableItem[itemSlot[13]].icon, dataItem.usableItem[itemSlot[13]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[13]].description ))){
				select = 13;
				sellwindow = true;
			}
			if(itemQuantity[13] > 0){
				GUI.Label ( new Rect(390, 470, 20, 20), itemQuantity[13].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(410,435,50,50),new GUIContent (dataItem.usableItem[itemSlot[14]].icon, dataItem.usableItem[itemSlot[14]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[14]].description ))){
				select = 14;
				sellwindow = true;
			}
			if(itemQuantity[14] > 0){
				GUI.Label ( new Rect(450, 470, 20, 20), itemQuantity[14].ToString()); //Quantity
			}
			
			if (GUI.Button ( new Rect(470,435,50,50),new GUIContent (dataItem.usableItem[itemSlot[15]].icon, dataItem.usableItem[itemSlot[15]].itemName + "\n" + "\n" + dataItem.usableItem[itemSlot[15]].description ))){
				select = 15;
				sellwindow = true;
			}
			if(itemQuantity[15] > 0){
				GUI.Label ( new Rect(510, 470, 20, 20), itemQuantity[15].ToString()); //Quantity
			}
			GUI.Box ( new Rect(280,170,240,60), GUI.tooltip);
			//-----------------------------
			GUI.Label ( new Rect(280, 495, 150, 50), "$ " + cash.ToString());
			
			if (GUI.Button ( new Rect(210,245,50,100), "Item")) {
				//Switch to Item Tab
			}
			if (GUI.Button ( new Rect(210,365,50,100), "Equip")) {
				//Switch to Equipment Tab
				equipInven = true;
				itemInven = false;
			}
		}
		
		if(menu && equipInven && !sellwindow){
			GUI.Box ( new Rect(260,140,280,385), "Equipment");
			//Close Window Button
			if (GUI.Button ( new Rect(490,142,30,30), "X")) {
				OnOffMenu();
			}
			//Item Name
			if (GUI.Button ( new Rect(210,245,50,100), "Item")) {
				//Switch to Item Tab
				itemInven = true;
				equipInven = false;
			}
			if (GUI.Button ( new Rect(210,365,50,100), "Equip")) {
				//Switch to Equipment Tab
			}
			GUI.Label ( new Rect(280, 495, 150, 50), "$ " + cash.ToString());
			//--------Equipment Slot---------
			if (GUI.Button ( new Rect(290,375,50,50),new GUIContent (dataItem.equipment[equipment[0]].icon, dataItem.equipment[equipment[0]].itemName + "\n" + "\n" + dataItem.equipment[equipment[0]].description ))){
				select = 0;
				sellwindow = true;
			}
			
			if (GUI.Button ( new Rect(350,375,50,50),new GUIContent (dataItem.equipment[equipment[1]].icon, dataItem.equipment[equipment[1]].itemName + "\n" + "\n" + dataItem.equipment[equipment[1]].description ))){
				select = 1;
				sellwindow = true;
			}
			
			if (GUI.Button ( new Rect(410,375,50,50),new GUIContent (dataItem.equipment[equipment[2]].icon, dataItem.equipment[equipment[2]].itemName + "\n" + "\n" + dataItem.equipment[equipment[2]].description ))){
				select = 2;
				sellwindow = true;
			}
			
			if (GUI.Button ( new Rect(470,375,50,50),new GUIContent (dataItem.equipment[equipment[3]].icon, dataItem.equipment[equipment[3]].itemName + "\n" + "\n" + dataItem.equipment[equipment[3]].description ))){
				select = 3;
				sellwindow = true;
			}
			//-----------------------------
			if (GUI.Button ( new Rect(290,435,50,50),new GUIContent (dataItem.equipment[equipment[4]].icon, dataItem.equipment[equipment[4]].itemName + "\n" + "\n" + dataItem.equipment[equipment[4]].description ))){
				select = 4;
				sellwindow = true;
			}
			
			if (GUI.Button ( new Rect(350,435,50,50),new GUIContent (dataItem.equipment[equipment[5]].icon, dataItem.equipment[equipment[5]].itemName + "\n" + "\n" + dataItem.equipment[equipment[5]].description ))){
				select = 5;
				sellwindow = true;
			}
			
			if (GUI.Button ( new Rect(410,435,50,50),new GUIContent (dataItem.equipment[equipment[6]].icon, dataItem.equipment[equipment[6]].itemName + "\n" + "\n" + dataItem.equipment[equipment[6]].description ))){
				select = 6;
				sellwindow = true;
			}
			
			if (GUI.Button ( new Rect(470,435,50,50),new GUIContent (dataItem.equipment[equipment[7]].icon, dataItem.equipment[equipment[7]].itemName + "\n" + "\n" + dataItem.equipment[equipment[7]].description ))){
				select = 7;
				sellwindow = true;
			}
			GUI.Box ( new Rect(280,170,240,60), GUI.tooltip);
			
		}
		
		//---------------Sell Item Confirm Window------------------
		if(sellwindow){
			if(itemInven){
				if(itemSlot[select] == 0){
					sellwindow = false;
				}
				GUI.Box ( new Rect(Screen.width / 2 - 140,230,280,120), "Price " + dataItem.usableItem[itemSlot[select]].price /2);
				
				//------------------Quantity--------------
				text = GUI.TextField(new Rect(Screen.width / 2 +5, 250, 50, 20), num.ToString() , 2);
				GUI.Label ( new Rect(Screen.width / 2 -65, 250, 60, 20), "Quantity");
				int temp = 0;
				if (int.TryParse(text , out temp)){
					//num = Mathf.Clamp(0, out temp);
					num = temp;
				}else if (text == ""){
					num = 0;
				}
				//-----------------------------------
				
			}else{
				if(equipment[select] == 0){
					sellwindow = false;
				}
				GUI.Box ( new Rect(Screen.width / 2 - 140,230,280,120), "Price " + dataItem.equipment[equipment[select]].price /2);
			}
			if (GUI.Button ( new Rect(Screen.width / 2 - 100,285,80,30), "Sell")) {
				if(itemInven){
					//Sell Usable Item
					if(num > 0){
						ShopSell(itemSlot[select] , select , dataItem.usableItem[itemSlot[select]].price /2 , num);
						sellwindow = false;
					}
				}else{
					//Sell Equipment
					ShopSell(equipment[select] , select , dataItem.equipment[equipment[select]].price /2 , 1);
					sellwindow = false;
				}
				
			}
			if (GUI.Button ( new Rect(Screen.width / 2 + 35,285,80,30), "Cancel")) {
				sellwindow = false;
			}
		}
		//---------------------------------------------------------------------------------------------
		//---------------------------------------------------------------------------------------------
		//-----------------------------------BUY----------------------------------------------------
		//---------------------------------------------------------------------------------------------
		//---------------------------------------------------------------------------------------------
		
		//-----------Buy Usable Item---------------------
		if(menu && shopItem && !buywindow && !buyerror){
			GUI.Box ( new Rect(260,140,280,285), "Shop");
			GUI.Label ( new Rect(280, 395, 150, 50), "$ " + cash.ToString());
			//Close Window Button
			if (GUI.Button ( new Rect(490,142,30,30), "X")) {
				OnOffMenu();
			}
			if (GUI.Button ( new Rect(290,255,50,50),new GUIContent (dataItem.usableItem[itemShopSlot[0]].icon, dataItem.usableItem[itemShopSlot[0]].itemName + "\n" + "\n" + dataItem.usableItem[itemShopSlot[0]].description ))){
				select = 0;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(350,255,50,50),new GUIContent (dataItem.usableItem[itemShopSlot[1]].icon, dataItem.usableItem[itemShopSlot[1]].itemName + "\n" + "\n" + dataItem.usableItem[itemShopSlot[1]].description ))){
				select = 1;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(410,255,50,50),new GUIContent (dataItem.usableItem[itemShopSlot[2]].icon, dataItem.usableItem[itemShopSlot[2]].itemName + "\n" + "\n" + dataItem.usableItem[itemShopSlot[2]].description ))){
				select = 2;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(470,255,50,50),new GUIContent (dataItem.usableItem[itemShopSlot[3]].icon, dataItem.usableItem[itemShopSlot[3]].itemName + "\n" + "\n" + dataItem.usableItem[itemShopSlot[3]].description ))){
				select = 3;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(290,315,50,50),new GUIContent (dataItem.usableItem[itemShopSlot[4]].icon, dataItem.usableItem[itemShopSlot[4]].itemName + "\n" + "\n" + dataItem.usableItem[itemShopSlot[4]].description ))){
				select = 4;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(350,315,50,50),new GUIContent (dataItem.usableItem[itemShopSlot[5]].icon, dataItem.usableItem[itemShopSlot[5]].itemName + "\n" + "\n" + dataItem.usableItem[itemShopSlot[5]].description ))){
				select = 5;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(410,315,50,50),new GUIContent (dataItem.usableItem[itemShopSlot[6]].icon, dataItem.usableItem[itemShopSlot[6]].itemName + "\n" + "\n" + dataItem.usableItem[itemShopSlot[6]].description ))){
				select = 6;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(470,315,50,50),new GUIContent (dataItem.usableItem[itemShopSlot[7]].icon, dataItem.usableItem[itemShopSlot[7]].itemName + "\n" + "\n" + dataItem.usableItem[itemShopSlot[7]].description ))){
				select = 7;
				buywindow = true;
			}
			GUI.Box ( new Rect(280,170,240,60), GUI.tooltip);
			
			if (GUI.Button ( new Rect(210,245,50,75), "Item")) {
				//Switch to Item Tab
			}
			if (GUI.Button ( new Rect(210,320,50,75), "Equip")) {
				//Switch to Equipment Tab
				shopEquip = true;
				shopItem = false;
			}
		}
		
		//-----------Buy Equipment Item---------------------
		if(menu && shopEquip && !buywindow && !buyerror){
			GUI.Box ( new Rect(260,140,280,285), "Shop");
			GUI.Label ( new Rect(280, 395, 150, 50), "$ " + cash.ToString());
			//Close Window Button
			if (GUI.Button ( new Rect(490,142,30,30), "X")) {
				OnOffMenu();
			}
			if (GUI.Button ( new Rect(290,255,50,50),new GUIContent (dataItem.equipment[equipmentShopSlot[0]].icon, dataItem.equipment[equipmentShopSlot[0]].itemName + "\n" + "\n" + dataItem.equipment[equipmentShopSlot[0]].description ))){
				select = 0;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(350,255,50,50),new GUIContent (dataItem.equipment[equipmentShopSlot[1]].icon, dataItem.equipment[equipmentShopSlot[1]].itemName + "\n" + "\n" + dataItem.equipment[equipmentShopSlot[1]].description ))){
				select = 1;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(410,255,50,50),new GUIContent (dataItem.equipment[equipmentShopSlot[2]].icon, dataItem.equipment[equipmentShopSlot[2]].itemName + "\n" + "\n" + dataItem.equipment[equipmentShopSlot[2]].description ))){
				select = 2;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(470,255,50,50),new GUIContent (dataItem.equipment[equipmentShopSlot[3]].icon, dataItem.equipment[equipmentShopSlot[3]].itemName + "\n" + "\n" + dataItem.equipment[equipmentShopSlot[3]].description ))){
				select = 3;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(290,315,50,50),new GUIContent (dataItem.equipment[equipmentShopSlot[4]].icon, dataItem.equipment[equipmentShopSlot[4]].itemName + "\n" + "\n" + dataItem.equipment[equipmentShopSlot[4]].description ))){
				select = 4;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(350,315,50,50),new GUIContent (dataItem.equipment[equipmentShopSlot[5]].icon, dataItem.equipment[equipmentShopSlot[5]].itemName + "\n" + "\n" + dataItem.equipment[equipmentShopSlot[5]].description ))){
				select = 5;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(410,315,50,50),new GUIContent (dataItem.equipment[equipmentShopSlot[6]].icon, dataItem.equipment[equipmentShopSlot[6]].itemName + "\n" + "\n" + dataItem.equipment[equipmentShopSlot[6]].description ))){
				select = 6;
				buywindow = true;
			}
			
			if (GUI.Button ( new Rect(470,315,50,50),new GUIContent (dataItem.equipment[equipmentShopSlot[7]].icon, dataItem.equipment[equipmentShopSlot[7]].itemName + "\n" + "\n" + dataItem.equipment[equipmentShopSlot[7]].description ))){
				select = 7;
				buywindow = true;
			}
			GUI.Box ( new Rect(280,170,240,60), GUI.tooltip);
			
			if (GUI.Button ( new Rect(210,245,50,75), "Item")) {
				//Switch to Item Tab
				shopItem = true;
				shopEquip = false;
			}
			if (GUI.Button ( new Rect(210,320,50,75), "Equip")) {
				//Switch to Equipment Tab
			}
		}
		
		//---------------Buy Item Confirm Window------------------
		if(buywindow){
			if(shopItem){
				if(itemShopSlot[select] == 0){
					buywindow = false;
				}
				GUI.Box ( new Rect(Screen.width / 2 - 140,230,280,120), "Price " + dataItem.usableItem[itemShopSlot[select]].price);
				//------------------Quantity--------------
				text = GUI.TextField(new Rect(Screen.width / 2 +5, 250, 50, 20), num.ToString() , 2);
				GUI.Label ( new Rect(Screen.width / 2 -65, 250, 60, 20), "Quantity");
				int temp = 0;
				if (int.TryParse(text , out temp)){
					//num = Mathf.Clamp(0, out temp);
					num = temp;
				}else if (text == ""){
					num = 0;
				}
				//-----------------------------------
			}else{
				if(equipmentShopSlot[select] == 0){
					buywindow = false;
				}
				GUI.Box ( new Rect(Screen.width / 2 - 140,230,280,120), "Price " + dataItem.equipment[equipmentShopSlot[select]].price);
			}
			if (GUI.Button ( new Rect(Screen.width / 2 - 100,285,80,30), "Buy")) {
				if(shopItem){
					//Sell Usable Item
					//print (num);
					if(num > 0){
						ShopBuy(itemShopSlot[select] , select , dataItem.usableItem[itemShopSlot[select]].price * num , num);
						buywindow = false;
					}
				}else{
					//Sell Equipment
					ShopBuy(equipmentShopSlot[select] , select , dataItem.equipment[equipmentShopSlot[select]].price , 1);
					buywindow = false;
				}
				
			}
			if (GUI.Button ( new Rect(Screen.width / 2 + 35,285,80,30), "Cancel")) {
				buywindow = false;
			}
		}
		//Error When Buying
		if(buyerror){
			GUI.Box ( new Rect(Screen.width / 2 - 140,230,280,120), buyErrorLog);
			if (GUI.Button ( new Rect(Screen.width / 2 - 40,285,80,30), "OK")) {
				buyerror = false;
			}
		}
		
	}
	
	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Player") {
			player = other.gameObject;
			enter = true;
		}
		
	}
	
	
	void OnTriggerExit(Collider other){
		if (other.gameObject.tag == "Player") {
			enter = false;
		}
	}
	
	void OnOffMenu(){
		//Freeze Time Scale to 0 if Window is Showing
		if(!menu && Time.timeScale != 0.0f){
			GlobalConditionC.interacting = true;
			menu = true;
			itemInven = false;
			shopItem = false;
			shopEquip = false;
			equipInven = false;
			sellwindow = false;
			buywindow = false;
			buyerror = false;
			//shopMain = false;
			//Screen.lockCursor = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			Time.timeScale = 0.0f;
		}else if(menu){
			GlobalConditionC.interacting = false;
			menu = false;
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Time.timeScale = 1.0f;
		}
	}
}