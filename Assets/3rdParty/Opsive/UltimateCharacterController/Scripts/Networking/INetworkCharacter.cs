/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Character
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items.Actions;
    using UnityEngine;

    /// <summary>
    /// Acts as a bridge between the character controller and the underlying networking implementation.
    /// </summary>
    public interface INetworkCharacter
    {
        /// <summary>
        /// Loads the inventory's default loadout.
        /// </summary>
        void LoadDefaultLoadout();

        /// <summary>
        /// Equips or unequips the item with the specified ItemIdentifier and slot.
        /// </summary>
        /// <param name="itemIdentifierID">The ID of the ItemIdentifier that should be equipped.</param>
        /// <param name="slotID">The slot of the item that should be equipped.</param>
        /// <param name="equip">Should the item be equipped? If false it will be unequipped.</param>
        void EquipUnequipItem(uint itemIdentifierID, int slotID, bool equip);

        /// <summary>
        /// The ItemIdentifier has been picked up.
        /// </summary>
        /// <param name="itemIdentifierID">The ID of the ItemIdentifier that was picked up.</param>
        /// <param name="amount">The number of ItemIdentifier picked up.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        void ItemIdentifierPickup(uint itemIdentifierID, int amount, int slotID, bool immediatePickup, bool forceEquip);

        /// <summary>
        /// Removes all of the items from the inventory.
        /// </summary>
        void RemoveAllItems();

#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
        /// <summary>
        /// Fires the weapon.
        /// </summary>
        /// <param name="itemAction">The ItemAction that is being fired.</param>
        /// <param name="strength">(0 - 1) value indicating the amount of strength to apply to the shot.</param>
        void Fire(ItemAction itemAction, float strength);

        /// <summary>
        /// Starts to reload the item.
        /// </summary>
        /// <param name="itemAction">The ItemAction that is being reloaded.</param>
        void StartItemReload(ItemAction itemAction);

        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <param name="itemAction">The ItemAction that is being reloaded.</param>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        void ReloadItem(ItemAction itemAction, bool fullClip);

        /// <summary>
        /// The item has finished reloading.
        /// </summary>
        /// <param name="itemAction">The ItemAction that is being reloaded.</param>
        /// <param name="success">Was the item reloaded successfully?</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        void ItemReloadComplete(ItemAction itemAction, bool success, bool immediateReload);
#endif

#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
        /// <summary>
        /// The melee weapon hit a collider.
        /// </summary>
        /// <param name="itemAction">The ItemAction that caused the collision.</param>
        /// <param name="hitboxIndex">The index of the hitbox that caused the collision.</param>
        /// <param name="raycastHit">The raycast that caused the collision.</param>
        /// <param name="hitGameObject">The GameObject that was hit.</param>
        /// <param name="hitCharacterLocomotion">The hit Ultimate Character Locomotion component.</param>
        void MeleeHitCollider(ItemAction itemAction, int hitboxIndex, RaycastHit raycastHit, GameObject hitGameObject, UltimateCharacterLocomotion hitCharacterLocomotion);
#endif

        /// <summary>
        /// Throws the throwable object.
        /// </summary>
        /// <param name="itemAction">The ThrowableItem that is performing the throw.</param>
        void ThrowItem(ItemAction itemAction);

        /// <summary>
        /// Enables the object mesh renderers for the ThrowableItem.
        /// </summary>
        /// <param name="itemAction">The ThrowableItem that is having the renderers enabled.</param>
        void EnableThrowableObjectMeshRenderers(ItemAction itemAction);

        /// <summary>
        /// Starts or stops the begin or end actions.
        /// </summary>
        /// <param name="itemAction">The MagicItem that is starting or stopping the actions.</param>
        /// <param name="beginActions">Should the begin actions be started?</param>
        /// <param name="start">Should the actions be started?</param>
        void StartStopBeginEndMagicActions(ItemAction itemAction, bool beginActions, bool start);

        /// <summary>
        /// Casts a magic CastAction.
        /// </summary>
        /// <param name="itemAction">The MagicItem that is performing the cast.</param>
        /// <param name="index">The index of the CastAction.</param>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="direction">The direction of the cast.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        void MagicCast(ItemAction itemAction, int index, uint castID, Vector3 direction, Vector3 targetPosition);

        /// <summary>
        /// Performs the magic impact.
        /// </summary>
        /// <param name="itemAction">The MagicItem that is performing the impact.</param>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that originated the impact.</param>
        /// <param name="target">The object that received the impact.</param>
        /// <param name="position">The position of the impact.</param>
        /// <param name="normal">The impact normal direction.</param>
        void MagicImpact(ItemAction itemAction, uint castID, GameObject source, GameObject target, Vector3 position, Vector3 normal);

        /// <summary>
        /// Stops the magic CastAction.
        /// </summary>
        /// <param name="itemAction">The MagicItem that is stopping the cast.</param>
        /// <param name="index">The index of the CastAction.</param>
        /// <param name="castID">The ID of the cast.</param>
        void StopMagicCast(ItemAction itemAction, int index, uint castID);

        /// <summary>
        /// Activates or deactives the flashlight.
        /// </summary>
        /// <param name="active">Should the flashlight be activated?</param>
        void ToggleFlashlight(ItemAction itemAction, bool active);

        /// <summary>
        /// Pushes the target Rigidbody in the specified direction.
        /// </summary>
        /// <param name="targetRigidbody">The Rigidbody to push.</param>
        /// <param name="force">The amount of force to apply.</param>
        /// <param name="point">The point at which to apply the push force.</param>
        void PushRigidbody(Rigidbody targetRigidbody, Vector3 force, Vector3 point);

        /// <summary>
        /// Sets the rotation of the character.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        void SetRotation(Quaternion rotation, bool snapAnimator);

        /// <summary>
        /// Sets the position of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        void SetPosition(Vector3 position, bool snapAnimator);

        /// <summary>
        /// Resets the rotation and position to their default values.
        /// </summary>
        void ResetRotationPosition();

        /// <summary>
        /// Sets the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool snapAnimator);

        /// <summary>
        /// Activates or deactivates the character.
        /// </summary>
        /// <param name="active">Is the character active?</param>
        /// <param name="uiEvent">Should the OnShowUI event be executed?</param>
        void SetActive(bool active, bool uiEvent);
    }
}