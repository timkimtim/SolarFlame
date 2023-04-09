using Items.Scripts;
using UnityEngine;

namespace Inventory.Scripts
{
    [System.Serializable]
    public class InventorySlot
    {
        [SerializeField] private InventoryItemData itemData; // reference to the data
        [SerializeField] private int stackSize; // current stack size

        public InventoryItemData ItemData => itemData;
        public int StackSize => stackSize;

        /// <summary>
        /// constructor to make a occupied inventory slot
        /// </summary>
        public InventorySlot(InventoryItemData source, int amount)
        {
            itemData = source;
            stackSize = amount;
        }

        /// <summary>
        /// constructor to make an empty inventory slot
        /// </summary>
        public InventorySlot()
        {
            ClearSlot();
        }

        public void ClearSlot()
        {
            itemData = null;
            stackSize = -1;
        }

        /// <summary>
        /// Assigns an item to a slot
        /// </summary>
        /// <param name="inventorySlot"></param>
        public void AssignItem(InventorySlot inventorySlot)
        {
            if (itemData == inventorySlot.itemData) AddToStack(inventorySlot.stackSize); // if slot contains an item - add to stack
            else // overwrite slot with the inventory slot that we are passing in
            {
                itemData = inventorySlot.itemData;
                stackSize = 0;
                AddToStack(inventorySlot.stackSize);
            }
        }

        /// <summary>
        /// Updates slot directly
        /// </summary>
        public void UpdateInventorySlot(InventoryItemData data, int amount)
        {
            itemData = data;
            stackSize = amount;
        }

        /// <summary>
        /// Would there be enough room in the stack for amount we are trying to add
        /// </summary>
        public bool EnoughRoomLeftInStack(int amountToAdd, out int amountRemaining)
        {
            amountRemaining = itemData.maxStackSize - stackSize;
            return EnoughRoomLeftInStack(amountToAdd);
        }

        public bool EnoughRoomLeftInStack(int amountToAdd)
        {
            return itemData == null || itemData != null && stackSize + amountToAdd <= itemData.maxStackSize;
        }

        public void AddToStack(int amount)
        {
            stackSize += amount;
        }

        public void RemoveFromStack(int amount)
        {
            stackSize -= amount;
        }

        public bool SplitStack(out InventorySlot splitStack)
        {
            if (stackSize <= 1) // is there enough to actually split
            {
                splitStack = null;
                return false;
            }

            var halfStack = Mathf.RoundToInt(stackSize / 2);
            RemoveFromStack(halfStack);

            splitStack = new InventorySlot(itemData, halfStack); // Creates a copy of this slot with half the stack size
            return true;
        }
    }
}
