using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialPlayerControlller : MonoBehaviour
{
    [Header("Refrences")]
    public PlayerController playerController;
    public Transform cameraTransform;

    [Header("controll")]
    public bool isFling = false;
    public float rotSpeed = 3f;
    public float yaw;
    public float roll;
    public float rudder;

    [Header("Wings")]
    [Range(0f, 1f)] public float wingInput;
    public bool wingsExtended;
    [Range(0f, 1f)] public float wingExtendedThreshold = 0.2f;

    [Header("Spring")]
    [Range(0f, 1f)] public float springInput;
    [Range(0f, 1f)] public float springCompression;
    [Min(0f)] public float originalRestLength = 1.1f;
    [Min(0f)] public float minimumRestLength = 1f;
    [Min(0f)] public float restLengthChangeSpeed = 1f;
    public float restLength;

    private InputAction yawAction;
    private InputAction rollAction;
    private InputAction rudderAction;
    private InputAction wingAction;
    private InputAction springAction;


    void Start()
    {
        PlayerInput input = playerController.playerInput;
        if (input == null)
            input = playerController.GetComponent<PlayerInput>();

        yawAction = input.actions.FindAction("TutPreview/Yaw", true);
        rollAction = input.actions.FindAction("TutPreview/Roll", true);
        rudderAction = input.actions.FindAction("TutPreview/Rudder", true);
        wingAction = input.actions.FindAction("TutPreview/Wings", true);
        springAction = input.actions.FindAction("TutPreview/Spring", true);

        restLength = originalRestLength;
    }

    void Update()
    {
        yaw = yawAction.ReadValue<float>() * rotSpeed;
        roll = rollAction.ReadValue<float>() * rotSpeed;
        rudder = rudderAction.ReadValue<float>() * rotSpeed;

        wingInput = Mathf.Clamp01(wingAction.ReadValue<float>());
        wingsExtended = wingInput > wingExtendedThreshold;

        springInput = Mathf.Clamp01(springAction.ReadValue<float>());
        UpdateSpringLength();

        // transform.Rotate(
        //     yaw * playerController.invertPitch * Time.unscaledDeltaTime,
        //     roll * playerController.invertYaw * Time.unscaledDeltaTime,
        //     rudder * 2f * Time.unscaledDeltaTime);
        if (isFling)
        {
            transform.Rotate(yaw * playerController.invertPitch * Time.unscaledDeltaTime, -roll * playerController.invertYaw * Time.unscaledDeltaTime, rudder * 2f * Time.unscaledDeltaTime);
        }
        else
        {
            ApplyCameraRelativeRotation(yaw * playerController.invertPitch, roll * playerController.invertYaw, rudder * 2f);
        }
    }

    private void UpdateSpringLength()
    {
        float clampedMinimumLength = Mathf.Min(minimumRestLength, originalRestLength);

        if (springAction.IsPressed())
        {
            restLength = Mathf.MoveTowards(
                restLength,
                clampedMinimumLength,
                restLengthChangeSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            // The real player restores its target spring length on release.
            restLength = originalRestLength;
        }

        springCompression = Mathf.InverseLerp(
            originalRestLength,
            clampedMinimumLength,
            restLength);
    }

    private void ApplyCameraRelativeRotation(float pitchAmount, float rollAmount, float yawAmount)
    {
        if (cameraTransform == null) return;

        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;

        transform.Rotate(camRight, pitchAmount * Time.unscaledDeltaTime, Space.World); // nose up/down as seen on screen 
        transform.Rotate(0, rollAmount * Time.unscaledDeltaTime, 0); // banks in the screen plane 
        transform.Rotate(camForward, yawAmount * Time.unscaledDeltaTime, Space.World); // swings left/right as seen on screen


    }
}
