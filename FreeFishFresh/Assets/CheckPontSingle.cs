using UnityEngine;

public class CheckPontSingle : MonoBehaviour
{
    private FlagBehaviour Flag;
    void Start()
    {
        Flag = GetComponentInChildren<FlagBehaviour>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("triggered");
            Flag.ChangeColor();
        }
    }
}
