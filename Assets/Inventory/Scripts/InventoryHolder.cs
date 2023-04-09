using UnityEngine;
using UnityEngine.Events;

namespace Inventory.Scripts
{
    [System.Serializable]
    public class InventoryHolder : MonoBehaviour
    {
        [SerializeField] private int primaryInventorySize;
        [SerializeField] protected InventorySystem primaryInventorySystem;

        public InventorySystem PrimaryInventorySystem => primaryInventorySystem;

        public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

        protected virtual void Awake()
        {
            primaryInventorySystem = new InventorySystem(primaryInventorySize);
        }
    }
}
