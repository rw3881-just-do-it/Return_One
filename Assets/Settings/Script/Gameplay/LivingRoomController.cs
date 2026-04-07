using UnityEngine;
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
        [SerializeField] private TextMeshProUGUI attriText;
        [SerializeField] private GameObject tvPrompt; // "按E看电视" 提示
        [SerializeField] private GameObject doorPrompt; // "按E开门" 提示

        [Header("Player")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private SpriteRenderer playerSprite;

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
            InitializeScene();
            SetupInteractionZones();

            // Check if Anchilo should visit (first time in living room)
            if (GameManager.Instance?.PlayerData != null &&
                !GameManager.Instance.PlayerData.AnchiloVisited)
            {
                // Trigger Anchilo visit after a short delay
                Invoke(nameof(TriggerAnchiloVisit), 2f);
            }
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

            // Update UI
            UpdateUI();
        }

        private void SetupInteractionZones()
        {
            // TV interaction zone
            if (tvInteractionZone == null)
            {
                GameObject tvZone = new GameObject("TV_InteractionZone");
                tvZone.transform.position = new Vector3(0, -1, 0); // Adjust position
                tvInteractionZone = tvZone.AddComponent<BoxCollider2D>();
                tvInteractionZone.isTrigger = true;
                tvInteractionZone.size = new Vector2(3f, 2f);

                InteractionZone tvZoneScript = tvZone.AddComponent<InteractionZone>();
                tvZoneScript.Initialize(InteractionType.TV, this);
            }

            // Door interaction zone
            if (doorInteractionZone == null)
            {
                GameObject doorZone = new GameObject("Door_InteractionZone");
                doorZone.transform.position = new Vector3(-5, 0, 0); // Adjust position
                doorInteractionZone = doorZone.AddComponent<BoxCollider2D>();
                doorInteractionZone.isTrigger = true;
                doorInteractionZone.size = new Vector2(2f, 3f);

                InteractionZone doorZoneScript = doorZone.AddComponent<InteractionZone>();
                doorZoneScript.Initialize(InteractionType.Door, this);
            }
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

            // Flip sprite based on direction
            if (playerSprite != null && moveInput.x != 0)
            {
                playerSprite.flipX = moveInput.x < 0;
            }
        }

        private void TryInteract()
        {
            if (isNearTV)
            {
                InteractWithTV();
            }
            else if (isNearDoor)
            {
                InteractWithDoor();
            }
        }

        private void InteractWithTV()
        {
            Debug.Log("Player interacts with TV");

            // Check if player has unlocked full systems
            if (GameManager.Instance?.PlayerData?.FullSystemsUnlocked == true)
            {
                // Go to TV interface scene
                GameManager.Instance.EnterTVMode();
            }
            else
            {
                // Show message: need to talk to Anchilo first
                ShowMessage("请先与访客交谈...");
            }
        }

        private void InteractWithDoor()
        {
            Debug.Log("Player interacts with Door");

            // Load door talking scene
            SceneController.LoadScene("Door_Talking_Scene");
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

            // Update day text
            if (dayText)
            {
                dayText.text = $"第{data.CurrentDay}天";
            }

            // Update money text (placeholder - you can add money system later)
            if (moneyText)
            {
                moneyText.text = "¥0"; // TODO: Add money system
            }

            // Update attributes text
            if (attriText)
            {
                attriText.text = $"体力:{data.Stats.Health} " +
                                 $"智力:{data.Stats.Intelligence} " +
                                 $"身体:{data.Stats.PhysicalStrength} " +
                                 $"意志:{data.Stats.MentalStrength}";
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
    }

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
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                controller?.OnEnterInteractionZone(interactionType);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                controller?.OnExitInteractionZone(interactionType);
            }
        }
    }
}