using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(EventSetting))]
public class CustomEventEditor : Editor{
	
	public override void OnInspectorGUI(){
		EventSetting script = (EventSetting)target;
		
		script.type = (ParametersType)EditorGUILayout.EnumPopup("Event", script.type);

		script.noteId = EditorGUILayout.TextField("Event Id Note", script.noteId);
		//EditorGUILayout.LabelField("Event ID = " + script.GetComponents<EventSetting>().);

		if(script.type == ParametersType.Dialogue){
			script.message = EditorGUILayout.TextArea(script.message, GUILayout.Height(60));
			script.showText = EditorGUILayout.ObjectField("Text Source", script.showText , typeof(Text), true) as Text;
			EditorGUILayout.LabelField("Assign Text UI (UI/Text) to 'Text Source'");

			script.textBoxVar = EditorGUILayout.ObjectField("Text Box", script.textBoxVar , typeof(Image), true) as Image;
			script.imageVar = EditorGUILayout.ObjectField("Show Image", script.imageVar , typeof(Image), true) as Image;
			script.colorVar = EditorGUILayout.ColorField("Image Color", script.colorVar);
			script.imageVar2 = EditorGUILayout.ObjectField("Show Image 2", script.imageVar2 , typeof(Image), true) as Image;
			script.colorVar3 = EditorGUILayout.ColorField("Image Color 2", script.colorVar3);
			script.stringVar = EditorGUILayout.TextField("Character Name", script.stringVar);
			script.nameText = EditorGUILayout.ObjectField("Name Text Source", script.nameText , typeof(Text), true) as Text;
		}

		if(script.type == ParametersType.Wait){
			script.floatVar = EditorGUILayout.FloatField("Wait Duration", script.floatVar);
		}
		if(script.type == ParametersType.ActivateOtherEvent){
			script.eventVar = EditorGUILayout.ObjectField("Event", script.eventVar , typeof(EventActivator), true) as EventActivator;
		}

		if(script.type == ParametersType.SpawnPrefabAtObject){
			script.objectVar = EditorGUILayout.ObjectField("Prefab", script.objectVar , typeof(Transform), true) as Transform;
			script.target = EditorGUILayout.ObjectField("Spawn At", script.target , typeof(Transform), true) as Transform;
			script.targetIsPlayer = EditorGUILayout.Toggle("Target is Player", script.targetIsPlayer);

			script.msgString = EditorGUILayout.TextField("Send Message to", script.msgString);
		}

		if(script.type == ParametersType.SendMessage){
			script.msgString = EditorGUILayout.TextField("Send Message", script.msgString);
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
		}

		if(script.type == ParametersType.GoToEvent){
			script.intVar = EditorGUILayout.IntField("Go to Event ID", script.intVar);
		}

		if(script.type == ParametersType.EnableObject || script.type == ParametersType.DisableObject || script.type == ParametersType.DeleteObject){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.targetIsPlayer = EditorGUILayout.Toggle("Target is Player", script.targetIsPlayer);
		}
		if(script.type == ParametersType.PlayLegacyAnimation){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.targetIsPlayer = EditorGUILayout.Toggle("Target is Player", script.targetIsPlayer);
			script.animClip = EditorGUILayout.ObjectField("Animation Clip", script.animClip , typeof(AnimationClip), true) as AnimationClip;
			script.intVar = EditorGUILayout.IntField("Layer", script.intVar);
			script.boolVar = EditorGUILayout.Toggle("Stop Animation", script.boolVar);
		}
		if(script.type == ParametersType.PlayMecanimAnimation){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.targetIsPlayer = EditorGUILayout.Toggle("Target is Player", script.targetIsPlayer);
			script.animClip = EditorGUILayout.ObjectField("Animation Clip", script.animClip , typeof(AnimationClip), true) as AnimationClip;

			script.boolVar = EditorGUILayout.Toggle("Set Trigger", script.boolVar);
			script.stringVar = EditorGUILayout.TextField("Trigger Name", script.stringVar);
		}
		if(script.type == ParametersType.ControlBoolean){
			EditorGUILayout.LabelField("ID (0-999)");
			script.intVar = EditorGUILayout.IntField("Bool ID", script.intVar);
			script.boolVar = EditorGUILayout.Toggle("Switch", script.boolVar);
		}
		if(script.type == ParametersType.ControlVariable){
			EditorGUILayout.LabelField("ID (0-999)");
			script.intVar = EditorGUILayout.IntField("Variable ID", script.intVar);
			script.intVar2 = EditorGUILayout.IntField("Value", script.intVar2);
			script.math = (IntValueSet)EditorGUILayout.EnumPopup("Set", script.math);
		}
		if(script.type == ParametersType.PlayBGM){
			script.target = EditorGUILayout.ObjectField("Source", script.target , typeof(Transform), true) as Transform;
			script.audiClip = EditorGUILayout.ObjectField("Audio Clip", script.audiClip , typeof(AudioClip), true) as AudioClip;
			script.boolVar = EditorGUILayout.Toggle("Stop BGM", script.boolVar);
			script.boolVar2 = EditorGUILayout.Toggle("Loop", script.boolVar2);
		}
		if(script.type == ParametersType.PlayOneShotAudio){
			script.target = EditorGUILayout.ObjectField("Source", script.target , typeof(Transform), true) as Transform;
			script.audiClip = EditorGUILayout.ObjectField("Audio Clip", script.audiClip , typeof(AudioClip), true) as AudioClip;
		}
		if(script.type == ParametersType.ChangeScene){
			script.stringVar = EditorGUILayout.TextField("Go to Scene", script.stringVar);
			script.boolVar = EditorGUILayout.Toggle("Spawn at Obj name", script.boolVar);
			script.stringVar2 = EditorGUILayout.TextField("SpawnPoint Name", script.stringVar2);
			//script.vector3Var = EditorGUILayout.Vector3Field("Position", script.vector3Var);
			//script.vector3Var2 = EditorGUILayout.Vector3Field("Rotation", script.vector3Var2);
			//EditorGUILayout.LabelField("Leave it blank if you don't want main player to spawn at any object position");
		}
		if(script.type == ParametersType.SetObjectLocation){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.vector3Var = EditorGUILayout.Vector3Field("Position", script.vector3Var);
		}
		if(script.type == ParametersType.SetObjectRotation){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.targetIsPlayer = EditorGUILayout.Toggle("Target is Player", script.targetIsPlayer);
			script.vector3Var = EditorGUILayout.Vector3Field("Rotation", script.vector3Var);

			script.target2 = EditorGUILayout.ObjectField("Set Rotation equal obj", script.target2 , typeof(Transform), true) as Transform;
			//EditorGUILayout.LabelField("You can leave rotation equal obj to blank");
		}
		if(script.type == ParametersType.SetObjectAtObjectPosition){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.targetIsPlayer = EditorGUILayout.Toggle("Target is Player", script.targetIsPlayer);
			script.target2 = EditorGUILayout.ObjectField("Spawn at", script.target2 , typeof(Transform), true) as Transform;
		}
		if(script.type == ParametersType.MoveObjectTowards){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.targetIsPlayer = EditorGUILayout.Toggle("Target is Player", script.targetIsPlayer);

			script.target2 = EditorGUILayout.ObjectField("Move Towards", script.target2 , typeof(Transform), true) as Transform;
			script.boolVar5 = EditorGUILayout.Toggle("Move Towards Player", script.boolVar5);

			script.floatVar = EditorGUILayout.FloatField("Move Speed", script.floatVar);
			script.boolVar = EditorGUILayout.Toggle("Use CharacterController", script.boolVar);
			script.boolVar2 = EditorGUILayout.Toggle("Look At Target", script.boolVar2);
			script.boolVar3 = EditorGUILayout.Toggle("Lock Y Angle", script.boolVar3);

			script.floatVar2 = EditorGUILayout.FloatField("Reach Distance", script.floatVar2);
			script.msgString = EditorGUILayout.TextField("If Reach Send Message", script.msgString);
			script.sendBack = EditorGUILayout.Toggle("Wait until reach", script.sendBack);
			script.stopMoving = EditorGUILayout.Toggle("Stop when reach ", script.stopMoving);
		}
		if(script.type == ParametersType.StopMovingAndLooking){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.boolVar = EditorGUILayout.Toggle("Stop Moving", script.boolVar);
			script.boolVar2 = EditorGUILayout.Toggle("Stop Look Rotation", script.boolVar2);
		}
		if(script.type == ParametersType.LookAtTarget){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.targetIsPlayer = EditorGUILayout.Toggle("Target is Player", script.targetIsPlayer);
			script.target2 = EditorGUILayout.ObjectField("Look at", script.target2 , typeof(Transform), true) as Transform;
			script.boolVar3 = EditorGUILayout.Toggle("Look at Player", script.boolVar3);

			script.floatVar = EditorGUILayout.FloatField("Rotate Speed", script.floatVar);
			script.boolVar = EditorGUILayout.Toggle("Look Rotation", script.boolVar);
			script.boolVar2 = EditorGUILayout.Toggle("Lock Y Angle", script.boolVar2);
		}

		if(script.type == ParametersType.AmbientEditor){
			script.colorVar = EditorGUILayout.ColorField("Fog Color", script.colorVar);
			script.floatVar = EditorGUILayout.FloatField("Fog Density", script.floatVar);
			script.matVar = EditorGUILayout.ObjectField("Sky Box", script.matVar , typeof(Material), true) as Material;
			EditorGUILayout.LabelField("Leave it blank if you don't want to change Sky Box");
		}

		if(script.type == ParametersType.LightEditor){
			script.lightVar = EditorGUILayout.ObjectField("Light", script.lightVar , typeof(Light), true) as Light;
			script.colorVar = EditorGUILayout.ColorField("Light Color", script.colorVar);
			script.lightIntesity = EditorGUILayout.FloatField("Intensity", script.lightIntesity);
			script.floatVar2 = EditorGUILayout.FloatField("Bounce Intensity", script.floatVar2);
			script.floatVar3 = EditorGUILayout.FloatField("Shadow Strength", script.floatVar3);
		}

		if(script.type == ParametersType.SetTimeScale){
			script.floatVar3 = EditorGUILayout.FloatField("Time Scale", script.floatVar3);
			EditorGUILayout.LabelField("If Time Scale = 0 time event may not working");
		}

		if(script.type == ParametersType.BreakEvent){
			EditorGUILayout.LabelField("Event will End Here");
		}

		if(script.type == ParametersType.FadeImage){
			script.imageVar = EditorGUILayout.ObjectField("Image", script.imageVar , typeof(Image), true) as Image;
			script.colorVar = EditorGUILayout.ColorField("Start Color", script.colorVar);
			script.colorVar2 = EditorGUILayout.ColorField("End Color", script.colorVar2);
			script.floatVar3 = EditorGUILayout.FloatField("Fade Duration", script.floatVar3);
		}

		if(script.type == ParametersType.ScreenShake){
			script.floatVar = EditorGUILayout.FloatField("Shake Value", script.floatVar);
			script.floatVar2 = EditorGUILayout.FloatField("Shake Duration", script.floatVar2);

			EditorGUILayout.LabelField("If Main Camera has other movement camera script on it.");
			EditorGUILayout.LabelField("This event may not working.");
		}

		if(script.type == ParametersType.ConditionCheck){
			script.condiForm = (CondiCheckForm)EditorGUILayout.EnumPopup("Check From", script.condiForm);
			script.intVar = EditorGUILayout.IntField("Variable ID", script.intVar);
			script.intVar2 = EditorGUILayout.IntField("Value", script.intVar2);
			script.condi = (ConditionChecker)EditorGUILayout.EnumPopup("Condition", script.condi);
			script.boolVar = EditorGUILayout.Toggle("Check from Boolean", script.boolVar);

			script.intVar3 = EditorGUILayout.IntField("If Pass Go to Event", script.intVar3);
			script.msgString = EditorGUILayout.TextField("If Pass Send Message", script.msgString);
			script.intVar4 = EditorGUILayout.IntField("If Fail Go to Event", script.intVar4);
			script.msgString2 = EditorGUILayout.TextField("If Fail Send Message", script.msgString2);
			EditorGUILayout.LabelField("Leave Send Message blank if you don't need to send any message");
		}

		if(script.type == ParametersType.MoveImage){
			script.imageVar = EditorGUILayout.ObjectField("Image", script.imageVar , typeof(Image), true) as Image;
			script.boolVar4 = EditorGUILayout.Toggle("Moving", script.boolVar4);
			script.vector3Var = EditorGUILayout.Vector3Field("Move to Position", script.vector3Var);
			script.floatVar4 = EditorGUILayout.FloatField("Move Speed", script.floatVar4);

			script.boolVar2 = EditorGUILayout.Toggle("Scaling", script.boolVar2);
			script.vector2Var = EditorGUILayout.Vector2Field("Size", script.vector2Var);
			script.floatVar5 = EditorGUILayout.FloatField("Scaling Speed", script.floatVar5);
		}

		if(script.type == ParametersType.AddCash){
			script.intVar = EditorGUILayout.IntField("Value", script.intVar);
		}
		if(script.type == ParametersType.AddStatusPoint){
			script.intVar = EditorGUILayout.IntField("Value", script.intVar);
		}
		if(script.type == ParametersType.AddSkillPoint){
			script.intVar = EditorGUILayout.IntField("Value", script.intVar);
		}
		if(script.type == ParametersType.AddItem){
			script.intVar = EditorGUILayout.IntField("Item ID", script.intVar);
			script.intVar2 = EditorGUILayout.IntField("Quantity", script.intVar2);
		}
		if(script.type == ParametersType.AddEquipment){
			script.intVar = EditorGUILayout.IntField("Item ID", script.intVar);
		}
		if(script.type == ParametersType.SetParent){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
			script.target2 = EditorGUILayout.ObjectField("Parent to", script.target2 , typeof(Transform), true) as Transform;
			script.boolVar = EditorGUILayout.Toggle("Parent to Player", script.boolVar);
		}
		if(script.type == ParametersType.Unparent){
			script.target = EditorGUILayout.ObjectField("Target", script.target , typeof(Transform), true) as Transform;
		}
		if(script.type == ParametersType.AddQuest){
			script.intVar = EditorGUILayout.IntField("Item ID", script.intVar);
		}

		EditorGUILayout.LabelField("");
		script.note = EditorGUILayout.TextField("Note", script.note);
	}
}

