using System.Collections.Generic;
using JetBrains.Annotations;
//using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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

    [Header("Spring Parameters")]
    public float originalrestlenght = 1.1f;
    public float restLenght = 1.1f;
    public float lenghtChancheSpeed = 1f;
    public float springStrenght = 20f;
    public float springDamping = 20f;
    public float pogoFriction = 0.9f;
    public Transform pogoTip;
    public Transform rayCenter;
    public List<Transform> suspensionRays = new List<Transform>();


    //inputs
    public float rollInput;
    public float rudderInput;
    public float yawInput;
    public float wingInput;
    public int resetInput;
    public int springInput;
    private InputAction rollAction;
    private InputAction rudderAction;
    private InputAction yawAction;
    private InputAction wingAction;
    private InputAction resetAction;
    private InputAction springAction;


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
        restLenght = originalrestlenght;
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
        springAction = playerInput.actions["Spring"];


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
        // chanche suspension lenght
        if (springAction.IsPressed())
        {
            restLenght -= lenghtChancheSpeed;
            Debug.Log("c was pressed");
        }
        if (springAction.WasReleasedThisFrame())
        {
            restLenght = originalrestlenght;
            Debug.Log("C was released");
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
        //Debug.Log($"Lift: {lift}, Drag: {drag}");



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

        // ------Jump mechanic
        //old jump

        /* if (isTipGrounded)
        {
            Vector3 bounceDirection = transform.up; // Local "up" direction
            float accumulatedForce = speed;
            rb.AddForce(bounceDirection * (staticJump + accumulatedForce / 5) * bounceForceMultiplier, ForceMode.Impulse);
            isTipGrounded = false; // Prevent multiple bounces
        }
 */

        //new jump
        /*
        foreach (Transform ray in suspensionRays)
        {
            Suspension(ray);
        }
        */

        Suspension(rayCenter, pogoTip);

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
        float trueRudder = (rudderInput * rotSpeed / 2.5f) * wingInfuelce * (1.5f - wingInput);



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

    private void Suspension(Transform rayPos, Transform pogoTip)
    {
        if (Physics.Raycast(rayPos.position, -transform.up, out RaycastHit hit, restLenght))
        {
            //----Suspension----

            //calc srinng lenght
            float springLenght = hit.distance;
            Vector3 springDirection = transform.up;
            //calc Spring offset
            float offset = restLenght - springLenght;
            //calc delta (spring velocity)
            float springVelocity = Vector3.Dot(springDirection, rb.GetPointVelocity(rayPos.position));
            //calc springForce
            float springForce = (offset * springStrenght) - (springVelocity * springDamping);

            //Ray gismo for supesion
            Debug.DrawRay(rayPos.position, -transform.up * springLenght, Color.green);

            //applyForces
            rb.AddForceAtPosition(hit.normal * springForce, rayPos.position + transform.up * 1, ForceMode.Force);

            // Add friction to pogo
            //float pogoFriction = 0.9f;
            Vector3 pogoVel = rb.GetPointVelocity(hit.point);
            // where is this point at the contact or the supention base??

            float xSpeed = (Vector3.Dot(pogoVel, rayPos.right));
            float ySpeed = (Vector3.Dot(pogoVel, rayPos.up));
            float zSpeed = (Vector3.Dot(pogoVel, rayPos.forward));

            float xSlip = xSpeed * pogoFriction;
            float yslip = 0;
            // is this the same as spring dampening?
            float zslip = zSpeed * pogoFriction;

            // apply forces
            rb.AddForceAtPosition(new Vector3(-xSlip, -yslip, -zslip), hit.point, ForceMode.Force);

            // move tip
            pogoTip.transform.position = hit.point;

        }
        else
        {
            // move tip
            pogoTip.transform.position = rayPos.position - (transform.up * restLenght);
        }
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
