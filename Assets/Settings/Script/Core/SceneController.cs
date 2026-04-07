using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace UnityTV.Core
{
    /// <summary>
    /// Handles scene loading and transitions
    /// </summary>
    public static class SceneController
    {
        private static bool isLoading = false;

        public static event Action<string> OnSceneLoadStarted;
        public static event Action<string> OnSceneLoadCompleted;
        public static event Action<float> OnSceneLoadProgress;

        /// <summary>
        /// Load scene by name
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            if (isLoading)
            {
                Debug.LogWarning($"Already loading a scene! Ignoring request to load {sceneName}");
                return;
            }

            Debug.Log($"Loading scene: {sceneName}");
            GameManager.Instance.StartCoroutine(LoadSceneAsync(sceneName));
        }

        /// <summary>
        /// Load scene by build index
        /// </summary>
        public static void LoadScene(int sceneIndex)
        {
            if (isLoading)
            {
                Debug.LogWarning($"Already loading a scene! Ignoring request to load scene {sceneIndex}");
                return;
            }

            Debug.Log($"Loading scene index: {sceneIndex}");
            GameManager.Instance.StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        /// <summary>
        /// Reload current scene
        /// </summary>
        public static void ReloadCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            LoadScene(currentScene.name);
        }

        /// <summary>
        /// Async scene loading with progress callback
        /// </summary>
        private static IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;
            OnSceneLoadStarted?.Invoke(sceneName);

            // Optional: Show loading screen
            // LoadingScreen.Show();

            yield return new WaitForSeconds(0.1f); // Small delay for loading screen to appear

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Wait until scene is almost loaded (90%)
            while (asyncLoad.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnSceneLoadProgress?.Invoke(progress);
                yield return null;
            }

            // Scene is ready, activate it
            OnSceneLoadProgress?.Invoke(1f);
            yield return new WaitForSeconds(0.2f); // Brief pause at 100%

            asyncLoad.allowSceneActivation = true;

            // Wait for scene activation
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Optional: Hide loading screen
            // LoadingScreen.Hide();

            OnSceneLoadCompleted?.Invoke(sceneName);
            isLoading = false;

            Debug.Log($"Scene loaded: {sceneName}");
        }

        /// <summary>
        /// Async scene loading by index
        /// </summary>
        private static IEnumerator LoadSceneAsync(int sceneIndex)
        {
            isLoading = true;
            string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
            OnSceneLoadStarted?.Invoke(sceneName);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnSceneLoadProgress?.Invoke(progress);
                yield return null;
            }

            OnSceneLoadProgress?.Invoke(1f);
            yield return new WaitForSeconds(0.2f);

            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            OnSceneLoadCompleted?.Invoke(sceneName);
            isLoading = false;
        }

        /// <summary>
        /// Check if a scene exists in build settings
        /// </summary>
        public static bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get current scene name
        /// </summary>
        public static string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Get current scene index
        /// </summary>
        public static int GetCurrentSceneIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }
    }
}