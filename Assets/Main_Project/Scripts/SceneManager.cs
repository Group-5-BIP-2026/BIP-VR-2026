using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to be loaded when the object is selected (must be specified in Build Settings).")]
    [SerializeField]
    private string targetSceneName;

    [Tooltip("Optional: small delay before scene change in seconds.")]
    [SerializeField]
    private float loadDelaySeconds = 0f;

    private bool isLoading;

    // Diese Methode im Inspector auf das Grab-Select Event legen.
    public void LoadTargetSceneOnGrab()
    {
        if (isLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError($"{nameof(SceneManager)}: targetSceneName is empty.", this);
            return;
        }

        isLoading = true;

        if (loadDelaySeconds > 0f)
        {
            Invoke(nameof(LoadTargetSceneNow), loadDelaySeconds);
            return;
        }

        LoadTargetSceneNow();
    }

    // Hilfsmethode, falls du den Wechsel direkt per Button/Test ausloesen willst.
    public void LoadTargetSceneNow()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
    }
}
