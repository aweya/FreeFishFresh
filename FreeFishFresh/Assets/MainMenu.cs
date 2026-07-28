using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public PlayerController playercontroller;
    public GameObject startButton;

    void Awake()
    {
        playercontroller.playerInput.SwitchCurrentActionMap("UI");
        EventSystem.current.SetSelectedGameObject(startButton);
    }
    public void StartButton()
    {
        SceneManager.LoadSceneAsync(1);
    }


}
