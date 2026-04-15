using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityTV.Core;
using System.Collections;  // For IEnumerator
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
        private int channelSwitchCount = 0;
        private int lastChannel = -1;
        private int consecutiveWatchCount = 0;
        private int lastWatchedChannel = -1;

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

            // Channel 6 - Combat (LOCKED - partner's game in development)
            if (channel6Button)
            {
                channel6Button.onClick.RemoveAllListeners();
                channel6Button.onClick.AddListener(() => OnChannel6Clicked());
                channel6Button.interactable = false; // Always locked

                // Update button text to show it's locked
                TextMeshProUGUI buttonText = channel6Button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText)
                {
                    buttonText.text = "频道 VI\n战斗游戏\n[开发中...]";
                    buttonText.color = Color.gray;
                }

                Debug.Log("[TVInterface] Channel 6 configured (LOCKED - in development)");
            }
            else
            {
                Debug.LogError("[TVInterface] Channel 6 button not assigned!");
            }

            Debug.Log("[TVInterface] All channel buttons setup complete!");
        }

        private void OnChannel6Clicked()
        {
            // Show message that it's in development
            Debug.Log("[TVInterface] Channel 6 is locked - game in development");
            ShowChannelMessage("此频道正在开发中...");
        }

        private void ShowChannelMessage(string message)
        {
            Debug.Log($"[TVInterface] Message: {message}");
            // TODO: Show actual UI notification
        }

        private bool IsChannelUnlocked(int channelNumber)
        {
            // For testing: if no GameManager, unlock all channels except 6
            if (GameManager.Instance?.PlayerData == null)
            {
                bool unlocked = (channelNumber != 6); // All except 6
                Debug.LogWarning($"[TVInterface] No GameManager/PlayerData - channel {channelNumber} unlocked: {unlocked}");
                return unlocked;
            }

            // Channel 6 is special - locked because it's your partner's game
            if (channelNumber == 6)
            {
                Debug.Log("[TVInterface] Channel 6 is locked (partner's game in development)");
                return false; // Always locked for now
            }

            // Channels 1-5 are unlocked by default
            bool isUnlocked = GameManager.Instance.PlayerData.UnlockedChannels.Contains(channelNumber);
            Debug.Log($"[TVInterface] Channel {channelNumber} unlock status: {isUnlocked}");
            return isUnlocked;
        }

        private void SelectChannel(int channelNumber)
        {
            int currentModel = WorldModelManager.Instance?.CurrentModel ?? 0;

            // 检测频道1↔6切换（用于卡频道7）
            if ((lastChannel == 1 && channelNumber == 6) ||
                (lastChannel == 6 && channelNumber == 1))
            {
                channelSwitchCount++;
                Debug.Log($"[TV] 频道1↔6切换次数: {channelSwitchCount}/10");

                if (channelSwitchCount >= 10)
                {
                    // 卡入频道7
                    EnterChannel7();
                    return;
                }
            }

            lastChannel = channelNumber;

            // 检测连续观看同一频道
            if (channelNumber == lastWatchedChannel)
            {
                consecutiveWatchCount++;

                if (consecutiveWatchCount >= 2 && currentModel == 0)
                {
                    // 模型0: 连续观看2次 → 画面扭曲
                    ShowDistortedHorror();
                    GameManager.Instance?.PlayerData?.UpdateStats(stress: 30, ideal: 20);
                    WorldModelManager.Instance?.ForceChangeModel(-1);
                    return;
                }
            }
            else
            {
                consecutiveWatchCount = 1;
                lastWatchedChannel = channelNumber;
            }

            // 频道2特殊处理
            if (channelNumber == 2)
            {
                HandleChannel2(currentModel);
                return;
            }

            // 应用频道效果
            ApplyChannelEffects(channelNumber, currentModel);
        }

        private void HandleChannel2(int model)
        {
            switch (model)
            {
                case 0:
                    // 正常观看，理想+30压力-20，切换到模型1
                    ShowWatchingPanel("频道2: 观看电视画面");
                    GameManager.Instance?.PlayerData?.UpdateStats(ideal: 30, stress: -20);
                    WorldModelManager.Instance?.ForceChangeModel(1);
                    break;

                case 1:
                    // 频道2消失，直接跳过
                    Debug.Log("[TV] 频道2在模型1中消失");
                    ShowMessage("频道2不可用");
                    break;

                case 2:
                    // 多次尝试 → 直接到后天
                    Debug.Log("[TV] 模型2尝试观看频道2 → 跳到后天");
                    // TODO: 实现跳天逻辑
                    break;

                case 3:
                    // 频道2不存在
                    ShowMessage("频道2已经不存在了");
                    break;
            }
        }

        private void EnterChannel7()
        {
            Debug.Log("[TV] 进入频道7!");

            int model = WorldModelManager.Instance?.CurrentModel ?? 0;

            // 应用频道7效果
            GameManager.Instance?.PlayerData?.UpdateStats(ideal: 30, stress: 30);

            if (model == 3)
            {
                // 模型3: 显示递归场景
                ShowRecursiveScene();
            }
            else if (model == -3)
            {
                // 模型-3: 闪退后进入第二天
                Application.Quit();  // 实际游戏中应该是假闪退
            }
            else
            {
                // 其他模型: 闪退并随机切换
                Application.Quit();  // 假闪退
                int newModel = Random.Range(0, 2) == 0 ? -1 : 1;
                WorldModelManager.Instance?.ForceChangeModel(newModel);
            }
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
            int currentModel = WorldModelManager.Instance?.CurrentModel ?? 0;
            ApplyChannelEffects(currentChannel, currentModel);

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

        /// <summary>
        /// 显示扭曲画面效果
        /// </summary>
        private void ShowDistortedHorror()
        {
            Debug.Log("[TVInterface] 显示扭曲画面恐怖效果");

            if (channelInfoText)
            {
                channelInfoText.text = "<color=red>画面扭曲...</color>";
            }

            Invoke(nameof(HideDistortedHorror), 2f);
        }

        private void HideDistortedHorror()
        {
            if (channelInfoText)
            {
                channelInfoText.text = "";
            }
        }

        /// <summary>
        /// 应用频道效果（根据模型）
        /// </summary>
        private void ApplyChannelEffects(int channelNumber, int currentModel)
        {
            Debug.Log($"[TVInterface] 应用频道 {channelNumber} 效果，当前模型 {currentModel}");

            if (GameManager.Instance?.PlayerData == null) return;

            // 根据模型应用不同的理想值增益
            int idealGain = 0;

            switch (currentModel)
            {
                case 0:
                    idealGain = 10;
                    break;
                case 1:
                    idealGain = 10;
                    break;
                case 2:
                    idealGain = 20;
                    break;
                case 3:
                    idealGain = 30;
                    break;
            }

            GameManager.Instance.PlayerData.UpdateStats(ideal: idealGain);

            // 使用新的ChannelEffects系统
            switch (channelNumber)
            {
                case 1:
                    ChannelEffects.ApplyChannel1(GameManager.Instance.PlayerData);
                    break;
                case 3:
                    ChannelEffects.ApplyChannel3(GameManager.Instance.PlayerData);
                    break;
                case 4:
                    ChannelEffects.ApplyChannel4(GameManager.Instance.PlayerData);
                    break;
                case 5:
                    ChannelEffects.ApplyChannel5(GameManager.Instance.PlayerData);
                    break;
            }

            ShowWatchingPanel($"观看频道 {channelNumber}");
        }

        /// <summary>
        /// 显示观看面板
        /// </summary>
        private void ShowWatchingPanel(string message)
        {
            Debug.Log($"[TVInterface] {message}");

            if (channelSelectionPanel)
            {
                channelSelectionPanel.SetActive(false);
            }

            if (watchingPanel)
            {
                watchingPanel.SetActive(true);
            }

            if (channelInfoText)
            {
                channelInfoText.text = message;
            }

            Invoke(nameof(ReturnToChannelSelection), 5f);
        }

        private void ReturnToChannelSelection()
        {
            if (watchingPanel)
            {
                watchingPanel.SetActive(false);
            }

            if (channelSelectionPanel)
            {
                channelSelectionPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 显示递归场景（频道7特殊效果）
        /// </summary>
        private void ShowRecursiveScene()
        {
            Debug.Log("[TVInterface] 显示递归场景（频道7）");

            if (channelInfoText)
            {
                channelInfoText.text = "频道7：你看到了自己在玩游戏...\n控制里面的角色关掉电视";
            }

            StartCoroutine(WaitForRecursiveExit());
        }

        private System.Collections.IEnumerator WaitForRecursiveExit()
        {
            yield return new WaitForSeconds(2f);

            Debug.Log("[TVInterface] 频道7递归场景结束");

            GameManager.Instance?.PlayerData?.UpdateStats(stress: 30, ideal: -10);

            SceneController.LoadScene("02_LivingRoom");
        }

        /// <summary>
        /// 显示提示消息
        /// </summary>
        private void ShowMessage(string message)
        {
            Debug.Log($"[TVInterface] Message: {message}");

            if (channelInfoText)
            {
                channelInfoText.text = message;
                Invoke(nameof(ClearMessage), 3f);
            }
        }

        private void ClearMessage()
        {
            if (channelInfoText)
            {
                channelInfoText.text = "";
            }
        }
    }
}