using UnityEngine;

public class DebugVisualizer : MonoBehaviour
{
    public Rigidbody rb;
    public Transform gliderTransform; // Reference to the glider's transform
    public float liftMultiplier = 1f; // Multiplier for lift force visualization
    public float debugArrowScale = 1f; // Scale of the lift arrow
    public Vector3 arrowOffset = Vector3.zero; // Offset for better visibility

    private void OnDrawGizmos()
    {
        if (rb == null || gliderTransform == null) return;

        // Calculate airflow (opposite to velocity direction)
        Vector3 airflow = rb.linearVelocity.normalized;

        // Calculate Angle of Attack (AoA)
        float angleOfAttack = Vector3.SignedAngle(gliderTransform.up, airflow, gliderTransform.right);

        // Calculate lift
        Vector3 liftDirection = Vector3.Cross(airflow, -gliderTransform.right).normalized; // Lift is perpendicular to airflow
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, gliderTransform.forward); // Forward speed
        Vector3 lift = liftDirection * forwardSpeed * liftMultiplier;

        // Draw lift arrow
        Gizmos.color = Color.green;
        Vector3 startPosition = transform.position + arrowOffset;
        Vector3 endPosition = startPosition + lift * debugArrowScale;
        Gizmos.DrawLine(startPosition, endPosition);
        Gizmos.DrawSphere(endPosition, 0.1f);

        // Display AoA as text in the Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPosition, startPosition + airflow * 2f); // Show airflow direction for clarity
        Debug.Log($"AoA: {angleOfAttack:F1}° | Lift: {lift.magnitude:F1}");
    }
}
