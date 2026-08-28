using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HookBehaviour : MonoBehaviour
{
    public PlayerController playerController;
    public float pullTime = 3f;
    public float distance = 20f;
    private bool isHooking;

    private void OnTriggerEnter(Collider other)
    {
        if (isHooking || !other.CompareTag("Player"))
            return;

        playerController = other.GetComponent<PlayerController>();

        if (playerController != null)
            StartCoroutine(HookRoutine());
    }


    IEnumerator HookRoutine()
    {
        isHooking = true;

        // The pull is scripted below, but the player must not be able to steer,
        // boost, or reset while it is in progress.
        InputActionMap fishActions = playerController.playerInput.actions.FindActionMap("Fish", false);
        bool fishActionsWereEnabled = fishActions != null && fishActions.enabled;
        if (fishActionsWereEnabled)
            fishActions.Disable();

        // slowmo
        playerController.targetTimescale = 0f;

        Transform player = playerController.transform;

        Vector3 startPosition = player.position;
        Vector3 startPositionHook = transform.position;

        // Slightly below the hook, so it doesn't end inside its collider.
        Vector3 targetPosition = transform.position;

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / pullTime)
        {
            player.position = startPosition + Vector3.up * t * distance;
            transform.position = startPositionHook + Vector3.up * t * distance;
            playerController.targetTimescale = 0f;
            yield return null;
        }

        player.position = targetPosition;
        transform.position = targetPosition;

        playerController.ResetGlider();

        if (fishActionsWereEnabled)
            fishActions.Enable();

        isHooking = false;
    }
}
