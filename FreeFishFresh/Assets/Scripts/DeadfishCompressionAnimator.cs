using UnityEngine;

/// <summary>
/// Drives the deadfish's base wing states and additive coil-compression layer.
/// </summary>
[RequireComponent(typeof(Animator))]
public class DeadfishCompressionAnimator : MonoBehaviour
{
    private static readonly int FlyingActivatedId = Animator.StringToHash("FlyingActivated");
    private static readonly int WingInputId = Animator.StringToHash("WingInput");
    private static readonly int SpringCompressionId = Animator.StringToHash("SpringCompression");

    [SerializeField] private PlayerController playerController;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (playerController == null)
            return;

        animator.SetBool(FlyingActivatedId, playerController.wingInput > 0.2f);
        animator.SetFloat(WingInputId, Mathf.Clamp01(playerController.wingInput));
        animator.SetFloat(SpringCompressionId, playerController.springCompression);
    }
}
