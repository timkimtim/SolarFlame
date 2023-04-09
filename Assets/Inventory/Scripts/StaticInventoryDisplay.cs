using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Scripts
{
    public class StaticInventoryDisplay : InventoryDisplay
    {
        [SerializeField] private InventoryHolder inventoryHolder;
        [SerializeField] private InventorySlotUI[] slots;
        
        protected override void Start()
        {
            base.Start();

            if (inventoryHolder != null)
            {
                inventorySystem = inventoryHolder.PrimaryInventorySystem;
                inventorySystem.OnInventorySlotChanged += UpdateSlot;
            }
            else Debug.LogWarning($"No inventory assigned to {this.gameObject}");

            AssignSlot(inventorySystem);
        }
        public override void AssignSlot(InventorySystem inventoryToDisplay)
        {
            slotDictionary = new Dictionary<InventorySlotUI, InventorySlot>();

            if (slots.Length != InventorySystem.InventorySize) Debug.Log($"Inventory slots out of sync on {this.gameObject}");

            for (var i = 0; i < inventorySystem.InventorySize; i++)
            {
                slotDictionary.Add(slots[i], inventorySystem.InventorySlots[i]);
                slots[i].Init(inventorySystem.InventorySlots[i]);
            }
        }
    }
}
