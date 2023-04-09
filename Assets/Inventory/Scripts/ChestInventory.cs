using Items.Interfaces;
using Items.Scripts;
using UnityEngine.Events;

namespace Inventory.Scripts
{
    public class ChestInventory : InventoryHolder, IInteractable
    {
        public UnityAction<IInteractable> OnInteractionComplete { get; set; }
        
        public void Interact(Interactor interactor, out bool interactSuccessful)
        {
            OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem);
            interactSuccessful = true;
        }

        public void EndInteraction()
        {
            // Use it for example if player leave the chest, so you can trigger close the chest
        }
    }
}
