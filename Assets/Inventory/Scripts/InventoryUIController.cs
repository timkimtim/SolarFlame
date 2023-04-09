using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inventory.Scripts
{
    public class InventoryUIController : MonoBehaviour
    {
        public DynamicInventoryDisplay chestPanel;
        public DynamicInventoryDisplay playerInventoryPanel;

        private void OnEnable()
        {
            InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
            PlayerInventoryHolder.OnPlayerInventoryDisplayRequested += DisplayPlayerInventory;
        }

        private void OnDisable()
        {
            InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
            PlayerInventoryHolder.OnPlayerInventoryDisplayRequested -= DisplayPlayerInventory;
        }

        private void Awake()
        {
            chestPanel.gameObject.SetActive(false);
            playerInventoryPanel.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (chestPanel.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                chestPanel.gameObject.SetActive(false);
            }
            if (playerInventoryPanel.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                playerInventoryPanel.gameObject.SetActive(false);
            }
        }

        private void DisplayInventory(InventorySystem inventoryToDisplay)
        {
            chestPanel.gameObject.SetActive(true);
            chestPanel.RefreshDynamicInventory(inventoryToDisplay);
        }

        private void DisplayPlayerInventory(InventorySystem inventoryToDisplay)
        {
            playerInventoryPanel.gameObject.SetActive(true);
            playerInventoryPanel.RefreshDynamicInventory(inventoryToDisplay);
        }
    }
}
