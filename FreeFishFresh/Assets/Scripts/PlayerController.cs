using System.Collections.Generic;
using JetBrains.Annotations;
//using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("misc")]
    public Transform cameraTransform;
    public Transform resetPoint;
    public float rollToCameraSpeed = 180f; // max degrees/sec
    public float rollSmoothing = 8f;
    public float speed;
    [Header("Flight")]
    public Transform Wings;
    public PlayerInput playerInput;
    private float originalXScale;
    private float originalYScale;
    public float liftMult = 1f;
    public float maxAeroSpeed = 40f;
    public float dragMult = 0.1f;
    public float staticJump = 200f;
    public float rotSpeed = 3f;
    public float sideLift;
    public float rudderStabalisation = 1;
    public float pitchStabalisation = 0f;
    public float maxControlSpeed = 30f; // speed above this stops adding extra control authority

    [Header("Spring Parameters")]
    public float originalrestlenght = 1.1f;
    public float minLenght = 1f;
    public float restLenght = 1.1f;
    public float lenghtChancheSpeed = 1f;
    public float springStrenght = 20f;
    public float springDamping = 20f;
    public float pogoFriction = 0.9f;
    public float maxLeanGrip = 0.6f; // how much sideways "lean" force the tip can grip before it just slips - higher = more aggressive steering

    public Transform pogoTip;
    public bool isTipGrounded = false;
    public Transform rayCenter;
    public List<Transform> suspensionRays = new List<Transform>();

    [Header("Boost")]
    public float boostForce = 1f;

    //old
    public float bounceForceMultiplier = 3;
    public float debugSpeed = 80.0f;



    //inputs
    [Header("Inputs")]
    public float rollInput;
    public float rudderInput;
    public float yawInput;
    public float wingInput;
    public int resetInput;
    public int springInput;
    public float boostInput;
    private InputAction rollAction;
    private InputAction rudderAction;
    private InputAction yawAction;
    private InputAction wingAction;
    private InputAction resetAction;
    private InputAction springAction;
    private InputAction boostAction;


    public Vector3 sideLiftDirection;

    private Rigidbody rb;

    // Debug variables
    public float debugArrowScale = 10f; // Scale for lift arrow
    public Vector3 arrowOffset = Vector3.up * 2; // Offset for better visibility

    private Vector3 lift; // Store lift force
    private Vector3 airflow; // Store airflow direction

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
        boostAction = playerInput.actions[("Boost")];

    }

    void Update()
    {
        //new inputs
        // Read the current analog trigger values
        rollInput = rollAction.ReadValue<float>();
        yawInput = yawAction.ReadValue<float>();
        rudderInput = rudderAction.ReadValue<float>();
        wingInput = wingAction.ReadValue<float>();
        boostInput = boostAction.ReadValue<float>();

        if (resetAction.WasPressedThisFrame())
        {
            ResetGlider();
        }

        // chanche suspension lenght
        if (springAction.IsPressed())
        {
            if (restLenght > minLenght)
            {
                restLenght -= lenghtChancheSpeed;
            }
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

        // Wing scaling
        Wings.localScale = new Vector3(originalXScale, originalYScale, wingInput * 10);
        //ApplyAerodynamics();


        // Calculate airflow (opposite to velocity)
        airflow = rb.linearVelocity.normalized;




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
        rb.AddForce(drag);

        // Debug forces for testing
        //Debug.Log($"Lift: {lift}, Drag: {drag}");





        // ---- Rudder / pitch neutralizer (weathervaning) ----
        // Nudges the nose (transform.up) back toward the direction the glider is
        // actually moving (rb.linearVelocity), like a real fin/rudder would.
        if (speed > 0.5f) // skip when basically stationary, avoids jitter on the pogo
        {
            Vector3 flightDirection = rb.linearVelocity.normalized;

            // Angle to swing the nose toward flightDirection, measured around the
            // cockpit axis (transform.forward) -> this is the yaw/rudder correction.
            float yawError = Vector3.SignedAngle(transform.up, flightDirection, transform.forward);

            // Same idea measured around the wing axis (transform.right) -> pitch correction.
            float pitchError = Vector3.SignedAngle(transform.up, flightDirection, transform.right);

            // Only correct a fraction of the error each physics step. More wing out =
            // more weathervane authority (matches your original intent).
            float yawCorrection = yawError * wingInput * rudderStabalisation * Time.fixedDeltaTime;
            float pitchCorrection = pitchError * wingInput * pitchStabalisation * Time.fixedDeltaTime;

            transform.Rotate(pitchCorrection, 0f, yawCorrection, Space.Self);
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

        Suspension(rayCenter, pogoTip);

        //Boost
        Boost();

        // ----Character controls (rotations and thrust)

        // rb.AddForce(Vector3.up * spaceInput * speed);// debug

        //--new code---
        //limit control with airspeed
        float wingInfuelce;
        if (wingInput > 0.2)
        {
            float clampedSpeed = Mathf.Min(speed, maxControlSpeed);
            wingInfuelce = (1.2f - wingInput) * (1 + clampedSpeed / 10f);
        }
        else
        {
            wingInfuelce = 1;
            //RollTowardsCamera();
        }

        float trueYaw = (yawInput * rotSpeed) * wingInfuelce;
        float trueRoll = (rollInput * rotSpeed / 2.5f) * wingInfuelce * (1.5f - wingInput);
        float trueRudder = (rudderInput * rotSpeed / 1.5f) * wingInfuelce;



        // charcontrol lol
        if (wingInput > 0.2)
        {
            transform.Rotate(trueYaw, -trueRudder, -trueRoll);
        }
        else
        {
            ApplyCameraRelativeRotation(trueYaw, trueRoll, trueRudder);

        }



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


    }
    private void ApplyCameraRelativeRotation(float pitchAmount, float rollAmount, float yawAmount)
    {
        if (cameraTransform == null) return;

        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;

        transform.Rotate(camRight, pitchAmount, Space.World);   // nose up/down as seen on screen
        transform.Rotate(0, rollAmount, 0); // banks in the screen plane
        transform.Rotate(camForward, yawAmount, Space.World);   // swings left/right as seen on screen
    }
    private void RollTowardsCamera()
    {
        if (cameraTransform == null) return;

        Vector3 rollAxis = -transform.up;

        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, rollAxis);
        if (camForward.sqrMagnitude < 0.0001f) return;

        camForward.Normalize();

        float rollError = Vector3.SignedAngle(transform.forward, camForward, rollAxis);

        // Step shrinks as rollError shrinks, so it eases to a stop instead of chattering.
        float easedStep = rollError * (1f - Mathf.Exp(-rollSmoothing * Time.fixedDeltaTime));
        float maxStep = rollToCameraSpeed * Time.fixedDeltaTime;
        float rollStep = Mathf.Clamp(easedStep, -maxStep, maxStep);

        transform.Rotate(0f, rollStep, 0f, Space.Self);
    }


    private void Suspension(Transform rayPos, Transform pogoTip)
    {
        if (Physics.Raycast(rayPos.position, -transform.up, out RaycastHit hit, restLenght))
        {
            if (hit.collider.isTrigger == false)
            {
                //----Suspension----
                isTipGrounded = true;
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

                //calculate direction
                Vector3 forceDirection = (hit.normal + transform.up).normalized;

                //apply SupensionForces
                rb.AddForceAtPosition(forceDirection * springForce, rayPos.position + transform.up * 1, ForceMode.Force);

                // Add friction to pogo
                //float pogoFriction = 0.9f;
                Vector3 pogoVel = rb.GetPointVelocity(hit.point);


                float xSpeed = (Vector3.Dot(pogoVel, rayPos.right));
                float ySpeed = (Vector3.Dot(pogoVel, rayPos.up));
                float zSpeed = (Vector3.Dot(pogoVel, rayPos.forward));

                float xSlip = xSpeed * pogoFriction;
                //float yslip = 0;
                // is this the same as spring dampening? yes!
                float zslip = zSpeed * pogoFriction;

                // apply Frictionforces
                rb.AddForceAtPosition(new Vector3(-xSlip, 0, -zslip), hit.point, ForceMode.Force);

                // move tip
                pogoTip.transform.position = hit.point;
            }
        }
        else
        {
            isTipGrounded = false;
            // move tip
            pogoTip.transform.position = rayPos.position - (transform.up * restLenght);
            //Ray gismo for supesion
            Debug.DrawRay(rayPos.position, -transform.up * restLenght, Color.red);
        }
    }

    private void Boost()
    {
        //apply force
        rb.AddForceAtPosition(transform.up * boostInput * boostForce, transform.position, ForceMode.Force);
        //Debug.Log("boosIput"+boostInput);
    }
    private void ApplyAerodynamics()
    {
        if (speed < 0.1f) return; // no meaningful airflow, avoids normalizing a zero vector

        airflow = rb.linearVelocity / speed;

        // Angle of attack: how far the airflow sits above/below the nose, measured
        // in the pitch plane (the plane the wings sweep as the nose pitches).
        Vector3 airflowInPitchPlane = Vector3.ProjectOnPlane(airflow, transform.forward);
        angleOfAttack = Vector3.SignedAngle(transform.up, airflowInPitchPlane, transform.right);

        // Cap the speed used for force magnitude, not the AoA shape - stops the numbers running away at high velocity.
        float aeroSpeed = Mathf.Min(speed, maxAeroSpeed);
        float dynamicPressure = aeroSpeed * aeroSpeed; // stand-in for 0.5 * airDensity * v^2

        // sin(2*AoA): cheap, branch-free lift curve with a built-in stall.
        float aoaRad = Mathf.Clamp(angleOfAttack, -90f, 90f) * Mathf.Deg2Rad;
        float liftCoefficient = Mathf.Sin(2f * aoaRad);

        // Perpendicular to the airflow, roughly out the top of the wing. Built off
        // transform.right, so it naturally rotates with roll - bank, and part of
        // this starts pointing sideways, which is what turns you.
        Vector3 liftDirection = Vector3.Cross(airflow, -transform.right).normalized;
        if (Vector3.Dot(liftDirection, transform.up) < 0f)
            liftDirection = -liftDirection;

        lift = liftDirection * dynamicPressure * liftCoefficient * wingInput * liftMult;
        rb.AddForce(lift);

        // Drag opposes airflow. (1 + |liftCoefficient|) is a cheap induced-drag
        // stand-in - more lift costs more drag.
        float dragCoefficient = 1f + Mathf.Abs(liftCoefficient);
        Vector3 drag = -airflow * dynamicPressure * dragCoefficient * wingInput * dragMult;
        rb.AddForce(drag);
    }

    public void ResetGlider()
    {
        // Reset position and velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = resetPoint.position; // Adjust to your desired reset position
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
