using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkillButtonCanvasC : MonoBehaviour {
	public int buttonId = 0;
	public Image iconImageObj;
	public Sprite icon;
	public Sprite iconLocked;
	
	public string skillName = "";
	public string description = "";
	public string description2 = "";
	public string description3 = "";
	
	public SkillTreeCanvasC skillTree;
	
	public GameObject tooltip;
	public Image tooltipIcon;
	public Text tooltipName;
	public Text tooltipText1;
	public Text tooltipText2;
	public Text tooltipText3;

	void Start(){
		if(!skillTree){
			skillTree = transform.root.GetComponent<SkillTreeCanvasC>();
		}
	}
	
	void Update(){
		if(tooltip && tooltip.activeSelf == true){
			Vector2 tooltipPos = Input.mousePosition;
			tooltipPos.x += 7;
			tooltip.transform.position = tooltipPos;
		}
	}
	
	public void UpdateIcon(){
		iconImageObj.color = Color.white;
		if(skillTree.skillSlots[buttonId].locked){
			iconImageObj.sprite = iconLocked;
			return;
		}else{
			iconImageObj.sprite = icon;
		}
		
		if(!skillTree.skillSlots[buttonId].learned){
			iconImageObj.color = Color.gray;
		}
	}
	
	public void ShowSkillTooltip(){
		if(!tooltip){
			return;
		}
		tooltipIcon.sprite = icon;
		tooltipName.text = skillName;
		
		tooltipText1.text = description;
		tooltipText2.text = description2;
		tooltipText3.text = description3;
		
		tooltip.SetActive(true);
	}
	
	public void HideTooltip(){
		if(!tooltip){
			return;
		}
		tooltip.SetActive(false);
	}
}