using UnityEngine;
using System;

namespace UnityTV.Core
{
    /// <summary>
    /// Manages game time progression - day/night cycle and 10-day deadline
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private float secondsPerTimeBlock = 120f; // How long each time of day lasts in real seconds
        [SerializeField] private bool timePaused = false;

        [Header("Current Time")]
        [SerializeField] private int currentDay = 1;
        [SerializeField] private Player.TimeOfDay currentTimeOfDay = Player.TimeOfDay.Morning;

        [Header("Game Constants")]
        private const int TOTAL_DAYS = 10;
        private const int DEADLINE_DAY = 10;

        // Time tracking
        private float timeAccumulator = 0f;

        // Events
        public event Action<int> OnDayChanged;
        public event Action<Player.TimeOfDay> OnTimeOfDayChanged;
        public event Action<int> OnDayEnded;
        public event Action OnDeadlineReached;
        public event Action<float> OnTimeProgressed; // Sends percentage (0-1) of current time block

        public int CurrentDay => currentDay;
        public Player.TimeOfDay CurrentTimeOfDay => currentTimeOfDay;
        public bool IsTimePaused => timePaused;

        private void Update()
        {
            if (!timePaused && GameManager.Instance.PlayerData != null)
            {
                UpdateTime();
            }
        }

        /// <summary>
        /// Update time progression
        /// </summary>
        private void UpdateTime()
        {
            timeAccumulator += Time.deltaTime;

            // Send progress update
            float progress = timeAccumulator / secondsPerTimeBlock;
            OnTimeProgressed?.Invoke(Mathf.Clamp01(progress));

            // Check if we should advance to next time block
            if (timeAccumulator >= secondsPerTimeBlock)
            {
                timeAccumulator = 0f;
                AdvanceTime();
            }
        }

        /// <summary>
        /// Advance to the next time of day
        /// </summary>
        private void AdvanceTime()
        {
            Player.TimeOfDay previousTime = currentTimeOfDay;

            switch (currentTimeOfDay)
            {
                case Player.TimeOfDay.Morning:
                    currentTimeOfDay = Player.TimeOfDay.Afternoon;
                    break;

                case Player.TimeOfDay.Afternoon:
                    currentTimeOfDay = Player.TimeOfDay.Evening;
                    break;

                case Player.TimeOfDay.Evening:
                    currentTimeOfDay = Player.TimeOfDay.Night;
                    break;

                case Player.TimeOfDay.Night:
                    // End of day - advance to next day
                    EndDay();
                    currentTimeOfDay = Player.TimeOfDay.Morning;
                    break;
            }

            if (previousTime != currentTimeOfDay)
            {
                OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
                UpdatePlayerData();

                Debug.Log($"Time advanced: {previousTime} → {currentTimeOfDay}");
            }
        }

        /// <summary>
        /// End the current day and start a new one
        /// </summary>
        private void EndDay()
        {
            Debug.Log($"Day {currentDay} ended");

            // Trigger day end event before incrementing
            OnDayEnded?.Invoke(currentDay + 1);

            // Increment day
            currentDay++;
            OnDayChanged?.Invoke(currentDay);

            UpdatePlayerData();

            // Check if deadline reached
            if (currentDay > DEADLINE_DAY)
            {
                OnDeadlineReached?.Invoke();
            }
        }

        /// <summary>
        /// Update player data with current time
        /// </summary>
        private void UpdatePlayerData()
        {
            if (GameManager.Instance?.PlayerData != null)
            {
                GameManager.Instance.PlayerData.CurrentDay = currentDay;
                GameManager.Instance.PlayerData.CurrentTimeOfDay = currentTimeOfDay;
            }
        }

        /// <summary>
        /// Manually advance to next time of day (for testing or special events)
        /// </summary>
        public void ForceAdvanceTime()
        {
            timeAccumulator = 0f;
            AdvanceTime();
        }

        /// <summary>
        /// Manually advance to next day
        /// </summary>
        public void ForceAdvanceDay()
        {
            currentTimeOfDay = Player.TimeOfDay.Night;
            timeAccumulator = 0f;
            AdvanceTime();
        }

        /// <summary>
        /// Start the game timer
        /// </summary>
        public void StartGame()
        {
            currentDay = 1;
            currentTimeOfDay = Player.TimeOfDay.Morning;
            timeAccumulator = 0f;
            timePaused = false;

            UpdatePlayerData();

            Debug.Log("Time system started - Day 1, Morning");
        }

        /// <summary>
        /// Pause time progression
        /// </summary>
        public void PauseTime()
        {
            timePaused = true;
            Debug.Log("Time paused");
        }

        /// <summary>
        /// Resume time progression
        /// </summary>
        public void ResumeTime()
        {
            timePaused = false;
            Debug.Log("Time resumed");
        }

        /// <summary>
        /// Reset time to beginning
        /// </summary>
        public void ResetTime()
        {
            currentDay = 1;
            currentTimeOfDay = Player.TimeOfDay.Morning;
            timeAccumulator = 0f;
            timePaused = true;

            Debug.Log("Time system reset");
        }

        /// <summary>
        /// Load time from saved data
        /// </summary>
        public void LoadTime(int day, Player.TimeOfDay timeOfDay)
        {
            currentDay = day;
            currentTimeOfDay = timeOfDay;
            timeAccumulator = 0f;

            Debug.Log($"Time loaded - Day {day}, {timeOfDay}");
        }

        /// <summary>
        /// Get days remaining until deadline
        /// </summary>
        public int GetDaysRemaining()
        {
            return Mathf.Max(0, DEADLINE_DAY - currentDay + 1);
        }

        /// <summary>
        /// Get time remaining in current time block (0-1)
        /// </summary>
        public float GetTimeBlockProgress()
        {
            return Mathf.Clamp01(timeAccumulator / secondsPerTimeBlock);
        }

        /// <summary>
        /// Check if it's a specific time of day
        /// </summary>
        public bool IsTimeOfDay(Player.TimeOfDay timeOfDay)
        {
            return currentTimeOfDay == timeOfDay;
        }

        /// <summary>
        /// Check if it's morning
        /// </summary>
        public bool IsMorning()
        {
            return currentTimeOfDay == Player.TimeOfDay.Morning;
        }

        /// <summary>
        /// Check if it's night
        /// </summary>
        public bool IsNight()
        {
            return currentTimeOfDay == Player.TimeOfDay.Night;
        }

        /// <summary>
        /// Get formatted time string for display
        /// </summary>
        public string GetFormattedTime()
        {
            string timeOfDayStr = currentTimeOfDay switch
            {
                Player.TimeOfDay.Morning => "Morning",
                Player.TimeOfDay.Afternoon => "Afternoon",
                Player.TimeOfDay.Evening => "Evening",
                Player.TimeOfDay.Night => "Night",
                _ => "Unknown"
            };

            return $"Day {currentDay} - {timeOfDayStr}";
        }

        /// <summary>
        /// Get Chinese formatted time string
        /// </summary>
        public string GetFormattedTimeChinese()
        {
            string timeOfDayStr = currentTimeOfDay switch
            {
                Player.TimeOfDay.Morning => "早晨",
                Player.TimeOfDay.Afternoon => "下午",
                Player.TimeOfDay.Evening => "傍晚",
                Player.TimeOfDay.Night => "夜晚",
                _ => "未知"
            };

            return $"第{currentDay}天 - {timeOfDayStr}";
        }

        /// <summary>
        /// Debug: Set specific time (for testing)
        /// </summary>
        [ContextMenu("Debug: Advance to Afternoon")]
        private void DebugAfternoon()
        {
            currentTimeOfDay = Player.TimeOfDay.Morning;
            ForceAdvanceTime();
        }

        [ContextMenu("Debug: Advance to Evening")]
        private void DebugEvening()
        {
            currentTimeOfDay = Player.TimeOfDay.Afternoon;
            ForceAdvanceTime();
        }

        [ContextMenu("Debug: Advance to Night")]
        private void DebugNight()
        {
            currentTimeOfDay = Player.TimeOfDay.Evening;
            ForceAdvanceTime();
        }

        [ContextMenu("Debug: Advance to Next Day")]
        private void DebugNextDay()
        {
            ForceAdvanceDay();
        }

        [ContextMenu("Debug: Jump to Day 9")]
        private void DebugDay9()
        {
            currentDay = 9;
            currentTimeOfDay = Player.TimeOfDay.Morning;
            UpdatePlayerData();
        }
    }
}