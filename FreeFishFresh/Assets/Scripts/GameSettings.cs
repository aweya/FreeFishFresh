using UnityEngine;

/// <summary>
/// Owns player preferences that persist across scenes and game launches.
/// Other scripts read these values but do not save them themselves.
/// </summary>
public class GameSettings : MonoBehaviour
{
    private const string InvertYawKey = "settings.invertYaw";
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

    public bool InvertYaw { get; private set; }
    public event System.Action<bool> InvertYawChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        // Preserve the project's existing yaw direction (-1) until a player chooses otherwise.
        InvertYaw = PlayerPrefs.GetInt(InvertYawKey, 1) == 1;
    }

    public void ToggleInvertYaw()
    {
        SetInvertYaw(!InvertYaw);
    }

    public void SetInvertYaw(bool enabled)
    {
        if (InvertYaw == enabled)
            return;

        InvertYaw = enabled;
        PlayerPrefs.SetInt(InvertYawKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        InvertYawChanged?.Invoke(InvertYaw);
    }
}
