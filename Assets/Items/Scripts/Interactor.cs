using System;
using Items.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items.Scripts
{
    public class Interactor : MonoBehaviour
    {
        public Transform interactionPoint;
        public LayerMask interactionLayer;
        public float interactionPointRadius = 1f;
        public bool isInteracting { get; set; }

        private void Update()
        {
            var colliders = Physics.OverlapSphere(interactionPoint.position, interactionPointRadius, interactionLayer);

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    var interactable = colliders[i].GetComponent<IInteractable>();

                    if (interactable != null) StartInteraction(interactable);
                }
            }
        }

        void StartInteraction(IInteractable interactable)
        {
            interactable.Interact(this, out bool interactSuccessful);
            isInteracting = true;
        }

        void EndInteraction()
        {
            isInteracting = false;
        }
    }
}
