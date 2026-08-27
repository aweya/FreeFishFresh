using UnityEngine;

/// <summary>
/// Owns player preferences that persist across scenes and game launches.
/// Other scripts read these values but do not save them themselves.
/// </summary>
public class GameSettings : MonoBehaviour
{
    private const string InvertFrontBackKey = "settings.invertFrontBack";
    private const string InvertLeftRightKey = "settings.invertLeftRight";
    private const string InvertFlyingUpDownKey = "settings.invertFlyingUpDown";
    private const string LegacyInvertPitchKey = "settings.invertPitch";
    private const string LegacyInvertYawKey = "settings.invertYaw";
    private static GameSettings instance;

    public static GameSettings Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindFirstObjectByType<GameSettings>();
            if (instance != null)
                return instance;

            GameObject settingsObject = new GameObject("GameSettings");
            instance = settingsObject.AddComponent<GameSettings>();
            return instance;
        }
    }

    public bool InvertFrontBack { get; private set; }
    public bool InvertLeftRight { get; private set; }
    public bool InvertFlyingUpDown { get; private set; }

    public event System.Action SettingsChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        // OFF preserves each scene's authored control directions; ON reverses them.
        InvertFrontBack = LoadFrontBackSetting();
        InvertLeftRight = PlayerPrefs.GetInt(InvertLeftRightKey, 0) == 1;
        InvertFlyingUpDown = PlayerPrefs.GetInt(InvertFlyingUpDownKey, 0) == 1;
    }

    public void ToggleInvertFrontBack()
    {
        InvertFrontBack = !InvertFrontBack;
        SaveSetting(InvertFrontBackKey, InvertFrontBack);
    }

    public void ToggleInvertLeftRight()
    {
        InvertLeftRight = !InvertLeftRight;
        SaveSetting(InvertLeftRightKey, InvertLeftRight);
    }

    public void ToggleInvertFlyingUpDown()
    {
        InvertFlyingUpDown = !InvertFlyingUpDown;
        SaveSetting(InvertFlyingUpDownKey, InvertFlyingUpDown);
    }

    private void SaveSetting(string key, bool enabled)
    {
        PlayerPrefs.SetInt(key, enabled ? 1 : 0);
        PlayerPrefs.Save();
        SettingsChanged?.Invoke();
    }

    private static bool LoadFrontBackSetting()
    {
        if (PlayerPrefs.HasKey(InvertFrontBackKey))
            return PlayerPrefs.GetInt(InvertFrontBackKey) == 1;
        if (PlayerPrefs.HasKey(LegacyInvertPitchKey))
            return PlayerPrefs.GetInt(LegacyInvertPitchKey) != 1;
        if (PlayerPrefs.HasKey(LegacyInvertYawKey))
            return PlayerPrefs.GetInt(LegacyInvertYawKey) == 1;

        return false;
    }
}
