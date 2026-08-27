using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class InvertSettingsPlayModeTests
{
    private const string ScenePath = "Assets/Scenes/Pogo Tutorial first Contact.unity";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

    private static readonly string[] PreferenceKeys =
    {
        "settings.invertFrontBack",
        "settings.invertLeftRight",
        "settings.invertFlyingUpDown",
        "settings.invertPitch",
        "settings.invertYaw"
    };

    private readonly Dictionary<string, int?> savedPreferences = new Dictionary<string, int?>();

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();

        foreach (string key in PreferenceKeys)
        {
            savedPreferences[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
            PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.Save();
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
            ScenePath,
            new LoadSceneParameters(LoadSceneMode.Single));
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // Scene shutdown can trigger unrelated tutorial/input errors while test objects are destroyed.
        LogAssert.ignoreFailingMessages = true;

        foreach (KeyValuePair<string, int?> preference in savedPreferences)
        {
            if (preference.Value.HasValue)
                PlayerPrefs.SetInt(preference.Key, preference.Value.Value);
            else
                PlayerPrefs.DeleteKey(preference.Key);
        }

        PlayerPrefs.Save();
        savedPreferences.Clear();

        System.Type controllerType = System.Type.GetType("PlayerController, Assembly-CSharp");
        foreach (Object controllerObject in Object.FindObjectsByType(
            controllerType,
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            Component controller = (Component)controllerObject;
            Object.Destroy(controller.gameObject);
        }
        yield return null;

        System.Type settingsType = System.Type.GetType("GameSettings, Assembly-CSharp");
        Component settings = Object.FindFirstObjectByType(settingsType) as Component;
        if (settings != null)
        {
            Object.Destroy(settings.gameObject);
            yield return null;
        }

        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator PauseMenuInvertButtonsReverseControllerDirections()
    {
        yield return null;

        Component controller = FindComponent("PlayerController, Assembly-CSharp");
        Component pauseMenu = FindComponent("PauseMenu, Assembly-CSharp");

        Assert.That(controller, Is.Not.Null);
        Assert.That(pauseMenu, Is.Not.Null);

        int frontBackBefore = GetIntField(controller, "invertPitch");
        int leftRightBefore = GetIntField(controller, "invertYaw");
        int flyingBefore = GetIntField(controller, "invertFlyingPitch");
        GameObject pauseMain = (GameObject)GetField(pauseMenu, "pauseMain");

        ClickButton(pauseMain, "Invert Front/Back");
        Assert.That(GetIntField(controller, "invertPitch"), Is.EqualTo(-frontBackBefore));

        ClickButton(pauseMain, "Invert Left/Right");
        Assert.That(GetIntField(controller, "invertYaw"), Is.EqualTo(-leftRightBefore));

        ClickButton(pauseMain, "Invert Up/Down While Flying");
        Assert.That(GetIntField(controller, "invertFlyingPitch"), Is.EqualTo(-flyingBefore));
    }

    [UnityTest]
    public IEnumerator MainMenuInvertButtonsPersistIntoGameplay()
    {
        yield return null;

        Component initialController = FindComponent("PlayerController, Assembly-CSharp");
        int frontBackBefore = GetIntField(initialController, "invertPitch");
        int leftRightBefore = GetIntField(initialController, "invertYaw");
        int flyingBefore = GetIntField(initialController, "invertFlyingPitch");

        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
            MainMenuScenePath,
            new LoadSceneParameters(LoadSceneMode.Single));

        Component mainMenu = FindComponent("MainMenu, Assembly-CSharp");
        GameObject settingsPanel = (GameObject)GetField(mainMenu, "settingsPanel");
        ClickButton(settingsPanel, "Invert Front/Back");
        ClickButton(settingsPanel, "Invert Left/Right");
        ClickButton(settingsPanel, "Invert Up/Down While Flying");

        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
            ScenePath,
            new LoadSceneParameters(LoadSceneMode.Single));
        yield return null;

        Component reloadedController = FindComponent("PlayerController, Assembly-CSharp");
        Assert.That(GetIntField(reloadedController, "invertPitch"), Is.EqualTo(-frontBackBefore));
        Assert.That(GetIntField(reloadedController, "invertYaw"), Is.EqualTo(-leftRightBefore));
        Assert.That(GetIntField(reloadedController, "invertFlyingPitch"), Is.EqualTo(-flyingBefore));
    }

    private static void ClickButton(GameObject root, string labelPrefix)
    {
        System.Type buttonType = System.Type.GetType("UnityEngine.UI.Button, UnityEngine.UI");
        System.Type labelType = System.Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
        Assert.That(buttonType, Is.Not.Null);
        Assert.That(labelType, Is.Not.Null);

        foreach (Component button in root.GetComponentsInChildren(buttonType, true))
        {
            Component label = button.GetComponentInChildren(labelType, true);
            string text = label == null ? null : (string)labelType.GetProperty("text").GetValue(label);
            if (text != null && text.StartsWith(labelPrefix, System.StringComparison.OrdinalIgnoreCase))
            {
                object onClick = buttonType.GetProperty("onClick").GetValue(button);
                onClick.GetType().GetMethod("Invoke").Invoke(onClick, null);
                return;
            }
        }

        Assert.Fail("Could not find button beginning with: " + labelPrefix);
    }

    private static Component FindComponent(string assemblyQualifiedTypeName)
    {
        System.Type type = System.Type.GetType(assemblyQualifiedTypeName);
        Assert.That(type, Is.Not.Null);
        return Object.FindFirstObjectByType(type) as Component;
    }

    private static int GetIntField(Component component, string fieldName)
    {
        return (int)GetField(component, fieldName);
    }

    private static object GetField(Component component, string fieldName)
    {
        FieldInfo field = component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(field, Is.Not.Null);
        return field.GetValue(component);
    }
}
