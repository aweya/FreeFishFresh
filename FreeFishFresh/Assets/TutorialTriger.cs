using UnityEngine;

public class TutorialTriger : MonoBehaviour
{
    public bool movableTutorial = false;
    public GameObject tutorialPanel;
    public bool triggerOnce = true;

    bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnce && triggered)
            return;

        triggered = true;

        TutorialManager.Instance.ShowTutorial(tutorialPanel, movableTutorial);
    }
}
