using Unity.VisualScripting;
using UnityEngine;

public class MooveLovel : MonoBehaviour
{
    public Transform level;
    public PlayerController playerController;
    public bool wingsOpened;
    private float previousPlayerY;

    void Start()
    {
        previousPlayerY = playerController.transform.position.y;
    }

    void Update()
    {
        if (!wingsOpened)
        {
            MoveLevel();
        }
        if (playerController.wingInput > 0.7f)
        {
            wingsOpened = true;
        }
    }

    private void MoveLevel()
    {
        float deltaY = playerController.transform.position.y - previousPlayerY;

        Vector3 pos = level.position;
        pos.y += deltaY;
        level.position = pos;

        previousPlayerY = playerController.transform.position.y;
    }

}
