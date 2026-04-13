using UnityEngine;
using System;
using System.Collections.Generic;

namespace VampireSurvival.Core
{
    [System.Serializable]
    public class VampireSurvivalData
    {
        public static VampireSurvivalData instance { get; private set; } = new VampireSurvivalData();

        [Header("Base Player Stats")]
        public const int baseMaxHealth = 20;// 基础最大生命值
        public const int baseAttack = 0; // 基础攻击力
        public const int baseSprintCount = 0; // 可叠加的冲刺次数
        public const float baseSprintRecoveryTime = 2f; // 冲刺恢复时间（秒）,不受影响
        public const float baseSprintContinueTime = 1f; // 冲刺持续时间，不受影响
        public const float baseMoveSpeed = 2f;// 基础移动速度

        [Header("Base Drop")]
        public const float baseDropRate = 0.05f; // 基础掉落率

        [Header("Influenced Value")]
        public int maxHealth = baseMaxHealth; // 当前最大生命值，受勇气属性影响
        public float moveSpeed = baseMoveSpeed; // 当前移动速度，受敏捷属性影响
        public int sprintCount = baseSprintCount; // 当前可用的冲刺次数，受敏捷属性影响
        public int Attack = baseAttack; // 当前攻击力，受力量属性影响

        [Header("Attributes (current / max, requirement)")]
        public AttributeStat Strength = new AttributeStat { Name = "Strength", Current = 10, Max = 100};
        public AttributeStat Intelligence = new AttributeStat { Name = "Intelligence", Current = 10, Max = 100};
        public AttributeStat Agility = new AttributeStat { Name = "Agility", Current = 10, Max = 100};
        public AttributeStat Insight = new AttributeStat { Name = "Insight", Current = 10, Max = 100};
        public AttributeStat Dexterity = new AttributeStat { Name = "Dexterity", Current = 10, Max = 100};
        public AttributeStat Courage = new AttributeStat { Name = "Courage", Current = 10, Max = 100};

        [Header("Credit")]
        public int Credit = 0;

        [Header("Mission")]
        public string CurrentMission = "1-1";

        [Header("Time")]
        public float TimePast = 0f;

        // Event invoked when any attribute changes
        public event Action<AttributeType, AttributeStat> OnAttributeChanged;

        [System.Serializable]
        public class AttributeStat
        {
            public string Name;
            public int Current = 10;
            public int Max = 100;

            /// <summary>
            /// 实现一个方法来设置当前属性值，确保它不会超过最大值或低于0。
            /// </summary>
            /// <param name="value"></param>
            public virtual void Set(int value)
            {
                Current = Mathf.Clamp(value, 0, Max);
            }

            /// <summary>
            /// 实现一个方法来修改当前属性值，确保它不会超过最大值或低于0。
            /// </summary>
            /// <param name="delta"></param>
            public virtual void Modify(int delta)
            {
                Current = Mathf.Clamp(Current + delta, 0, Max);
            }
        }

        /// <summary>
        /// 对外提供一个方法来设置属性值，并在修改后触发事件通知UI或其他系统更新显示。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void SetAttribute(AttributeType type, int value)
        {
            var stat = GetStat(type);
            if (stat == null) return;

            stat.Set(value);

            switch(type)
            {
                case AttributeType.Courage:
                    UpdateMaxHealthFromCourage(); // 勇气改变时更新最大生命值
                    break;
                case AttributeType.Strength:
                    UpdateAttackFromStrength(); // 力量改变时更新攻击力
                    break;
                case AttributeType.Intelligence:
                    break;
                case AttributeType.Agility:
                    break;
                case AttributeType.Insight:
                    break;
                case AttributeType.Dexterity:
                    break;
            }
            OnAttributeChanged?.Invoke(type, stat);
        }

        /// <summary>
        /// 对外提供一个方法来修改属性值，并在修改后触发事件通知UI或其他系统更新显示。
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <param name="delta">属性变化值</param>
        public void ModifyAttribute(AttributeType type, int delta)
        {
            var stat = GetStat(type);
            if (stat == null) return;

            stat.Modify(delta);

            // If courage changed, update health cap
            switch (type)
            {
                case AttributeType.Courage:
                    // 勇气改变时更新最大生命值
                    UpdateMaxHealthFromCourage(); 
                    break;
                case AttributeType.Strength:
                    // 力量改变时更新攻击力
                    UpdateAttackFromStrength(); 
                    break;
                case AttributeType.Intelligence:
                    break;
                case AttributeType.Agility:
                    break;
                case AttributeType.Insight:
                    break;
                case AttributeType.Dexterity:
                    break;

            }

            OnAttributeChanged?.Invoke(type, stat);
        }

        /// <summary>
        /// 获得指定属性类型的属性数据
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <returns>对应的属性数据</returns>
        public AttributeStat GetStat(AttributeType type)
        {
            return type switch
            {
                AttributeType.Strength => Strength,
                AttributeType.Intelligence => Intelligence,
                AttributeType.Agility => Agility,
                AttributeType.Insight => Insight,
                AttributeType.Dexterity => Dexterity,
                AttributeType.Courage => Courage,
                _ => null,
            };
        }

        /// <summary>
        /// 根据勇气属性动态调整最大生命值
        /// </summary>
        private void UpdateMaxHealthFromCourage()
        {
            if (Courage.Current >= 20)
            {
                maxHealth = baseMaxHealth + Courage.Current / 10 * 5; // 每10点勇气增加5点最大生命值
            }
            else
            {
                maxHealth = baseMaxHealth; // 勇气不足20点时保持基础最大生命值
            }
        }

        /// <summary>
        /// 根据力量属性动态调整攻击力
        /// </summary>
        private void UpdateAttackFromStrength()
        {
            // 根据力量属性调整攻击力的逻辑
            if (Strength.Current >= 20)
            {
                Attack = (Strength.Current - 10) / 10; // 每10点力量增加1点攻击力
            }
            else
            {
                Attack = 0; // 力量不足20点时保持基础攻击力
            }
        }

        /// <summary>
        /// 属性枚举
        /// </summary>
        public enum AttributeType
        {
            Strength,    // 力量 -> 攻击力
            Intelligence,// 智力 -> 可选buff
            Agility,     // 敏捷 -> 移动速度 / 闪避
            Insight,     // 见闻 -> 商店武器等级
            Dexterity,   // 巧手 -> 可叠加武器数量
            Courage      // 勇气 -> 血量
        }

        /// <summary>
        /// 依据关卡名字确定当前关卡的最大敌人生成数量，供敌人生成系统调用
        /// </summary>
        public Dictionary<string, int> MissionNum = new()
        {
            { "1-1", 100 },
            { "1-2", 200 },
            { "1-3", 300 },
            { "2-1", 300 },
            { "2-2", 400 },
            { "2-3", 500 },
            { "3-1", 600 },
            { "3-2", 700 },
            { "3-3", 1000 }
        };
    }
}

