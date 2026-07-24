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
            playercontroller.targetTimescale = 0f;

            playercontroller.playerInput.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playercontroller.targetTimescale = 1f;

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



}