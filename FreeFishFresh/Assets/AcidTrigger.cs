using UnityEngine;

public class AcidTrigger : MonoBehaviour
{
    public PlayerController playerController;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController.ResetGlider();
        }
    }
}
