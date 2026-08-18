using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private const string GameplayMapName = "Fish";
    private const string UiMapName = "UI";
    private const string PreviewMapName = "TutPreview";

    public static TutorialManager Instance;
    public bool paused = false;

    public GameObject tutorialRoot;
    public PlayerController playerController;

    public bool IsOpen => paused;

    private GameObject currentTutorial;
    private bool currentTutorialIsMovable;

    private PlayerInput playerInput;
    private InputActionMap previewMap;
    private InputAction navigateAction;
    private InputAction menuAction;
    private bool navigateWasEnabled;
    private bool menuWasEnabled;

    private void Awake()
    {
        Instance = this;
        tutorialRoot.SetActive(false);
    }

    public void ShowTutorial(GameObject tutorial, bool movableTutorial)
    {
        if (tutorial == null)
        {
            Debug.LogError("Cannot show a tutorial without a tutorial panel.", this);
            return;
        }

        if (paused)
            CloseTutorial();

        CacheInputReferences();

        paused = true;
        currentTutorial = tutorial;
        currentTutorialIsMovable = movableTutorial;

        playerController.SetGamePaused(true);
        playerInput.SwitchCurrentActionMap(UiMapName);

        CaptureUiActionStates();

        // Tutorials are modal, so the separate pause menu must not also open.
        menuAction.Disable();

        if (currentTutorialIsMovable)
        {
            // Directional controls operate the preview without moving UI focus.
            navigateAction.Disable();
            previewMap.Enable();
        }

        tutorialRoot.SetActive(true);
        currentTutorial.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SelectContinueButton();
    }

    public void ContinueTutorial()
    {
        CloseTutorial();
    }

    private void CloseTutorial()
    {
        if (!paused)
            return;

        if (currentTutorial != null)
            currentTutorial.SetActive(false);

        if (previewMap != null)
            previewMap.Disable();

        RestoreActionState(navigateAction, navigateWasEnabled);
        RestoreActionState(menuAction, menuWasEnabled);

        tutorialRoot.SetActive(false);

        playerController.SetGamePaused(false);
        playerInput.SwitchCurrentActionMap(GameplayMapName);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentTutorial = null;
        currentTutorialIsMovable = false;
        paused = false;
    }

    private void CacheInputReferences()
    {
        if (playerInput != null)
            return;

        playerInput = playerController.playerInput;
        if (playerInput == null)
            playerInput = playerController.GetComponent<PlayerInput>();

        previewMap = playerInput.actions.FindActionMap(PreviewMapName, true);

        InputActionMap uiMap = playerInput.actions.FindActionMap(UiMapName, true);
        navigateAction = uiMap.FindAction("Navigate", true);
        menuAction = uiMap.FindAction("Menu", true);
    }

    private void CaptureUiActionStates()
    {
        navigateWasEnabled = navigateAction.enabled;
        menuWasEnabled = menuAction.enabled;
    }

    private static void RestoreActionState(InputAction action, bool shouldBeEnabled)
    {
        if (action == null)
            return;

        if (shouldBeEnabled)
            action.Enable();
        else
            action.Disable();
    }

    private void SelectContinueButton()
    {
        Button button = currentTutorial.GetComponentInChildren<Button>(true);
        if (button == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
