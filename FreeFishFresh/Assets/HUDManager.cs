using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public LevelCheckPoints levelCheckPoints;

    public TMP_Text timerText;
    public TMP_Text statusText;

    void Update()
    {
        timerText.text = levelCheckPoints.raceTimer.ToString("F2");

        statusText.text = levelCheckPoints.StatusMessage;
    }
}
