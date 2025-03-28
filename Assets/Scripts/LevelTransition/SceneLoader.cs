using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public event Action <float> OnProgressUpdated;
    public event Action OnSceneReady;

    [Tooltip("This is used only for the loading to level")]
    [SerializeField] private float fakeLoadingSpeed = 0.6f;

    [Tooltip("This is used for loading to menus")]
    [SerializeField] private float fakeLoadingMenu = 0.2f;

    public void LoadScene(string sceneName)
    {
        // Debug.Log("Loading scene: " + sceneName);
        StartCoroutine(LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        var loadingSpeed = sceneName.StartsWith("Day") ? fakeLoadingSpeed : fakeLoadingMenu;
        
        var loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null) yield break;
        
        loadOperation.allowSceneActivation = false;

        var fakeProgress = 0f;
        while (fakeProgress < 1f)
        {
            fakeProgress += Time.unscaledDeltaTime * loadingSpeed;

            var actualProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            fakeProgress = Mathf.Min(fakeProgress, actualProgress);

            OnProgressUpdated?.Invoke(fakeProgress);
            yield return null;
        }

        loadOperation.allowSceneActivation = true;
        OnSceneReady?.Invoke();
    }
}
