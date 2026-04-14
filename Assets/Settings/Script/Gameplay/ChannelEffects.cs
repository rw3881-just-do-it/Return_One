using UnityEngine;
using UnityTV.Player;

namespace UnityTV.Gameplay
{
    /// <summary>
    /// Utility class to handle the specific gameplay consequences of each TV channel.
    /// This keeps the UI controller clean and separates logic from interface.
    /// </summary>
    public static class ChannelEffects
    {
        public static void ApplyChannel1(PlayerData data)
        {
            // Channel I - Medical
            Debug.Log("[Effect] Applied Medical Channel stats");
            data.UpdateStats(intelligence: 5, mentalStrength: 3, stress: -5);
        }

        public static void ApplyChannel3(PlayerData data)
        {
            // Channel III - Office/Business
            Debug.Log("[Effect] Applied Business Channel stats");
            data.UpdateStats(intelligence: 3, mentalStrength: 3, stress: 5);
        }

        public static void ApplyChannel4(PlayerData data)
        {
            // Channel IV - Merchant/Trade
            Debug.Log("[Effect] Applied Trade Channel stats");
            data.UpdateStats(physicalStrength: 3, mentalStrength: 5, stress: 2);
        }

        public static void ApplyChannel5(PlayerData data)
        {
            // Channel V - Science
            Debug.Log("[Effect] Applied Science Channel stats");
            data.UpdateStats(intelligence: 8, stress: 10);
        }
    }
}