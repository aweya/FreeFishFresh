using UnityEngine;

public class TutorialPlayerControlller : MonoBehaviour
{
    [Header("Refrences")]
    public PlayerController playerController;

    [Header("controll")]
    public float rotSpeed = 3f;
    public float yaw;
    public float roll;
    public float rudder;
    private Rigidbody rb;

    //inputs


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        yaw = playerController.yawInput * rotSpeed;
        roll = playerController.rollInput * rotSpeed;
        rudder = playerController.rudderInput * rotSpeed;

        // trueYaw* invertPitch, trueRoll *invertYaw, trueRudder *2¨

        transform.Rotate(yaw * Time.unscaledTime, roll * Time.unscaledTime, rudder * Time.unscaledTime);
    }

    /*        private void ApplyCameraRelativeRotation(float pitchAmount, float rollAmount, float yawAmount)
        {
            if (cameraTransform == null) return;

            Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;

            transform.Rotate(camRight, pitchAmount, Space.World); // nose up/down as seen on screen 
            transform.Rotate(0, rollAmount, 0); // banks in the screen plane 
            transform.Rotate(camForward, yawAmount, Space.World); // swings left/right as seen on screen


        } */
}
