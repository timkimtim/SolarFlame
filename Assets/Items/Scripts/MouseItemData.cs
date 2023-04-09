using System.Collections.Generic;
using Inventory.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Items.Scripts
{
    public class MouseItemData : MonoBehaviour
    {
        public Image itemSprite;
        public TextMeshProUGUI itemCount;
        public InventorySlot AssignedInventorySlot;

        private void Awake()
        {
            itemSprite.color = Color.clear;
            itemCount.text = "";
        }

        public void UpdateSlot(InventorySlot inventorySlot)
        {
            AssignedInventorySlot.AssignItem(inventorySlot);

            itemSprite.sprite = inventorySlot.ItemData.icon;
            itemSprite.color = Color.white;

            if (inventorySlot.StackSize > 1) itemCount.text = inventorySlot.StackSize.ToString();
            else itemCount.text = "";
        }
        private void Update()
        {
            // TODO: add controller support
            if (AssignedInventorySlot.ItemData != null) // if has an item follow the mouse position
            {
                transform.position = Mouse.current.position.ReadValue();

                if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUIObject())
                {
                    ClearSlot();
                    // TODO: Drop an item to the ground
                }
            }
        }

        public void ClearSlot ()
        {
            AssignedInventorySlot.ClearSlot();
            itemCount.text = "";
            itemSprite.color = Color.clear;
            itemSprite.sprite = null;
        }

        private static bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}
