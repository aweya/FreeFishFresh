using UnityEngine;

public class CheckPontSingle : MonoBehaviour
{
    private FlagBehaviour flag;

    [HideInInspector]
    public Transform CheckPointReset;

    [HideInInspector]
    public LevelCheckPoints level;

    void Start()
    {
        flag = GetComponentInChildren<FlagBehaviour>();
        CheckPointReset = transform.Find("CheckPointReset");
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