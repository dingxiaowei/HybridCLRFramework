/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Traits
{
    /// <summary>
    /// Shows a custom inspector for the Health component.
    /// </summary>
    [CustomEditor(typeof(Health))]
    public class HealthInspector : StateBehaviorInspector
    {
        private ReorderableList m_ReorderableTakeDamageAudioClipsList;
        private ReorderableList m_ReorderableHealAudioClipsList;
        private ReorderableList m_ReorderableDeathAudioClipsList;

        private Health m_Health;
        private AttributeManager m_AttributeManager;
        private ReorderableList m_ReorderableHitboxList;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Health = target as Health;
            m_AttributeManager = m_Health.GetComponent<AttributeManager>();
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
                EditorGUILayout.PropertyField(PropertyFromName("m_Invincible"));
                EditorGUILayout.PropertyField(PropertyFromName("m_TimeInvincibleAfterSpawn"));

                // The names will be retrieved by the Attribute Manager.
                var attributeNames = new string[m_AttributeManager.Attributes.Length + 1];
                attributeNames[0] = "(None)";
                var healthNameIndex = 0;
                var shieldNameIndex = 0;
                for (int i = 0; i < m_AttributeManager.Attributes.Length; ++i) {
                    attributeNames[i + 1] = m_AttributeManager.Attributes[i].Name;
                    if (m_Health.HealthAttributeName == attributeNames[i + 1]) {
                        healthNameIndex = i + 1;
                    }
                    if (m_Health.ShieldAttributeName == attributeNames[i + 1]) {
                        shieldNameIndex = i + 1;
                    }
                }

                var selectedHealthNameIndex = EditorGUILayout.Popup("Health Attribute", healthNameIndex, attributeNames);
                if (healthNameIndex != selectedHealthNameIndex) {
                    m_Health.HealthAttributeName = (selectedHealthNameIndex == 0 ? string.Empty : m_AttributeManager.Attributes[selectedHealthNameIndex - 1].Name);
                    InspectorUtility.SetDirty(target);
                }
                // Show the current health value.
                if (Application.isPlaying && !string.IsNullOrEmpty(m_Health.HealthAttributeName) && selectedHealthNameIndex > 0 && selectedHealthNameIndex - 1 < m_AttributeManager.Attributes.Length) {
                    EditorGUI.indentLevel++;
                    GUI.enabled = false;
                    EditorGUILayout.TextField("Value", m_AttributeManager.Attributes[selectedHealthNameIndex - 1].Value.ToString());
                    GUI.enabled = true;
                    EditorGUI.indentLevel--;
                }

                var selectedShieldNameIndex = EditorGUILayout.Popup("Shield Attribute", shieldNameIndex, attributeNames);
                if (shieldNameIndex != selectedShieldNameIndex) {
                    m_Health.ShieldAttributeName = (selectedShieldNameIndex == 0 ? string.Empty : m_AttributeManager.Attributes[selectedShieldNameIndex - 1].Name);
                    InspectorUtility.SetDirty(target);
                }
                // Show the current shield value.
                if (Application.isPlaying && !string.IsNullOrEmpty(m_Health.ShieldAttributeName) && selectedShieldNameIndex > 0 && selectedShieldNameIndex - 1 < m_AttributeManager.Attributes.Length) {
                    EditorGUI.indentLevel++;
                    GUI.enabled = false;
                    EditorGUILayout.TextField("Value", m_AttributeManager.Attributes[selectedShieldNameIndex - 1].Value.ToString());
                    GUI.enabled = true;
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Hitboxes")) {
                    EditorGUI.indentLevel++;
                    HitboxInspector.DrawHitbox(ref m_ReorderableHitboxList, serializedObject, PropertyFromName("m_Hitboxes"), OnHitboxElementDraw);
                    EditorGUILayout.PropertyField(PropertyFromName("m_MaxHitboxCollisionCount"));
                    EditorGUI.indentLevel--;
                }

                var healthCallback = GetHealthDrawCallback();
                if (healthCallback != null) {
                    healthCallback();
                }

                if (Foldout("Audio")) {
                    EditorGUI.indentLevel++;
                    if (InspectorUtility.Foldout(target, "Take Damage")) {
                        EditorGUI.indentLevel++;
                        AudioClipSetInspector.DrawAudioClipSet(m_Health.TakeDamageAudioClipSet, PropertyFromName("m_TakeDamageAudioClipSet"), ref m_ReorderableTakeDamageAudioClipsList, OnTakeDamageAudioClipDraw, OnTakeDamageAudioClipListAdd, OnTakeDamageAudioClipListRemove);
                        EditorGUI.indentLevel--;
                    }
                    if (InspectorUtility.Foldout(target, "Heal")) {
                        EditorGUI.indentLevel++;
                        AudioClipSetInspector.DrawAudioClipSet(m_Health.HealAudioClipSet, PropertyFromName("m_HealAudioClipSet"), ref m_ReorderableHealAudioClipsList, OnHealAudioClipDraw, OnHealAudioClipListAdd, OnHealAudioClipListRemove);
                        EditorGUI.indentLevel--;
                    }
                    if (InspectorUtility.Foldout(target, "Death")) {
                        EditorGUI.indentLevel++;
                        AudioClipSetInspector.DrawAudioClipSet(m_Health.DeathAudioClipSet, PropertyFromName("m_DeathAudioClipSet"), ref m_ReorderableDeathAudioClipsList, OnDeathAudioClipDraw, OnDeathAudioClipListAdd, OnDeathAudioClipListRemove);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Death")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_SpawnedObjectsOnDeath"), true);
                    EditorGUILayout.PropertyField(PropertyFromName("m_DestroyedObjectsOnDeath"), true);
                    var deactivateOnDeath = PropertyFromName("m_DeactivateOnDeath");
                    EditorGUILayout.PropertyField(deactivateOnDeath);
                    if (deactivateOnDeath.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_DeactivateOnDeathDelay"));
                        EditorGUI.indentLevel--;
                    }
                    var deathLayerProperty = PropertyFromName("m_DeathLayer");
                    deathLayerProperty.intValue = EditorGUILayout.LayerField("Death Layer", deathLayerProperty.intValue);
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Events")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnDamageEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnHealEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnDeathEvent"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Returns the health actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The health actions to draw before the State list is drawn.</returns>
        protected virtual Action GetHealthDrawCallback() { return null; }

        /// <summary>
        /// Draws the Hitbox ReordableList element.
        /// </summary>
        private void OnHitboxElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            HitboxInspector.HitboxElementDraw(m_ReorderableHitboxList, rect, index, isActive, isFocused);
        }

        /// <summary>
        /// Draws a visual representation of the hitbox.
        /// </summary>
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawHitboxGizmo(Health health, GizmoType gizmoType)
        {
            HitboxInspector.DrawHitboxGizmo(health.Hitboxes, gizmoType);
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnTakeDamageAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableTakeDamageAudioClipsList, rect, index, m_Health.TakeDamageAudioClipSet, null);
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnHealAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableHealAudioClipsList, rect, index, m_Health.HealAudioClipSet, null);
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnDeathAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableDeathAudioClipsList, rect, index, m_Health.DeathAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnTakeDamageAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Health.TakeDamageAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnHealAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Health.HealAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnDeathAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Health.DeathAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnTakeDamageAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Health.TakeDamageAudioClipSet, null);
            m_Health.TakeDamageAudioClipSet.AudioClips = (AudioClip[])list.list;
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnHealAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Health.HealAudioClipSet, null);
            m_Health.HealAudioClipSet.AudioClips = (AudioClip[])list.list;
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnDeathAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Health.DeathAudioClipSet, null);
            m_Health.DeathAudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}