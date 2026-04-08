using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityTV.Core;
using UnityTV.Player;

namespace UnityTV.Gameplay
{
    /// <summary>
    /// TV界面控制器
    /// 处理频道选择和观看
    /// </summary>
    public class TVInterfaceController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject channelSelectionPanel; // 频道选择面板
        [SerializeField] private GameObject watchingPanel; // 观看中面板
        [SerializeField] private TextMeshProUGUI channelInfoText; // 频道信息
        [SerializeField] private Image tvScreen; // 电视屏幕
        [SerializeField] private Button exitButton; // 退出按钮

        [Header("Channel Buttons")]
        [SerializeField] private Button channel1Button; // 频道I - 医学
        [SerializeField] private Button channel2Button; // 频道II - 警务
        [SerializeField] private Button channel3Button; // 频道III - 商务
        [SerializeField] private Button channel4Button; // 频道IV - 贸易
        [SerializeField] private Button channel5Button; // 频道V - 科学
        [SerializeField] private Button channel6Button; // 频道VI - 战斗

        [Header("Channel Content")]
        [SerializeField] private Sprite[] channelScreens; // 各频道的画面
        [SerializeField] private float watchDuration = 5f; // 观看时长

        [Header("Stat Gains")]
        [SerializeField] private int baseStatGain = 5; // 基础属性增长

        private int currentChannel = -1;
        private bool isWatching = false;
        private float watchTimer = 0f;

        private void Start()
        {
            Debug.Log("[TVInterface] Initializing...");

            // Check if GameManager exists
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[TVInterface] GameManager not found! Creating one for testing...");
                GameObject gmObject = new GameObject("GameManager");
                gmObject.AddComponent<GameManager>();

                // Wait for initialization
                Invoke(nameof(DelayedStart), 0.1f);
                return;
            }

            InitializeUI();
            SetupChannelButtons();
            Debug.Log("[TVInterface] Initialization complete!");
        }

        private void DelayedStart()
        {
            if (GameManager.Instance != null && GameManager.Instance.PlayerData == null)
            {
                // Create test player data
                var testData = new UnityTV.Player.PlayerData();
                testData.PlayerName = "Test Player";
                testData.InitializeFromQuestionnaire(
                    "Test Player",
                    UnityTV.Player.CareerType.Doctor,
                    UnityTV.Player.HousingType.QuietHome,
                    UnityTV.Player.FearType.Darkness,
                    false
                );
                // Don't complete character creation, just assign data directly
                typeof(GameManager).GetField("PlayerData",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                    ?.SetValue(GameManager.Instance, testData);

                Debug.Log("[TVInterface] Created test player data");
            }

            Start(); // Call Start again
        }

        private void Update()
        {
            if (isWatching)
            {
                UpdateWatching();
            }
        }

        private void InitializeUI()
        {
            // Show channel selection, hide watching panel
            if (channelSelectionPanel)
            {
                channelSelectionPanel.SetActive(true);
                Debug.Log("[TVInterface] Channel selection panel shown");
            }
            else
            {
                Debug.LogError("[TVInterface] Channel selection panel is not assigned!");
            }

            if (watchingPanel)
            {
                watchingPanel.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[TVInterface] Watching panel is not assigned!");
            }

            // Setup exit button
            if (exitButton)
            {
                exitButton.onClick.RemoveAllListeners(); // Clear old listeners
                exitButton.onClick.AddListener(ExitTV);
                Debug.Log("[TVInterface] Exit button configured");
            }
            else
            {
                Debug.LogError("[TVInterface] Exit button is not assigned!");
            }
        }

        private void SetupChannelButtons()
        {
            Debug.Log("[TVInterface] Setting up channel buttons...");

            // Channel 1 - Medical (Doctor career)
            if (channel1Button)
            {
                channel1Button.onClick.RemoveAllListeners();
                channel1Button.onClick.AddListener(() => SelectChannel(1));
                channel1Button.interactable = IsChannelUnlocked(1);
                Debug.Log($"[TVInterface] Channel 1 configured. Unlocked: {IsChannelUnlocked(1)}");
            }
            else
            {
                Debug.LogError("[TVInterface] Channel 1 button not assigned!");
            }

            // Channel 2 - Police
            if (channel2Button)
            {
                channel2Button.onClick.RemoveAllListeners();
                channel2Button.onClick.AddListener(() => SelectChannel(2));
                channel2Button.interactable = IsChannelUnlocked(2);
                Debug.Log($"[TVInterface] Channel 2 configured. Unlocked: {IsChannelUnlocked(2)}");
            }
            else
            {
                Debug.LogError("[TVInterface] Channel 2 button not assigned!");
            }

            // Channel 3 - Office
            if (channel3Button)
            {
                channel3Button.onClick.RemoveAllListeners();
                channel3Button.onClick.AddListener(() => SelectChannel(3));
                channel3Button.interactable = IsChannelUnlocked(3);
                Debug.Log($"[TVInterface] Channel 3 configured. Unlocked: {IsChannelUnlocked(3)}");
            }
            else
            {
                Debug.LogError("[TVInterface] Channel 3 button not assigned!");
            }

            // Channel 4 - Merchant
            if (channel4Button)
            {
                channel4Button.onClick.RemoveAllListeners();
                channel4Button.onClick.AddListener(() => SelectChannel(4));
                channel4Button.interactable = IsChannelUnlocked(4);
                Debug.Log($"[TVInterface] Channel 4 configured. Unlocked: {IsChannelUnlocked(4)}");
            }
            else
            {
                Debug.LogError("[TVInterface] Channel 4 button not assigned!");
            }

            // Channel 5 - Scientist (locked in first playthrough)
            if (channel5Button)
            {
                channel5Button.onClick.RemoveAllListeners();
                channel5Button.onClick.AddListener(() => SelectChannel(5));
                channel5Button.interactable = IsChannelUnlocked(5);
                Debug.Log($"[TVInterface] Channel 5 configured. Unlocked: {IsChannelUnlocked(5)}");
            }
            else
            {
                Debug.LogError("[TVInterface] Channel 5 button not assigned!");
            }

            // Channel 6 - Combat (unlocked after Anchilo)
            if (channel6Button)
            {
                channel6Button.onClick.RemoveAllListeners();
                channel6Button.onClick.AddListener(() => SelectChannel(6));
                channel6Button.interactable = IsChannelUnlocked(6);
                Debug.Log($"[TVInterface] Channel 6 configured. Unlocked: {IsChannelUnlocked(6)}");
            }
            else
            {
                Debug.LogError("[TVInterface] Channel 6 button not assigned!");
            }

            Debug.Log("[TVInterface] All channel buttons setup complete!");
        }

        private bool IsChannelUnlocked(int channelNumber)
        {
            // For testing: if no GameManager, unlock all channels
            if (GameManager.Instance?.PlayerData == null)
            {
                Debug.LogWarning($"[TVInterface] No GameManager/PlayerData - unlocking channel {channelNumber} for testing");
                return true; // Allow all channels when testing individual scene
            }

            // Channel 6 is special - unlocked after Anchilo
            if (channelNumber == 6)
            {
                bool unlocked = GameManager.Instance.PlayerData.Channel6Unlocked;
                Debug.Log($"[TVInterface] Channel 6 unlock status: {unlocked}");
                return unlocked;
            }

            // Channel 5 locked in first playthrough
            if (channelNumber == 5)
            {
                // TODO: Check if player completed first playthrough
                Debug.Log("[TVInterface] Channel 5 locked (first playthrough)");
                return false;
            }

            bool isUnlocked = GameManager.Instance.PlayerData.UnlockedChannels.Contains(channelNumber);
            Debug.Log($"[TVInterface] Channel {channelNumber} unlock status: {isUnlocked}");
            return isUnlocked;
        }

        private void SelectChannel(int channelNumber)
        {
            Debug.Log($"Selected Channel {channelNumber}");

            // Check if GameManager exists
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found! Cannot access player data.");
                return;
            }

            currentChannel = channelNumber;

            // Channel 6 is special - goes to combat scene
            if (channelNumber == 6)
            {
                GameManager.Instance.EnterCombat();
                return;
            }

            // Start watching other channels
            StartWatching(channelNumber);
        }

        private void StartWatching(int channelNumber)
        {
            // Hide selection, show watching panel
            if (channelSelectionPanel) channelSelectionPanel.SetActive(false);
            if (watchingPanel) watchingPanel.SetActive(true);

            // Update screen image
            if (tvScreen && channelScreens != null && channelNumber <= channelScreens.Length)
            {
                tvScreen.sprite = channelScreens[channelNumber - 1];
            }

            // Update info text
            UpdateChannelInfo(channelNumber);

            // Start watching
            isWatching = true;
            watchTimer = 0f;
        }

        private void UpdateChannelInfo(int channelNumber)
        {
            if (channelInfoText == null) return;

            string info = channelNumber switch
            {
                1 => "频道 I - 医学节目\n正在观看医学纪录片...\n智力 +5, 意志 +3",
                2 => "频道 II - 警务节目\n正在观看警务实录...\n身体 +5, 意志 +5",
                3 => "频道 III - 商务节目\n正在观看职场指南...\n智力 +3, 意志 +3",
                4 => "频道 IV - 行商节目\n正在观看贸易纪实...\n身体 +3, 意志 +5",
                5 => "频道 V - 科学节目\n正在观看科学前沿...\n智力 +8",
                _ => "正在观看节目..."
            };

            channelInfoText.text = info;
        }

        private void UpdateWatching()
        {
            watchTimer += Time.deltaTime;

            // Update progress (could add a progress bar here)
            float progress = watchTimer / watchDuration;

            if (watchTimer >= watchDuration)
            {
                FinishWatching();
            }
        }

        private void FinishWatching()
        {
            isWatching = false;

            // Apply stat gains based on channel
            ApplyStatGains(currentChannel);

            // Show completion message
            Debug.Log($"Finished watching Channel {currentChannel}");

            // Return to channel selection
            if (channelSelectionPanel) channelSelectionPanel.SetActive(true);
            if (watchingPanel) watchingPanel.SetActive(false);

            // Reset
            currentChannel = -1;
            watchTimer = 0f;

            // Advance time (watching takes time)
            if (GameManager.Instance?.TimeManager != null)
            {
                GameManager.Instance.TimeManager.ForceAdvanceTime();
            }
        }

        private void ApplyStatGains(int channelNumber)
        {
            if (GameManager.Instance?.PlayerData == null) return;

            switch (channelNumber)
            {
                case 1: // Medical - Intelligence + Mental Strength
                    GameManager.Instance.PlayerData.UpdateStats(
                        intelligence: baseStatGain,
                        mentalStrength: 3
                    );
                    break;

                case 2: // Police - Physical + Mental Strength
                    GameManager.Instance.PlayerData.UpdateStats(
                        physicalStrength: baseStatGain,
                        mentalStrength: baseStatGain
                    );
                    break;

                case 3: // Office - Balanced
                    GameManager.Instance.PlayerData.UpdateStats(
                        intelligence: 3,
                        mentalStrength: 3
                    );
                    break;

                case 4: // Merchant - Physical + Mental
                    GameManager.Instance.PlayerData.UpdateStats(
                        physicalStrength: 3,
                        mentalStrength: baseStatGain
                    );
                    break;

                case 5: // Scientist - High Intelligence
                    GameManager.Instance.PlayerData.UpdateStats(
                        intelligence: 8
                    );
                    break;
            }

            // Reduce stress a bit from relaxing
            GameManager.Instance.PlayerData.UpdateStats(stress: -2);

            Debug.Log("Stats updated after watching channel");
        }

        private void ExitTV()
        {
            Debug.Log("[TVInterface] Exiting TV - returning to Living Room");

            // Always return directly to living room
            SceneController.LoadScene("02_LivingRoom");
        }

        // Quick channel selection with number keys
        private void OnEnable()
        {
            // Listen for number key presses
        }

        private void OnDisable()
        {
            // Clean up
        }
    }
}