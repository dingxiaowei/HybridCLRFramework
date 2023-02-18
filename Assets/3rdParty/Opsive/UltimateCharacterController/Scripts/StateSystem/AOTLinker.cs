/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

#if NET_4_6 || UNITY_2018_3_OR_NEWER || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID || UNITY_WII || UNITY_WIIU || UNITY_SWITCH || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE || UNITY_WSA

namespace Opsive.UltimateCharacterController.StateSystem
{
    using System;
    using UnityEngine;

    // This class is required in order for the preset system to work with AOT platforms. The preset system uses reflection to generate the delegates
    // and reflection doesn't play well with AOT because the classes need to be defined ahead of time. Define the classes here so the compiler will
    // add in the correct type. This code is not actually used anywhere, it is purely for the compiler.
    public class AOTLinker : MonoBehaviour
    {
        public void Linker()
        {
#pragma warning disable 0219
            var intGenericDelegate = new Preset.GenericDelegate<int>();
            var intFuncDelegate = new Func<int>(() => { return 0; });
            var intActionDelegate = new Action<int>((int value) => { });
            var floatGenericDelegate = new Preset.GenericDelegate<float>();
            var floatFuncDelegate = new Func<float>(() => { return 0; });
            var floatActionDelegate = new Action<float>((float value) => { });
            var uintGenericDelegate = new Preset.GenericDelegate<uint>();
            var uintFuncDelegate = new Func<uint>(() => { return 0; });
            var uintActionDelegate = new Action<uint>((uint value) => { });
            var doubleGenericDelegate = new Preset.GenericDelegate<double>();
            var doubleFuncDelegate = new Func<double>(() => { return 0; });
            var doubleActionDelegate = new Action<double>((double value) => { });
            var longGenericDelegate = new Preset.GenericDelegate<long>();
            var longFuncDelegate = new Func<long>(() => { return 0; });
            var longActionDelegate = new Action<long>((long value) => { });
            var boolGenericDelegate = new Preset.GenericDelegate<bool>();
            var boolFuncDelegate = new Func<bool>(() => { return true; });
            var boolActionDelegate = new Action<bool>((bool value) => { });
            var stringGenericDelegate = new Preset.GenericDelegate<string>();
            var stringFuncDelegate = new Func<string>(() => { return string.Empty; });
            var stringActionDelegate = new Action<string>((string value) => { });
            var byteGenericDelegate = new Preset.GenericDelegate<byte>();
            var byteFuncDelegate = new Func<byte>(() => { return new byte(); });
            var byteActionDelegate = new Action<byte>((byte value) => { });
            var vector2GenericDelegate = new Preset.GenericDelegate<Vector2>();
            var vector2FuncDelegate = new Func<Vector2>(() => { return Vector2.zero; });
            var vector2ActionDelegate = new Action<Vector2>((Vector2 value) => { });
            var vector3GenericDelegate = new Preset.GenericDelegate<Vector3>();
            var vector3FuncDelegate = new Func<Vector3>(() => { return Vector3.zero; });
            var vector3ActionDelegate = new Action<Vector3>((Vector3 value) => { });
            var vector4GenericDelegate = new Preset.GenericDelegate<Vector4>();
            var vector4FuncDelegate = new Func<Vector4>(() => { return Vector4.zero; });
            var vector4ActionDelegate = new Action<Vector4>((Vector4 value) => { });
            var quaternionGenericDelegate = new Preset.GenericDelegate<Quaternion>();
            var quaternionFuncDelegate = new Func<Quaternion>(() => { return Quaternion.identity; });
            var quaternionActionDelegate = new Action<Quaternion>((Quaternion value) => { });
            var colorGenericDelegate = new Preset.GenericDelegate<Color>();
            var colorFuncDelegate = new Func<Color>(() => { return Color.white; });
            var colorActionDelegate = new Action<Color>((Color value) => { });
            var rectGenericDelegate = new Preset.GenericDelegate<Rect>();
            var rectFuncDelegate = new Func<Rect>(() => { return Rect.zero; });
            var rectActionDelegate = new Action<Rect>((Rect value) => { });
            var matrix4x4GenericDelegate = new Preset.GenericDelegate<Matrix4x4>();
            var matrix4x4FuncDelegate = new Func<Matrix4x4>(() => { return Matrix4x4.zero; });
            var matrix4x4ActionDelegate = new Action<Matrix4x4>((Matrix4x4 value) => { });
            var animationCurveGenericDelegate = new Preset.GenericDelegate<AnimationCurve>();
            var animationCurveFuncDelegate = new Func<AnimationCurve>(() => { return new AnimationCurve(); });
            var animationCurveActionDelegate = new Action<AnimationCurve>((AnimationCurve value) => { });
            var layerMaskGenericDelegate = new Preset.GenericDelegate<LayerMask>();
            var layerMaskFuncDelegate = new Func<LayerMask>(() => { return new LayerMask(); });
            var layerMaskActionDelegate = new Action<LayerMask>((LayerMask value) => { });
            var humanBodyBonesGenericDelegate = new Preset.GenericDelegate<HumanBodyBones>();
            var humanBodyBonesFuncDelegate = new Func<HumanBodyBones>(() => { return 0; });
            var humanBodyBonesActionDelegate = new Action<HumanBodyBones>((HumanBodyBones value) => { });
            var queryTriggerInteractionGenericDelegate = new Preset.GenericDelegate<QueryTriggerInteraction>();
            var queryTriggerInteractionFuncDelegate = new Func<QueryTriggerInteraction>(() => { return 0; });
            var queryTriggerInteractionActionDelegate = new Action<QueryTriggerInteraction>((QueryTriggerInteraction value) => { });
            var forceModeGenericDelegate = new Preset.GenericDelegate<ForceMode>();
            var forceModeFuncDelegate = new Func<ForceMode>(() => { return 0; });
            var forceModeActionDelegate = new Action<ForceMode>((ForceMode value) => { });
            var unityObjectGenericDelegate = new Preset.GenericDelegate<UnityEngine.Object>();
            var unityObjectFuncDelegate = new Func<UnityEngine.Object>(() => { return new UnityEngine.Object(); });
            var unityObjectActionDelegate = new Action<UnityEngine.Object>((UnityEngine.Object value) => { });
            var gameObjectGenericDelegate = new Preset.GenericDelegate<GameObject>();
            var gameObjectFuncDelegate = new Func<GameObject>(() => { return null; });
            var gameObjectActionDelegate = new Action<GameObject>((GameObject value) => { });
            var transformGenericDelegate = new Preset.GenericDelegate<Transform>();
            var transformFuncDelegate = new Func<Transform>(() => { return null; });
            var transformActionDelegate = new Action<Transform>((Transform value) => { });
            var minMaxFloatGenericDelegate = new Preset.GenericDelegate<Utility.MinMaxFloat>();
            var minMaxFloatFuncDelegate = new Func<Utility.MinMaxFloat>(() => { return new Utility.MinMaxFloat(); });
            var minMaxFloatActionDelegate = new Action<Utility.MinMaxFloat>((Utility.MinMaxFloat value) => { });
            var minMaxVector3GenericDelegate = new Preset.GenericDelegate<Utility.MinMaxVector3>();
            var minMaxVector3FuncDelegate = new Func<Utility.MinMaxVector3>(() => { return new Utility.MinMaxVector3(); });
            var minMaxVector3ActionDelegate = new Action<Utility.MinMaxVector3>((Utility.MinMaxVector3 value) => { });
            var lookVectorModeGenericDelegate = new Preset.GenericDelegate<UltimateCharacterController.Input.PlayerInput.LookVectorMode>();
            var lookVectorModeFuncDelegate = new Func<UltimateCharacterController.Input.PlayerInput.LookVectorMode>(() => { return 0; });
            var lookVectorModeActionDelegate = new Action<UltimateCharacterController.Input.PlayerInput.LookVectorMode>((UltimateCharacterController.Input.PlayerInput.LookVectorMode value) => { });
            var preloadedPrefabGenericDelegate = new Preset.GenericDelegate<Shared.Game.ObjectPool.PreloadedPrefab>();
            var preloadedPrefabFuncDelegate = new Func<Shared.Game.ObjectPool.PreloadedPrefab>(() => { return new Shared.Game.ObjectPool.PreloadedPrefab(); });
            var preloadedPrefabActionDelegate = new Action<Shared.Game.ObjectPool.PreloadedPrefab>((Shared.Game.ObjectPool.PreloadedPrefab value) => { });
            var abilityStartTypeGenericDelegate = new Preset.GenericDelegate<Character.Abilities.Ability.AbilityStartType>();
            var abilityStartTypeFuncDelegate = new Func<Character.Abilities.Ability.AbilityStartType>(() => { return 0; });
            var abilityStartTypeActionDelegate = new Action<Character.Abilities.Ability.AbilityStartType>((Character.Abilities.Ability.AbilityStartType value) => { });
            var abilityStopTypeGenericDelegate = new Preset.GenericDelegate<Character.Abilities.Ability.AbilityStopType>();
            var abilityStopTypeFuncDelegate = new Func<Character.Abilities.Ability.AbilityStopType>(() => { return 0; });
            var abilityStopTypeActionDelegate = new Action<Character.Abilities.Ability.AbilityStopType>((Character.Abilities.Ability.AbilityStopType value) => { });
            var abilityBoolOverrideGenericDelegate = new Preset.GenericDelegate<Character.Abilities.Ability.AbilityBoolOverride>();
            var abilityBoolOverrideFuncDelegate = new Func<Character.Abilities.Ability.AbilityBoolOverride>(() => { return 0; });
            var abilityBoolOverrideActionDelegate = new Action<Character.Abilities.Ability.AbilityBoolOverride>((Character.Abilities.Ability.AbilityBoolOverride value) => { });
            var comboInputElementGenericDelegate = new Preset.GenericDelegate<Character.Abilities.Starters.ComboTimeout.ComboInputElement>();
            var comboInputElementFuncDelegate = new Func<Character.Abilities.Starters.ComboTimeout.ComboInputElement>(() => { return new Character.Abilities.Starters.ComboTimeout.ComboInputElement(); });
            var comboInputElementActionDelegate = new Action<Character.Abilities.Starters.ComboTimeout.ComboInputElement>((Character.Abilities.Starters.ComboTimeout.ComboInputElement value) => { });
            var restrictionTypeGenericDelegate = new Preset.GenericDelegate<Character.Abilities.RestrictPosition.RestrictionType>();
            var restrictionTypeFuncDelegate = new Func<Character.Abilities.RestrictPosition.RestrictionType>(() => { return 0; });
            var restrictionTypeActionDelegate = new Action<Character.Abilities.RestrictPosition.RestrictionType>((Character.Abilities.RestrictPosition.RestrictionType value) => { });
            var objectDetectionModeGenericDelegate = new Preset.GenericDelegate<Character.Abilities.DetectObjectAbilityBase.ObjectDetectionMode>();
            var objectDetectionModeTypeFuncDelegate = new Func<Character.Abilities.DetectObjectAbilityBase.ObjectDetectionMode>(() => { return 0; });
            var objectDetectionModeTypeActionDelegate = new Action<Character.Abilities.DetectObjectAbilityBase.ObjectDetectionMode>((Character.Abilities.DetectObjectAbilityBase.ObjectDetectionMode value) => { });
            var autoEquipTypeGenericDelegate = new Preset.GenericDelegate<Character.Abilities.Items.EquipUnequip.AutoEquipType>();
            var autoEquipTypeFuncDelegate = new Func<Character.Abilities.Items.EquipUnequip.AutoEquipType>(() => { return 0; });
            var autoEquipTypeActionDelegate = new Action<Character.Abilities.Items.EquipUnequip.AutoEquipType>((Character.Abilities.Items.EquipUnequip.AutoEquipType value) => { });
            var shakeTargetGenericDelegate = new Preset.GenericDelegate<Character.Effects.Shake.ShakeTarget>();
            var shakeTargetFuncDelegate = new Func<Character.Effects.Shake.ShakeTarget>(() => { return 0; });
            var shakeTargetActionDelegate = new Action<Character.Effects.Shake.ShakeTarget>((Character.Effects.Shake.ShakeTarget value) => { });
            var attributeAutoUpdateValueTypeGenericDelegate = new Preset.GenericDelegate<Traits.Attribute.AutoUpdateValue>();
            var attributeAutoUpdateFuncDelegate = new Func<Traits.Attribute.AutoUpdateValue>(() => { return 0; });
            var attributeAutoUpdateActionDelegate = new Action<Traits.Attribute.AutoUpdateValue>((Traits.Attribute.AutoUpdateValue value) => { });
            var surfaceImpactGenericDelegate = new Preset.GenericDelegate<SurfaceSystem.SurfaceImpact>();
            var surfaceImpactFuncDelegate = new Func<SurfaceSystem.SurfaceImpact>(() => { return null; });
            var surfaceImpactActionDelegate = new Action<SurfaceSystem.SurfaceImpact>((SurfaceSystem.SurfaceImpact value) => { });
            var uvTextureGenericDelegate = new Preset.GenericDelegate<SurfaceSystem.UVTexture>();
            var uvTextureFuncDelegate = new Func<SurfaceSystem.UVTexture>(() => { return new SurfaceSystem.UVTexture(); });
            var uvTextureActionDelegate = new Action<SurfaceSystem.UVTexture>((SurfaceSystem.UVTexture value) => { });
            var objectSurfaceGenericDelegate = new Preset.GenericDelegate<SurfaceSystem.ObjectSurface>();
            var objectSurfaceFuncDelegate = new Func<SurfaceSystem.ObjectSurface>(() => { return new SurfaceSystem.ObjectSurface(); });
            var objectSurfaceActionDelegate = new Action<SurfaceSystem.ObjectSurface>((SurfaceSystem.ObjectSurface value) => { });
            var objectSpawnInfoGenericDelegate = new Preset.GenericDelegate<Utility.ObjectSpawnInfo>();
            var objectSpawnInfoFuncDelegate = new Func<Utility.ObjectSpawnInfo>(() => { return null; });
            var objectSpawnInfoActionDelegate = new Action<Utility.ObjectSpawnInfo>((Utility.ObjectSpawnInfo value) => { });
            var animationEventTriggerGenericDelegate = new Preset.GenericDelegate<Utility.AnimationEventTrigger>();
            var animationEventTriggerFuncDelegate = new Func<Utility.AnimationEventTrigger>(() => { return null; });
            var animationEventTriggerActionDelegate = new Action<Utility.AnimationEventTrigger>((Utility.AnimationEventTrigger value) => { });
            var characterFootEffectsFootGenericDelegate = new Preset.GenericDelegate<Character.CharacterFootEffects.Foot>();
            var characterFootEffectsFootFuncDelegate = new Func<Character.CharacterFootEffects.Foot>(() => { return new Character.CharacterFootEffects.Foot(); });
            var characterFootEffectsFootActionDelegate = new Action<Character.CharacterFootEffects.Foot>((Character.CharacterFootEffects.Foot value) => { });
            var characterFootEffectsFootstepPlacementModeGenericDelegate = new Preset.GenericDelegate<Character.CharacterFootEffects.FootstepPlacementMode>();
            var characterFootEffectsFootstepPlacementModeFuncDelegate = new Func<Character.CharacterFootEffects.FootstepPlacementMode>(() => { return 0; });
            var characterFootEffectsFootstepPlacementModeActionDelegate = new Action<Character.CharacterFootEffects.FootstepPlacementMode>((Character.CharacterFootEffects.FootstepPlacementMode value) => { });
            var spawnPointSpawnShapeGenericDelegate = new Preset.GenericDelegate<Game.SpawnPoint.SpawnShape>();
            var spawnShapeFuncDelegate = new Func<Game.SpawnPoint.SpawnShape>(() => { return 0; });
            var spawnShapeActionDelegate = new Action<Game.SpawnPoint.SpawnShape>((Game.SpawnPoint.SpawnShape value) => { });
            var respawnerSpawnPositioningModeGenericDelegate = new Preset.GenericDelegate<Traits.Respawner.SpawnPositioningMode>();
            var respawnerSpawnPositioningFuncDelegate = new Func<Traits.Respawner.SpawnPositioningMode>(() => { return 0; });
            var respawnerSpawnPositioningActionDelegate = new Action<Traits.Respawner.SpawnPositioningMode>((Traits.Respawner.SpawnPositioningMode value) => { });
            var movingPlatformWaypointGenericDelegate = new Preset.GenericDelegate<Objects.MovingPlatform.Waypoint>();
            var movingPlatformWaypointFuncDelegate = new Func<Objects.MovingPlatform.Waypoint>(() => { return new Objects.MovingPlatform.Waypoint(); });
            var movingPlatformWaypointActionDelegate = new Action<Objects.MovingPlatform.Waypoint>((Objects.MovingPlatform.Waypoint value) => { });
            var movingPlatformPathMovementTypeGenericDelegate = new Preset.GenericDelegate<Objects.MovingPlatform.PathMovementType>();
            var movingPlatformPathMovementTypeFuncDelegate = new Func<Objects.MovingPlatform.PathMovementType> (() => { return 0; });
            var movingPlatformPathMovementTypeActionDelegate = new Action<Objects.MovingPlatform.PathMovementType>((Objects.MovingPlatform.PathMovementType value) => { });
            var movingPlatformPathDirectionGenericDelegate = new Preset.GenericDelegate<Objects.MovingPlatform.PathDirection>();
            var movingPlatformPathDirectionFuncDelegate = new Func<Objects.MovingPlatform.PathDirection>(() => { return 0; });
            var movingPlatformPathDirectionActionDelegate = new Action<Objects.MovingPlatform.PathDirection>((Objects.MovingPlatform.PathDirection value) => { });
            var movingPlatformMovementInterpolationModeGenericDelegate = new Preset.GenericDelegate<Objects.MovingPlatform.MovementInterpolationMode>();
            var movingPlatformMovementInterpolationModeFuncDelegate = new Func<Objects.MovingPlatform.MovementInterpolationMode>(() => { return 0; });
            var movingPlatformMovementInterpolationModeActionDelegate = new Action<Objects.MovingPlatform.MovementInterpolationMode>((Objects.MovingPlatform.MovementInterpolationMode value) => { });
            var movingPlatformRotateInterpolationModeGenericDelegate = new Preset.GenericDelegate<Objects.MovingPlatform.RotateInterpolationMode>();
            var movingPlatformRotateInterpolationModeFuncDelegate = new Func<Objects.MovingPlatform.RotateInterpolationMode>(() => { return 0; });
            var movingPlatformRotateInterpolationModeActionDelegate = new Action<Objects.MovingPlatform.RotateInterpolationMode>((Objects.MovingPlatform.RotateInterpolationMode value) => { });
            var audioClipSetGenericDelegate = new Preset.GenericDelegate<Audio.AudioClipSet>();
            var audioClipSetFuncDelegate = new Func<Audio.AudioClipSet>(() => { return null; });
            var audioClipSetActionDelegate = new Action<Audio.AudioClipSet>((Audio.AudioClipSet value) => { });
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            var autoReloadTypeGenericDelegate = new Preset.GenericDelegate<Character.Abilities.Items.Reload.AutoReloadType>();
            var autoReloadTypeFuncDelegate = new Func<Character.Abilities.Items.Reload.AutoReloadType>(() => { return 0; });
            var autoReloadTypeModeActionDelegate = new Action<Character.Abilities.Items.Reload.AutoReloadType>((Character.Abilities.Items.Reload.AutoReloadType value) => { });
            var shootableWeaponFireModeGenericDelegate = new Preset.GenericDelegate<Items.Actions.ShootableWeapon.FireMode>();
            var shootableWeaponFireModeFuncDelegate = new Func<Items.Actions.ShootableWeapon.FireMode>(() => { return 0; });
            var shootableWeaponFireModeActionDelegate = new Action<Items.Actions.ShootableWeapon.FireMode>((Items.Actions.ShootableWeapon.FireMode value) => { });
            var shootableWeaponFireTypeGenericDelegate = new Preset.GenericDelegate<Items.Actions.ShootableWeapon.FireType>();
            var shootableWeaponFireTypeFuncDelegate = new Func<Items.Actions.ShootableWeapon.FireType>(() => { return 0; });
            var shootableWeaponFireTypeActionDelegate = new Action<Items.Actions.ShootableWeapon.FireType>((Items.Actions.ShootableWeapon.FireType value) => { });
            var shootableWeaponProjectileVisibilityGenericDelegate = new Preset.GenericDelegate<Items.Actions.ShootableWeapon.ProjectileVisiblityType>();
            var shootableWeaponProjectileVisiblityTypeFuncDelegate = new Func<Items.Actions.ShootableWeapon.ProjectileVisiblityType>(() => { return 0; });
            var shootableWeaponProjectileVisiblityTypeActionDelegate = new Action<Items.Actions.ShootableWeapon.ProjectileVisiblityType>((Items.Actions.ShootableWeapon.ProjectileVisiblityType value) => { });
            var shootableWeaponReloadClipTypeGenericDelegate = new Preset.GenericDelegate<Items.Actions.ShootableWeapon.ReloadClipType>();
            var shootableWeaponReloadClipTypeFuncDelegate = new Func<Items.Actions.ShootableWeapon.ReloadClipType>(() => { return 0; });
            var shootableWeaponReloadClipTypeActionDelegate = new Action<Items.Actions.ShootableWeapon.ReloadClipType>((Items.Actions.ShootableWeapon.ReloadClipType value) => { });
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
            var meleeWeaponTrailVisibilityGenericDelegate = new Preset.GenericDelegate<Items.Actions.MeleeWeapon.TrailVisibilityType>();
            var meleeWeaponTrailVisibilityFuncDelegate = new Func<Items.Actions.MeleeWeapon.TrailVisibilityType>(() => { return 0; });
            var meleeWeaponTrailVisibilityActionDelegate = new Action<Items.Actions.MeleeWeapon.TrailVisibilityType>((Items.Actions.MeleeWeapon.TrailVisibilityType value) => { });
#endif
            var magicItemCastDirectionGenericDelegate = new Preset.GenericDelegate<Items.Actions.MagicItem.CastDirection>();
            var magicItemCastDirectionFuncDelegate = new Func<Items.Actions.MagicItem.CastDirection>(() => { return 0; });
            var magicItemCastDirectionActionDelegate = new Action<Items.Actions.MagicItem.CastDirection>((Items.Actions.MagicItem.CastDirection value) => { });
            var magicItemCastUseTypeGenericDelegate = new Preset.GenericDelegate<Items.Actions.MagicItem.CastUseType>();
            var magicItemCastUseTypeFuncDelegate = new Func<Items.Actions.MagicItem.CastUseType>(() => { return 0; });
            var magicItemCastUseTypeActionDelegate = new Action<Items.Actions.MagicItem.CastUseType>((Items.Actions.MagicItem.CastUseType value) => { });
            var magicItemCastInterruptSourceGenericDelegate = new Preset.GenericDelegate<Items.Actions.MagicItem.CastInterruptSource>();
            var magicItemCastInterruptSourceFuncDelegate = new Func<Items.Actions.MagicItem.CastInterruptSource>(() => { return 0; });
            var magicItemCastInterruptSourceActionDelegate = new Action<Items.Actions.MagicItem.CastInterruptSource>((Items.Actions.MagicItem.CastInterruptSource value) => { });
            var healthFlashMonitorFlashGenericDelegate = new Preset.GenericDelegate<UI.HealthFlashMonitor.Flash>();
            var healthFlashMonitorFlashFuncDelegate = new Func<UI.HealthFlashMonitor.Flash>(() => { return new UI.HealthFlashMonitor.Flash(); });
            var healthFlashMonitorFlashActionDelegate = new Action<UI.HealthFlashMonitor.Flash>((UI.HealthFlashMonitor.Flash value) => { });
#pragma warning restore 0219
        }
    }
}
#endif