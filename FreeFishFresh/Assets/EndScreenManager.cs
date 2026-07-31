using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreenManager : MonoBehaviour
{
    public bool active;
    public GameObject EndScreenRoot;

    public TMP_Text timeText;
    public TMP_Text bestTimeText;
    public GameObject firstButton;

    public LevelCheckPoints level;


    void Start()
    {
        active = false;
        ToggleMenu(active);

    }

    public void ShowEndScreen(float finishTime)
    {
        active = true;
        ToggleMenu(active);


        Cursor.lockState = CursorLockMode.None;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);

        timeText.text = "Your time was " + finishTime.ToString("F2") + " s";
    }

    private void ToggleMenu(bool active)
    {
        EndScreenRoot.SetActive(active);
        Cursor.visible = active;


    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}