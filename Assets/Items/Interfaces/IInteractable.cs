using Items.Scripts;
using UnityEngine.Events;

namespace Items.Interfaces
{
    public interface IInteractable
    {
        public UnityAction<IInteractable> OnInteractionComplete { get; set; }

        public void Interact(Interactor interactor, out bool interactSuccessful);

        public void EndInteraction();
    }
}
