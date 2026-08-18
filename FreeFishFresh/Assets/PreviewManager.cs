using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PreviewManager : MonoBehaviour
{
    public float activationTime = 0.5f;

    [Header("References")]
    public GameObject LiveSign;
    public GameObject continueButton;

    [Header("Player Progress")]
    public bool didYaw;
    public bool didRoll;
    public bool didRudder;

    private InputAction yawAction;
    private InputAction rollAction;
    private InputAction rudderAction;
    private float timer;

    private void OnEnable()
    {
        didYaw = false;
        didRoll = false;
        didRudder = false;
        timer = 0f;

        LiveSign.SetActive(true);
        continueButton.SetActive(false);

        PlayerInput input = TutorialManager.Instance.playerController.playerInput;
        yawAction = input.actions.FindAction("TutPreview/Yaw", true);
        rollAction = input.actions.FindAction("TutPreview/Roll", true);
        rudderAction = input.actions.FindAction("TutPreview/Rudder", true);
    }

    private void Update()
    {
        if (continueButton.activeSelf)
            return;

        timer += Time.unscaledDeltaTime;

        if (Mathf.Abs(yawAction.ReadValue<float>()) > 0.25f)
            didYaw = true;
        if (Mathf.Abs(rollAction.ReadValue<float>()) > 0.25f)
            didRoll = true;
        if (Mathf.Abs(rudderAction.ReadValue<float>()) > 0.25f)
            didRudder = true;

        if (timer >= activationTime && didYaw && didRoll && didRudder)
            CompleteTutorialPreview();
    }

    private void CompleteTutorialPreview()
    {
        LiveSign.SetActive(false);
        continueButton.SetActive(true);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(continueButton);
    }
}
