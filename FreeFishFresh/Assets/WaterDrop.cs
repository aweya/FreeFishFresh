using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterDrop : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SetWetState(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        SetWetState(other, false);
    }

    private static void SetWetState(Collider other, bool isWet)
    {
        WaterPhysics waterPhysics = other.GetComponentInParent<WaterPhysics>();
        if (waterPhysics == null || !waterPhysics.CompareTag("Player"))
            return;

        waterPhysics.isWet = isWet;
    }
}
