using UnityEngine;

public class CheckPontSingle : MonoBehaviour
{
    public FlagBehaviour flag;
    public Transform CheckPointReset;

    [HideInInspector]
    public LevelCheckPoints level;

    private CheckPointDecoration decoration;

    private void Awake()
    {
        decoration = GetComponent<CheckPointDecoration>();
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

        if (flag == null)
            return;

        flag.ChangeColor();

        if (decoration != null)
            decoration.SetRopeColor(flag.activeColor);
    }
}
