using UnityEngine;

public class FlagBehaviour : MonoBehaviour
{
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void ChangeColor()
    {
        rend.material.color = Color.green;
    }
}