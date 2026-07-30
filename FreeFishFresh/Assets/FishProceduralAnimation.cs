using UnityEngine;

public class FishProceduralAnimation : MonoBehaviour
{
    [Header("Existing components")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Animation Rigging target")]
    [SerializeField] private Transform bodyRigTarget;

    [Tooltip("Direction reference, normally the player or visual root.")]
    [SerializeField] private Transform referenceTransform;

    [Header("Target movement")]
    [SerializeField] private float sideBendDistance = 0.35f;
    [SerializeField] private float verticalBendDistance = 0.25f;
    [SerializeField] private float wingBendDistance = 0.3f;

    [Header("Physics reaction")]
    [SerializeField] private float velocityInfluence = 0.025f;
    [SerializeField] private float accelerationInfluence = 0.01f;
    [SerializeField] private float maximumOffset = 0.6f;

    [Header("Spring")]
    [SerializeField] private float springStrength = 45f;
    [SerializeField] private float springDamping = 7f;

    [Header("Visible automatic jiggle")]
    [SerializeField] private float jiggleAmount = 0.08f;
    [SerializeField] private float jiggleSpeed = 5f;

    private Vector3 initialLocalPosition;
    private Vector3 currentOffset;
    private Vector3 offsetVelocity;

    private Vector3 previousVelocity;
    private Vector3 acceleration;

    private void Awake()
    {
        if (player == null)
            player = GetComponentInParent<PlayerController>();

        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody>();

        if (referenceTransform == null)
            referenceTransform = transform;

        if (bodyRigTarget == null)
        {
            Debug.LogError("FishProceduralJiggle: Body Rig Target is missing.");
            enabled = false;
            return;
        }

        initialLocalPosition = bodyRigTarget.localPosition;

        if (playerRigidbody != null)
            previousVelocity = playerRigidbody.linearVelocity;
    }

    private void FixedUpdate()
    {
        if (playerRigidbody == null)
            return;

        Vector3 velocity = playerRigidbody.linearVelocity;

        acceleration =
            (velocity - previousVelocity) / Time.fixedDeltaTime;

        previousVelocity = velocity;
    }

    private void LateUpdate()
    {
        if (playerRigidbody == null)
            return;

        Vector3 localVelocity =
            referenceTransform.InverseTransformDirection(
                playerRigidbody.linearVelocity
            );

        Vector3 localAcceleration =
            referenceTransform.InverseTransformDirection(acceleration);

        float wingAmount = player != null
            ? Mathf.Clamp01(player.wingInput)
            : 0f;

        // The target moves opposite to the physical movement,
        // causing the body to lag and bend.
        float horizontalOffset =
            -localVelocity.x * velocityInfluence
            -localAcceleration.x * accelerationInfluence;

        float verticalOffset =
            -localVelocity.y * velocityInfluence
            -localAcceleration.y * accelerationInfluence;

        // Constant visible movement for testing.
        float automaticJiggle =
            Mathf.Sin(Time.time * jiggleSpeed) * jiggleAmount;

        Vector3 desiredOffset = new Vector3(
            horizontalOffset * sideBendDistance + automaticJiggle,
            verticalOffset * verticalBendDistance,
            -wingAmount * wingBendDistance
        );

        desiredOffset =
            Vector3.ClampMagnitude(desiredOffset, maximumOffset);

        // Underdamped spring: follows, overshoots and jiggles.
        Vector3 springAcceleration =
            (desiredOffset - currentOffset) * springStrength
            - offsetVelocity * springDamping;

        offsetVelocity += springAcceleration * Time.deltaTime;
        currentOffset += offsetVelocity * Time.deltaTime;

        bodyRigTarget.localPosition =
            initialLocalPosition + currentOffset;
    }

    public void AddJiggleImpulse(Vector3 localImpulse)
    {
        offsetVelocity += localImpulse;
    }
}