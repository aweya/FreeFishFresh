using UnityEngine;

/// <summary>
/// Exposes the scene-loading action used by the gameplay Canvas prefab's
/// Inspector-authored Main Menu button.
/// </summary>
public sealed class ReturnToMainMenuButton : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        MainMenu.LoadMainMenuScene();
    }
}
