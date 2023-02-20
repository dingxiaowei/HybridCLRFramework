﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Items.Actions
{
    /// <summary>
    /// Interface for an item that can be used (fired, swung, thrown, etc).
    /// </summary>
    public interface IUsableItem
    {
        /// <summary>
        /// Returns the item that the UsableItem is attached to.
        /// </summary>
        /// <returns>The item that the UsableItem is attached to.</returns>
        Item Item { get; }

        /// <summary>
        /// Returns true if the inventory can equip an item that doesn't have any consumable items left.
        /// </summary>
        /// <returns>True if the inventory can equip an item that doesn't have any consumable items left.</returns>
        bool CanEquipEmptyItem { get; }

        /// <summary>
        /// Returns true if the character should turn to face the target.
        /// </summary>
        /// <returns>True if the character should turn to face the target.</returns>
        bool FaceTarget { get; }

        /// <summary>
        /// Does the item require root motion position during use?
        /// </summary>
        /// <returns>True if the item requires root motion position during use.</returns>
        bool ForceRootMotionPosition { get; }

        /// <summary>
        /// Does the item require root motion rotation during use?
        /// </summary>
        /// <returns>True if the item requires root motion rotation during use.</returns>
        bool ForceRootMotionRotation { get; }

        /// <summary>
        /// Specifies if the item should wait for the OnAnimatorItemUse animation event or wait for the specified duration before reloading.
        /// </summary>
        /// <returns>Value of if the item should use the OnAnimatorItemUse animation event or wait the specified duration.</returns>
        AnimationEventTrigger UseEvent { get; }

        /// <summary>
        /// Specifies if the item should wait for the OnAnimatorItemUseComplete animation event or wait for the specified duration before reloading.
        /// </summary>
        /// <returns>Value of if the item should use the OnAnimatorItemUseComplete animation event or wait the specified duration.</returns>
        AnimationEventTrigger UseCompleteEvent { get; }

        /// <summary>
        /// The set for the Use AnimatorAudioStateSet.
        /// </summary>
        /// <returns>Returns the set for the Use AnimatorAudioStateSet.</returns>
        AnimatorAudioStateSet UseAnimatorAudioStateSet { get; }

        /// <summary>
        /// Returns the amount of extra time it takes for the ability to stop after use.
        /// </summary>
        /// <returns>The amount of extra time it takes for the ability to stop after use.</returns>
        float StopUseAbilityDelay { get; }

        /// <summary>
        /// Returns the ItemType which can be consumed by the item.
        /// </summary>
        /// <returns>The ItemType which can be consumed by the item.</returns>
        ItemType GetConsumableItemType();

        /// <summary>
        /// Returns the amount of UsableItemType which has been consumed by the UsableItem.
        /// </summary>
        /// <returns>The amount consumed of the UsableItemType.</returns>
        float GetConsumableItemTypeCount();

        /// <summary>
        /// Sets the UsableItemType amount on the UsableItem.
        /// </summary>
        /// <param name="count">The amount to set the UsableItemType to.</param>
        void SetConsumableItemTypeCount(float count);

        /// <summary>
        /// Removes the amount of UsableItemType which has been consumed by the UsableItem.
        /// </summary>
        void RemoveConsumableItemTypeCount();

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="itemAbility">The item ability that is trying to use the item.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        bool CanUseItem(ItemAbility itemAbility, UsableItem.UseAbilityState abilityState);

        /// <summary>
        /// Starts the item use.
        /// </summary>
        void StartItemUse();

        /// <summary>
        /// Uses the item.
        /// </summary>
        void UseItem();

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>Returns the substate index that the item should be in.</returns>
        int GetItemSubstateIndex();

        /// <summary>
        /// Is the item in use?
        /// </summary>
        /// <returns>Returns true if the item is in use.</returns>
        bool IsItemInUse();

        /// <summary>
        /// Is the item waiting to be used? This will return true if the item is waiting to be charged or pulled back.
        /// </summary>
        /// <returns>Returns true if the item is waiting to be used.</returns>
        bool IsItemUsePending();

        /// <summary>
        /// Allows the item to update while it is being used.
        /// </summary>
        void UseItemUpdate();

        /// <summary>
        /// The item has been used.
        /// </summary>
        void ItemUseComplete();

        /// <summary>
        /// Tries to stop the item use.
        /// </summary>
        void TryStopItemUse();

        /// <summary>
        /// Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        bool CanStopItemUse();

        /// <summary>
        /// Stops the item use.
        /// </summary>
        void StopItemUse();
    }
}