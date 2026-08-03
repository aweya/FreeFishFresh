using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public LevelCheckPoints levelCheckPoints;
    public PlayerController playerController;

    public TMP_Text timerText;
    public TMP_Text statusText;
    public Image tachoNadel;

    [Header("Tachometer")]
    public float minSpeed = 0f;
    public float maxSpeed = 120f;
    public float minNeedleAngle = -130f;
    public float maxNeedleAngle = 130f;
    public float needleSmoothSpeed = 10f;
    [Min(1)] public int tachoSteps = 4;
    public float labelRadius = 130f;

    private void Start()
    {
        CreateTachometerLabels();
    }

    void Update()
    {
        timerText.text = levelCheckPoints.raceTimer.ToString("F2");

        statusText.text = levelCheckPoints.StatusMessage;

        UpdateTachometer();
    }

    private void UpdateTachometer()
    {
        if (playerController == null || tachoNadel == null)
            return;

        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, playerController.speed);
        float targetAngle = Mathf.Lerp(maxNeedleAngle, minNeedleAngle, speedPercent);
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        RectTransform needleTransform = tachoNadel.rectTransform;
        needleTransform.localRotation = Quaternion.Lerp(
            needleTransform.localRotation,
            targetRotation,
            needleSmoothSpeed * Time.deltaTime);
    }

    private void CreateTachometerLabels()
    {
        if (timerText == null || tachoNadel == null)
            return;

        RectTransform needleTransform = tachoNadel.rectTransform;
        Transform dialParent = needleTransform.parent;

        for (int step = 0; step <= tachoSteps; step++)
        {
            float percent = step / (float)tachoSteps;
            float speedValue = Mathf.Lerp(minSpeed, maxSpeed, percent);
            float angle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, percent);
            float radians = angle * Mathf.Deg2Rad;

            TMP_Text label = Instantiate(timerText, dialParent);
            label.name = $"Tacho Label {step}";
            label.text = speedValue.ToString("0");
            label.alignment = TextAlignmentOptions.Center;

            RectTransform labelTransform = label.rectTransform;
            labelTransform.anchorMin = new Vector2(0.5f, 0.5f);
            labelTransform.anchorMax = new Vector2(0.5f, 0.5f);
            labelTransform.pivot = new Vector2(0.5f, 0.5f);

            Vector2 direction = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
            labelTransform.anchoredPosition = needleTransform.anchoredPosition + direction * labelRadius;
            labelTransform.localRotation = Quaternion.identity;
        }
    }
}
