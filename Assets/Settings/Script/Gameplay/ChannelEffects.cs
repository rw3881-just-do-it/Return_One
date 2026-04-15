using UnityEngine;
using UnityTV.Core;
using UnityTV.Player;
using System.Collections.Generic;

namespace UnityTV.Gameplay
{
    /// <summary>
    /// 频道效果静态类 - 处理所有频道的效果逻辑
    /// Static utility class for TV channel effects
    /// </summary>
    public static class ChannelEffects
    {
        /// <summary>
        /// 频道1: 随机1个属性+3
        /// Channel 1: Random attribute +3
        /// </summary>
        public static void ApplyChannel1(PlayerData playerData)
        {
            if (playerData == null) return;

            // 随机选择一个属性
            int random = Random.Range(0, 6);
            string attributeName = "";

            switch (random)
            {
                case 0:
                    playerData.UpdateStats(strength: 3);
                    attributeName = "力量";
                    break;
                case 1:
                    playerData.UpdateStats(intelligence: 3);
                    attributeName = "智力";
                    break;
                case 2:
                    playerData.UpdateStats(agility: 3);
                    attributeName = "敏捷";
                    break;
                case 3:
                    playerData.UpdateStats(perception: 3);
                    attributeName = "见闻";
                    break;
                case 4:
                    playerData.UpdateStats(dexterity: 3);
                    attributeName = "巧手";
                    break;
                case 5:
                    playerData.UpdateStats(courage: 3);
                    attributeName = "勇气";
                    break;
            }

            Debug.Log($"[Channel1] 随机增加 {attributeName} +3");
        }

        /// <summary>
        /// 频道2: 指定1个属性+5, 压力-10 (4回合冷却)
        /// Channel 2: Selected attribute +5, Stress -10 (4 turn cooldown)
        /// </summary>
        public static void ApplyChannel2(PlayerData playerData, string selectedAttribute)
        {
            if (playerData == null) return;

            // 应用选中的属性加成
            switch (selectedAttribute.ToLower())
            {
                case "strength":
                case "力量":
                    playerData.UpdateStats(strength: 5);
                    break;
                case "intelligence":
                case "智力":
                    playerData.UpdateStats(intelligence: 5);
                    break;
                case "agility":
                case "敏捷":
                    playerData.UpdateStats(agility: 5);
                    break;
                case "perception":
                case "见闻":
                    playerData.UpdateStats(perception: 5);
                    break;
                case "dexterity":
                case "巧手":
                    playerData.UpdateStats(dexterity: 5);
                    break;
                case "courage":
                case "勇气":
                    playerData.UpdateStats(courage: 5);
                    break;
            }

            // 检查压力减少冷却
            if (playerData.Stats.TurnsSinceStressReduction >= 4)
            {
                playerData.UpdateStats(stress: -10);
                playerData.Stats.TurnsSinceStressReduction = 0;
                Debug.Log("[Channel2] 压力 -10 (冷却重置)");
            }
            else
            {
                int turnsLeft = 4 - playerData.Stats.TurnsSinceStressReduction;
                Debug.Log($"[Channel2] 压力减少冷却中 (还需 {turnsLeft} 回合)");
            }

            Debug.Log($"[Channel2] {selectedAttribute} +5");
        }

        /// <summary>
        /// 频道3: 要求最高的1个属性+6, 压力+6, 理想+5
        /// Channel 3: Highest requirement attribute +6, Stress +6, Ideal +5
        /// </summary>
        public static void ApplyChannel3(PlayerData playerData)
        {
            if (playerData == null || playerData.CareerGoal == null) return;

            // 获取最高需求的属性
            var topAttributes = playerData.Stats.GetTopNRequiredAttributes(playerData.CareerGoal, 1);

            if (topAttributes.Count > 0)
            {
                ApplyAttributeBonus(playerData, topAttributes[0], 6);
            }

            playerData.UpdateStats(stress: 6, ideal: 5);

            Debug.Log($"[Channel3] 最高需求属性 +6, 压力 +6, 理想 +5");
        }

        /// <summary>
        /// 频道4: 要求最高的2个属性+7, 压力+14, 理想+10
        /// Channel 4: Top 2 requirement attributes +7, Stress +14, Ideal +10
        /// </summary>
        public static void ApplyChannel4(PlayerData playerData)
        {
            if (playerData == null || playerData.CareerGoal == null) return;

            // 获取最高需求的2个属性
            var topAttributes = playerData.Stats.GetTopNRequiredAttributes(playerData.CareerGoal, 2);

            foreach (var attr in topAttributes)
            {
                ApplyAttributeBonus(playerData, attr, 7);
            }

            playerData.UpdateStats(stress: 14, ideal: 10);

            Debug.Log($"[Channel4] 最高需求2属性 +7, 压力 +14, 理想 +10");
        }

        /// <summary>
        /// 频道5: 要求最高的3个属性+8, 压力+24, 理想+15
        /// Channel 5: Top 3 requirement attributes +8, Stress +24, Ideal +15
        /// </summary>
        public static void ApplyChannel5(PlayerData playerData)
        {
            if (playerData == null || playerData.CareerGoal == null) return;

            // 获取最高需求的3个属性
            var topAttributes = playerData.Stats.GetTopNRequiredAttributes(playerData.CareerGoal, 3);

            foreach (var attr in topAttributes)
            {
                ApplyAttributeBonus(playerData, attr, 8);
            }

            playerData.UpdateStats(stress: 24, ideal: 15);

            Debug.Log($"[Channel5] 最高需求3属性 +8, 压力 +24, 理想 +15");
        }

        /// <summary>
        /// 频道6: 理想值增益(根据解锁频道), 压力-20
        /// Channel 6: Ideal bonus (based on unlocked channels), Stress -20
        /// </summary>
        public static void ApplyChannel6Watched(PlayerData playerData)
        {
            if (playerData == null) return;

            int idealGain = 0;

            // 根据解锁的频道确定理想值增益
            if (playerData.UnlockedChannels.Contains(5))
            {
                idealGain = 20; // 频道5解锁
            }
            else if (playerData.UnlockedChannels.Contains(4))
            {
                idealGain = 10; // 频道4解锁
            }
            else if (playerData.UnlockedChannels.Contains(3))
            {
                idealGain = 5; // 频道3解锁
            }

            playerData.UpdateStats(ideal: idealGain, stress: -20);

            Debug.Log($"[Channel6] 观看战斗: 理想 +{idealGain}, 压力 -20");
        }

        /// <summary>
        /// 不观看频道6的惩罚
        /// Penalty for NOT watching Channel 6
        /// </summary>
        public static void ApplyChannel6Skipped(PlayerData playerData)
        {
            if (playerData == null) return;

            playerData.UpdateStats(ideal: -20, stress: -20);

            Debug.Log("[Channel6] 未观看战斗: 理想 -20, 压力 -20");
        }

        /// <summary>
        /// 辅助方法: 根据属性名称应用加成
        /// Helper: Apply attribute bonus by name
        /// </summary>
        private static void ApplyAttributeBonus(PlayerData playerData, string attributeName, int bonus)
        {
            switch (attributeName)
            {
                case "Strength":
                    playerData.UpdateStats(strength: bonus);
                    break;
                case "Intelligence":
                    playerData.UpdateStats(intelligence: bonus);
                    break;
                case "Agility":
                    playerData.UpdateStats(agility: bonus);
                    break;
                case "Perception":
                    playerData.UpdateStats(perception: bonus);
                    break;
                case "Dexterity":
                    playerData.UpdateStats(dexterity: bonus);
                    break;
                case "Courage":
                    playerData.UpdateStats(courage: bonus);
                    break;
            }

            Debug.Log($"  → {attributeName} +{bonus}");
        }

        /// <summary>
        /// 检查频道是否解锁
        /// Check if channel is unlocked
        /// </summary>
        public static bool IsChannelUnlocked(int channelNumber, PlayerData playerData)
        {
            if (playerData == null) return false;

            switch (channelNumber)
            {
                case 1:
                case 2:
                    return true; // 始终解锁

                case 3:
                    return playerData.UnlockedChannels.Contains(3);

                case 4:
                    return playerData.UnlockedChannels.Contains(4);

                case 5:
                    return playerData.UnlockedChannels.Contains(5);

                case 6:
                    return false; // 频道6锁定 - 伙伴的游戏

                default:
                    return false;
            }
        }
    }
}