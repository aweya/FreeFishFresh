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
    public Button invertPitchButton;
    public TMP_Text invertYawButtonLabel;
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

        EnsureInvertYawButton();

        if (invertPitchButton != null)
            invertPitchButton.onClick.AddListener(ToggleInvertYawSetting);

        RefreshSettingsLabel();
    }

    void Update()
    {
        if (playercontroller.ESCAction.WasPressedThisFrame())
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
            playercontroller.gamePaused = paused;

            playercontroller.playerInput.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playercontroller.gamePaused = paused;

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

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);
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

    public void ToggleInvertYawSetting()
    {
        GameSettings.Instance.ToggleInvertYaw();
        RefreshSettingsLabel();
    }

    private void RefreshSettingsLabel()
    {
        if (invertYawButtonLabel != null)
            invertYawButtonLabel.text = "Invert Yaw: " + (GameSettings.Instance.InvertYaw ? "ON" : "OFF");
    }

    private void EnsureInvertYawButton()
    {
        if (invertPitchButton != null || pauseMain == null)
            return;

        Button template = pauseMain.GetComponentInChildren<Button>(true);
        if (template == null)
            return;

        GameObject buttonObject = Instantiate(template.gameObject, template.transform.parent);
        buttonObject.name = "InvertYawButton";

        invertPitchButton = buttonObject.GetComponent<Button>();
        invertPitchButton.onClick = new Button.ButtonClickedEvent();
        invertYawButtonLabel = buttonObject.GetComponentInChildren<TMP_Text>(true);

        RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
        float lowestButtonPosition = template.GetComponent<RectTransform>().anchoredPosition.y;
        foreach (Button button in pauseMain.GetComponentsInChildren<Button>(true))
            lowestButtonPosition = Mathf.Min(lowestButtonPosition, button.GetComponent<RectTransform>().anchoredPosition.y);

        buttonTransform.anchoredPosition = new Vector2(0f, lowestButtonPosition - 92f);
    }



}
