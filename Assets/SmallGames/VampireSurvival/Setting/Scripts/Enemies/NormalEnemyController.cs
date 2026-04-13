using UnityEngine;
using VampireSurvival.Enemies;
using VampireSurvival.Player;
using VampireSurvival.Core;
using System;


namespace VampireSurvival.Enemies
{
    /// <summary>
    /// 该类实现具体敌人的行为逻辑，包括移动、攻击、受击处理等
    /// </summary>
    public class NormalEnemyController : Enemies
    {
        private Transform target;

        [Header("Drop")]
        [SerializeField] private GameObject dropPrefab;
        [SerializeField] private float dropChance;

        public event Action OnEnemyDefeated; // 敌人被击败时触发的事件，供外部订阅（例如更新UI、触发关卡事件等）

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            currentHP = maxHP;
            dropChance = VampireSurvivalData.baseDropRate;

            target = PlayerHealthController.instance?.transform;

            OnEnemyDefeated += UpdateScore; // 订阅敌人被击败事件，更新分数
            OnEnemyDefeated += SpawnDrop; // 订阅敌人被击败事件，处理掉落逻辑
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Move();
        }

        /// <summary>
        /// 实现怪物的移动逻辑，向玩家方向移动
        /// </summary>
        public override void Move()
        {
            if (target == null) return;
            Vector2 moveDir = (target.position - transform.position).normalized;
            transform.Translate(moveDir * moveSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 敌人受击处理
        /// </summary>
        public override void TakeDamage(int damage)
        {
            currentHP -= damage;
            if (currentHP <= 0)
            {
                OnEnemyDefeated?.Invoke();
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 更新分数
        /// </summary>
        public override void UpdateScore()
        {
            // 注意：若项目中有 GameManager 或分数系统，请在此处替换为真实调用
            Debug.Log($"Enemy defeated. Add score: {scoreValue}");

            if (VampireSurvivalData.instance != null)
            {
                VampireSurvivalData.instance.Credit += scoreValue;
            }
            else
            {
                Debug.LogWarning("VampireSurvivalData instance not found. Score not updated.");
            }
        }

        /// <summary>
        /// 几率掉落物品
        /// </summary>
        private void SpawnDrop()
        {
            if (dropPrefab == null) return;
            if (UnityEngine.Random.value <= dropChance)
            {
                Instantiate(dropPrefab, transform.position, Quaternion.identity);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision?.collider == null) return;
            if (collision.collider.CompareTag("Player"))
            {
                // 直接委托给 Player 的 TakeDamage 方法处理受击无敌等逻辑
                if (PlayerHealthController.instance != null)
                {
                    PlayerHealthController.instance.TakeDamage(collisionDamage);
                }
                else
                {
                    Debug.LogWarning("PlayerHealthController instance not found. Cannot apply damage to player.");
                }
            }
        }

        // 移除 OnCollisionStay2D 中的伤害应用，避免重复伤害逻辑由玩家端控制
        private void OnCollisionStay2D(Collision2D collision)
        {
            // 在持续碰撞期间也触发伤害调用。
            // 真实是否扣血由 PlayerHealthController.instance 的 isHit / isInSprint 控制，
            // 所以这里可每帧尝试一次伤害调用，玩家脚本会处理无敌窗口以避免瞬间重复扣血。
            if (collision?.collider == null) return;
            if (!collision.collider.CompareTag("Player")) return;

            if (PlayerHealthController.instance != null)
            {
                PlayerHealthController.instance.TakeDamage(collisionDamage);
            }
            else
            {
                Debug.LogWarning("PlayerHealthController instance not found. Cannot apply damage to player.");
            }
        }
    }

}