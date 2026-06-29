using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;          // Reference to the player's Transform
    public Vector3 offset = new Vector3(0, 5, -10);  // Offset from the player's position
    public float smoothSpeed = 0.125f; // Speed of camera's smooth following

    private Vector3 lastPlayerPosition;  // Store the last position to determine movement direction
    private Vector3 currentVelocity;    // Current velocity (direction the player is moving)

    void Start()
    {
        if (player != null)
        {
            lastPlayerPosition = player.position; // Initialize last position
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Always follow the player’s position, even if they are stationary
            Vector3 desiredPosition = player.position + offset;

            // Smoothly move the camera towards the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Update the camera's position
            transform.position = smoothedPosition;

            // Optionally, make the camera always look at the player
            transform.LookAt(player);
        }
    }
}
