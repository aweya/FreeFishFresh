using UnityEngine;

public class Lantern : MonoBehaviour
{
    [Header("References")]
    public Transform hinge;
    public Light lightscource;

    [Header("Lantern Settings")]
    [Min(0f)]
    public float flicker = 100;

    [Min(0f)]
    public float sway = 5f;

    [SerializeField, Min(0f)] private float flickerSpeed = 8f;
    [SerializeField, Min(0f)] private float swaySpeed = 0.8f;

    private Quaternion hingeStartRotation;
    private float flickerSeed;
    private float swaySeed;

    void Awake()
    {
        if (lightscource == null)
            lightscource = GetComponentInChildren<Light>();

        if (hinge != null)
            hingeStartRotation = hinge.localRotation;

        // Different seeds stop multiple lanterns from moving in sync.
        flickerSeed = Random.Range(0f, 1000f);
        swaySeed = Random.Range(0f, 1000f);
    }

    void Update()
    {
        FlickerLight();
        SwayLantern();
    }

    // Flicker the light smoothly like a flame instead of changing it every frame.
    private void FlickerLight()
    {
        if (lightscource == null)
            return;

        float noise = Mathf.PerlinNoise(flickerSeed, Time.time * flickerSpeed);
        lightscource.intensity = Mathf.Lerp(flicker * 0.65f, flicker, noise);
    }

    // Rotate the hinge around its starting pose to imitate wind-driven swaying.
    private void SwayLantern()
    {
        if (hinge == null)
            return;

        float time = Time.time * swaySpeed;
        float forwardSway = (Mathf.PerlinNoise(swaySeed, time) * 2f - 1f) * sway;
        float sideSway = (Mathf.PerlinNoise(swaySeed + 100f, time) * 2f - 1f) * sway * 0.5f;

        hinge.localRotation = hingeStartRotation * Quaternion.Euler(forwardSway, 0f, sideSway);
    }
}
