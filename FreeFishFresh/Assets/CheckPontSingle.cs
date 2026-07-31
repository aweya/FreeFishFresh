using UnityEngine;

public class CheckPontSingle : MonoBehaviour
{
    public FlagBehaviour flag;


    public Transform CheckPointReset;

    [HideInInspector]
    public LevelCheckPoints level;


    void Start()
    {

        // CheckPointReset = transform.Find("CheckPointReset");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            level.CheckPointTriggered(this, false);
        }
    }

    public void Activate()
    {
        Debug.Log(name + " triggered");
        flag.ChangeColor();
    }
}