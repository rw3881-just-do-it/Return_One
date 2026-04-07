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
            InitializeUI();
            SetupChannelButtons();
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
            if (channelSelectionPanel) channelSelectionPanel.SetActive(true);
            if (watchingPanel) watchingPanel.SetActive(false);

            // Setup exit button
            if (exitButton)
            {
                exitButton.onClick.AddListener(ExitTV);
            }
        }

        private void SetupChannelButtons()
        {
            // Channel 1 - Medical (Doctor career)
            if (channel1Button)
            {
                channel1Button.onClick.AddListener(() => SelectChannel(1));
                channel1Button.interactable = IsChannelUnlocked(1);
            }

            // Channel 2 - Police
            if (channel2Button)
            {
                channel2Button.onClick.AddListener(() => SelectChannel(2));
                channel2Button.interactable = IsChannelUnlocked(2);
            }

            // Channel 3 - Office
            if (channel3Button)
            {
                channel3Button.onClick.AddListener(() => SelectChannel(3));
                channel3Button.interactable = IsChannelUnlocked(3);
            }

            // Channel 4 - Merchant
            if (channel4Button)
            {
                channel4Button.onClick.AddListener(() => SelectChannel(4));
                channel4Button.interactable = IsChannelUnlocked(4);
            }

            // Channel 5 - Scientist (locked in first playthrough)
            if (channel5Button)
            {
                channel5Button.onClick.AddListener(() => SelectChannel(5));
                channel5Button.interactable = IsChannelUnlocked(5);
            }

            // Channel 6 - Combat (unlocked after Anchilo)
            if (channel6Button)
            {
                channel6Button.onClick.AddListener(() => SelectChannel(6));
                channel6Button.interactable = IsChannelUnlocked(6);
            }
        }

        private bool IsChannelUnlocked(int channelNumber)
        {
            if (GameManager.Instance?.PlayerData == null) return false;

            // Channel 6 is special - unlocked after Anchilo
            if (channelNumber == 6)
            {
                return GameManager.Instance.PlayerData.Channel6Unlocked;
            }

            // Channel 5 locked in first playthrough
            if (channelNumber == 5)
            {
                // TODO: Check if player completed first playthrough
                return false;
            }

            return GameManager.Instance.PlayerData.UnlockedChannels.Contains(channelNumber);
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
            Debug.Log("Exiting TV");

            // Check if GameManager exists
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found! Loading Living Room directly.");
                SceneController.LoadScene("02_LivingRoom");
                return;
            }

            // Return to living room
            GameManager.Instance.ExitTVMode();
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