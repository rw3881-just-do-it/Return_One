using UnityEngine;
using UnityTV.Core;
using UnityTV.Player;
using System.Collections;

namespace UnityTV.Gameplay
{
    /// <summary>
    /// 环境控制器
    /// Controls background (ideal) and furniture (stress) changes
    /// </summary>
    public class EnvironmentController : MonoBehaviour
    {
        [Header("Background Sprites (Ideal-based)")]
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private Sprite normalBackground; // 模型0
        [SerializeField] private Sprite abilityBackground; // 模型1 - 能力期
        [SerializeField] private Sprite interestBackground; // 模型2 - 兴趣期
        [SerializeField] private Sprite fantasyBackground; // 模型3 - 幻想期

        [Header("Furniture Objects (Stress-based)")]
        [SerializeField] private GameObject eyesFurniture; // 压力50 - 眼睛
        [SerializeField] private GameObject mouthFurniture; // 压力100 - 嘴巴
        [SerializeField] private GameObject heartFurniture; // 压力150 - 心脏

        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 1f;
        [SerializeField] private bool useSmoothtransition = true;

        // Current state tracking
        private int currentBackgroundLevel = 0; // 0-3
        private int currentFurnitureLevel = 0; // 0-3

        private void Start()
        {
            // Subscribe to world model changes
            if (WorldModelManager.Instance != null)
            {
                WorldModelManager.Instance.OnModelChanged += OnWorldModelChanged;
            }

            // Initialize furniture objects
            if (eyesFurniture) eyesFurniture.SetActive(false);
            if (mouthFurniture) mouthFurniture.SetActive(false);
            if (heartFurniture) heartFurniture.SetActive(false);

            // Set initial background
            if (backgroundRenderer && normalBackground)
            {
                backgroundRenderer.sprite = normalBackground;
            }

            UpdateEnvironment();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (WorldModelManager.Instance != null)
            {
                WorldModelManager.Instance.OnModelChanged -= OnWorldModelChanged;
            }
        }

        private void Update()
        {
            UpdateEnvironment();
        }

        /// <summary>
        /// 当世界模型改变时
        /// </summary>
        private void OnWorldModelChanged(int newModel)
        {
            Debug.Log($"[Environment] World model changed to {newModel}");
            UpdateEnvironment();
        }

        /// <summary>
        /// 更新环境（背景和家具）
        /// </summary>
        private void UpdateEnvironment()
        {
            if (GameManager.Instance?.PlayerData == null) return;

            PlayerData data = GameManager.Instance.PlayerData;

            // Update background based on ideal value
            UpdateBackground(data.Stats.Ideal);

            // Update furniture based on stress value
            UpdateFurniture(data.Stats.Stress);
        }

        /// <summary>
        /// 根据理想值更新背景
        /// </summary>
        private void UpdateBackground(int ideal)
        {
            int targetLevel = 0;

            if (ideal >= 150)
                targetLevel = 3; // 幻想期
            else if (ideal >= 100)
                targetLevel = 2; // 兴趣期
            else if (ideal >= 50)
                targetLevel = 1; // 能力期
            else
                targetLevel = 0; // 现实

            if (targetLevel != currentBackgroundLevel)
            {
                ChangeBackground(targetLevel);
            }
        }

        /// <summary>
        /// 改变背景
        /// </summary>
        private void ChangeBackground(int level)
        {
            currentBackgroundLevel = level;

            Sprite targetSprite = null;

            switch (level)
            {
                case 0:
                    targetSprite = normalBackground;
                    Debug.Log("[Environment] 背景: 现实");
                    break;
                case 1:
                    targetSprite = abilityBackground;
                    Debug.Log("[Environment] 背景: 能力期（白领、医生、警察、行商）");
                    break;
                case 2:
                    targetSprite = interestBackground;
                    Debug.Log("[Environment] 背景: 兴趣期（富有、复古、粉丝来信）");
                    break;
                case 3:
                    targetSprite = fantasyBackground;
                    Debug.Log("[Environment] 背景: 幻想期（夸张武器、制服、雕像）");
                    break;
            }

            if (backgroundRenderer && targetSprite)
            {
                if (useSmoothtransition)
                {
                    StartCoroutine(SmoothBackgroundTransition(targetSprite));
                }
                else
                {
                    backgroundRenderer.sprite = targetSprite;
                }
            }
        }

        /// <summary>
        /// 平滑背景过渡
        /// </summary>
        private IEnumerator SmoothBackgroundTransition(Sprite targetSprite)
        {
            if (backgroundRenderer == null) yield break;

            // Fade out
            float elapsed = 0f;
            Color startColor = backgroundRenderer.color;

            while (elapsed < transitionDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / (transitionDuration / 2f));
                backgroundRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            // Change sprite
            backgroundRenderer.sprite = targetSprite;

            // Fade in
            elapsed = 0f;
            while (elapsed < transitionDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / (transitionDuration / 2f));
                backgroundRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            backgroundRenderer.color = startColor;
        }

        /// <summary>
        /// 根据压力值更新家具
        /// </summary>
        private void UpdateFurniture(int stress)
        {
            int targetLevel = 0;

            if (stress >= 150)
                targetLevel = 3; // 心脏
            else if (stress >= 100)
                targetLevel = 2; // 嘴巴
            else if (stress >= 50)
                targetLevel = 1; // 眼睛
            else
                targetLevel = 0; // 无

            if (targetLevel != currentFurnitureLevel)
            {
                ChangeFurniture(targetLevel);
            }
        }

        /// <summary>
        /// 改变家具
        /// </summary>
        private void ChangeFurniture(int level)
        {
            currentFurnitureLevel = level;

            // Hide all first
            if (eyesFurniture) eyesFurniture.SetActive(false);
            if (mouthFurniture) mouthFurniture.SetActive(false);
            if (heartFurniture) heartFurniture.SetActive(false);

            // Show appropriate furniture
            switch (level)
            {
                case 0:
                    Debug.Log("[Environment] 家具: 无");
                    break;
                case 1:
                    if (eyesFurniture) eyesFurniture.SetActive(true);
                    Debug.Log("[Environment] 家具: 眼睛（被他人肯定）");
                    break;
                case 2:
                    if (mouthFurniture) mouthFurniture.SetActive(true);
                    Debug.Log("[Environment] 家具: 嘴巴（他人的评价）");
                    break;
                case 3:
                    if (heartFurniture) heartFurniture.SetActive(true);
                    Debug.Log("[Environment] 家具: 心脏（崇拜的强者）");
                    break;
            }
        }

        /// <summary>
        /// 检查玩家是否靠近特殊家具（负向模型）
        /// </summary>
        public bool IsNearHorrorFurniture(Vector3 playerPosition, float checkRadius = 2f)
        {
            if (WorldModelManager.Instance?.CurrentModel >= 0) return false;

            bool nearFurniture = false;

            if (eyesFurniture && eyesFurniture.activeSelf)
            {
                float distance = Vector3.Distance(playerPosition, eyesFurniture.transform.position);
                if (distance < checkRadius)
                {
                    nearFurniture = true;
                }
            }

            if (mouthFurniture && mouthFurniture.activeSelf)
            {
                float distance = Vector3.Distance(playerPosition, mouthFurniture.transform.position);
                if (distance < checkRadius)
                {
                    nearFurniture = true;
                }
            }

            if (heartFurniture && heartFurniture.activeSelf)
            {
                float distance = Vector3.Distance(playerPosition, heartFurniture.transform.position);
                if (distance < checkRadius)
                {
                    nearFurniture = true;
                }
            }

            return nearFurniture;
        }

        /// <summary>
        /// 设置背景渲染器
        /// </summary>
        public void SetBackgroundRenderer(SpriteRenderer renderer)
        {
            backgroundRenderer = renderer;
        }
    }
}