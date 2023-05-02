using System;
using Items.Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Inventory.Scripts
{
    public class PlayerInventoryHolder : InventoryHolder
    {
        [SerializeField] protected int mainInventorySize;
        [SerializeField] protected InventorySystem mainInventorySystem;
        
        [SerializeField] protected int equipSlotsSize;
        [SerializeField] protected InventorySystem equipSlotsSystem;
        
        [SerializeField] protected int builderSlotsSize;
        [SerializeField] protected InventorySystem builderSlotsSystem;

        public InventorySystem MainInventorySystem => mainInventorySystem;
        public InventorySystem EquipSlotsSystem => equipSlotsSystem;
        public InventorySystem BuilderSlotsSystem => builderSlotsSystem;

        public static UnityAction<InventorySystem> OnPlayerInventoryDisplayRequested;
        public static UnityAction<InventorySystem> OnPlayerEquipSlotsDisplayRequested;
        public static UnityAction<InventorySystem> OnPlayerBuilderSlotsDisplayRequested;
        
        protected override void Awake()
        {
            base.Awake();

            mainInventorySystem = new InventorySystem(mainInventorySize);
            equipSlotsSystem = new InventorySystem(equipSlotsSize);
            builderSlotsSystem = new InventorySystem(builderSlotsSize);
        }

        private void Update()
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                OnPlayerInventoryDisplayRequested?.Invoke(mainInventorySystem);
                OnPlayerEquipSlotsDisplayRequested?.Invoke(equipSlotsSystem);
                OnPlayerBuilderSlotsDisplayRequested?.Invoke(builderSlotsSystem);
            }
        }

        public bool AddToInventory(InventoryItemData data, int amount)
        {
            if (primaryInventorySystem.AddToInventory(data, amount))
            {
                return true;
            } else if (mainInventorySystem.AddToInventory(data, amount))
            {
                return true;
            }

            return false;
        }
    }
}
