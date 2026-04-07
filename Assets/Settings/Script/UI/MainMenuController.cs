using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityTV.Core;

namespace UnityTV.UI
{
    /// <summary>
    /// 主菜单控制器 - 老电视风格
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;

        [Header("UI Elements")]
        [SerializeField] private GameObject tvFrame; // 电视框架装饰
        [SerializeField] private Image logo; // 归一logo
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Effects")]
        [SerializeField] private GameObject staticEffect; // 静态噪声效果（可选）
        [SerializeField] private AudioSource menuMusic;

        private void Start()
        {
            InitializeMenu();
            SetupButtons();
            CheckSaveFile();
        }

        private void InitializeMenu()
        {
            // Play menu music if available
            if (menuMusic != null && !menuMusic.isPlaying)
            {
                menuMusic.Play();
            }

            // Show version
            if (versionText)
            {
                versionText.text = "v0.1 - 第一周目";
            }

            // Optional: Add screen shake or static effect
            if (staticEffect)
            {
                // Can add animation here
            }
        }

        private void SetupButtons()
        {
            // New Game button
            if (newGameButton)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }

            // Continue button
            if (continueButton)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            // Quit button
            if (quitButton)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void CheckSaveFile()
        {
            // Check if save file exists
            bool hasSaveFile = GameManager.Instance?.SaveManager?.HasSaveFile() ?? false;

            // Enable/disable continue button
            if (continueButton)
            {
                continueButton.interactable = hasSaveFile;

                // Visual feedback
                TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText)
                {
                    buttonText.color = hasSaveFile ? Color.white : Color.gray;
                }
            }
        }

        private void OnNewGameClicked()
        {
            Debug.Log("New Game clicked");

            // Check if save file exists
            bool hasSaveFile = GameManager.Instance?.SaveManager?.HasSaveFile() ?? false;

            if (hasSaveFile)
            {
                // Show confirmation dialog
                ShowConfirmationDialog(
                    "开始新游戏将覆盖现有存档，确定吗？",
                    () => StartNewGame(),
                    null
                );
            }
            else
            {
                StartNewGame();
            }
        }

        private void StartNewGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
            else
            {
                Debug.LogError("GameManager not found!");
            }
        }

        private void OnContinueClicked()
        {
            Debug.Log("Continue clicked");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ContinueGame();
            }
            else
            {
                Debug.LogError("GameManager not found!");
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("Quit clicked");

            ShowConfirmationDialog(
                "确定要退出游戏吗？",
                () => QuitGame(),
                null
            );
        }

        private void QuitGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
            }
        }

        private void ShowConfirmationDialog(string message, System.Action onConfirm, System.Action onCancel)
        {
            // TODO: Implement proper dialog UI
            // For now, just confirm directly (or use Unity's built-in dialog in editor)

#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("确认", message, "确定", "取消"))
            {
                onConfirm?.Invoke();
            }
            else
            {
                onCancel?.Invoke();
            }
#else
                // In build, just confirm
                onConfirm?.Invoke();
#endif
        }

        // Optional: Add keyboard shortcuts
        private void Update()
        {
            // ESC to quit
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnQuitClicked();
            }

            // Enter to start new game
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnNewGameClicked();
            }
        }
    }
}