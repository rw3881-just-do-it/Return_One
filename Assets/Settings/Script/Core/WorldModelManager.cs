using UnityEngine;
using UnityTV.Player;
using System;

namespace UnityTV.Core
{
    /// <summary>
    /// 世界模型管理器 - 核心状态机
    /// Manages 7 world models: -3, -2, -1, 0, 1, 2, 3
    /// </summary>
    public class WorldModelManager : MonoBehaviour
    {
        public static WorldModelManager Instance { get; private set; }

        // Current world model (-3 to +3)
        public int CurrentModel { get; private set; } = 0;

        // Events
        public event Action<int> OnModelChanged;

        // Thresholds
        private const int IDEAL_THRESHOLD_1 = 50;   // Model +1
        private const int IDEAL_THRESHOLD_2 = 100;  // Model +2
        private const int IDEAL_THRESHOLD_3 = 150;  // Model +3

        private const int STRESS_THRESHOLD_1 = 50;  // Model -1
        private const int STRESS_THRESHOLD_2 = 100; // Model -2
        private const int STRESS_THRESHOLD_3 = 150; // Model -3

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            UpdateModelBasedOnStats();
        }

        /// <summary>
        /// 根据理想值和压力值自动更新世界模型
        /// </summary>
        private void UpdateModelBasedOnStats()
        {
            if (GameManager.Instance?.PlayerData == null) return;

            PlayerData data = GameManager.Instance.PlayerData;
            int newModel = CalculateModel(data.Stats.Ideal, data.Stats.Stress);

            if (newModel != CurrentModel)
            {
                ChangeModel(newModel);
            }
        }

        /// <summary>
        /// 计算应该处于哪个模型
        /// 理想值优先于压力值（正向压倒负向）
        /// </summary>
        private int CalculateModel(int ideal, int stress)
        {
            // 理想值判断（正向模型）
            if (ideal >= IDEAL_THRESHOLD_3)
                return 3;
            else if (ideal >= IDEAL_THRESHOLD_2)
                return 2;
            else if (ideal >= IDEAL_THRESHOLD_1)
                return 1;

            // 压力值判断（负向模型）
            if (stress >= STRESS_THRESHOLD_3)
                return -3;
            else if (stress >= STRESS_THRESHOLD_2)
                return -2;
            else if (stress >= STRESS_THRESHOLD_1)
                return -1;

            // 默认模型0
            return 0;
        }

        /// <summary>
        /// 强制改变模型
        /// </summary>
        public void ForceChangeModel(int model)
        {
            model = Mathf.Clamp(model, -3, 3);
            if (model != CurrentModel)
            {
                ChangeModel(model);
            }
        }

        /// <summary>
        /// 改变模型并触发事件
        /// </summary>
        private void ChangeModel(int newModel)
        {
            int oldModel = CurrentModel;
            CurrentModel = newModel;

            Debug.Log($"[WorldModel] Changed from Model {oldModel} to Model {newModel}");

            OnModelChanged?.Invoke(CurrentModel);

            // Apply model-specific effects
            ApplyModelEffects(CurrentModel);
        }

        /// <summary>
        /// 应用模型特定效果
        /// </summary>
        private void ApplyModelEffects(int model)
        {
            switch (model)
            {
                case -3:
                    Debug.Log("[WorldModel] 进入模型-3: 极度压力状态");
                    break;
                case -2:
                    Debug.Log("[WorldModel] 进入模型-2: 高压力状态");
                    break;
                case -1:
                    Debug.Log("[WorldModel] 进入模型-1: 压力状态");
                    break;
                case 0:
                    Debug.Log("[WorldModel] 进入模型0: 现实状态");
                    break;
                case 1:
                    Debug.Log("[WorldModel] 进入模型1: 能力期理想");
                    break;
                case 2:
                    Debug.Log("[WorldModel] 进入模型2: 兴趣期理想");
                    break;
                case 3:
                    Debug.Log("[WorldModel] 进入模型3: 幻想期理想");
                    break;
            }
        }

        /// <summary>
        /// 是否是正向模型（理想驱动）
        /// </summary>
        public bool IsPositiveModel()
        {
            return CurrentModel > 0;
        }

        /// <summary>
        /// 是否是负向模型（压力驱动）
        /// </summary>
        public bool IsNegativeModel()
        {
            return CurrentModel < 0;
        }

        /// <summary>
        /// 获取模型名称
        /// </summary>
        public string GetModelName()
        {
            switch (CurrentModel)
            {
                case -3: return "幻影期";
                case -2: return "焦虑期";
                case -1: return "压力期";
                case 0: return "现实";
                case 1: return "能力期";
                case 2: return "兴趣期";
                case 3: return "幻想期";
                default: return "未知";
            }
        }
    }
}