using UnityEngine;
using TMPro;

namespace UnityTV.Core
{
    /// <summary>
    /// UI Manager - handles global UI operations like notifications and screens
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Notification")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 2f;

        [Header("Game Over Screen")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverTitleText;
        [SerializeField] private TextMeshProUGUI gameOverMessageText;

        private float notificationTimer = 0f;
        private bool notificationActive = false;

        private void Awake()
        {
            // Hide UI elements initially
            if (notificationPanel) notificationPanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
        }

        private void Update()
        {
            // Handle notification auto-hide
            if (notificationActive)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f)
                {
                    HideNotification();
                }
            }
        }

        /// <summary>
        /// Show a notification message to the player
        /// </summary>
        public void ShowNotification(string message, float duration = -1f)
        {
            Debug.Log($"[UIManager] Notification: {message}");

            if (notificationPanel == null || notificationText == null)
            {
                // If UI not set up, just log
                return;
            }

            notificationText.text = message;
            notificationPanel.SetActive(true);

            notificationTimer = duration > 0 ? duration : notificationDuration;
            notificationActive = true;
        }

        /// <summary>
        /// Hide notification
        /// </summary>
        public void HideNotification()
        {
            if (notificationPanel) notificationPanel.SetActive(false);
            notificationActive = false;
        }

        /// <summary>
        /// Show game over screen
        /// </summary>
        public void ShowGameOverScreen(bool victory)
        {
            Debug.Log($"[UIManager] Game Over - Victory: {victory}");

            if (gameOverPanel == null)
            {
                Debug.LogWarning("Game Over panel not set up!");
                return;
            }

            gameOverPanel.SetActive(true);

            if (gameOverTitleText)
            {
                gameOverTitleText.text = victory ? "成功！" : "失败...";
            }

            if (gameOverMessageText)
            {
                if (victory)
                {
                    gameOverMessageText.text = "你成功找到了理想的工作！\n恭喜完成第一周目！";
                }
                else
                {
                    gameOverMessageText.text = "时间到了，但你还没有达到目标...\n要不要再试一次？";
                }
            }
        }

        /// <summary>
        /// Hide game over screen
        /// </summary>
        public void HideGameOverScreen()
        {
            if (gameOverPanel) gameOverPanel.SetActive(false);
        }

        /// <summary>
        /// Show loading screen (placeholder)
        /// </summary>
        public void ShowLoadingScreen()
        {
            Debug.Log("[UIManager] Showing loading screen");
            // TODO: Implement loading screen
        }

        /// <summary>
        /// Hide loading screen (placeholder)
        /// </summary>
        public void HideLoadingScreen()
        {
            Debug.Log("[UIManager] Hiding loading screen");
            // TODO: Implement loading screen
        }
    }
}