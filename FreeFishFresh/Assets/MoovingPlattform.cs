
using UnityEngine;

public class MoovingPlattform : MonoBehaviour
{
    [Header("references")]
    public PlayerController playerController;
    public Transform Island;
    public Transform start;
    public Transform resetPoint;
    public Transform stop;

    [Header("values")]
    [Min(0f)] public float speed = 2f;
    [Min(0.01f)] public float smoothTime = 0.5f;
    [Min(0.001f)] public float arrivalDistance = 0.01f;

    private bool movingToStop = true;
    private Vector3 currentVelocity;

    private void OnEnable()
    {
        if (playerController != null)
            playerController.ResetPerformed += HandlePlayerReset;
    }

    private void OnDisable()
    {
        if (playerController != null)
            playerController.ResetPerformed -= HandlePlayerReset;
    }

    private void HandlePlayerReset(float penalty)
    {
        if (Island == null || resetPoint == null)
            return;

        Island.position = resetPoint.position;
        currentVelocity = Vector3.zero;
        movingToStop = false;
    }

    private void FixedUpdate()
    {
        if (Island == null || start == null || stop == null)
            return;

        Vector3 target = movingToStop ? stop.position : start.position;
        Island.position = Vector3.SmoothDamp(
            Island.position,
            target,
            ref currentVelocity,
            smoothTime,
            speed,
            Time.fixedDeltaTime
        );

        if ((Island.position - target).sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            Island.position = target;
            currentVelocity = Vector3.zero;
            movingToStop = !movingToStop;
        }
    }
}
