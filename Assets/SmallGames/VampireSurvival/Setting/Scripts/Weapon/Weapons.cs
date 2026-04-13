using System;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvival.Core;

namespace VampireSurvival.Weapon
{
    /// <summary>
    /// 通用武器基类（可序列化，用于ScriptableObject或代码实例化）
    /// 包含：名字、描述、种类(远程/近程/特殊)、弹射次数、贯穿次数、攻击范围、攻速、伤害、
    /// 可否叠化、叠化对象列表、可否战斗掉落、出现关卡列表，以及常用方法（攻击、解锁检查、叠化检查、关卡检测、敌人死亡钩子等）。
    /// </summary>
    [Serializable]
    public class Weapon
    {
        // 基本信息
        public string WeaponID; // 可用作唯一标识（例如 "pistol_mk1"）
        public string WeaponName;
        [TextArea] public string Description;

        public enum WeaponType 
        {   Melee, //近战
            Ranged, //远程
            Special //特别（例如情绪起爆器等非传统攻击行为的武器）
        }
        public WeaponType Type = WeaponType.Ranged;

        // 射击/效果参数
        [Tooltip("弹射次数（0表示无弹射；多弹射表示一次攻击发射多个弹体）")]
        public int ProjectileCount = 0;

        [Tooltip("贯穿次数（0表示无贯穿；>0 表示穿透敌人数）")]
        public int PierceCount = 0;

        [Tooltip("攻击范围（近程为攻击半径，远程可作为子弹大小）")]
        public float AttackRange = 1f;

        [Tooltip("每次攻击间隔（秒），即攻击速度的倒数")]
        public float AttackSpeed = 1f;

        [Tooltip("基础伤害值（可由属性/外部系统叠加）")]
        public int Damage = 1;

        // 叠化（合成/进化）
        public bool CanStack = false;
        [Tooltip("可用于叠化/合成的武器ID列表（WeaponID）")]
        public List<string> StackableWith = new List<string>();

        // 掉落与出现
        public bool CanDropInCombat = true;
        [Tooltip("出现于哪些关卡（格式：\"1-1\",\"2-3\" 等）")]
        public List<string> AppearInLevels = new List<string>();

        // 解锁条件（基于玩家属性）
        public VampireSurvivalData.AttributeType? UnlockAttribute = null;
        public int UnlockRequirement = 0;

        // 可扩展的特殊行为钩子（例如 情绪起爆器 在敌人死亡时触发）
        public virtual void OnEnemyDeath(GameObject deadEnemy)
        {
            // 默认无特殊行为。子类或具体武器可以重写。
        }

        /// <summary>
        /// 默认攻击实现。子类或具体武器应 override 以实现独特行为（投射子弹、近战判定、范围/触发等）。
        /// 这里提供一个通用流程：触发攻击日志、尝试对目标造成伤害（若存在 Health 组件）。
        /// </summary>
        /// <param name="attacker">攻击者对象（可为玩家）</param>
        /// <param name="target">目标对象（可为单体目标，若为null则视为范围/发射行为）</param>
        public virtual void Attack(GameObject attacker, GameObject target = null)
        {
            // 简单的单体伤害处理（仅作默认实现）
            if (target != null)
            {
                Debug.Log($"{WeaponName} 攻击 {target.name}，伤害 {Damage}，弹射 {ProjectileCount}，贯穿 {PierceCount}。");
                // 尝试对目标造成伤害
            }
            else
            {
                // 无指定目标（例如发射弹体或造成范围伤害）——由子类实现更具体的行为
                Debug.Log($"{WeaponName} 发起攻击（无指定目标）——请在子类中实现弹道/范围逻辑。");
            }
        }

        /// <summary>
        /// 检查玩家是否满足本武器的解锁条件（若未设置解锁属性则视为已解锁）。
        /// 使用 VampireSurvival.Core.VampireSurvivalData.instance 中的属性值进行判断。
        /// </summary>
        public virtual bool IsUnlocked()
        {
            try
            {
                if (UnlockAttribute == null) return true;
                var data = VampireSurvivalData.instance;
                if (data == null) return false;

                var stat = data.GetStat(UnlockAttribute.Value);
                if (stat == null) return false;

                return stat.Current >= UnlockRequirement;
            }
            catch (Exception ex)
            {
                Debug.LogError($"IsUnlocked 检查出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 判断是否能与指定武器（通过 ID）叠化/合成
        /// </summary>
        public virtual bool CanStackWith(string otherWeaponID)
        {
            if (!CanStack) return false;
            if (string.IsNullOrEmpty(otherWeaponID)) return false;
            return StackableWith != null && StackableWith.Contains(otherWeaponID);
        }

        /// <summary>
        /// 判断武器是否会在指定关卡出现。接受关卡格式为 "1-1" 或 (major,minor) 两种重载。
        /// </summary>
        public virtual bool AppearsInLevel(string levelId)
        {
            if (string.IsNullOrEmpty(levelId)) return false;
            return AppearInLevels != null && AppearInLevels.Contains(levelId);
        }

        public virtual bool AppearsInLevel(int major, int minor)
        {
            return AppearsInLevel($"{major}-{minor}");
        }

        /// <summary>
        /// 获取简短信息（用于UI显示/调试）
        /// </summary>
        public virtual string GetSummary()
        {
            return $"{WeaponName} [{Type}] DMG:{Damage} AS:{AttackSpeed}s RNG:{AttackRange} PRJ:{ProjectileCount} PIERCE:{PierceCount}";
        }
    }
}