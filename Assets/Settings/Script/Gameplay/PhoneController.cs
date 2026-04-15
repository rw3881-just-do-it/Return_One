using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityTV.Core;
using UnityTV.Player;

namespace UnityTV.Gameplay
{
    /// <summary>
    /// 手机系统控制器
    /// Phone System Controller - handles shop, messages, and rules
    /// </summary>
    public class PhoneController : MonoBehaviour
    {
        [Header("Phone UI")]
        [SerializeField] private GameObject phonePanel; // 手机主面板
        [SerializeField] private GameObject phoneHomeScreen; // 手机主屏幕（显示三个按钮）
        [SerializeField] private Button shopButton; // 商店按钮
        [SerializeField] private Button messagesButton; // 消息按钮
        [SerializeField] private Button rulesButton; // 规则按钮
        [SerializeField] private Button phoneCloseButton; // 关闭手机按钮

        [Header("Shop Screen")]
        [SerializeField] private GameObject shopScreen; // 商店界面
        [SerializeField] private Transform furnitureContainer; // 家具列表容器
        [SerializeField] private GameObject furniturePrefab; // 家具项预制件
        [SerializeField] private Button shopBackButton; // 商店返回按钮

        [Header("Messages Screen")]
        [SerializeField] private GameObject messagesScreen; // 消息界面
        [SerializeField] private Transform messagesContainer; // 消息列表容器
        [SerializeField] private GameObject messagePrefab; // 消息项预制件
        [SerializeField] private Button messagesBackButton; // 消息返回按钮

        [Header("Rules Screen")]
        [SerializeField] private GameObject rulesScreen; // 规则界面
        [SerializeField] private TextMeshProUGUI rulesText; // 规则文本
        [SerializeField] private Button rulesBackButton; // 规则返回按钮

        [Header("Player Reference")]
        [SerializeField] private SpriteRenderer playerSprite; // 玩家精灵（用于显示看手机动作）

        // Phone state
        private bool isPhoneOpen = false;

        private void Start()
        {
            SetupButtons();
            ClosePhone();
        }

        private void SetupButtons()
        {
            // Home screen buttons
            if (shopButton) shopButton.onClick.AddListener(OpenShop);
            if (messagesButton) messagesButton.onClick.AddListener(OpenMessages);
            if (rulesButton) rulesButton.onClick.AddListener(OpenRules);
            if (phoneCloseButton) phoneCloseButton.onClick.AddListener(ClosePhone);

            // Back buttons
            if (shopBackButton) shopBackButton.onClick.AddListener(BackToHome);
            if (messagesBackButton) messagesBackButton.onClick.AddListener(BackToHome);
            if (rulesBackButton) rulesBackButton.onClick.AddListener(BackToHome);
        }

        /// <summary>
        /// 打开手机
        /// Open the phone
        /// </summary>
        public void OpenPhone()
        {
            if (isPhoneOpen) return;

            Debug.Log("[Phone] Opening phone...");

            isPhoneOpen = true;

            // Show phone panel
            if (phonePanel) phonePanel.SetActive(true);

            // Show home screen
            ShowHomeScreen();

            // TODO: Play phone open animation
            // TODO: Change player sprite to "looking at phone" pose

            // Pause time while using phone (optional)
            // GameManager.Instance?.TimeManager?.PauseTime();
        }

        /// <summary>
        /// 关闭手机
        /// Close the phone
        /// </summary>
        public void ClosePhone()
        {
            Debug.Log("[Phone] Closing phone...");

            isPhoneOpen = false;

            // Hide phone panel
            if (phonePanel) phonePanel.SetActive(false);

            // Hide all screens
            HideAllScreens();

            // TODO: Reset player sprite to normal pose

            // Resume time
            // GameManager.Instance?.TimeManager?.ResumeTime();
        }

        /// <summary>
        /// 显示主屏幕
        /// Show home screen with 3 buttons
        /// </summary>
        private void ShowHomeScreen()
        {
            HideAllScreens();
            if (phoneHomeScreen) phoneHomeScreen.SetActive(true);
        }

        /// <summary>
        /// 隐藏所有界面
        /// Hide all screens
        /// </summary>
        private void HideAllScreens()
        {
            if (phoneHomeScreen) phoneHomeScreen.SetActive(false);
            if (shopScreen) shopScreen.SetActive(false);
            if (messagesScreen) messagesScreen.SetActive(false);
            if (rulesScreen) rulesScreen.SetActive(false);
        }

        /// <summary>
        /// 打开商店
        /// Open shop screen
        /// </summary>
        private void OpenShop()
        {
            Debug.Log("[Phone] Opening shop...");
            HideAllScreens();
            if (shopScreen) shopScreen.SetActive(true);

            // Load furniture items
            LoadFurnitureShop();
        }

        /// <summary>
        /// 打开消息
        /// Open messages screen
        /// </summary>
        private void OpenMessages()
        {
            Debug.Log("[Phone] Opening messages...");
            HideAllScreens();
            if (messagesScreen) messagesScreen.SetActive(true);

            // Load messages
            LoadMessages();
        }

        /// <summary>
        /// 打开规则说明
        /// Open rules screen
        /// </summary>
        private void OpenRules()
        {
            Debug.Log("[Phone] Opening rules...");
            HideAllScreens();
            if (rulesScreen) rulesScreen.SetActive(true);

            // Display rules
            DisplayRules();
        }

        /// <summary>
        /// 返回主屏幕
        /// Back to home screen
        /// </summary>
        private void BackToHome()
        {
            ShowHomeScreen();
        }

        /// <summary>
        /// 加载商店家具
        /// Load furniture items in shop
        /// </summary>
        private void LoadFurnitureShop()
        {
            if (furnitureContainer == null) return;

            // Clear existing items
            foreach (Transform child in furnitureContainer)
            {
                Destroy(child.gameObject);
            }

            // Create furniture items (example data)
            CreateFurnitureItem("舒适沙发", "增加压力恢复速度", 100, 0, 5, 0, 0, -10);
            CreateFurnitureItem("书架", "增加智力属性", 150, 5, 0, 0, 0, 0);
            CreateFurnitureItem("健身器材", "增加身体属性", 200, 0, 0, 10, 0, 0);
            CreateFurnitureItem("冥想垫", "增加意志属性", 120, 0, 0, 0, 5, -5);
            CreateFurnitureItem("高级床铺", "恢复更多体力", 250, 0, 0, 0, 0, -15);
            CreateFurnitureItem("游戏机", "降低压力值", 180, 0, 0, 0, 0, -20);
        }

        /// <summary>
        /// 创建家具商品项
        /// Create a furniture shop item
        /// </summary>
        private void CreateFurnitureItem(string name, string description, int price,
            int intelligenceBonus, int idealBonus, int physicalBonus, int mentalBonus, int stressBonus)
        {
            if (furniturePrefab == null || furnitureContainer == null) return;

            GameObject item = Instantiate(furniturePrefab, furnitureContainer);

            // Set furniture data
            FurnitureItem furnitureItem = item.GetComponent<FurnitureItem>();
            if (furnitureItem != null)
            {
                furnitureItem.Initialize(name, description, price,
                    intelligenceBonus, idealBonus, physicalBonus, mentalBonus, stressBonus);
            }
        }

        /// <summary>
        /// 加载消息列表
        /// Load messages
        /// </summary>
        private void LoadMessages()
        {
            if (messagesContainer == null) return;

            // Clear existing messages
            foreach (Transform child in messagesContainer)
            {
                Destroy(child.gameObject);
            }

            // Check if player has partner
            bool hasPartner = GameManager.Instance?.PlayerData?.HasPartner ?? false;

            if (hasPartner)
            {
                // Create partner message
                CreateMessage("对象", "嘿，今天工作怎么样？加油哦！", "avatar_partner");
            }

            // System message
            CreateMessage("系统消息", "欢迎使用归一电视！记住，你只有10天时间找到工作。", "avatar_system");

            // Anchilo message (if visited)
            if (GameManager.Instance?.PlayerData?.AnchiloVisited ?? false)
            {
                CreateMessage("安奇洛", "有什么需要帮助的随时找我！", "avatar_anchilo");
            }
        }

        /// <summary>
        /// 创建消息项
        /// Create a message item
        /// </summary>
        private void CreateMessage(string senderName, string messageText, string avatarName)
        {
            if (messagePrefab == null || messagesContainer == null) return;

            GameObject item = Instantiate(messagePrefab, messagesContainer);

            // Set message data
            MessageItem messageItem = item.GetComponent<MessageItem>();
            if (messageItem != null)
            {
                messageItem.Initialize(senderName, messageText, avatarName);
            }
        }

        /// <summary>
        /// 显示规则说明
        /// Display game rules
        /// </summary>
        private void DisplayRules()
        {
            if (rulesText == null) return;

            // 获取当前模型
            int currentModel = WorldModelManager.Instance?.CurrentModel ?? 0;

            // 获取对应模型的规则文本
            string rulesContent = TVRulesTextManager.GetFormattedRules(currentModel);

            rulesText.text = $"<b>归一电视使用规则</b>\n\n{rulesContent}";
        }

        public bool IsPhoneOpen => isPhoneOpen;
    }

    /// <summary>
    /// 家具商品项
    /// Furniture shop item
    /// </summary>
    public class FurnitureItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;

        private string furnitureName;
        private int price;
        private int intelligenceBonus;
        private int idealBonus;
        private int physicalBonus;
        private int mentalBonus;
        private int stressBonus;

        public void Initialize(string name, string description, int furniturePrice,
            int intel, int ideal, int physical, int mental, int stress)
        {
            furnitureName = name;
            price = furniturePrice;
            intelligenceBonus = intel;
            idealBonus = ideal;
            physicalBonus = physical;
            mentalBonus = mental;
            stressBonus = stress;

            if (nameText) nameText.text = name;
            if (descriptionText) descriptionText.text = description;
            if (priceText) priceText.text = $"¥{price}";

            if (buyButton)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        private void OnBuyClicked()
        {
            Debug.Log($"[Shop] Attempting to buy {furnitureName}");

            // TODO: Check if player has enough money
            // TODO: Deduct money and apply bonuses
            // TODO: Add furniture to living room

            // For now, just apply bonuses directly
            if (GameManager.Instance?.PlayerData != null)
            {
                GameManager.Instance.PlayerData.UpdateStats(
                    intelligence: intelligenceBonus,
                    ideal: idealBonus,
                    //physicalStrength: physicalBonus,
                    //mentalStrength: mentalBonus,
                    stress: stressBonus
                );

                Debug.Log($"[Shop] Purchased {furnitureName}! Applied bonuses.");
            }
        }
    }

    /// <summary>
    /// 消息项
    /// Message item
    /// </summary>
    public class MessageItem : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI senderText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button messageButton;

        private string messageFull;

        public void Initialize(string sender, string message, string avatarName)
        {
            messageFull = message;

            if (senderText) senderText.text = sender;
            if (messageText) messageText.text = message;

            // TODO: Load avatar sprite from Resources
            // if (avatarImage) avatarImage.sprite = Resources.Load<Sprite>($"Avatars/{avatarName}");

            if (messageButton)
            {
                messageButton.onClick.AddListener(OnMessageClicked);
            }
        }

        private void OnMessageClicked()
        {
            Debug.Log($"[Messages] Message clicked: {messageFull}");
            // TODO: Open detailed message view scene
        }
    }
}