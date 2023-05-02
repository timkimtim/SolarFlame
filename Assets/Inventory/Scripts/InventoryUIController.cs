using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inventory.Scripts
{
    public class InventoryUIController : MonoBehaviour
    {
        public DynamicInventoryDisplay chestPanel;
        public DynamicInventoryDisplay playerInventoryPanel;
        public DynamicInventoryDisplay playeEquipSlotsPanel;
        public DynamicInventoryDisplay playeBuilderSlotsPanel;
        public GameObject playerInventoryHolder;

        private void OnEnable()
        {
            InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
            PlayerInventoryHolder.OnPlayerInventoryDisplayRequested += DisplayPlayerInventory;
            PlayerInventoryHolder.OnPlayerEquipSlotsDisplayRequested += DisplayEquipSlots;
            PlayerInventoryHolder.OnPlayerBuilderSlotsDisplayRequested += DisplayBuilderSlots;
        }

        private void OnDisable()
        {
            InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
            PlayerInventoryHolder.OnPlayerInventoryDisplayRequested -= DisplayPlayerInventory;
            PlayerInventoryHolder.OnPlayerEquipSlotsDisplayRequested -= DisplayEquipSlots;
            PlayerInventoryHolder.OnPlayerBuilderSlotsDisplayRequested -= DisplayBuilderSlots;
        }

        private void Awake()
        {
            chestPanel.gameObject.SetActive(false);
            playerInventoryHolder.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (chestPanel.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                chestPanel.gameObject.SetActive(false);
            }
            if (playerInventoryHolder.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                playerInventoryHolder.gameObject.SetActive(false);
            }
        }

        private void DisplayInventory(InventorySystem inventoryToDisplay)
        {
            chestPanel.gameObject.SetActive(true);
            chestPanel.RefreshDynamicInventory(inventoryToDisplay);
        }

        private void DisplayPlayerInventory(InventorySystem inventoryToDisplay)
        {
            playerInventoryHolder.gameObject.SetActive(true);
            playerInventoryPanel.RefreshDynamicInventory(inventoryToDisplay);
        }
        private void DisplayEquipSlots(InventorySystem inventoryToDisplay)
        {
            playeEquipSlotsPanel.RefreshDynamicInventory(inventoryToDisplay);
        }
        private void DisplayBuilderSlots(InventorySystem inventoryToDisplay)
        {
            playeBuilderSlotsPanel.RefreshDynamicInventory(inventoryToDisplay);
        }
    }
}
