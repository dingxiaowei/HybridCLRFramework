/// ---------------------------------------------
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
    /// Interface for an item that can be reloaded.
    /// </summary>
    public interface IReloadableItem
    {
        /// <summary>
        /// Returns the item that the ReloadableItem is attached to.
        /// </summary>
        /// <returns>The item that the ReloadableItem is attached to.</returns>
        Item Item { get; }

        /// <summary>
        /// Returns the set for the Reload AnimatorAudioStateSet.
        /// </summary>
        /// <returns>The set for the Reload AnimatorAudioStateSet.</returns>
        AnimatorAudioStateSet ReloadAnimatorAudioStateSet { get; }

        /// <summary>
        /// Specifies if the item should wait for the OnAnimatorItemReload animation event or wait for the specified duration before reloading.
        /// </summary>
        /// <returns>Value of if the item should use the OnAnimatorItemReload animation event or wait the specified duration.</returns>
        AnimationEventTrigger ReloadEvent { get; }

        /// <summary>
        /// Specifies if the item should wait for the OnAnimatorItemReloadComplete animation event or wait for the specified duration before reloading.
        /// </summary>
        /// <returns>Value of if the item should use the OnAnimatorItemReloadComplete animation event or wait the specified duration.</returns>
        AnimationEventTrigger ReloadCompleteEvent { get; }

        /// <summary>
        /// Specifies when the item should automatically be reloaded.
        /// </summary>
        /// <returns>Value indicating when the item should automatically be reloaded.</returns>
        Reload.AutoReloadType AutoReload { get; }

        /// <summary>
        /// Can the camera zoom while the item is reloading?
        /// </summary>
        /// <returns>True if the camera can zoom while the item is reloading.</returns>
        bool CanCameraZoom { get; }

        /// <summary>
        /// Returns the ItemType which can be reloaded by the item.
        /// </summary>
        /// <returns>The ItemType which can be reloaded by the item.</returns>
        ItemType GetReloadableItemType();

        /// <summary>
        /// Starts to reload the item.
        /// </summary>
        void StartItemReload();

        /// <summary>
        /// Can the item be reloaded?
        /// </summary>
        /// <param name="checkEquipStatus">Should the reload ensure the item is equipped?</param>
        /// <returns>True if the item can be reloaded.</returns>
        bool CanReloadItem(bool checkEquipStatus);

        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        void ReloadItem(bool fullClip);

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>the substate index that the item should be in.</returns>
        int GetItemSubstateIndex();

        /// <summary>
        /// The item has finished reloading.
        /// </summary>
        /// <param name="success">Was the item reloaded successfully?</param>
        void ItemReloadComplete(bool success);
    }
}