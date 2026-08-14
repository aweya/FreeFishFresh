using System;
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


    [Min(0f)] public float resetPenalty = 5f;
    public event System.Action<float> ResetPerformed;

    public float speed;

    [Header("Control")]
    public Transform rotationPoint;
    public float rotationPointSpeed = 0.1f;
    public Transform originalRotationPoint;
    public float torqueStrength = 100f;
    public AnimationCurve slowMoCurve;
    public float targetTimescale = 1f;
    public bool gamePaused = false;
    public int invertPitch = 1;
    public int invertYaw = 1;
    public int invertFlyingPitch = 1;
    private int defaultFrontBackDirection;
    private int defaultLeftRightDirection;
    private int defaultFlyingUpDownDirection;

    public float rotSpeed = 3f;

    [Header("Aerodynamics")]
    public float airDensity = 1.225f;
    public float area = 1f;
    public float rudderArea = 4f;
    public float bodyArea = 4f;
    public float dragModifier = 0.5f;
    public AnimationCurve liftCurve;   // CL vs angle of attack (degrees)
    public AnimationCurve dragCurve;   // CD vs angle of attack (degrees)
    public AnimationCurve rudderLiftCurve;
    public AnimationCurve rudderDragCurve;
    public float rudderAngle = 50;
    public float rudderSpeed = 1f;

    public Transform Wings;
    public Transform rudderTransform;
    public Transform body;
    public PlayerInput playerInput;
    private float originalXScale;
    private float originalYScale;
    public float rudderStabalisation = 1;
    public float pitchStabalisation = 0f;
    public float maxControlSpeed = 30f; // speed above this stops adding extra control authority

    [Header("Spring Parameters")]
    public AnimationCurve springScaleCurve;
    public float originalrestlenght = 1.1f;
    public float minLenght = 1f;
    public float restLenght = 1.1f;
    public float lenghtChancheSpeed = 1f;
    public float originalSpringStrenght = 1f;
    public float springStrenght = 20f;
    public float springboostMultiplier = 2f;
    public AnimationCurve springResetCurve;
    public float resetTime = 1f;
    public float springDamping = 20f;
    public float pogoFriction = 0.9f;
    public float maxLeanGrip = 0.6f; // how much sideways "lean" force the tip can grip before it just slips - higher = more aggressive steering

    [Header("Spring Animation")]
    [Range(0f, 1f)] public float springCompression;
    [Tooltip("The actual spring length at which animation compression reaches 1. This is the physical bottom-out, not the player pull-in length.")]
    [Min(0f)] public float fullyCompressedSpringLength = 0.1f;
    private float currentSpringLength;

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
    public float springInput;
    public float boostInput;
    private InputAction rollAction;
    private InputAction rudderAction;
    private InputAction yawAction;
    private InputAction wingAction;
    private InputAction resetAction;
    private InputAction springAction;
    private InputAction boostAction;
    public InputAction ESCAction;


    public Vector3 sideLiftDirection;

    private Rigidbody rb;

    private Vector3 lift; // Store lift force
    private Vector3 airflow; // Store airflow direction

    private float angleOfAttack; // Store AoA value

    void Start()
    {
        springStrenght = originalSpringStrenght;
        restLenght = originalrestlenght;
        currentSpringLength = restLenght;
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
        defaultFrontBackDirection = DirectionSign(invertPitch, -1);
        defaultLeftRightDirection = DirectionSign(invertYaw, 1);
        defaultFlyingUpDownDirection = DirectionSign(invertFlyingPitch, 1);
        ApplyControlSettings();

        //resetPoint.position = transform.position;
        originalRotationPoint.position = rotationPoint.position;
        //define Inputs
        playerInput = GetComponent<PlayerInput>();
        playerInput.SwitchCurrentActionMap("Fish");

        rollAction = playerInput.actions["Roll"];
        yawAction = playerInput.actions["Yaw"];
        rudderAction = playerInput.actions["Rudder"];
        wingAction = playerInput.actions["Wings"];
        resetAction = playerInput.actions["Reset"];
        springAction = playerInput.actions["Spring"];
        ESCAction = playerInput.actions["Menu"];
        boostAction = playerInput.actions[("Boost")];
    }

    private void OnEnable()
    {
        GameSettings.Instance.SettingsChanged += ApplyControlSettings;
        ApplyControlSettings();
    }

    private void OnDisable()
    {
        GameSettings.Instance.SettingsChanged -= ApplyControlSettings;
    }

    private void ApplyControlSettings()
    {
        GameSettings settings = GameSettings.Instance;
        invertPitch = ApplyInversion(defaultFrontBackDirection, settings.InvertFrontBack);
        invertYaw = ApplyInversion(defaultLeftRightDirection, settings.InvertLeftRight);
        invertFlyingPitch = ApplyInversion(defaultFlyingUpDownDirection, settings.InvertFlyingUpDown);
    }

    private static int DirectionSign(int value, int fallback)
    {
        if (value == 0)
            return fallback;

        return value < 0 ? -1 : 1;
    }

    private static int ApplyInversion(int defaultDirection, bool inverted)
    {
        return inverted ? -defaultDirection : defaultDirection;
    }

    public void SetGamePaused(bool paused)
    {
        gamePaused = paused;
        targetTimescale = paused ? 0f : 1f;
        Time.timeScale = targetTimescale;
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
        springInput = springAction.ReadValue<float>();

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
        }
        if (springAction.WasReleasedThisFrame())
        {
            restLenght = originalrestlenght;
            springStrenght = originalSpringStrenght * springboostMultiplier;
        }
        //reset strengt
        if (springStrenght > originalSpringStrenght)
        {
            springStrenght = Mathf.Lerp(springStrenght, originalrestlenght, Time.deltaTime);
        }

        //manage rotation point
        if (isTipGrounded)
        {
            rotationPoint.position = pogoTip.position;
        }
        else
        {
            if (rotationPoint.localPosition.y < originalRotationPoint.localPosition.y)
            {
                rotationPoint.position += transform.up * rotationPointSpeed;
            }
        }
        //t---ime
        if (!isTipGrounded)
        {
            //alter time
            if (gamePaused)
            {
                targetTimescale = 0;
            }
            else
            {
                targetTimescale = 1f;
            }
        }

        Time.timeScale = targetTimescale;
    }

    void FixedUpdate()
    {
        speed = rb.linearVelocity.magnitude;

        // Wing scaling
        Wings.localScale = new Vector3(originalXScale, originalYScale, wingInput * 10);


        // RudderNutrolizer();
        AeroAtTransform(rudderTransform, rudderArea);
        AeroAtTransform(body, bodyArea);

        Aerodynamics();
        //new jump

        Suspension(rayCenter, pogoTip);
        UpdateSpringCompression();

        //Boost
        Boost();

        // ----Character controls (rotations and thrust)

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
            //flight contol 
            //with rudder
            //transform.Rotate(trueYaw * invertYaw, -trueRudder, -trueRoll);
            //no rudder
            transform.Rotate(trueYaw * invertFlyingPitch, -trueRudder, 0);

            Quaternion targetRudderRotation = Quaternion.Euler(0f, 0f, trueRoll * rudderAngle);
            rudderTransform.localRotation = Quaternion.RotateTowards(
                rudderTransform.localRotation,
                targetRudderRotation,
                rudderSpeed * Time.fixedDeltaTime
            );

        }
        else
        {
            //pogo controll
            ApplyCameraRelativeRotation(trueYaw * invertPitch, trueRoll * invertYaw, trueRudder * 2);

        }



    }

    private void Aerodynamics()
    {
        // air relative to the craft, world space (no wind yet)
        Vector3 worldFlowVelocity = -rb.linearVelocity;
        // once you add wind:      worldFlowVelocity += wind;
        // once you go per-surface: worldFlowVelocity -= Vector3.Cross(rb.angularVelocity, transform.position - rb.worldCenterOfMass);
        // Debug.DrawRay(transform.position, worldFlowVelocity, Color.red);

        Vector3 localFlowVelocity = transform.InverseTransformDirection(worldFlowVelocity);

        // your axes: Y = nose (chordwise), Z = cockpit (normal/lift), X = right (spanwise)
        // kill the spanwise component - a 2D wing model doesn't use it
        localFlowVelocity = new Vector3(0f, localFlowVelocity.y, localFlowVelocity.z);

        //Debug.DrawRay(transform.position, transform.TransformDirection(localFlowVelocity), Color.green);

        float dynamicPressure = 0.5f * airDensity * localFlowVelocity.sqrMagnitude;
        float angleOfAttack = Mathf.Atan2(localFlowVelocity.z, -localFlowVelocity.y);
        //angleOfAttack = Math.Abs(angleOfAttack);

        float liftCoefficient = liftCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);
        float dragCoefficient = dragCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);

        Vector3 dragDirection = transform.TransformDirection(localFlowVelocity.normalized);
        Vector3 liftDirection = Vector3.Cross(dragDirection, transform.right); // right = spanwise

        lift = liftDirection * liftCoefficient * dynamicPressure * area; // you already have `lift` as a field for the gizmo
        Vector3 drag = dragDirection * dragCoefficient * dynamicPressure * area * dragModifier;

        Vector3 combinedForces = (lift + drag) * wingInput;

        rb.AddForce(combinedForces); // plain AddForce, not AddForceAtPosition — keeps this translation-only until you add torque on purpose

        // Debug.DrawRay(transform.position, drag * wingInput, Color.red);
        // Debug.DrawRay(transform.position, lift * wingInput, Color.green);

    }

    private void AeroAtTransform(Transform aeroSurface, float area)
    {
        // air relative to the craft, world space (no wind yet)
        Vector3 worldFlowVelocity = -rb.linearVelocity;
        // once you add wind:      worldFlowVelocity += wind;
        // once you go per-surface: worldFlowVelocity -= Vector3.Cross(rb.angularVelocity, transform.position - rb.worldCenterOfMass);
        // Debug.DrawRay(transform.position, worldFlowVelocity, Color.red);

        Vector3 localFlowVelocity = aeroSurface.InverseTransformDirection(worldFlowVelocity);

        // your axes: Y = nose (chordwise), x = cockpit (normal/lift), z = right (spanwise)
        // kill the spanwise component - a 2D wing model doesn't use it
        localFlowVelocity = new Vector3(localFlowVelocity.x, localFlowVelocity.y, 0);



        float dynamicPressure = 0.5f * airDensity * localFlowVelocity.sqrMagnitude;
        float angleOfAttack = Mathf.Atan2(-localFlowVelocity.x, -localFlowVelocity.y);


        float liftCoefficient = rudderLiftCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);
        float dragCoefficient = rudderDragCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);

        Vector3 dragDirection = aeroSurface.TransformDirection(localFlowVelocity.normalized);
        Vector3 liftDirection = Vector3.Cross(dragDirection, aeroSurface.forward); // right = spanwise

        Vector3 rudderLift = liftDirection * liftCoefficient * dynamicPressure * area; // you already have `lift` as a field for the gizmo
        //Vector3 drag = dragDirection * dragCoefficient * dynamicPressure * area * dragModifier;
        Vector3 drag = Vector3.zero;

        Vector3 combinedForces = (rudderLift + drag) * wingInput;

        rb.AddForceAtPosition(combinedForces, aeroSurface.position); // plain AddForce, not AddForceAtPosition — keeps this translation-only until you add torque on purpose

        Debug.DrawRay(aeroSurface.position, rudderLift, Color.green);

    }






    private void ApplyCameraRelativeRotation(float pitchAmount, float rollAmount, float yawAmount)
    {
        if (cameraTransform == null) return;

        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;




        /* if (isTipGrounded == false)
        {
            transform.Rotate(camRight, pitchAmount, Space.World); // nose up/down as seen on screen 
            transform.Rotate(0, rollAmount, 0); // banks in the screen plane 
            transform.Rotate(camForward, yawAmount, Space.World); // swings left/right as seen on screen
        }
        else
        {
            // Pitch
            transform.RotateAround(pogoTip.position, camRight, pitchAmount);

            // Roll
            transform.RotateAround(pogoTip.position, transform.up, rollAmount);

            // Yaw
            transform.RotateAround(pogoTip.position, camForward, yawAmount);
        }
 */
        /* if (isTipGrounded == false)
        {
            transform.Rotate(camRight, pitchAmount, Space.World); // nose up/down as seen on screen 
            transform.Rotate(0, rollAmount, 0); // banks in the screen plane 
            transform.Rotate(camForward, yawAmount, Space.World); // swings left/right as seen on screen
        }
        else
        {
            rb.AddTorque(camRight * pitchAmount * torqueStrength, ForceMode.Force);
            rb.AddTorque(transform.up * rollAmount * torqueStrength, ForceMode.Force);
            rb.AddTorque(camForward * yawAmount * torqueStrength, ForceMode.Force);

        }
 */
        transform.Rotate(camRight, pitchAmount, Space.World); // nose up/down as seen on screen 
        transform.Rotate(0, rollAmount, 0); // banks in the screen plane 
        transform.Rotate(camForward, yawAmount, Space.World); // swings left/right as seen on screen


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
                currentSpringLength = springLenght;
                Vector3 springDirection = transform.up;
                //calc Spring offset
                float offset = restLenght - springLenght;
                //calc delta (spring velocity)
                float springVelocity = Vector3.Dot(springDirection, rb.GetPointVelocity(rayPos.position));
                //make spring nonlinear
                float nonlinearfactor = springScaleCurve.Evaluate(springCompression);
                //calc springForce
                float springForce = (offset * springStrenght * nonlinearfactor) - (springVelocity * springDamping);

                //--slomoeffect--
                if (gamePaused)
                {
                    targetTimescale = 0f;
                }
                else
                {
                    //alter time using offset
                    float offsetAmount = springLenght / restLenght;
                    float scaledOffsetAmount = slowMoCurve.Evaluate(offsetAmount);

                    targetTimescale = Mathf.Clamp(scaledOffsetAmount, 0.05f, 1f);
                }

                //Ray gismo for supesion
                Debug.DrawRay(rayPos.position, -transform.up * springLenght, Color.green);

                //calculate direction
                Vector3 forceDirection = (hit.normal + transform.up).normalized;


                // try adding proprotional stregnt to angle

                //apply SupensionForces
                rb.AddForceAtPosition(transform.up * springForce, rayPos.position + transform.up * 1, ForceMode.Force);
                Debug.DrawRay(hit.point, forceDirection * 3, Color.blue);

                // Add friction to pogo

                Vector3 pogoVel = rb.GetPointVelocity(hit.point);


                float xSpeed = Vector3.Dot(pogoVel, rayPos.right);
                float zSpeed = (Vector3.Dot(pogoVel, rayPos.forward));

                float xSlip = xSpeed * pogoFriction;
                //float yslip = 0;
                // is this the same as spring dampening? yes!
                float zslip = zSpeed * pogoFriction;

                //i wonder if this friction force needs to be applied/projected on the hit.normal plane
                //Vector3 frictionForce = (-xSlip * rayPos.right) + (-zslip * rayPos.forward);
                Vector3 tangentVel = Vector3.ProjectOnPlane(pogoVel, hit.normal); // slide velocity along the actual surface
                Vector3 frictionForce = -tangentVel * pogoFriction;

                // apply Frictionforces
                rb.AddForceAtPosition(frictionForce, hit.point, ForceMode.Force);
                Debug.DrawRay(hit.point, frictionForce / 10f, Color.gray);

                // move tip
                pogoTip.transform.position = hit.point;
            }
        }
        else
        {
            isTipGrounded = false;
            currentSpringLength = restLenght;
            // move tip
            pogoTip.transform.position = rayPos.position - (transform.up * restLenght);
            //Ray gismo for supesion
            Debug.DrawRay(rayPos.position, -transform.up * restLenght, Color.red);
        }
    }

    private void UpdateSpringCompression()
    {
        // Uses the actual spring length. Player pull-in changes that length slightly,
        // but only a real physical bottom-out reaches 1.
        springCompression = Mathf.InverseLerp(
            originalrestlenght,
            fullyCompressedSpringLength,
            currentSpringLength);
    }


    private void Boost()
    {
        //apply force
        rb.AddForceAtPosition(transform.up * boostInput * boostForce, transform.position, ForceMode.Force);
        //Debug.Log("boosIput"+boostInput);
    }


    public void ResetGlider()
    {
        targetTimescale = 1f;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.transform.position = resetPoint.position;
        rb.transform.rotation = resetPoint.rotation;

        Physics.SyncTransforms();
        ResetPerformed?.Invoke(resetPenalty);
        Debug.Log("Glider Reset");
    }


    // ----------------unsused stuff
    private void RudderNutrolizer()
    {
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
    }

    /*     private void RollTowardsCamera()
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
        } */
}
