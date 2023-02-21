/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    /// <summary>
    /// Interface for a usable Item object input.
    /// </summary>
    public interface IItemInput
    {
        bool UseEquippedItemInput(IUsableItem item, int actionIndex);
        bool UseItemHotbarInput(int slotIndex);
        bool DropItemHotbarInput(int slotIndex);
    }
}