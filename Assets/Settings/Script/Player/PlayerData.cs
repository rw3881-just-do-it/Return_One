using UnityEngine;
using System;
using System.Collections.Generic;

namespace UnityTV.Player
{
    /// <summary>
    /// Core player data class - stores all player information and progress
    /// Serializable for save/load functionality
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        [Header("Personal Information")]
        public string PlayerName;
        public CareerType CareerChoice;
        public HousingType HousingPreference;
        public FearType FearChoice;
        public bool HasPartner;

        [Header("Current Stats")]
        public PlayerStats Stats;

        [Header("Career Progress")]
        public CareerGoal CareerGoal;

        [Header("Inventory")]
        public PlayerInventory Inventory;

        [Header("Progress Tracking")]
        public int CurrentDay = 1;
        public TimeOfDay CurrentTimeOfDay = TimeOfDay.Morning;
        public bool AnchiloVisited = false; // First NPC visit flag
        public bool FullSystemsUnlocked = false;

        [Header("Relationships")]
        public Dictionary<string, int> NPCRelationships;
        public int PartnerRelationship; // Only if HasPartner = true

        [Header("Unlocked Content")]
        public List<int> UnlockedChannels;
        public bool Channel6Unlocked = false; // Channel 6 locked - partner's game

        [Header("Completed Events")]
        public List<string> CompletedEvents;

        // Events
        public event Action<PlayerStats> OnStatsChanged;
        public event Action<string> OnEventCompleted;

        public PlayerData()
        {
            Stats = new PlayerStats();
            Inventory = new PlayerInventory();
            NPCRelationships = new Dictionary<string, int>();
            UnlockedChannels = new List<int> { 1, 2, 3, 4, 5 }; // All except 6 initially
            CompletedEvents = new List<string>();
        }

        /// <summary>
        /// Initialize player data based on character creation choices
        /// </summary>
        public void InitializeFromQuestionnaire(
            string playerName,
            CareerType career,
            HousingType housing,
            FearType fear,
            bool hasPartner)
        {
            PlayerName = playerName;
            CareerChoice = career;
            HousingPreference = housing;
            FearChoice = fear;
            HasPartner = hasPartner;

            // Set career goal based on choice
            CareerGoal = CareerGoalFactory.CreateGoal(career);

            // Initialize starting stats
            Stats.Initialize();

            // Add partner to relationships if applicable
            if (HasPartner)
            {
                NPCRelationships["Partner"] = 50; // Start at neutral
            }
        }

        /// <summary>
        /// Update player stats (called when watching TV, completing combat, etc.)
        /// </summary>
        public void UpdateStats(
            int strength = 0,
            int intelligence = 0,
            int agility = 0,
            int perception = 0,
            int dexterity = 0,
            int courage = 0,
            int stress = 0,
            int ideal = 0,
            int score = 0)
        {
            Stats.Strength = Mathf.Clamp(Stats.Strength + strength, 0, 100);
            Stats.Intelligence = Mathf.Clamp(Stats.Intelligence + intelligence, 0, 100);
            Stats.Agility = Mathf.Clamp(Stats.Agility + agility, 0, 100);
            Stats.Perception = Mathf.Clamp(Stats.Perception + perception, 0, 100);
            Stats.Dexterity = Mathf.Clamp(Stats.Dexterity + dexterity, 0, 100);
            Stats.Courage = Mathf.Clamp(Stats.Courage + courage, 0, 100);

            Stats.Stress = Mathf.Clamp(Stats.Stress + stress, 0, Stats.MaxStress);
            Stats.Ideal = Mathf.Clamp(Stats.Ideal + ideal, 0, Stats.MaxIdeal);
            Stats.Score += score; // Score can be unlimited

            OnStatsChanged?.Invoke(Stats);

            // Check for game over conditions
            if (Stats.IsIdealMaxed())
            {
                Debug.LogWarning("[PlayerData] Ideal maxed! Game Over!");
            }
            if (Stats.IsStressMaxed())
            {
                Debug.LogWarning("[PlayerData] Stress maxed! Game Over!");
            }
        }

        /// <summary>
        /// Advance turn (called after watching a channel)
        /// </summary>
        public void AdvanceTurn()
        {
            Stats.CurrentTurn++;
            Stats.TurnsSinceStressReduction++;

            Debug.Log($"[PlayerData] Turn advanced to {Stats.CurrentTurn}/{Stats.MaxTurns}");

            if (Stats.IsTurnsExhausted())
            {
                Debug.LogWarning("[PlayerData] Turns exhausted! Game Over!");
            }
        }

        /// <summary>
        /// Mark an event as completed
        /// </summary>
        public void CompleteEvent(string eventId)
        {
            if (!CompletedEvents.Contains(eventId))
            {
                CompletedEvents.Add(eventId);
                OnEventCompleted?.Invoke(eventId);
            }
        }

        /// <summary>
        /// Check if an event has been completed
        /// </summary>
        public bool HasCompletedEvent(string eventId)
        {
            return CompletedEvents.Contains(eventId);
        }

        /// <summary>
        /// Unlock Channel 6 (Combat) after Anchilo's visit
        /// </summary>
        public void UnlockChannelVI()
        {
            if (!Channel6Unlocked)
            {
                Channel6Unlocked = true;
                if (!UnlockedChannels.Contains(6))
                {
                    UnlockedChannels.Add(6);
                }
                Debug.Log("Channel VI unlocked!");
            }
        }

        /// <summary>
        /// Unlock full systems after Anchilo's visit
        /// </summary>
        public void UnlockFullSystems()
        {
            if (!FullSystemsUnlocked)
            {
                FullSystemsUnlocked = true;
                AnchiloVisited = true;
                UnlockChannelVI();
                Debug.Log("Full systems unlocked!");
            }
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        public void AddItem(string itemId, int quantity = 1)
        {
            Inventory.AddItem(itemId, quantity);
        }

        /// <summary>
        /// Update NPC relationship
        /// </summary>
        public void UpdateNPCRelationship(string npcName, int change)
        {
            if (NPCRelationships.ContainsKey(npcName))
            {
                NPCRelationships[npcName] = Mathf.Clamp(NPCRelationships[npcName] + change, 0, 100);
            }
            else
            {
                NPCRelationships[npcName] = Mathf.Clamp(change, 0, 100);
            }
        }

        /// <summary>
        /// Get progress percentage towards career goal
        /// </summary>
        public float GetCareerProgress()
        {
            if (CareerGoal == null) return 0f;
            return CareerGoal.GetProgressPercentage(Stats);
        }
    }

    /// <summary>
    /// Player statistics
    /// </summary>
    /// <summary>
    /// Player statistics - NEW 6-attribute system
    /// </summary>
    [System.Serializable]
    public class PlayerStats
    {
        // Core attributes (6 total) - all start at 10, max 100
        public int Strength = 10;      // 力量
        public int Intelligence = 10;  // 智力
        public int Agility = 10;       // 敏捷
        public int Perception = 10;    // 见闻
        public int Dexterity = 10;     // 巧手
        public int Courage = 10;       // 勇气

        // Secondary attributes
        public int Ideal = 0;          // 理想值 (starts at 0, max 200)
        public int MaxIdeal = 200;

        public int Stress = 100;       // 压力值 (starts at 100, max 200)
        public int MaxStress = 200;

        // Score/Currency (only gained from Channel 6)
        public int Score = 0;          // 积分

        // Turn tracking
        public int CurrentTurn = 1;    // 当前回合 (1-40)
        public int MaxTurns = 40;      // 最大回合数

        // Channel usage tracking
        public int TurnsSinceStressReduction = 0; // 距离上次减压的回合数

        public void Initialize()
        {
            // All attributes start at 10
            Strength = 10;
            Intelligence = 10;
            Agility = 10;
            Perception = 10;
            Dexterity = 10;
            Courage = 10;

            // Ideal starts at 0
            Ideal = 0;

            // Stress starts at 100
            Stress = 100;

            // Score starts at 0
            Score = 0;

            // Turn starts at 1
            CurrentTurn = 1;
            TurnsSinceStressReduction = 0;
        }

        /// <summary>
        /// Check if any attribute is below minimum (30)
        /// </summary>
        public bool HasAttributeBelowMinimum()
        {
            return Strength < 30 || Intelligence < 30 || Agility < 30 ||
                   Perception < 30 || Dexterity < 30 || Courage < 30;
        }

        /// <summary>
        /// Check if ideal is maxed (game over)
        /// </summary>
        public bool IsIdealMaxed()
        {
            return Ideal >= MaxIdeal;
        }

        /// <summary>
        /// Check if stress is maxed (game over)
        /// </summary>
        public bool IsStressMaxed()
        {
            return Stress >= MaxStress;
        }

        /// <summary>
        /// Check if turns are exhausted (game over)
        /// </summary>
        public bool IsTurnsExhausted()
        {
            return CurrentTurn > MaxTurns;
        }

        /// <summary>
        /// Get top N highest required attributes for career
        /// </summary>
        public List<string> GetTopNRequiredAttributes(CareerGoal career, int n)
        {
            if (career == null) return new List<string>();

            var attributeList = new List<(string name, int required)>
        {
            ("Strength", career.RequiredStrength),
            ("Intelligence", career.RequiredIntelligence),
            ("Agility", career.RequiredAgility),
            ("Perception", career.RequiredPerception),
            ("Dexterity", career.RequiredDexterity),
            ("Courage", career.RequiredCourage)
        };

            // Sort by required value descending
            attributeList.Sort((a, b) => b.required.CompareTo(a.required));

            var result = new List<string>();
            for (int i = 0; i < Mathf.Min(n, attributeList.Count); i++)
            {
                if (attributeList[i].required > 0)
                {
                    result.Add(attributeList[i].name);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Player inventory system
    /// </summary>
    [System.Serializable]
    public class PlayerInventory
    {
        public Dictionary<string, int> Items = new Dictionary<string, int>();
        public List<string> EquippedWeapons = new List<string>();
        public string CurrentWeapon = null;

        public void AddItem(string itemId, int quantity = 1)
        {
            if (Items.ContainsKey(itemId))
            {
                Items[itemId] += quantity;
            }
            else
            {
                Items[itemId] = quantity;
            }
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            return Items.ContainsKey(itemId) && Items[itemId] >= quantity;
        }

        public void RemoveItem(string itemId, int quantity = 1)
        {
            if (HasItem(itemId, quantity))
            {
                Items[itemId] -= quantity;
                if (Items[itemId] <= 0)
                {
                    Items.Remove(itemId);
                }
            }
        }

        public void EquipWeapon(string weaponId)
        {
            if (!EquippedWeapons.Contains(weaponId))
            {
                EquippedWeapons.Add(weaponId);
            }
            CurrentWeapon = weaponId;
        }
    }

    /// <summary>
    /// Career types from questionnaire
    /// </summary>
    public enum CareerType
    {
        Doctor,          // 医生
        Police,          // 警察
        OfficeWorker,    // 公司职员
        Merchant,        // 行商
        Scientist        // 科学家 (locked first playthrough)
    }

    /// <summary>
    /// Housing preferences from questionnaire
    /// </summary>
    public enum HousingType
    {
        QuietHome,       // 能够安心工作的恬静之家
        FitnessHome,     // 能够自由锻炼的刚猛之家
        GeekHome,        // 能够放松自我的宅宅之家
        CulinaryHome     // 能够狂野烹饪的美食之家
    }

    /// <summary>
    /// Fear types from questionnaire (determines Channel VI enemies)
    /// </summary>
    public enum FearType
    {
        Gaze,       // 注视 - Eye enemies
        Insects,    // 虫类 - Insect enemies
        Darkness,   // 黑暗 - Dark creatures
        Death       // 死亡 - Undead enemies
    }

    /// <summary>
    /// Time of day enum
    /// </summary>
    public enum TimeOfDay
    {
        Morning,
        Afternoon,
        Evening,
        Night
    }
}