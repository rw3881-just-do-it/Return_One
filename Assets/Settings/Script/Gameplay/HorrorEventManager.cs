using UnityEngine;
using UnityTV.Core;
using UnityTV.Player;
using System.Collections;

namespace UnityTV.Gameplay
{
    /// <summary>
    /// 恐怖事件管理器
    /// Manages horror events: knocking, heartbeat, special triggers
    /// </summary>
    public class HorrorEventManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource knockSoundSource;
        [SerializeField] private AudioSource heartbeatSoundSource;

        [Header("Event Settings")]
        [SerializeField] private float knockChancePerMinute = 0.3f; // 30% chance per minute
        [SerializeField] private float heartbeatChancePerMinute = 0.5f; // 50% chance per minute

        [Header("Audio Clips")]
        [SerializeField] private AudioClip knockSound;
        [SerializeField] private AudioClip heartbeatSound;

        // Event tracking
        private float knockTimer = 0f;
        private float heartbeatTimer = 0f;
        private bool knockEventActive = false;
        private bool heartbeatEventActive = false;

        // References
        private LivingRoomController livingRoomController;

        private void Start()
        {
            livingRoomController = FindObjectOfType<LivingRoomController>();

            // Create audio sources if not assigned
            if (knockSoundSource == null)
            {
                GameObject knockObj = new GameObject("KnockSound");
                knockObj.transform.SetParent(transform);
                knockSoundSource = knockObj.AddComponent<AudioSource>();
                knockSoundSource.playOnAwake = false;
                knockSoundSource.spatialBlend = 0; // 2D sound
            }

            if (heartbeatSoundSource == null)
            {
                GameObject heartbeatObj = new GameObject("HeartbeatSound");
                heartbeatObj.transform.SetParent(transform);
                heartbeatSoundSource = heartbeatObj.AddComponent<AudioSource>();
                heartbeatSoundSource.playOnAwake = false;
                heartbeatSoundSource.spatialBlend = 0;
                heartbeatSoundSource.loop = true; // Heartbeat loops
            }
        }

        private void Update()
        {
            if (WorldModelManager.Instance == null) return;

            int currentModel = WorldModelManager.Instance.CurrentModel;

            // Knock sound events (all models)
            CheckKnockEvent(currentModel);

            // Heartbeat sound events (negative models only)
            if (currentModel < 0)
            {
                CheckHeartbeatEvent(currentModel);
            }
        }

        /// <summary>
        /// 检查敲门声事件
        /// </summary>
        private void CheckKnockEvent(int model)
        {
            knockTimer += Time.deltaTime;

            // Check every 60 seconds
            if (knockTimer >= 60f)
            {
                knockTimer = 0f;

                // Random chance based on model
                float chance = Random.value;
                bool shouldKnock = false;

                switch (model)
                {
                    case 0:
                        shouldKnock = chance < knockChancePerMinute; // 30% chance
                        break;
                    case 1:
                        shouldKnock = chance < knockChancePerMinute; // 30% chance
                        break;
                    case 2:
                        shouldKnock = true; // 必然响起
                        break;
                    case 3:
                        shouldKnock = true; // 一定会听到
                        break;
                }

                if (shouldKnock)
                {
                    TriggerKnockEvent(model);
                }
            }
        }

        /// <summary>
        /// 检查心跳声事件（负向模型）
        /// </summary>
        private void CheckHeartbeatEvent(int model)
        {
            heartbeatTimer += Time.deltaTime;

            if (heartbeatTimer >= 60f)
            {
                heartbeatTimer = 0f;

                float chance = Random.value;
                if (chance < heartbeatChancePerMinute)
                {
                    TriggerHeartbeatEvent();
                }
            }
        }

        /// <summary>
        /// 触发敲门声事件
        /// </summary>
        private void TriggerKnockEvent(int model)
        {
            Debug.Log($"[Horror] 敲门声触发! 模型: {model}");

            if (knockSoundSource && knockSound)
            {
                knockSoundSource.PlayOneShot(knockSound);
            }

            knockEventActive = true;

            // Show door exclamation mark
            if (livingRoomController != null)
            {
                livingRoomController.ShowDoorExclamation();
            }

            // Start coroutine to wait for player response
            StartCoroutine(WaitForKnockResponse(model));
        }

        /// <summary>
        /// 等待玩家对敲门声的反应
        /// </summary>
        private IEnumerator WaitForKnockResponse(int model)
        {
            float timeout = 30f; // 30秒超时
            float elapsed = 0f;

            while (elapsed < timeout && knockEventActive)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 如果玩家没有去开门
            if (knockEventActive)
            {
                Debug.Log("[Horror] 玩家忽略了敲门声");

                // 负向模型: 不开门会增加压力
                if (model < 0)
                {
                    GameManager.Instance?.PlayerData?.UpdateStats(stress: 10);
                }

                knockEventActive = false;
            }
        }

        /// <summary>
        /// 玩家开门响应（由LivingRoomController调用）
        /// </summary>
        public void OnPlayerOpenedDoor(int model)
        {
            knockEventActive = false;

            switch (model)
            {
                case 0:
                    // 模型0: 从左边重新出现，切换到模型1
                    Debug.Log("[Horror] 模型0开门 → 玩家传送，切换到模型1");
                    WorldModelManager.Instance?.ForceChangeModel(1);
                    // TODO: Teleport player to left side
                    break;

                case 1:
                    // 模型1: 空无一人对话框，理想+50压力-50
                    Debug.Log("[Horror] 模型1开门 → 空无一人对话");
                    GameManager.Instance?.PlayerData?.UpdateStats(ideal: 50, stress: -50);
                    // TODO: Show empty visitor dialogue
                    break;

                case 2:
                    // 模型2: 循环2次场景，理想+100压力-50
                    Debug.Log("[Horror] 模型2开门 → 场景循环");
                    GameManager.Instance?.PlayerData?.UpdateStats(ideal: 100, stress: -50);
                    // TODO: Loop scene twice
                    break;

                case 3:
                    // 模型3: 另一个自己 → 结局
                    Debug.Log("[Horror] 模型3开门 → 结局【理想的我】");
                    // TODO: Load ending scene
                    break;
            }
        }

        /// <summary>
        /// 触发心跳声事件（负向模型）
        /// </summary>
        private void TriggerHeartbeatEvent()
        {
            Debug.Log("[Horror] 心跳声触发!");

            if (heartbeatSoundSource && heartbeatSound)
            {
                heartbeatSoundSource.clip = heartbeatSound;
                heartbeatSoundSource.Play();
            }

            heartbeatEventActive = true;

            // Show UI indicator
            // TODO: Add heartbeat visual indicator

            StartCoroutine(WaitForHeartbeatResponse());
        }

        /// <summary>
        /// 等待玩家对心跳声的反应
        /// </summary>
        private IEnumerator WaitForHeartbeatResponse()
        {
            float timeout = 20f;
            float elapsed = 0f;

            while (elapsed < timeout && heartbeatEventActive)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (heartbeatEventActive)
            {
                // 玩家没有看电视（频道2）
                Debug.Log("[Horror] 玩家忽略心跳声 → 压力+10");
                GameManager.Instance?.PlayerData?.UpdateStats(stress: 10);
            }

            heartbeatEventActive = false;

            if (heartbeatSoundSource)
            {
                heartbeatSoundSource.Stop();
            }
        }

        /// <summary>
        /// 玩家观看频道2响应（由TVInterfaceController调用）
        /// </summary>
        public void OnPlayerWatchedChannel2()
        {
            if (heartbeatEventActive)
            {
                Debug.Log("[Horror] 玩家观看频道2 → 压力-30, 理想+20");
                GameManager.Instance?.PlayerData?.UpdateStats(stress: -30, ideal: 20);

                heartbeatEventActive = false;

                if (heartbeatSoundSource)
                {
                    heartbeatSoundSource.Stop();
                }
            }
        }

        /// <summary>
        /// 强制触发敲门声（用于测试）
        /// </summary>
        public void ForceKnockEvent()
        {
            int model = WorldModelManager.Instance?.CurrentModel ?? 0;
            TriggerKnockEvent(model);
        }

        /// <summary>
        /// 强制触发心跳声（用于测试）
        /// </summary>
        public void ForceHeartbeatEvent()
        {
            TriggerHeartbeatEvent();
        }
    }
}