using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public LayerMask collisionLayer;
    public LayerMask interactionLayer;

    PlayerInput input;
    InteractionControllerUI ui;
    Transform mainCamera;

    private void Start()
    {
        input = GetComponentInParent<PlayerInput>();

        ui = FindObjectOfType<InteractionControllerUI>();
        mainCamera = GetComponentInParent<CameraMovement>().transform;
        ui.SetCode(input.interactKey.ToString());
    }

    void Update()
    {
        Interactable interactWith = null;

        //First send a ray out forwards to hit anything
        if (Physics.Raycast(mainCamera.position, mainCamera.forward, out var hit, 10f, collisionLayer))
        {
            //Get the distance then send another ray for only the interaction layer using that distance
            float dis = Vector3.Distance(mainCamera.position, hit.point) + 0.05f;
            Debug.Log(hit.transform.name);
            if (Physics.Raycast(mainCamera.position, mainCamera.forward, out var interact, dis, interactionLayer))
            {
                Interactable inFront = interact.transform.GetComponent<Interactable>();
                if (inFront == null) return;
                if (dis > inFront.interactRange + 0.05f)
                    inFront = null;
                interactWith = inFront; //Set interactWith to the one we hit

                if (interactWith != null)
                {
                    ui.UpdateInteract(interactWith.description);
                    if (input.interact)
                        interactWith.Interact();
                }
            }
        }

        ui.InteractableSelected(interactWith != null);
    }
}
