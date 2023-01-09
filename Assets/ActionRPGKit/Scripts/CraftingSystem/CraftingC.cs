using UnityEngine;
using System.Collections;

public class CraftingC : MonoBehaviour {

	public int[] craftingListId = new int[3];
	private CraftData[] craftingData;
	public GameObject itemDatabase;
	public GameObject craftingDatabase;
	private GameObject player;
	public GUISkin uiSkin;
	public Texture2D button;
	
	private ItemDataC itemdb;
	private CraftingDataC craftdb;
	
	private int uiPage = 0;
	private int page = 0;
	private int maxPage = 1;
	private int selection = 0;
	private string showError = "";
	private bool enter = false;
	
	private int itemQty1;
	private int itemQty2;
	private int itemQty3;
	private int itemQty4;
	private int itemQty5;
	public bool activateSelf = true;
	
	void Start(){
		if(!player){
			player = GameObject.FindWithTag("Player");
		}
		itemdb = itemDatabase.GetComponent<ItemDataC>();
		craftdb = craftingDatabase.GetComponent<CraftingDataC>();
		
		GetCraftingData();
		//uiPage = 1;
	}
	
	void GetCraftingData (){
		craftingData = new CraftData[craftingListId.Length];
		int a = 0;
		while(a < craftingData.Length){
			craftingData[a] = craftdb.craftingData[craftingListId[a]];
			a++;
		}
		//Set Max Page
		maxPage = craftingData.Length / 9;
		if(craftingData.Length % 9 != 0){
			maxPage += 1;
		}
		print(maxPage);
	}
	
	void Update(){
		if(Input.GetKeyDown("e") && enter && activateSelf){
			OnOffMenu();
		}
	}
	
	void OnGUI(){
		GUI.skin = uiSkin;
		if(enter && uiPage == 0 && activateSelf){
			GUI.DrawTexture( new Rect(Screen.width / 2 - 130, Screen.height - 120, 260, 80), button);
		}
		
		if(uiPage == 1){
			//Show All Crafting List
			GUI.Box ( new Rect(Screen.width /2 - 215, 70 ,430,500), "Crafting Menu");
			if(GUI.Button(new Rect(Screen.width /2 + 175 ,75,33,33), "X")){
				OnOffMenu();
			}
			
			if(page + 0 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,95,350,40), craftingData[page + 0].itemName)) {
					selection = page + 0;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
			if(page + 1 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,145,350,40), craftingData[page + 1].itemName)) {
					selection = page + 1;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
			if(page + 2 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,195,350,40), craftingData[page + 2].itemName)) {
					selection = page + 2;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
			if(page + 3 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,245,350,40), craftingData[page + 3].itemName)) {
					selection = page + 3;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
			if(page + 4 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,295,350,40), craftingData[page + 4].itemName)) {
					selection = page + 4;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
			if(page + 5 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,345,350,40), craftingData[page + 5].itemName)) {
					selection = page + 5;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
			if(page + 6 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,395,350,40), craftingData[page + 6].itemName)) {
					selection = page + 6;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
			if(page + 7 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,445,350,40), craftingData[page + 7].itemName)) {
					selection = page + 7;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
			if(page + 8 < craftingData.Length){
				if(GUI.Button(new Rect(Screen.width /2 - 175,495,350,40), craftingData[page + 8].itemName)) {
					selection = page + 8;
					ShowItemQuantity();
					uiPage = 2;
				}
			}
		}
		
		//Show Ingredient
		if(uiPage == 2){
			GUI.Box ( new Rect(Screen.width /2 - 200, 70 ,400,300), craftingData[selection].itemName);
			if(GUI.Button(new Rect(Screen.width /2 + 155 ,75,40,40), "X")){
				showError = "";
				uiPage = 1;
			}
			
			GUI.Label ( new Rect(Screen.width /2 - 50, 330, 200, 100), showError);
			
			string ingrediantName1 = "";
			if((int)craftingData[selection].ingredient[0].itemType == 0){
				ingrediantName1 = itemdb.usableItem[craftingData[selection].ingredient[0].itemId].itemName;
				//-------------------
			}else if((int)craftingData[selection].ingredient[0].itemType == 1){
				ingrediantName1 = itemdb.equipment[craftingData[selection].ingredient[0].itemId].itemName;
				//-------------------
			}
			
			GUI.Label ( new Rect(Screen.width /2 - 180, 150, 500, 100), ingrediantName1);
			GUI.Label ( new Rect(Screen.width /2 + 10, 150, 500, 100), craftingData[selection].ingredient[0].quantity.ToString());
			GUI.Label ( new Rect(Screen.width /2 + 40, 150, 500, 100), "(" + itemQty1 + ")");
			
			//=========================================================
			
			if(craftingData[selection].ingredient.Length >= 2){
				string ingrediantName2 = "";
				if((int)craftingData[selection].ingredient[1].itemType == 0){
					ingrediantName2 = itemdb.usableItem[craftingData[selection].ingredient[1].itemId].itemName;
					//-------------------
				}else if((int)craftingData[selection].ingredient[1].itemType == 1){
					ingrediantName2 = itemdb.equipment[craftingData[selection].ingredient[1].itemId].itemName;
					//-------------------
				}
				
				GUI.Label ( new Rect(Screen.width /2 - 180, 170, 500, 100), ingrediantName2);
				GUI.Label ( new Rect(Screen.width /2 + 10, 170, 500, 100), craftingData[selection].ingredient[1].quantity.ToString());
				GUI.Label ( new Rect(Screen.width /2 + 40, 170, 500, 100), "(" + itemQty2 + ")");
			}
			//=========================================================
			
			if(craftingData[selection].ingredient.Length >= 3){
				string ingrediantName3 = "";
				if((int)craftingData[selection].ingredient[2].itemType == 0){
					ingrediantName3 = itemdb.usableItem[craftingData[selection].ingredient[2].itemId].itemName;
					//-------------------
				}else if((int)craftingData[selection].ingredient[2].itemType == 1){
					ingrediantName3 = itemdb.equipment[craftingData[selection].ingredient[2].itemId].itemName;
					//-------------------
				}
				
				GUI.Label ( new Rect(Screen.width /2 - 180, 190, 500, 100), ingrediantName3);
				GUI.Label ( new Rect(Screen.width /2 + 10, 190, 500, 100), craftingData[selection].ingredient[2].quantity.ToString());
				GUI.Label ( new Rect(Screen.width /2 + 40, 190, 500, 100), "(" + itemQty3 + ")");
			}
			//=========================================================
			
			if(craftingData[selection].ingredient.Length >= 4){
				string ingrediantName4 = "";
				if((int)craftingData[selection].ingredient[3].itemType == 0){
					ingrediantName4 = itemdb.usableItem[craftingData[selection].ingredient[3].itemId].itemName;
					//-------------------
				}else if((int)craftingData[selection].ingredient[3].itemType == 1){
					ingrediantName4 = itemdb.equipment[craftingData[selection].ingredient[3].itemId].itemName;
					//-------------------
				}
				
				GUI.Label ( new Rect(Screen.width /2 - 180, 210, 500, 100), ingrediantName4);
				GUI.Label ( new Rect(Screen.width /2 + 10, 210, 500, 100), craftingData[selection].ingredient[3].quantity.ToString());
				GUI.Label ( new Rect(Screen.width /2 + 40, 210, 500, 100), "(" + itemQty4 + ")");
			}
			//=========================================================
			
			if(craftingData[selection].ingredient.Length >= 5){
				string ingrediantName5 = "";
				if((int)craftingData[selection].ingredient[4].itemType == 0){
					ingrediantName5 = itemdb.usableItem[craftingData[selection].ingredient[4].itemId].itemName;
					//-------------------
				}else if((int)craftingData[selection].ingredient[4].itemType == 1){
					ingrediantName5 = itemdb.equipment[craftingData[selection].ingredient[4].itemId].itemName;
					//-------------------
				}
				
				GUI.Label ( new Rect(Screen.width /2 - 180, 230, 500, 100), ingrediantName5);
				GUI.Label ( new Rect(Screen.width /2 + 10, 230, 500, 100), craftingData[selection].ingredient[4].quantity.ToString());
				GUI.Label ( new Rect(Screen.width /2 + 40, 230, 500, 100), "(" + itemQty5 + ")");
			}
			//=========================================================
			
			if(GUI.Button(new Rect(Screen.width /2 -60 ,270,120,50), "Craft")){
				//uiPage = 1;
				bool canCraft = CheckIngredients();
				if(canCraft){
					AddandRemoveItem();
				}
				ShowItemQuantity();
			}
			
		}
		
	}
	
	void ShowItemQuantity(){
		int a = 0;
		
		if((int)craftingData[selection].ingredient[0].itemType == 0){
			//Usable
			a = player.GetComponent<InventoryC>().FindItemSlot(craftingData[selection].ingredient[0].itemId);
			if(a <= player.GetComponent<InventoryC>().itemSlot.Length){
				itemQty1 = player.GetComponent<InventoryC>().itemQuantity[a];
			}else{
				itemQty1 = 0;
			}
		}else if((int)craftingData[selection].ingredient[0].itemType == 1){
			//Equipment
			a = player.GetComponent<InventoryC>().FindEquipmentSlot(craftingData[selection].ingredient[0].itemId);
			if(a <= player.GetComponent<InventoryC>().equipment.Length){
				itemQty1 = 1;
			}else{
				itemQty1 = 0;
			}
		}
		
		//=========================================================
		if(craftingData[selection].ingredient.Length >= 2){
			if((int)craftingData[selection].ingredient[1].itemType == 0){
				//Usable
				a = player.GetComponent<InventoryC>().FindItemSlot(craftingData[selection].ingredient[1].itemId);
				if(a <= player.GetComponent<InventoryC>().itemSlot.Length){
					itemQty2 = player.GetComponent<InventoryC>().itemQuantity[a];
				}else{
					itemQty2 = 0;
				}
			}else if((int)craftingData[selection].ingredient[1].itemType == 1){
				//Equipment
				a = player.GetComponent<InventoryC>().FindEquipmentSlot(craftingData[selection].ingredient[1].itemId);
				if(a <= player.GetComponent<InventoryC>().equipment.Length){
					itemQty2 = 1;
				}else{
					itemQty2 = 0;
				}
			}
		}
		//=========================================================
		if(craftingData[selection].ingredient.Length >= 3){
			if((int)craftingData[selection].ingredient[2].itemType == 0){
				//Usable
				a = player.GetComponent<InventoryC>().FindItemSlot(craftingData[selection].ingredient[2].itemId);
				if(a <= player.GetComponent<InventoryC>().itemSlot.Length){
					itemQty3 = player.GetComponent<InventoryC>().itemQuantity[a];
				}else{
					itemQty3 = 0;
				}
			}else if((int)craftingData[selection].ingredient[2].itemType == 1){
				//Equipment
				a = player.GetComponent<InventoryC>().FindEquipmentSlot(craftingData[selection].ingredient[2].itemId);
				if(a <= player.GetComponent<InventoryC>().equipment.Length){
					itemQty3 = 1;
				}else{
					itemQty3 = 0;
				}
			}
		}
		//=========================================================
		if(craftingData[selection].ingredient.Length >= 4){
			if((int)craftingData[selection].ingredient[3].itemType == 0){
				//Usable
				a = player.GetComponent<InventoryC>().FindItemSlot(craftingData[selection].ingredient[3].itemId);
				if(a <= player.GetComponent<InventoryC>().itemSlot.Length){
					itemQty4 = player.GetComponent<InventoryC>().itemQuantity[a];
				}else{
					itemQty4 = 0;
				}
			}else if((int)craftingData[selection].ingredient[3].itemType == 1){
				//Equipment
				a = player.GetComponent<InventoryC>().FindEquipmentSlot(craftingData[selection].ingredient[3].itemId);
				if(a <= player.GetComponent<InventoryC>().equipment.Length){
					itemQty4 = 1;
				}else{
					itemQty4 = 0;
				}
			}
		}
		//=========================================================
		if(craftingData[selection].ingredient.Length >= 5){
			if((int)craftingData[selection].ingredient[4].itemType == 0){
				//Usable
				a = player.GetComponent<InventoryC>().FindItemSlot(craftingData[selection].ingredient[4].itemId);
				if(a <= player.GetComponent<InventoryC>().itemSlot.Length){
					itemQty5 = player.GetComponent<InventoryC>().itemQuantity[a];
				}else{
					itemQty5 = 0;
				}
			}else if((int)craftingData[selection].ingredient[4].itemType == 1){
				//Equipment
				a = player.GetComponent<InventoryC>().FindEquipmentSlot(craftingData[selection].ingredient[4].itemId);
				if(a <= player.GetComponent<InventoryC>().equipment.Length){
					itemQty5 = 1;
				}else{
					itemQty5 = 0;
				}
			}
		}
	}
	
	bool CheckIngredients(){
		int a = 0;
		while(a < craftingData[selection].ingredient.Length){
			bool  item = player.GetComponent<InventoryC>().CheckItem(craftingData[selection].ingredient[a].itemId , (int)craftingData[selection].ingredient[a].itemType, craftingData[selection].ingredient[a].quantity);
			if(!item){
				showError = "Not enought items";
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
			showError = "You Got " + craftingData[selection].itemName;
			
		}else{
			showError = "Inventory Full";
		}
		
	}
	
	void OnOffMenu(){
		//Freeze Time Scale to 0 if Window is Showing
		if(uiPage == 0 && Time.timeScale != 0.0f){
			if(!player){
				player = GameObject.FindWithTag("Player");
			}
			GlobalConditionC.interacting = true;
			uiPage = 1;
			Time.timeScale = 0.0f;
			showError = "";
			//Screen.lockCursor = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}else if(uiPage >= 1){
			GlobalConditionC.interacting = false;
			uiPage = 0;
			Time.timeScale = 1.0f;
			//Screen.lockCursor = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}
	
	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Player") {
			InventoryC inven = other.GetComponent<InventoryC>();
			if(inven){
				player = other.gameObject;
				enter = true;
			}
		}
	}
	
	void OnTriggerExit(Collider other){
		//if (other.gameObject.tag == "Player") {
		if(other.gameObject == player) {
			enter = false;
		}
	}
}

