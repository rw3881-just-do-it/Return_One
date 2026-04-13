using System;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvival.Core;

namespace VampireSurvival.Player
{
    /// <summary>
    /// 该类实现玩家的血量管理逻辑，包括血量的增加和减少、血量UI的更新、死亡处理等
    /// </summary>
    public class PlayerHealthController : MonoBehaviour
    {
        public static PlayerHealthController instance;

        public Slider healthSlider;

        public int maxHP = 20;
        public int currentHP;  // 当前血量，公开以便特殊情况的访问和修改

        public bool isHit; // 控制持续扣血的频率
        public bool isInSprint; // 冲刺状态

        public int CurrentHealth
        {
            get { return currentHP; }
            set
            {
                if (currentHP == value) return;
                currentHP = value;
                OnHealthChanged?.Invoke(currentHP);   // 触发事件
            }
        }

        public event Action<int> OnHealthChanged;
        public event Action OnDeath;

        private void Awake()
        {
            instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            OnHealthChanged += UpdateHealthSlider; 
            OnHealthChanged += Ondeath; 



            healthSlider.maxValue = maxHP;
            CurrentHealth = maxHP;
        }

        //处理受到伤害的逻辑
        public void TakeDamage(int damage)
        {
            Debug.Log($"{gameObject.name} takes {damage} damage");

            if (isHit || isInSprint)
            {
                Debug.Log("Player is currently invulnerable due to recent hit or sprinting. No damage taken.");
                return; // 如果正在持续扣血或冲刺中，暂时不再扣血
            }
            else
            {
                CurrentHealth -= damage;
                isHit = true; // 开始持续扣血的计时
            }
        }

        //更新滑条显示当前血量
        private void UpdateHealthSlider(int newHP)
        {
            healthSlider.value = newHP;
        }

        //处理死亡逻辑
        private void Ondeath(int newHP)
        {
            if (newHP <= 0)
            {
                Debug.Log("player died");

                //完善死亡逻辑，例如播放动画、重置场景等
                OnDeath?.Invoke();

            }
        }

        private void OnDisable()
        {
            OnHealthChanged -= UpdateHealthSlider;
            OnHealthChanged -= Ondeath;
        }
    }
}
