using UnityEngine;

public class DisableWings : MonoBehaviour
{
    public PlayerController playerController;

    void Start()
    {
        playerController.wingsDisabled = true;
    }


}
