using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private Camera playerCamera;

    public override void OnNetworkSpawn()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (!IsOwner)
        {
            if (playerMovement != null)
                playerMovement.enabled = false;
            return;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null && playerMovement != null)
            playerMovement.cameraTransform = playerCamera.transform;

        Debug.Log($"PlayerNetworkController: owner initialized for client {OwnerClientId}");
    }
}
