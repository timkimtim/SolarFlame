using System.Collections.Generic;
using System.Linq;
using Items.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory.Scripts
{
    [System.Serializable]
    public class InventorySystem
    {
        [SerializeField] private List<InventorySlot> inventorySlots;

        public List<InventorySlot> InventorySlots => inventorySlots;
        public int InventorySize => InventorySlots.Count;

        public UnityAction<InventorySlot> OnInventorySlotChanged;

        /// <summary>
        /// Constructor that sets the amount of slots
        /// </summary>
        public InventorySystem(int size)
        {
            inventorySlots = new List<InventorySlot>(size);

            for (var i = 0; i < size; i++)
            {
                inventorySlots.Add(new InventorySlot());
            }
        }

        public bool AddToInventory(InventoryItemData itemToAdd, int amountToAdd)
        {
            // Check whether item exists in inventory
            if (ContainsItem(itemToAdd, out List<InventorySlot> inventorySlotList))
            {
                foreach (var slot in inventorySlotList.Where(slot => slot.EnoughRoomLeftInStack(amountToAdd)))
                {
                    slot.AddToStack(amountToAdd);
                    OnInventorySlotChanged?.Invoke(slot);
                    return true;
                }
            }

            // Gets first available slot
            if (!HasFreeSlot(out InventorySlot freeSlot)) return false;
            if (!freeSlot.EnoughRoomLeftInStack(amountToAdd)) return false;
            
            freeSlot.UpdateInventorySlot(itemToAdd, amountToAdd);
            OnInventorySlotChanged?.Invoke(freeSlot);
            return true;
            
            // Add implementation to only take what can fill the stack, and check for another free slot to put the remainder in
        }

        /// <summary>
        /// Do any of our slots have an item to add in them
        /// </summary>
        public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> inventorySlotList)
        {
            inventorySlotList = InventorySlots.Where(slot => slot.ItemData == itemToAdd).ToList();
            return inventorySlotList != null;
        }

        public bool HasFreeSlot(out InventorySlot freeSlot)
        {
            freeSlot = InventorySlots.FirstOrDefault(slot => slot.ItemData == null);
            return freeSlot != null;
        }
    }
}
