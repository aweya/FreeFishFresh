using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuRoot;

    public GameObject pauseMain;
    public GameObject debugMenu;
    public GameObject checkpointMenu;
    public bool paused = false;
    public GameObject pauseMainFirstButton;
    public GameObject debugMenuFirstButton;
    public GameObject checkpointMenuFirstButton;

    [Header("Settings")]
    private Button invertFrontBackButton;
    private Button invertLeftRightButton;
    private Button invertFlyingUpDownButton;
    private TMP_Text invertFrontBackLabel;
    private TMP_Text invertLeftRightLabel;
    private TMP_Text invertFlyingUpDownLabel;
    [Header("Chekpoint Buttons")]



    public Transform checkpointContent;
    public Button checkpointButtonPrefab;

    [Header("Debug")]
    public LevelCheckPoints levelCheckPoints;


    public PlayerController playercontroller;


    void Awake()
    {
        Cursor.visible = true;
        pauseMenuRoot.SetActive(paused);

        FindSettingsButtonsByText();
        AddSettingsButtonListeners();
        RefreshSettingsLabels();
    }

    void Update()
    {
        InputAction menuAction = playercontroller.playerInput.currentActionMap?.FindAction("Menu");
        if (menuAction != null && menuAction.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        paused = !paused;

        pauseMenuRoot.SetActive(paused);
        ShowMenu(pauseMain, pauseMainFirstButton);

        if (paused)
        {
            playercontroller.SetGamePaused(true);

            playercontroller.playerInput.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playercontroller.SetGamePaused(false);

            playercontroller.playerInput.SwitchCurrentActionMap("Fish");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    void ShowMenu(GameObject menu, GameObject firstButton)
    {
        pauseMain.SetActive(false);
        debugMenu.SetActive(false);
        checkpointMenu.SetActive(false);

        menu.SetActive(true);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (firstButton != null)
                EventSystem.current.SetSelectedGameObject(firstButton);
        }
    }

    void BuildCheckpointButtons()
    {
        // Delete old buttons
        foreach (Transform child in checkpointContent)
        {
            Destroy(child.gameObject);
        }

        // Create one button per checkpoint
        for (int i = 0; i < levelCheckPoints.checkpoints.Count; i++)
        {
            int checkpointIndex = i;

            Button button = Instantiate(checkpointButtonPrefab, checkpointContent);

            button.GetComponentInChildren<TMP_Text>().text = "Checkpoint " + (i + 1);

            button.onClick.AddListener(() =>
            {
                SelectCheckpoint(checkpointIndex);
            });
        }
    }

    public void SelectCheckpoint(int index)
    {
        Debug.Log($"Selected checkpoint {index}");

        CheckPontSingle checkpoint = levelCheckPoints.checkpoints[index];
        levelCheckPoints.CheckPointTriggered(checkpoint, true);
        playercontroller.ResetGlider();
        TogglePause();
    }


    //-Buttons--
    public void Resume()
    {
        TogglePause();
    }
    public void OpenDebug()
    {
        ShowMenu(debugMenu, debugMenuFirstButton);
    }


    public void OpenCheckpointMenu()
    {
        BuildCheckpointButtons();
        ShowMenu(checkpointMenu, checkpointMenuFirstButton);
    }

    public void BackToPause()
    {
        ShowMenu(pauseMain, pauseMainFirstButton);
    }

    public void BackToDebug()
    {
        ShowMenu(debugMenu, debugMenuFirstButton);
    }

    public void ToggleInvertFrontBackSetting()
    {
        GameSettings.Instance.ToggleInvertFrontBack();
        RefreshSettingsLabels();
    }

    public void ToggleInvertLeftRightSetting()
    {
        GameSettings.Instance.ToggleInvertLeftRight();
        RefreshSettingsLabels();
    }

    public void ToggleInvertFlyingUpDownSetting()
    {
        GameSettings.Instance.ToggleInvertFlyingUpDown();
        RefreshSettingsLabels();
    }

    private void FindSettingsButtonsByText()
    {
        if (pauseMain == null)
            return;

        foreach (Button button in pauseMain.GetComponentsInChildren<Button>(true))
        {
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
                continue;

            string buttonText = label.text.Trim();
            if (buttonText.StartsWith("Invert Front/Back", System.StringComparison.OrdinalIgnoreCase)
                || buttonText.StartsWith("Invert Yaw", System.StringComparison.OrdinalIgnoreCase))
            {
                invertFrontBackButton = button;
                invertFrontBackLabel = label;
            }
            else if (buttonText.StartsWith("Invert Left/Right", System.StringComparison.OrdinalIgnoreCase))
            {
                invertLeftRightButton = button;
                invertLeftRightLabel = label;
            }
            else if (buttonText.StartsWith("Invert Up/Down While Flying", System.StringComparison.OrdinalIgnoreCase))
            {
                invertFlyingUpDownButton = button;
                invertFlyingUpDownLabel = label;
            }
        }
    }

    private void AddSettingsButtonListeners()
    {
        if (invertFrontBackButton != null)
            invertFrontBackButton.onClick.AddListener(ToggleInvertFrontBackSetting);
        if (invertLeftRightButton != null)
            invertLeftRightButton.onClick.AddListener(ToggleInvertLeftRightSetting);
        if (invertFlyingUpDownButton != null)
            invertFlyingUpDownButton.onClick.AddListener(ToggleInvertFlyingUpDownSetting);
    }

    private void RefreshSettingsLabels()
    {
        GameSettings settings = GameSettings.Instance;

        if (invertFrontBackLabel != null)
            invertFrontBackLabel.text = "Invert Front/Back: " + OnOff(settings.InvertFrontBack);
        if (invertLeftRightLabel != null)
            invertLeftRightLabel.text = "Invert Left/Right: " + OnOff(settings.InvertLeftRight);
        if (invertFlyingUpDownLabel != null)
            invertFlyingUpDownLabel.text = "Invert Up/Down While Flying: " + OnOff(settings.InvertFlyingUpDown);
    }

    private static string OnOff(bool enabled)
    {
        return enabled ? "ON" : "OFF";
    }



}
