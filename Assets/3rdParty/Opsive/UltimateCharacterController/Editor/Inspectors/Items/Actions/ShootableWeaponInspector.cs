/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Items.AnimatorAudioState;
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Items.Actions;
    using System;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Shows a custom inspector for the ShootableWeapon component.
    /// </summary>
    [CustomEditor(typeof(ShootableWeapon))]
    public class ShootableWeaponInspector : UsableItemInspector
    {
        private const string c_EditorPrefsSelectedReloadAnimatorAudioStateSetIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedReloadAnimatorAudioStateSetIndex";
        private const string c_EditorPrefsSelectedReloadAnimatorAudioStateSetStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedReloadAnimatorAudioStateSetStateIndex";
        private string SelectedReloadAnimatorAudioStateSetIndexKey { get { return c_EditorPrefsSelectedReloadAnimatorAudioStateSetIndexKey + "." + target.GetType() + "." + target.name + m_ShootableWeapon.ID; } }

        private ShootableWeapon m_ShootableWeapon;
        private ReorderableList m_ReorderableChargeAudioClipsList;
        private ReorderableList m_ReorderableDryFireAudioClipsList;
        private ReorderableList m_ReorderableReloadCompleteAudioClipsList;
        private ReorderableList m_ReorderableReloadAnimatorAudioStateSetList;
        private ReorderableList m_ReorderableReloadAnimatorAudioStateSetAudioList;
        private ReorderableList m_ReorderableReloadAnimatorAudioStateSetStateList;

        /// <summary>
        /// Initialize any starting values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_ShootableWeapon = target as ShootableWeapon;
            m_ShootableWeapon.ReloadAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
        }

        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                var projectile = PropertyFromName("m_Projectile");
                if (Foldout("Firing")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_ConsumableItemDefinition"));
                    var fireMode = PropertyFromName("m_FireMode");
                    EditorGUILayout.PropertyField(fireMode);
                    if (fireMode.enumValueIndex == (int)ShootableWeapon.FireMode.Burst) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_BurstCount"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_BurstDelay"));
                        EditorGUI.indentLevel--;
                    }
                    var fireType = PropertyFromName("m_FireType");
                    EditorGUILayout.PropertyField(fireType);
                    if (fireType.enumValueIndex != (int)ShootableWeapon.FireType.Instant) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_MinChargeLength"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_FullChargeLength"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_ChargeItemSubstateParameterValue"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_MinChargeStrength"));
                        if (Foldout("Charge Audio")) {
                            EditorGUI.indentLevel++;
                            m_ReorderableChargeAudioClipsList = AudioClipSetInspector.DrawAudioClipSet(m_ShootableWeapon.ChargeAudioClipSet, PropertyFromName("m_ChargeAudioClipSet"), m_ReorderableChargeAudioClipsList, OnChargeAudioClipDraw, OnChargeAudioClipListAdd, OnChargeAudioClipListRemove);
                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_FireCount"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_Spread"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_FireInLookSourceDirection"));
                    EditorGUILayout.HelpBox("If a projectile prefab is specified then this projectile will be fired from the weapon. If no projectile is specified then a hitscan will be used.", MessageType.Info);
                    EditorGUILayout.PropertyField(projectile);
                    if (projectile.objectReferenceValue == null) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_HitscanFireDelay"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_HitscanFireRange"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_MaxHitscanCollisionCount"));
                        InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnHitscanImpactEvent"));
                    } else {
                        if ((projectile.objectReferenceValue as GameObject).GetComponent<UltimateCharacterController.Objects.Projectile>() == null) {
                            EditorGUILayout.HelpBox("The projectile must have the Projectile component attached to it.", MessageType.Error);
                        }
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_ProjectileFireVelocityMagnitude"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_ProjectileVisibility"));
                        var shootableWeapon = target as ShootableWeapon;
                        shootableWeapon.ProjectileStartLayer = EditorGUILayout.LayerField(new GUIContent("Projectile Start Layer",
                            "The layer that the projectile should occupy when initially spawned."), shootableWeapon.ProjectileStartLayer);
                        shootableWeapon.ProjectileFiredLayer= EditorGUILayout.LayerField(new GUIContent("Projectile Fired Layer",
                            "The layer that the projectile object should change to after being fired."), shootableWeapon.ProjectileFiredLayer);
                        EditorGUILayout.PropertyField(PropertyFromName("m_LayerChangeDelay"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_ProjectileEnableDelayAfterOtherUse"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_DryFireItemSubstateParameterValue"));
                    if (InspectorUtility.Foldout(target, "Dry Fire Audio")) {
                        EditorGUI.indentLevel++;
                        m_ReorderableDryFireAudioClipsList = AudioClipSetInspector.DrawAudioClipSet(m_ShootableWeapon.DryFireAudioClipSet, PropertyFromName("m_DryFireAudioClipSet"), m_ReorderableDryFireAudioClipsList, OnDryFireAudioClipDraw, OnDryFireAudioClipListAdd, OnDryFireAudioClipListRemove);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Impact")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactLayers")); 
                    if (projectile.objectReferenceValue == null) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_HitscanTriggerInteraction"));
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_DamageAmount"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactForce"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactForceFrames"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactStateName"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactStateDisableTimer"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SurfaceImpact"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Reload")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_ClipSize"));
                    var autoReloadProperty = PropertyFromName("m_AutoReload");
                    var autoReloadString = Enum.GetNames(typeof(UltimateCharacterController.Character.Abilities.Items.Reload.AutoReloadType));
                    autoReloadProperty.intValue = EditorGUILayout.MaskField(new GUIContent("Auto Reload", autoReloadProperty.tooltip), (int)autoReloadProperty.intValue, autoReloadString);
                    EditorGUILayout.PropertyField(PropertyFromName("m_ReloadType"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ReloadCanCameraZoom"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ReloadCrosshairsSpread"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Reload Event", PropertyFromName("m_ReloadEvent"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Reload Complete Event", PropertyFromName("m_ReloadCompleteEvent"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ReloadDetachAttachClip"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Reload Detach Clip Event", PropertyFromName("m_ReloadDetachClipEvent"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Reload Show Projectile Event", PropertyFromName("m_ReloadShowProjectileEvent"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Reload Attach Projectile Event", PropertyFromName("m_ReloadAttachProjectileEvent"));
                    var reloadClipProperty = PropertyFromName("m_ReloadDropClip");
                    EditorGUILayout.PropertyField(reloadClipProperty);
                    if (reloadClipProperty.objectReferenceValue != null) {
                        EditorGUI.indentLevel++;
                        var shootableWeapon = target as ShootableWeapon;
                        shootableWeapon.ReloadClipTargetLayer = EditorGUILayout.LayerField(new GUIContent("Reload Clip Target Layer",
                            "The layer that the clip object should change to after being reloaded."), shootableWeapon.ReloadClipTargetLayer);
                        EditorGUILayout.PropertyField(PropertyFromName("m_ReloadClipLayerChangeDelay"));
                        InspectorUtility.DrawAnimationEventTrigger(target, "Reload Drop Clip Event", PropertyFromName("m_ReloadDropClipEvent"));
                        EditorGUI.indentLevel--;
                    }
                    InspectorUtility.DrawAnimationEventTrigger(target, "Reload Attach Clip Event", PropertyFromName("m_ReloadAttachClipEvent"));
                    if (Foldout("Animator Audio")) {
                        EditorGUI.indentLevel++;
                        AnimatorAudioStateSetInspector.DrawAnimatorAudioStateSet(m_ShootableWeapon, m_ShootableWeapon.ReloadAnimatorAudioStateSet, "m_ReloadAnimatorAudioStateSet", true,
                                    ref m_ReorderableReloadAnimatorAudioStateSetList, OnAnimatorAudioStateListDraw, OnAnimatorAudioStateListSelect,
                                    OnAnimatorAudioStateListAdd, OnAnimatorAudioStateListRemove, SelectedReloadAnimatorAudioStateSetIndexKey,
                                    ref m_ReorderableReloadAnimatorAudioStateSetAudioList, OnReloadAudioListElementDraw, OnReloadAudioListAdd, OnReloadAudioListRemove, 
                                    ref m_ReorderableReloadAnimatorAudioStateSetStateList,
                                    OnAnimatorAudioStateSetStateListDraw, OnAnimatorAudioStateSetStateListAdd, OnAnimatorAudioStateSetStateListReorder, OnAnimatorAudioStateSetStateListRemove,
                                    GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)));
                        EditorGUI.indentLevel--;
                    }
                    if (InspectorUtility.Foldout(target, "Reload Complete Audio")) {
                        EditorGUI.indentLevel++;
                        m_ReorderableReloadCompleteAudioClipsList = AudioClipSetInspector.DrawAudioClipSet(m_ShootableWeapon.ReloadCompleteAudioClipSet, PropertyFromName("m_ReloadCompleteAudioClipSet"), m_ReorderableReloadCompleteAudioClipsList, OnReloadCompleteAudioClipDraw, OnReloadCompleteAudioClipListAdd, OnReloadCompleteAudioClipListRemove);
                        EditorGUI.indentLevel--;
                    }
                }
                if (Foldout("Recoil")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_PositionRecoil"), true);
                    EditorGUILayout.PropertyField(PropertyFromName("m_RotationRecoil"), true);
                    EditorGUILayout.PropertyField(PropertyFromName("m_PositionCameraRecoil"), true);
                    EditorGUILayout.PropertyField(PropertyFromName("m_RotationCameraRecoil"), true);
                    EditorGUILayout.PropertyField(PropertyFromName("m_CameraRecoilAccumulation"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_LocalizeRecoilForce"));
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Muzzle Flash")) {
                    EditorGUI.indentLevel++;
                    var muzzleFlash = PropertyFromName("m_MuzzleFlash");
                    EditorGUILayout.PropertyField(muzzleFlash);
                    if (muzzleFlash.objectReferenceValue != null) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_PoolMuzzleFlash"));
                    }
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Shell")) {
                    EditorGUI.indentLevel++;
                    var shell = PropertyFromName("m_Shell");
                    EditorGUILayout.PropertyField(shell);
                    if (shell.objectReferenceValue != null) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_ShellVelocity"), true);
                        EditorGUILayout.PropertyField(PropertyFromName("m_ShellTorque"), true);
                        EditorGUILayout.PropertyField(PropertyFromName("m_ShellEjectDelay"));
                    }
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Smoke")) {
                    EditorGUI.indentLevel++;
                    var smoke = PropertyFromName("m_Smoke");
                    EditorGUILayout.PropertyField(smoke);
                    if (smoke.objectReferenceValue != null) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_SmokeSpawnDelay"));
                    }
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Tracer")) {
                    EditorGUI.indentLevel++;
                    if (PropertyFromName("m_Projectile").objectReferenceValue == null) {
                        var tracer = PropertyFromName("m_Tracer");
                        EditorGUILayout.PropertyField(tracer);
                        if (tracer.objectReferenceValue != null) {
                            EditorGUILayout.PropertyField(PropertyFromName("m_TracerDefaultLength"));
                            EditorGUILayout.PropertyField(PropertyFromName("m_TracerSpawnDelay"));
                        }
                    } else {
                        EditorGUILayout.HelpBox("A tracer can only be applied to hitscan weapons.", MessageType.Info);
                    }
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Attachments")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_DisableScopeCameraOnNoAim"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnDryFireAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableDryFireAudioClipsList, rect, index, m_ShootableWeapon.DryFireAudioClipSet, null);
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnReloadCompleteAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableReloadCompleteAudioClipsList, rect, index, m_ShootableWeapon.ReloadCompleteAudioClipSet, null);
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnChargeAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableChargeAudioClipsList, rect, index, m_ShootableWeapon.ChargeAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnDryFireAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_ShootableWeapon.DryFireAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnReloadCompleteAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_ShootableWeapon.ReloadCompleteAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnChargeAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_ShootableWeapon.ChargeAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnDryFireAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_ShootableWeapon.DryFireAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnReloadCompleteAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_ShootableWeapon.ReloadCompleteAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnChargeAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_ShootableWeapon.ChargeAudioClipSet, null);
        }

        /// <summary>
        /// Draws all of the added audio clip elements.
        /// </summary>
        private void OnReloadAudioListElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAudioClipDraw(m_ReorderableReloadAnimatorAudioStateSetAudioList, rect, index, m_ShootableWeapon.ReloadAnimatorAudioStateSet.States, SelectedReloadAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Adds a new audio clip element to the list.
        /// </summary>
        private void OnReloadAudioListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListAdd(list, m_ShootableWeapon.ReloadAnimatorAudioStateSet.States, SelectedReloadAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Removes the audio clip at the list index.
        /// </summary>
        private void OnReloadAudioListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListRemove(list, m_ShootableWeapon.ReloadAnimatorAudioStateSet.States, SelectedReloadAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Draws the AudioStateSet element.
        /// </summary>
        private void OnAnimatorAudioStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateElementDraw(m_ReorderableReloadAnimatorAudioStateSetList, m_ShootableWeapon.ReloadAnimatorAudioStateSet, rect, index, isActive, isFocused, target);
        }

        /// <summary>
        /// A new element has been selected.
        /// </summary>
        private void OnAnimatorAudioStateListSelect(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateSelect(list, SelectedReloadAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnAnimatorAudioStateListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListAdd(list, m_ShootableWeapon.ReloadAnimatorAudioStateSet, SelectedReloadAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Removes the state at the list index.
        /// </summary>
        private void OnAnimatorAudioStateListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListRemove(list, m_ShootableWeapon.ReloadAnimatorAudioStateSet, SelectedReloadAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Returns the state index key for the specified AnimatorAudioStateSet type.
        /// </summary>
        private string GetSelectedAnimatorAudioStateSetStateIndexKey(int index)
        {
            return c_EditorPrefsSelectedReloadAnimatorAudioStateSetStateIndexKey + "." + target.GetType() + "." + target.name + "." + index;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnAnimatorAudioStateSetStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var animatorAudioState = m_ShootableWeapon.ReloadAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= animatorAudioState.States.Length) {
                m_ReorderableReloadAnimatorAudioStateSetStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)), m_ReorderableReloadAnimatorAudioStateSetStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(animatorAudioState, animatorAudioState.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                StateInspector.UpdateDefaultStateValues(animatorAudioState.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnAnimatorAudioStateSetStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingAnimatorAudioStateSetStatePreset, CreateAnimatorAudioStateSetStatePreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_ShootableWeapon.ReloadAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.AddExistingPreset(animatorAudioState.GetType(), animatorAudioState.States, m_ReorderableReloadAnimatorAudioStateSetStateList, GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableReloadAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_ShootableWeapon.ReloadAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.CreatePreset(animatorAudioState, animatorAudioState.States, m_ReorderableReloadAnimatorAudioStateSetStateList, GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableReloadAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnAnimatorAudioStateSetStateListReorder(ReorderableList list)
        {
            var animatorAudioState = m_ShootableWeapon.ReloadAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[animatorAudioState.States.Length];
            Array.Copy(animatorAudioState.States, copiedStates, animatorAudioState.States.Length);
            for (int i = 0; i < animatorAudioState.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    animatorAudioState.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(animatorAudioState.States);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableReloadAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnAnimatorAudioStateSetStateListRemove(ReorderableList list)
        {
            var animatorAudioState = m_ShootableWeapon.ReloadAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.OnStateListRemove(animatorAudioState.States, GetSelectedAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedReloadAnimatorAudioStateSetIndexKey)), list);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableReloadAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Deserialize the animator audio state set after an undo/redo.
        /// </summary>
        protected override void OnUndoRedo()
        {
            base.OnUndoRedo();

            m_ShootableWeapon.ReloadAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
            Repaint();
        }
    }
}