using UnityEngine;

/// <summary>
/// Keeps the flying tutorial's render camera locked behind its preview fish.
/// This is intentionally separate from the real player's Cinemachine setup.
/// </summary>
[DefaultExecutionOrder(100)]
public class TutorialFlightCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private GameObject activeWhile;
    [SerializeField] private Vector3 targetLocalOffset = new Vector3(0f, 0f, -3.22f);

    private void OnEnable()
    {
        FollowTarget();
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (target == null
            || (activeWhile != null && !activeWhile.activeInHierarchy))
            return;

        transform.SetPositionAndRotation(
            target.position + target.rotation * targetLocalOffset,
            target.rotation);
    }
}
