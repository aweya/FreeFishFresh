using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the authored main-menu UI. All panels, buttons, layout groups, and
/// the level Scroll View live in the MainMenu scene and are editable in the Inspector.
/// </summary>
public class MainMenu : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject levelsPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Default Selection")]
    public Button mainFirstButton;
    public Button levelsFirstButton;
    public Button settingsFirstButton;
    public Button creditsFirstButton;

    [Header("Level Select")]
    public int startSceneBuildIndex = 1;
    public ScrollRect levelScrollRect;
    public RectTransform levelContent;

    [Header("Settings Labels")]
    public TMP_Text invertFrontBackLabel;
    public TMP_Text invertLeftRightLabel;
    public TMP_Text invertFlyingUpDownLabel;

    private GameObject lastLevelSelection;
    private bool loadingScene;

    private void Awake()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshSettingsLabels();
        ShowMainPanel();
    }

    private void Update()
    {
        bool cancelPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        cancelPressed |= Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

        if (cancelPressed && mainPanel != null && !mainPanel.activeSelf)
            ShowMainPanel();

        KeepSelectedLevelVisible();
    }

    public void ShowMainPanel()
    {
        ShowOnly(mainPanel, mainFirstButton);
    }

    public void OpenLevels()
    {
        ShowOnly(levelsPanel, levelsFirstButton);
        if (levelScrollRect != null)
            levelScrollRect.verticalNormalizedPosition = 1f;
    }

    public void OpenSettings()
    {
        RefreshSettingsLabels();
        ShowOnly(settingsPanel, settingsFirstButton);
    }

    public void OpenCredits()
    {
        ShowOnly(creditsPanel, creditsFirstButton);
    }

    private void ShowOnly(GameObject panel, Button firstButton)
    {
        if (mainPanel != null)
            mainPanel.SetActive(panel == mainPanel);
        if (levelsPanel != null)
            levelsPanel.SetActive(panel == levelsPanel);
        if (settingsPanel != null)
            settingsPanel.SetActive(panel == settingsPanel);
        if (creditsPanel != null)
            creditsPanel.SetActive(panel == creditsPanel);

        if (isActiveAndEnabled)
            StartCoroutine(SelectNextFrame(firstButton));
    }

    private static IEnumerator SelectNextFrame(Button button)
    {
        yield return null;
        if (button == null || EventSystem.current == null || !button.gameObject.activeInHierarchy)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private void KeepSelectedLevelVisible()
    {
        if (levelScrollRect == null || levelContent == null || levelsPanel == null
            || !levelsPanel.activeSelf || EventSystem.current == null)
            return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || selected == lastLevelSelection || !selected.transform.IsChildOf(levelContent))
            return;

        lastLevelSelection = selected;
        int itemCount = levelContent.childCount;
        if (itemCount <= 1)
            return;

        float itemPosition = selected.transform.GetSiblingIndex() / (float)(itemCount - 1);
        levelScrollRect.verticalNormalizedPosition = 1f - itemPosition;
    }

    public void StartButton()
    {
        int buildIndex = startSceneBuildIndex;
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            buildIndex = FindFirstPlayableBuildIndex();

        LoadScene(buildIndex);
    }

    public void LoadScene(int buildIndex)
    {
        if (loadingScene || buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            return;

        loadingScene = true;
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(buildIndex);
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

    public void RefreshSettingsLabels()
    {
        GameSettings settings = GameSettings.Instance;
        if (invertFrontBackLabel != null)
            invertFrontBackLabel.text = "Invert Front/Back: " + OnOff(settings.InvertFrontBack);
        if (invertLeftRightLabel != null)
            invertLeftRightLabel.text = "Invert Left/Right: " + OnOff(settings.InvertLeftRight);
        if (invertFlyingUpDownLabel != null)
            invertFlyingUpDownLabel.text = "Invert Up/Down While Flying: " + OnOff(settings.InvertFlyingUpDown);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static void LoadMainMenuScene()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int menuIndex = FindMainMenuBuildIndex();
        if (menuIndex >= 0)
            SceneManager.LoadSceneAsync(menuIndex);
        else
            Debug.LogError("No MainMenu scene (or fallback scene at build index 0) is enabled in Build Settings.");
    }

    private static int FindFirstPlayableBuildIndex()
    {
        int menuIndex = FindMainMenuBuildIndex();
        for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
        {
            if (index != menuIndex)
                return index;
        }

        return -1;
    }

    private static int FindMainMenuBuildIndex()
    {
        for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
        {
            string sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index));
            if (sceneName.Equals(MainMenuSceneName, StringComparison.OrdinalIgnoreCase))
                return index;
        }

        return SceneManager.sceneCountInBuildSettings > 0 ? 0 : -1;
    }

    private static string OnOff(bool enabled)
    {
        return enabled ? "ON" : "OFF";
    }
}
