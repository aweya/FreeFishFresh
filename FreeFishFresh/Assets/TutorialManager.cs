using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public bool paused = false;

    public GameObject tutorialRoot;
    public PlayerController playerController;


    GameObject currentTutorial;

    void Awake()
    {
        Instance = this;

        tutorialRoot.SetActive(false);
    }

    public void TogglePause()
    {
        paused = !paused;

        tutorialRoot.SetActive(paused);

        if (paused)
        {
            playerController.SetGamePaused(true);

            playerController.playerInput.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playerController.SetGamePaused(false);

            playerController.playerInput.SwitchCurrentActionMap("Fish");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ShowTutorial(GameObject tutorial)
    {
        //tutorialRoot.SetActive(true);
        TogglePause();
        // Hide previous tutorial
        if (currentTutorial != null)
            currentTutorial.SetActive(false);

        currentTutorial = tutorial;
        currentTutorial.SetActive(true);

        // Automatically find the Continue button
        Button button = currentTutorial.GetComponentInChildren<Button>(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);


    }

    public void ContinueTutorial()
    {
        currentTutorial.SetActive(false);
        // tutorialRoot.SetActive(false);

        TogglePause();
    }
}
