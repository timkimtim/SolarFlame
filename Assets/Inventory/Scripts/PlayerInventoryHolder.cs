using System;
using Items.Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Inventory.Scripts
{
    public class PlayerInventoryHolder : InventoryHolder
    {
        [SerializeField] protected int secondaryInventorySize;
        [SerializeField] protected InventorySystem secondaryInventorySystem;

        public InventorySystem SecondaryInventorySystem => secondaryInventorySystem;

        public static UnityAction<InventorySystem> OnPlayerInventoryDisplayRequested;
        
        protected override void Awake()
        {
            base.Awake();

            secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
        }

        private void Update()
        {
            if (Keyboard.current.bKey.wasPressedThisFrame) OnPlayerInventoryDisplayRequested?.Invoke(secondaryInventorySystem);
        }

        public bool AddToInventory(InventoryItemData data, int amount)
        {
            if (primaryInventorySystem.AddToInventory(data, amount))
            {
                return true;
            } else if (secondaryInventorySystem.AddToInventory(data, amount))
            {
                return true;
            }

            return false;
        }
    }
}
