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
        public bool Channel6Unlocked = true; // Changed to true for testing

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
            int health = 0,
            int intelligence = 0,
            int physicalStrength = 0,
            int mentalStrength = 0,
            int stress = 0,
            int ideal = 0)
        {
            Stats.Health = Mathf.Clamp(Stats.Health + health, 0, Stats.MaxHealth);
            Stats.Intelligence = Mathf.Max(0, Stats.Intelligence + intelligence);
            Stats.PhysicalStrength = Mathf.Max(0, Stats.PhysicalStrength + physicalStrength);
            Stats.MentalStrength = Mathf.Max(0, Stats.MentalStrength + mentalStrength);
            Stats.Stress = Mathf.Clamp(Stats.Stress + stress, 0, Stats.MaxStress);
            Stats.Ideal = Mathf.Max(0, Stats.Ideal + ideal);

            OnStatsChanged?.Invoke(Stats);
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
    [System.Serializable]
    public class PlayerStats
    {
        public int Health = 100;
        public int MaxHealth = 100;

        public int Intelligence = 0;
        public int PhysicalStrength = 0;
        public int MentalStrength = 0;

        public int Stress = 0;
        public int MaxStress = 100;

        public int Ideal = 0; // Hope/Ideal value

        public void Initialize()
        {
            Health = MaxHealth;
            Intelligence = 0;
            PhysicalStrength = 0;
            MentalStrength = 0;
            Stress = 0;
            Ideal = 50; // Start with some hope
        }

        /// <summary>
        /// Check if player is stressed out
        /// </summary>
        public bool IsOverwhelmed()
        {
            return Stress >= MaxStress;
        }

        /// <summary>
        /// Check if player has low health
        /// </summary>
        public bool IsInjured()
        {
            return Health < MaxHealth * 0.3f;
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