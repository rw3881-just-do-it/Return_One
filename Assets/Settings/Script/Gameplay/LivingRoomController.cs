using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityTV.Core;
using UnityTV.Player;

namespace UnityTV.Gameplay
{
    /// <summary>
    /// 主场景控制器 - Living Room
    /// 处理玩家移动、TV交互、门交互
    /// </summary>
    public class LivingRoomController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI moneyText;
        //[SerializeField] private TextMeshProUGUI attriText;
        [SerializeField] private GameObject tvPrompt; // "按E看电视" 提示
        [SerializeField] private GameObject doorPrompt; // "按E开门" 提示

        [Header("New UI - Bottom Bar (No Phone)")]
        [SerializeField] private Slider stressBar; // 压力值条形
        [SerializeField] private Slider idealBar; // 理想值条形
        [SerializeField] private TextMeshProUGUI stressText; // 压力值文本
        [SerializeField] private TextMeshProUGUI idealText; // 理想值文本
        [SerializeField] private Button rulesButton; // 规则按钮
        [SerializeField] private Button shopButton; // 商店按钮
        [SerializeField] private Button messageButton; // 消息按钮
        [SerializeField] private Button backButton;
        [SerializeField] private Button quitButton; // 退出到主菜单按钮

        [Header("New UI Panels")]
        [SerializeField] private GameObject rulesPanel; // 规则面板
        [SerializeField] private GameObject shopPanel; // 商店面板
        [SerializeField] private GameObject messagePanel; // 消息面板（访客风格）

        [Header("Top Right - 6 Attributes Display")]
        [SerializeField] private TextMeshProUGUI strengthText; // 力量
        [SerializeField] private TextMeshProUGUI intelligenceText; // 智力
        [SerializeField] private TextMeshProUGUI agilityText; // 敏捷
        [SerializeField] private TextMeshProUGUI perceptionText; // 见闻
        [SerializeField] private TextMeshProUGUI dexterityText; // 巧手
        [SerializeField] private TextMeshProUGUI courageText; // 勇气

        [Header("Player")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private SpriteRenderer playerSprite;
        [SerializeField] private Animator playerAnimator;

        [Header("Interaction Zones")]
        [SerializeField] private BoxCollider2D tvInteractionZone;
        [SerializeField] private BoxCollider2D doorInteractionZone;

        [Header("NPC Visit")]
        [SerializeField] private GameObject doorExclamationMark; // 门上的感叹号提示

        // State
        private bool isNearTV = false;
        private bool isNearDoor = false;
        private bool hasAnchiloVisited = false;
        private bool firstDayComplete = false;

        // Input
        private Vector2 moveInput;

        private void Start()
        {
            // IMPORTANT: Check if GameManager exists
            // If not (testing scene directly), create a temporary player data
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[LivingRoom] GameManager not found! This scene is meant to be loaded from the game flow.");
                Debug.LogWarning("[LivingRoom] Creating temporary setup for testing...");

                // For testing purposes, create a basic GameManager
                GameObject gmObject = new GameObject("GameManager");
                gmObject.AddComponent<GameManager>();

                // Wait one frame for GameManager to initialize
                Invoke(nameof(DelayedInitialize), 0.1f);
                return;
            }

            InitializeScene();
            SetupInteractionZones();

            // Check if Anchilo should visit (first time in living room)
            if (GameManager.Instance?.PlayerData != null &&
                !GameManager.Instance.PlayerData.AnchiloVisited)
            {
                // Trigger Anchilo visit after a short delay
                Invoke(nameof(TriggerAnchiloVisit), 2f);
            }
            else if (GameManager.Instance?.PlayerData == null)
            {
                Debug.LogWarning("[LivingRoom] No PlayerData! You need to create a character first.");
            }

            Debug.Log("[LivingRoom] Scene initialized");

            if (backButton != null) {
                backButton.onClick.AddListener(CloseMessagePanel);
            }
        }

        private void DelayedInitialize()
        {
            // Try again after GameManager is created
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
                GameManager.Instance.CompleteCharacterCreation(testData);

                Debug.Log("[LivingRoom] Created test player data for scene testing");
            }

            Start(); // Call Start again now that GameManager exists
        }

        private void Update()
        {
            HandleInput();
            UpdateUI();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void InitializeScene()
        {
            // Hide prompts initially
            if (tvPrompt) tvPrompt.SetActive(false);
            if (doorPrompt) doorPrompt.SetActive(false);
            if (doorExclamationMark) doorExclamationMark.SetActive(false);

            // Setup rules button
            if (rulesButton)
            {
                rulesButton.onClick.AddListener(OnRulesButtonClicked);
                rulesButton.gameObject.SetActive(GameManager.Instance?.PlayerData?.FullSystemsUnlocked ?? false);
            }

            // Setup shop button
            if (shopButton)
            {
                shopButton.onClick.AddListener(OnShopButtonClicked);
                shopButton.gameObject.SetActive(GameManager.Instance?.PlayerData?.FullSystemsUnlocked ?? false);
            }

            // Setup message button
            if (messageButton)
            {
                messageButton.onClick.AddListener(OnMessageButtonClicked);
                messageButton.gameObject.SetActive(GameManager.Instance?.PlayerData?.FullSystemsUnlocked ?? false);
            }

            // Setup quit button
            if (quitButton)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }

            // Hide all panels initially
            if (rulesPanel) rulesPanel.SetActive(false);
            if (shopPanel) shopPanel.SetActive(false);
            if (messagePanel) messagePanel.SetActive(false);

            // Hide stress/ideal bars until unlocked
            bool systemsUnlocked = GameManager.Instance?.PlayerData?.FullSystemsUnlocked ?? false;
            if (stressBar) stressBar.gameObject.SetActive(systemsUnlocked);
            if (idealBar) idealBar.gameObject.SetActive(systemsUnlocked);

            // Verify player setup
            if (playerTransform != null)
            {
                // Make sure player has required components
                Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    Debug.LogError("[LivingRoom] Player is missing Rigidbody2D! Adding it now.");
                    rb = playerTransform.gameObject.AddComponent<Rigidbody2D>();
                    rb.gravityScale = 0;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                }

                Collider2D col = playerTransform.GetComponent<Collider2D>();
                if (col == null)
                {
                    Debug.LogError("[LivingRoom] Player is missing Collider! Adding it now.");
                    playerTransform.gameObject.AddComponent<CapsuleCollider2D>();
                }

                Debug.Log($"[LivingRoom] Player setup verified. Tag: {playerTransform.tag}");
            }
            else
            {
                Debug.LogError("[LivingRoom] Player Transform not assigned!");
            }

            // Update UI
            UpdateUI();
        }

        private void SetupInteractionZones()
        {
            Debug.Log("[LivingRoom] Setting up interaction zones...");

            // Find TV and Door objects by tag
            GameObject tvObject = GameObject.FindGameObjectWithTag("TV");
            GameObject doorObject = GameObject.FindGameObjectWithTag("Door");

            if (tvObject == null)
            {
                Debug.LogError("[LivingRoom] TV object not found! Make sure an object has the 'TV' tag.");
            }
            else
            {
                Debug.Log($"[LivingRoom] Found TV at position: {tvObject.transform.position}");
            }

            if (doorObject == null)
            {
                Debug.LogError("[LivingRoom] Door object not found! Make sure an object has the 'Door' tag.");
            }
            else
            {
                Debug.Log($"[LivingRoom] Found Door at position: {doorObject.transform.position}");
            }

            // TV interaction zone
            if (tvObject != null)
            {
                // Check if zone already exists
                Transform existingZone = tvObject.transform.Find("TV_InteractionZone");
                if (existingZone != null)
                {
                    GameObject.Destroy(existingZone.gameObject);
                }

                GameObject tvZone = new GameObject("TV_InteractionZone");
                tvZone.transform.SetParent(tvObject.transform);
                tvZone.transform.localPosition = Vector3.zero;
                tvZone.layer = tvObject.layer; // Match parent layer

                tvInteractionZone = tvZone.AddComponent<BoxCollider2D>();
                tvInteractionZone.isTrigger = true;
                tvInteractionZone.size = new Vector2(4f, 3f); // Made bigger

                InteractionZone tvZoneScript = tvZone.AddComponent<InteractionZone>();
                tvZoneScript.Initialize(InteractionType.TV, this);

                Debug.Log($"[LivingRoom] ✓ TV interaction zone created! Size: {tvInteractionZone.size}, IsTrigger: {tvInteractionZone.isTrigger}");
            }

            // Door interaction zone
            if (doorObject != null)
            {
                // Check if zone already exists
                Transform existingZone = doorObject.transform.Find("Door_InteractionZone");
                if (existingZone != null)
                {
                    GameObject.Destroy(existingZone.gameObject);
                }

                GameObject doorZone = new GameObject("Door_InteractionZone");
                doorZone.transform.SetParent(doorObject.transform);
                doorZone.transform.localPosition = Vector3.zero;
                doorZone.layer = doorObject.layer; // Match parent layer

                doorInteractionZone = doorZone.AddComponent<BoxCollider2D>();
                doorInteractionZone.isTrigger = true;
                doorInteractionZone.size = new Vector2(3f, 4f); // Made bigger

                InteractionZone doorZoneScript = doorZone.AddComponent<InteractionZone>();
                doorZoneScript.Initialize(InteractionType.Door, this);

                Debug.Log($"[LivingRoom] ✓ Door interaction zone created! Size: {doorInteractionZone.size}, IsTrigger: {doorInteractionZone.isTrigger}");
            }

            Debug.Log("[LivingRoom] Interaction zone setup complete!");
        }

        private void HandleInput()
        {
            // Movement input (WASD or Arrow Keys)
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            // Interaction input (E key)
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryInteract();
            }
        }

        private void MovePlayer()
        {
            if (playerTransform == null) return;

            // Normalize diagonal movement
            Vector2 movement = moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
            playerTransform.Translate(movement);

            if (playerAnimator != null) { 
                bool isWalking = moveInput.magnitude > 0;
                playerAnimator.SetBool("isWalking", isWalking);
            }

            // Flip sprite based on direction
            if (playerSprite != null && moveInput.x != 0)
            {
                playerSprite.flipX = moveInput.x < 0;
            }
        }

        private void TryInteract()
        {
            Debug.Log($"[LivingRoom] E pressed! Near TV: {isNearTV}, Near Door: {isNearDoor}");

            if (isNearTV)
            {
                InteractWithTV();
            }
            else if (isNearDoor)
            {
                InteractWithDoor();
            }
            else
            {
                Debug.Log("[LivingRoom] Not near any interactable object!");
            }
        }

        private void InteractWithTV()
        {
            Debug.Log("[LivingRoom] Player interacts with TV");

            // Check if player has unlocked full systems
            if (GameManager.Instance?.PlayerData?.FullSystemsUnlocked == true)
            {
                Debug.Log("[LivingRoom] Loading TV interface...");
                // Go to TV interface scene directly
                SceneController.LoadScene("04_TVInterface");
            }
            else
            {
                // Show message: need to talk to Anchilo first
                ShowMessage("请先与访客交谈...");
                Debug.LogWarning("[LivingRoom] Full systems not unlocked. Talk to Anchilo first!");
            }
        }

        private void InteractWithDoor()
        {
            Debug.Log("Player interacts with Door");

            // Load door talking scene (use exact scene name from Build Settings)
            SceneController.LoadScene("03_DoorTalking");
        }

        private void TriggerAnchiloVisit()
        {
            Debug.Log("Anchilo is visiting!");

            // Show exclamation mark on door
            if (doorExclamationMark)
            {
                doorExclamationMark.SetActive(true);
            }

            // Show notification
            ShowMessage("有人在敲门！");
        }

        private void UpdateUI()
        {
            if (GameManager.Instance?.PlayerData == null) return;

            PlayerData data = GameManager.Instance.PlayerData;

            // Update day/turn text (top left)
            if (dayText)
            {
                dayText.text = $"回合 {data.Stats.CurrentTurn}/{data.Stats.MaxTurns}";
            }

            // Update score text (top left)
            if (moneyText)
            {
                moneyText.text = $"积分: {data.Stats.Score}";
            }

            // Update 6 attributes display (top right) - NEW!
            if (strengthText)
                strengthText.text = $"力量: {data.Stats.Strength}";

            if (intelligenceText)
                intelligenceText.text = $"智力: {data.Stats.Intelligence}";

            if (agilityText)
                agilityText.text = $"敏捷: {data.Stats.Agility}";

            if (perceptionText)
                perceptionText.text = $"见闻: {data.Stats.Perception}";

            if (dexterityText)
                dexterityText.text = $"巧手: {data.Stats.Dexterity}";

            if (courageText)
                courageText.text = $"勇气: {data.Stats.Courage}";

            // Update stress bar (only if systems unlocked)
            if (stressBar && data.FullSystemsUnlocked)
            {
                stressBar.maxValue = data.Stats.MaxStress;
                stressBar.value = data.Stats.Stress;

                if (stressText)
                {
                    stressText.text = $"压力: {data.Stats.Stress}/{data.Stats.MaxStress}";
                }

                // Change color based on stress level
                Image fillImage = stressBar.fillRect?.GetComponent<Image>();
                if (fillImage)
                {
                    float stressPercent = (float)data.Stats.Stress / data.Stats.MaxStress;
                    if (stressPercent > 0.7f)
                        fillImage.color = Color.red; // High stress
                    else if (stressPercent > 0.4f)
                        fillImage.color = Color.yellow; // Medium stress
                    else
                        fillImage.color = Color.green; // Low stress
                }
            }

            // Update ideal bar (only if systems unlocked)
            if (idealBar && data.FullSystemsUnlocked)
            {
                idealBar.maxValue = 200; // Max ideal value
                idealBar.value = data.Stats.Ideal;

                if (idealText)
                {
                    idealText.text = $"理想: {data.Stats.Ideal}/200";
                }

                // Ideal bar is always positive (blue/cyan color)
                Image fillImage = idealBar.fillRect?.GetComponent<Image>();
                if (fillImage)
                {
                    fillImage.color = new Color(0.2f, 0.8f, 1f); // Cyan color
                }
            }
        }

        private void ShowMessage(string message)
        {
            Debug.Log($"[Message] {message}");
            // TODO: Implement notification UI
        }

        // Called by InteractionZone when player enters/exits
        public void OnEnterInteractionZone(InteractionType type)
        {
            switch (type)
            {
                case InteractionType.TV:
                    isNearTV = true;
                    if (tvPrompt) tvPrompt.SetActive(true);
                    break;

                case InteractionType.Door:
                    isNearDoor = true;
                    if (doorPrompt) doorPrompt.SetActive(true);
                    break;
            }
        }

        public void OnExitInteractionZone(InteractionType type)
        {
            switch (type)
            {
                case InteractionType.TV:
                    isNearTV = false;
                    if (tvPrompt) tvPrompt.SetActive(false);
                    break;

                case InteractionType.Door:
                    isNearDoor = false;
                    if (doorPrompt) doorPrompt.SetActive(false);
                    break;
            }
        }
        /// <summary>
        /// 规则按钮点击
        /// </summary>
        private void OnRulesButtonClicked()
        {
            Debug.Log("[LivingRoom] Rules button clicked");

            if (rulesPanel != null)
            {
                // Close other panels
                if (shopPanel) shopPanel.SetActive(false);
                if (messagePanel) messagePanel.SetActive(false);

                // Toggle rules panel
                rulesPanel.SetActive(!rulesPanel.activeSelf);
            }
        }

        /// <summary>
        /// 商店按钮点击
        /// </summary>
        private void OnShopButtonClicked()
        {
            Debug.Log("[LivingRoom] Shop button clicked");

            if (shopPanel != null)
            {
                // Close other panels
                if (rulesPanel) rulesPanel.SetActive(false);
                if (messagePanel) messagePanel.SetActive(false);

                // Toggle shop panel
                shopPanel.SetActive(!shopPanel.activeSelf);
            }
        }

        /// <summary>
        /// 消息按钮点击
        /// </summary>
        private void OnMessageButtonClicked()
        {
            Debug.Log("[LivingRoom] Message button clicked");

            if (messagePanel != null)
            {
                // Close other panels
                if (rulesPanel) rulesPanel.SetActive(false);
                if (shopPanel) shopPanel.SetActive(false);

                // Toggle message panel
                messagePanel.SetActive(!messagePanel.activeSelf);
            }
        }
        public void CloseMessagePanel() {
            if (messagePanel != null) { 
                messagePanel.SetActive(false);
                Debug.Log("[LivingRoom] Message panel closed via back button.");
            }
        }

        /// <summary>
        /// 退出按钮点击
        /// Quit button clicked - return to main menu
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[LivingRoom] Quit button clicked");

            // Show confirmation dialog
            if (GameManager.Instance != null)
            {
                // TODO: Show "Are you sure?" dialog
                GameManager.Instance.ReturnToMainMenu();
            }
            else
            {
                SceneController.LoadScene("00_MainMenu");
            }
        }

        /// <summary>
        /// Unlock bottom bar buttons after Anchilo visit
        /// </summary>
        public void UnlockBottomButtons()
        {
            if (rulesButton) rulesButton.gameObject.SetActive(true);
            if (shopButton) shopButton.gameObject.SetActive(true);
            if (messageButton) messageButton.gameObject.SetActive(true);
            if (stressBar) stressBar.gameObject.SetActive(true);
            if (idealBar) idealBar.gameObject.SetActive(true);

            Debug.Log("[LivingRoom] Bottom buttons and bars unlocked!");
        }

        /// <summary>
        /// 显示门口的感叹号（敲门声触发时）
        /// </summary>
        public void ShowDoorExclamation()
        {
            if (doorExclamationMark != null)
            {
                doorExclamationMark.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏门口的感叹号
        /// </summary>
        public void HideDoorExclamation()
        {
            if (doorExclamationMark != null)
            {
                doorExclamationMark.SetActive(false);
            }
        }
    } // ← LivingRoomController 类在这里结束

    //end of LivingRoomController

    /// <summary>
    /// 交互区域类型
    /// </summary>
    public enum InteractionType
    {
        TV,
        Door,
        Phone
    }

    /// <summary>
    /// 交互区域触发器
    /// </summary>
    public class InteractionZone : MonoBehaviour
    {
        private InteractionType interactionType;
        private LivingRoomController controller;

        public void Initialize(InteractionType type, LivingRoomController ctrl)
        {
            interactionType = type;
            controller = ctrl;
            Debug.Log($"[InteractionZone] Initialized: {type}");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[InteractionZone] {interactionType} - Trigger Enter: {other.gameObject.name}, Tag: {other.tag}");

            if (other.CompareTag("Player"))
            {
                Debug.Log($"[InteractionZone] ✓ Player entered {interactionType} zone!");
                controller?.OnEnterInteractionZone(interactionType);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Debug.Log($"[InteractionZone] {interactionType} - Trigger Exit: {other.gameObject.name}, Tag: {other.tag}");

            if (other.CompareTag("Player"))
            {
                Debug.Log($"[InteractionZone] ✓ Player exited {interactionType} zone!");
                controller?.OnExitInteractionZone(interactionType);
            }
        }
    }
}
