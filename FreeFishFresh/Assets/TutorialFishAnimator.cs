using UnityEngine;

/// <summary>
/// Tutorial-only animation bridge. Keeps the preview fish independent from the
/// real player's animation driver while using the tutorial controller's state.
/// </summary>
[RequireComponent(typeof(Animator))]
public class TutorialFishAnimator : MonoBehaviour
{
    private static readonly int FlyingActivatedId = Animator.StringToHash("FlyingActivated");
    private static readonly int WingInputId = Animator.StringToHash("WingInput");
    private static readonly int SpringCompressionId = Animator.StringToHash("SpringCompression");

    [SerializeField] private TutorialPlayerControlller tutorialPlayer;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (tutorialPlayer == null)
            tutorialPlayer = GetComponentInParent<TutorialPlayerControlller>();

        // Preview panels pause gameplay, but their animations must keep moving.
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    private void Update()
    {
        if (tutorialPlayer == null)
            return;

        animator.SetBool(FlyingActivatedId, tutorialPlayer.wingsExtended);
        animator.SetFloat(WingInputId, tutorialPlayer.wingInput);
        animator.SetFloat(SpringCompressionId, tutorialPlayer.springCompression);
    }
}
