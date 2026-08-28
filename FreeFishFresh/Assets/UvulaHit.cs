using UnityEngine;

public class UvulaHit : MonoBehaviour
{
    public WhaleMouthToggle whaleMouthToggle;
    public int numberOfHits = 1;
    public float cooldownDuration = 10f;
    public float coolDown = 0f;

    private bool mouthOpened;

    private void Update()
    {
        if (coolDown >= 0)
        {
            coolDown -= Time.deltaTime;
        }

        if (!mouthOpened && numberOfHits <= 0)
        {
            mouthOpened = true;

            if (whaleMouthToggle != null)
                whaleMouthToggle.openMouth = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            ProcessHit();
        }
    }

    public void UvulaStrike()
    {
        ProcessHit();
    }

    private void ProcessHit()
    {
        if (coolDown <= 0)
        {
            numberOfHits--;
            coolDown = cooldownDuration;
        }
    }
}
