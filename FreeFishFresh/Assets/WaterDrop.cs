using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterDrop : MonoBehaviour
{
    public GameObject waterSplashEntry;
    public GameObject waterSplashExit;

    private void OnTriggerEnter(Collider other)
    {
        SetWetState(other, true);
        PlayEffect(other, waterSplashEntry);
    }

    private void OnTriggerExit(Collider other)
    {
        SetWetState(other, false);
        PlayEffect(other, waterSplashExit);
    }

    private static void SetWetState(Collider other, bool isWet)
    {
        WaterPhysics waterPhysics = other.GetComponentInParent<WaterPhysics>();
        if (waterPhysics == null || !waterPhysics.CompareTag("Player"))
            return;

        waterPhysics.isWet = isWet;
        waterPhysics.isFirstBoost = isWet;
    }

    private void PlayEffect(Collider other, GameObject splash)
    {
        WaterPhysics waterPhysics = other.GetComponentInParent<WaterPhysics>();

        if (waterPhysics == null || !waterPhysics.CompareTag("Player"))
            return;

        //esle haha

        Vector3 point = other.ClosestPoint(transform.position);
        Vector3 normal = (point - transform.position).normalized;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);


        Instantiate(splash, point, rotation);
    }
}
