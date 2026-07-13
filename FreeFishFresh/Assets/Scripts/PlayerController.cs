using JetBrains.Annotations;
//using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Transform Wings;
    public PlayerInput playerInput;
    private float originalXScale;
    private float originalYScale;
    public float liftMult = 1f;
    public float dragMult = 0.1f;
    public float staticJump = 200f;

    public float bounceForceMultiplier = 3;
    public float debugSpeed = 80.0f;
    public float rotSpeed = 3f;
    public bool isTipGrounded = false;
    public float sideLift;

    //inputs
    public float rollInput;
    public float rudderInput;
    public float yawInput;
    public float wingInput;
    public int resetInput;
    private InputAction rollAction;
    private InputAction rudderAction;
    private InputAction yawAction;
    private InputAction wingAction;
    private InputAction resetAction;


    public Vector3 sideLiftDirection;

    private Rigidbody rb;

    // Debug variables
    public float debugArrowScale = 10f; // Scale for lift arrow
    public Vector3 arrowOffset = Vector3.up * 2; // Offset for better visibility

    private Vector3 lift; // Store lift force
    private Vector3 airflow; // Store airflow direction
    private float speed;
    private float angleOfAttack; // Store AoA value

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Find the Wings object
        Wings = transform.Find("Wings");

        if (Wings != null)
        {
            // Store the original X and Y scale values
            originalXScale = Wings.localScale.x;
            originalYScale = Wings.localScale.y;
        }
        else
        {
            Debug.LogError("Wings object not found!");
        }
    }

    private void Awake()
    {
        //define Inputs
        playerInput = GetComponent<PlayerInput>();

        rollAction = playerInput.actions["Roll"];
        yawAction = playerInput.actions["Yaw"];
        rudderAction = playerInput.actions["Rudder"];
        wingAction = playerInput.actions["Wings"];
        resetAction = playerInput.actions["Reset"];


    }

    void Update()
    {
        //new inputs
        // Read the current analog trigger values
        rollInput = rollAction.ReadValue<float>();
        yawInput = yawAction.ReadValue<float>();
        rudderInput = rudderAction.ReadValue<float>();
        wingInput = wingAction.ReadValue<float>();
        if (resetAction.WasPressedThisFrame())
        {
            ResetGlider();
        }

    }

    void FixedUpdate()
    {
        speed = rb.linearVelocity.magnitude;
        // Calculate airflow (opposite to velocity)
        airflow = rb.linearVelocity.normalized;

        // Wing scaling
        Wings.localScale = new Vector3(originalXScale, originalYScale, wingInput * 10);







        // Project airflow onto YX plane
        Vector3 projectedAirflow = Vector3.ProjectOnPlane(airflow, Wings.forward);

        // Calculate AoA relative to the YX plane
        angleOfAttack = Vector3.SignedAngle(Wings.up, projectedAirflow, Wings.right);


        // Forward speed
        Vector3 forwardVelocity = Vector3.Project(rb.linearVelocity, transform.up);
        float forwardSpeed = forwardVelocity.magnitude;



        // Calculate lift direction (perpendicular to airflow)
        Vector3 liftDirection = Vector3.Cross(airflow, -transform.right).normalized;



        // Ensure liftDirection doesn't flip backward
        if (Vector3.Dot(liftDirection, transform.up) < 0)
        {
            liftDirection = -liftDirection;
        }


        //lift cals
        float optimalAoA = 15f; // AoA for max lift
        float stallAoA = 40f;   // AoA where stall begins

        float normalizedAoA = angleOfAttack / optimalAoA;
        float liftCoefficient;

        // Parabolic lift curve with stall behavior
        if (Mathf.Abs(angleOfAttack) <= stallAoA)
        {
            liftCoefficient = Mathf.Max(0.2f, 1f - Mathf.Pow(normalizedAoA, 2));
        }
        else
        {
            // Post-stall: Lift drops rapidly
            liftCoefficient = Mathf.Max(0.1f, 1f - ((Mathf.Abs(angleOfAttack) - stallAoA) / stallAoA));
        }


        //liftCoefficient=1;

        //Debug.Log(forwardSpeed );


        float helpSpeed = forwardSpeed;
        if (helpSpeed > 7f)
        {
            helpSpeed = forwardSpeed / 1.5f;
        }
        if (helpSpeed > 10f)
        {
            helpSpeed = forwardSpeed / 2f;
        }

        float helpSpeed2 = forwardSpeed;
        if (helpSpeed2 > 5f)
        {
            helpSpeed2 = forwardSpeed / 2f;
        }
        if (helpSpeed2 > 7f)
        {
            helpSpeed2 = forwardSpeed / 4f;
        }
        if (helpSpeed2 > 10f)
        {
            helpSpeed2 = forwardSpeed / 10f;
        }
        // Calculate lift force
        lift = liftDirection * helpSpeed * wingInput * liftCoefficient * liftMult;

        // Apply lift force
        rb.AddForce(lift);

        // Calculate banking lift for smoother turning
        sideLiftDirection = Vector3.Cross(transform.forward, Vector3.up).normalized;

        // Add lateral lift to aid turning at low speeds
        sideLift = wingInput * helpSpeed2 * liftCoefficient * 0.1f; // Adjust 0.3f for tuning
        rb.AddForce(sideLiftDirection * sideLift);

        // Calculate drag force (opposes airflow)
        Vector3 drag = -airflow * forwardSpeed * wingInput * dragMult;

        // Apply drag force
        // rb.AddForce(drag);

        // Debug forces for testing
        Debug.Log($"Lift: {lift}, Drag: {drag}");

        // Jump mechanic
        if (isTipGrounded)
        {
            Vector3 bounceDirection = transform.up; // Local "up" direction
            float accumulatedForce = rb.linearVelocity.magnitude;
            rb.AddForce(bounceDirection * (staticJump + accumulatedForce / 5) * bounceForceMultiplier, ForceMode.Impulse);
            isTipGrounded = false; // Prevent multiple bounces
        }

        // add a rudder nutralizer

        // Calculate the difference between the player's forward direction and the flight path
        Vector3 flightDirection = rb.linearVelocity.normalized; // Flight direction (velocity-based)
        Vector3 playerDirection = transform.forward; // Player's current forward direction

        // Calculate the angle difference (yaw) between the directions

        float rudderAngleDifference = Vector3.SignedAngle(playerDirection, flightDirection, Vector3.up);

        // Apply a smoothing factor and wing input for how fast the rudder auto-centers
        float rudderAdjustment = rudderAngleDifference * wingInput * Time.fixedDeltaTime * 0.1f; // Add Time.fixedDeltaTime for frame-rate independence
        if (rudderAngleDifference < 0f)
        {
            // Rotate the player smoothly to align with the flight path
            //transform.Rotate(0f,  0f,rudderAdjustment);
        }
        // Character controls (rotations and thrust)
        // rb.AddForce(Vector3.up * spaceInput * speed);// debug

        //--new code---
        //limit control with airspeed
        float wingInfuelce;
        if (wingInput > 0.2)
        {
            wingInfuelce = (1.2f - wingInput) * (1 + speed / 10f);
        }
        else
        {
            wingInfuelce = 1;
        }
        float trueYaw = (yawInput * rotSpeed) * wingInfuelce;
        float trueRoll = (rollInput * rotSpeed / 2f) * wingInfuelce;
        float trueRudder = (rudderInput * rotSpeed / 2f) * wingInfuelce;



        // charcontrol lol
        transform.Rotate(trueYaw, trueRoll, trueRudder);


    }

    void OnDrawGizmos()
    {
        if (rb == null) return;

        // Draw lift arrow
        Gizmos.color = Color.green;
        Vector3 startPosition = transform.position + arrowOffset;
        Vector3 endPosition = startPosition + lift * debugArrowScale;
        Gizmos.DrawLine(startPosition, endPosition);
        Gizmos.DrawSphere(endPosition, 0.1f);

        // Draw sideLift arrow
        Gizmos.color = Color.blue;
        Vector3 startPosition2 = transform.position + arrowOffset;
        Vector3 endPosition2 = startPosition2 + sideLiftDirection * debugArrowScale * sideLift;
        Gizmos.DrawLine(startPosition2, endPosition2);
        Gizmos.DrawSphere(endPosition2, 0.1f);


    }

    public void ResetGlider()
    {
        // Reset position and velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = new Vector3(416, 75, 123); // Adjust to your desired reset position
        transform.rotation = Quaternion.identity; // Reset orientation
        Debug.Log("Glider Reset");
    }

    public void ApplyBounce(float impactForce)
    {
        // Apply bounce force proportional to the impact force
        Debug.Log(impactForce);
        rb.AddForce(Vector3.up * (staticJump), ForceMode.Impulse);
    }
}
