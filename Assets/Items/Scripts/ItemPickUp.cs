using Inventory.Scripts;
using UnityEngine;

namespace Items.Scripts
{
    [RequireComponent(typeof(SphereCollider))]
    public class ItemPickUp : MonoBehaviour
    {
        public float pickUpRadius = 1f;
        public InventoryItemData itemData;

        private SphereCollider itemCollider;

        private void Awake()
        {
            itemCollider = GetComponent<SphereCollider>();
            itemCollider.isTrigger = true;
            itemCollider.radius = pickUpRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            var inventory = other.transform.GetComponent<InventoryHolder>();

            if (!inventory) return;

            if (inventory.InventorySystem.AddToInventory(itemData, 1))
            {
                Destroy(this.gameObject);
            }
        }
    }
}
