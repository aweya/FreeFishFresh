using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PreviewManager : MonoBehaviour
{
    public float activationTimeMovement = 0.5f;
    public float activationTimeTesnion = 5f;

    [Header("References")]
    public GameObject LiveSign;
    public GameObject continueButton;

    [Header("Preview kind")]
    public bool movement = false;
    public bool tension = true;

    [Header("Player Progress")]
    public bool didYaw;
    public bool didRoll;
    public bool didRudder;
    public bool didTension = false;

    private InputAction yawAction;
    private InputAction rollAction;
    private InputAction rudderAction;
    private InputAction tensionAction;
    private float timerMovement;
    private float timerTension;

    private void OnEnable()
    {
        didYaw = false;
        didRoll = false;
        didRudder = false;
        didTension = false;
        timerMovement = 0f;
        timerTension = 0f;

        LiveSign.SetActive(true);
        continueButton.SetActive(false);

        PlayerInput input = TutorialManager.Instance.playerController.playerInput;
        yawAction = input.actions.FindAction("TutPreview/Yaw", true);
        rollAction = input.actions.FindAction("TutPreview/Roll", true);
        rudderAction = input.actions.FindAction("TutPreview/Rudder", true);
        tensionAction = input.actions.FindAction("TutPreview/Spring", true);
    }

    private void Update()
    {
        if (continueButton.activeSelf)
            return;

        timerMovement += Time.unscaledDeltaTime;


        if (Mathf.Abs(yawAction.ReadValue<float>()) > 0.25f)
            didYaw = true;
        if (Mathf.Abs(rollAction.ReadValue<float>()) > 0.25f)
            didRoll = true;
        if (Mathf.Abs(rudderAction.ReadValue<float>()) > 0.25f)
            didRudder = true;
        if (Mathf.Abs(tensionAction.ReadValue<float>()) > 0.25f)
            didTension = true;


        if (movement)
        {
            Movement();
        }
        if (tension)
        {
            Tension();
        }

    }

    private void Tension()
    {
        if (didTension)
        {
            timerTension += Time.unscaledDeltaTime;
        }
        if (timerTension >= activationTimeTesnion)
        {
            CompleteTutorialPreview();
        }
    }

    private void Movement()
    {
        if (timerMovement >= activationTimeMovement && didYaw && didRoll && didRudder)
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
