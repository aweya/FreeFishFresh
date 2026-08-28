using UnityEngine;

public sealed class WhaleMouthToggle : MonoBehaviour
{
    public Animator animator;
    public bool openMouth;

    private static readonly int OpenMouthParameter = Animator.StringToHash("OpenMouth");
    private bool previousValue;

    private void Awake()
    {
        previousValue = !openMouth;
        ApplyValue();
    }

    private void Update()
    {
        if (openMouth != previousValue)
            ApplyValue();
    }

    private void ApplyValue()
    {
        if (animator != null)
            animator.SetBool(OpenMouthParameter, openMouth);

        previousValue = openMouth;
    }
}
