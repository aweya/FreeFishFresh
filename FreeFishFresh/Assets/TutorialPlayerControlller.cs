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
    private Rigidbody rb;

    private InputAction yawAction;
    private InputAction rollAction;
    private InputAction rudderAction;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        PlayerInput input = playerController.playerInput;
        if (input == null)
            input = playerController.GetComponent<PlayerInput>();

        yawAction = input.actions.FindAction("TutPreview/Yaw", true);
        rollAction = input.actions.FindAction("TutPreview/Roll", true);
        rudderAction = input.actions.FindAction("TutPreview/Rudder", true);
    }

    void Update()
    {
        yaw = yawAction.ReadValue<float>() * rotSpeed;
        roll = rollAction.ReadValue<float>() * rotSpeed;
        rudder = rudderAction.ReadValue<float>() * rotSpeed;



        // transform.Rotate(
        //     yaw * playerController.invertPitch * Time.unscaledDeltaTime,
        //     roll * playerController.invertYaw * Time.unscaledDeltaTime,
        //     rudder * 2f * Time.unscaledDeltaTime);
        if (isFling)
        {
            transform.Rotate(yaw * playerController.invertPitch * Time.unscaledDeltaTime, -roll * playerController.invertYaw * Time.unscaledDeltaTime, rudder * 2f * Time.unscaledDeltaTime);
            //extend the wing 
        }
        else
        {
            ApplyCameraRelativeRotation(yaw * playerController.invertPitch, roll * playerController.invertYaw, rudder * 2f);
        }
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
