using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Runs the fish conversation using only the text already authored in the Canvas.
/// </summary>
public class FirstCutscene : MonoBehaviour
{
    [Header("Cutscene")]
    [SerializeField] private GameObject cutsceneCanvas;
    [SerializeField] private CinemachineCamera fishCamera;
    [SerializeField] private CinemachineCamera objectiveCamera;

    [Header("Canvas panels")]
    [SerializeField] private GameObject firstDialogPanel;
    [SerializeField] private GameObject yesReplyPanel;
    [SerializeField] private GameObject noReplyPanel;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject goodbyePanel;

    [Header("Canvas text")]
    [SerializeField] private TMP_Text firstDialogText;
    [SerializeField] private TMP_Text yesReplyText;
    [SerializeField] private TMP_Text noReplyText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text goodbyeText;

    [Header("Choice")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Timing")]
    [SerializeField, Min(0.001f)] private float secondsPerCharacter = 0.03f;
    [SerializeField, Min(0f)] private float cameraBlendDelay = 0.25f;

    [Header("Behaviour")]
    [SerializeField] private bool playOnlyOnce = true;
    [SerializeField] private int cutscenePriority = 100;

    private bool hasPlayed;
    private bool cutsceneRunning;
    private bool choiceMade;
    private bool choseYes;

    private PlayerController playerController;
    private Rigidbody playerRigidbody;
    private bool playerControllerWasEnabled;
    private bool playerWasKinematic;
    private Vector3 playerVelocity;
    private Vector3 playerAngularVelocity;
    private int fishCameraPriority;
    private int objectiveCameraPriority;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    private void Awake()
    {
        PrepareCanvas();
        yesButton.onClick.AddListener(ChooseYes);
        noButton.onClick.AddListener(ChooseNo);

        HideAllPanels();
        cutsceneCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        if (yesButton != null)
            yesButton.onClick.RemoveListener(ChooseYes);

        if (noButton != null)
            noButton.onClick.RemoveListener(ChooseNo);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryStartCutscene(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryStartCutscene(other);
    }

    private void TryStartCutscene(Collider other)
    {
        if (cutsceneRunning || (hasPlayed && playOnlyOnce))
            return;

        PlayerController enteringPlayer = other.GetComponentInParent<PlayerController>();
        if (enteringPlayer == null && other.attachedRigidbody != null)
            enteringPlayer = other.attachedRigidbody.GetComponentInParent<PlayerController>();

        if (enteringPlayer == null || !HasRequiredReferences())
            return;

        playerController = enteringPlayer;
        hasPlayed = true;
        cutsceneRunning = true;
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        PrepareCanvas();
        FreezePlayer();
        SaveCameraPriorities();
        SetCutsceneCamera(fishCamera);

        cutsceneCanvas.SetActive(true);
        SetCursorForChoice(true);
        yield return null;

        // 1. Fish speaks, then the player chooses Yes or No.
        ShowPanel(firstDialogPanel);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        yield return TypeText(firstDialogText, false);

        choiceMade = false;
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        SelectButton(yesButton);

        while (!choiceMade)
        {
            ReadChoiceShortcuts();
            yield return null;
        }

        // 2. Show the reply belonging to the chosen answer.
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        if (choseYes)
        {
            ShowPanel(yesReplyPanel);
            yield return TypeText(yesReplyText, true);
        }
        else
        {
            ShowPanel(noReplyPanel);
            yield return TypeText(noReplyText, true);
        }

        // 3. Pan to the objective and show its Canvas-authored text.
        SetCutsceneCamera(objectiveCamera);
        yield return new WaitForSecondsRealtime(cameraBlendDelay);
        ShowPanel(objectivePanel);
        yield return TypeText(objectiveText, true);

        // 4. Pan back to the fish and show the final good-luck text.
        SetCutsceneCamera(fishCamera);
        yield return new WaitForSecondsRealtime(cameraBlendDelay);
        ShowPanel(goodbyePanel);
        yield return TypeText(goodbyeText, true);

        FinishCutscene();
    }

    private IEnumerator TypeText(TMP_Text text, bool waitForAdvance)
    {
        text.gameObject.SetActive(true);
        text.maxVisibleCharacters = 0;
        text.ForceMeshUpdate();

        int characterCount = text.textInfo.characterCount;
        for (int i = 0; i < characterCount; i++)
        {
            if (AdvancePressed())
            {
                text.maxVisibleCharacters = characterCount;
                break;
            }

            text.maxVisibleCharacters = i + 1;
            yield return new WaitForSecondsRealtime(secondsPerCharacter);
        }

        text.maxVisibleCharacters = characterCount;

        if (!waitForAdvance)
            yield break;

        // Requiring a new frame prevents one press from both skipping and advancing.
        yield return null;
        while (!AdvancePressed())
            yield return null;
    }

    private void ChooseYes()
    {
        if (!cutsceneRunning || choiceMade)
            return;

        choseYes = true;
        choiceMade = true;
    }

    private void ChooseNo()
    {
        if (!cutsceneRunning || choiceMade)
            return;

        choseYes = false;
        choiceMade = true;
    }

    private void ReadChoiceShortcuts()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.yKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
                ChooseYes();
            else if (Keyboard.current.nKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                ChooseNo();
        }

        if (Gamepad.current == null)
            return;

        if (Gamepad.current.dpad.left.wasPressedThisFrame)
            ChooseYes();
        else if (Gamepad.current.dpad.right.wasPressedThisFrame)
            ChooseNo();
    }

    private static bool AdvancePressed()
    {
        return (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)) ||
               (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }

    private void ShowPanel(GameObject panel)
    {
        HideAllPanels();
        panel.SetActive(true);
    }

    private void HideAllPanels()
    {
        SetActive(firstDialogPanel, false);
        SetActive(yesReplyPanel, false);
        SetActive(noReplyPanel, false);
        SetActive(objectivePanel, false);
        SetActive(goodbyePanel, false);
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private static void SelectButton(Button button)
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private void SaveCameraPriorities()
    {
        fishCameraPriority = fishCamera.Priority;
        objectiveCameraPriority = objectiveCamera.Priority;
    }

    private void SetCutsceneCamera(CinemachineCamera activeCamera)
    {
        fishCamera.Priority = activeCamera == fishCamera ? cutscenePriority : cutscenePriority - 1;
        objectiveCamera.Priority = activeCamera == objectiveCamera ? cutscenePriority : cutscenePriority - 1;
    }

    private void RestoreCameraPriorities()
    {
        fishCamera.Priority = fishCameraPriority;
        objectiveCamera.Priority = objectiveCameraPriority;
    }

    private void FreezePlayer()
    {
        playerControllerWasEnabled = playerController.enabled;
        playerController.enabled = false;

        playerRigidbody = playerController.GetComponent<Rigidbody>();
        if (playerRigidbody == null)
            return;

        playerWasKinematic = playerRigidbody.isKinematic;
        playerVelocity = playerRigidbody.linearVelocity;
        playerAngularVelocity = playerRigidbody.angularVelocity;
        playerRigidbody.isKinematic = true;
    }

    private void UnfreezePlayer()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = playerWasKinematic;
            playerRigidbody.linearVelocity = playerVelocity;
            playerRigidbody.angularVelocity = playerAngularVelocity;
        }

        if (playerController != null)
            playerController.enabled = playerControllerWasEnabled;
    }

    private void SetCursorForChoice(bool cutsceneActive)
    {
        if (cutsceneActive)
        {
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
    }

    private void FinishCutscene()
    {
        HideAllPanels();
        cutsceneCanvas.SetActive(false);
        RestoreCameraPriorities();
        SetCursorForChoice(false);
        UnfreezePlayer();
        cutsceneRunning = false;
    }

    private void PrepareCanvas()
    {
        if (cutsceneCanvas == null)
            return;

        RectTransform canvasTransform = cutsceneCanvas.GetComponent<RectTransform>();
        if (canvasTransform != null)
            canvasTransform.localScale = Vector3.one;

        Canvas canvas = cutsceneCanvas.GetComponent<Canvas>();
        if (canvas == null)
            return;

        canvas.enabled = true;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
    }

    private bool HasRequiredReferences()
    {
        bool valid = cutsceneCanvas != null && fishCamera != null && objectiveCamera != null &&
                     firstDialogPanel != null && yesReplyPanel != null && noReplyPanel != null &&
                     objectivePanel != null && goodbyePanel != null && firstDialogText != null &&
                     yesReplyText != null && noReplyText != null && objectiveText != null &&
                     goodbyeText != null && yesButton != null && noButton != null;

        if (!valid)
            Debug.LogError("FirstCutscene is missing one or more Canvas, camera, text, or button references.", this);

        return valid;
    }
}
