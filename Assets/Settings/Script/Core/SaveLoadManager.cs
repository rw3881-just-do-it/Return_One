using UnityEngine;
using System.IO;
using UnityTV.Player;

namespace UnityTV.Core
{
    /// <summary>
    /// Save/Load Manager - handles game save and load functionality
    /// Uses JSON serialization to save player data
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        private const string SAVE_FILE_NAME = "savegame.json";
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        private PlayerData currentPlayerData;

        /// <summary>
        /// Save game data to file
        /// </summary>
        public void SaveGame(PlayerData playerData)
        {
            if (playerData == null)
            {
                Debug.LogError("[SaveLoadManager] Cannot save null player data!");
                return;
            }

            try
            {
                currentPlayerData = playerData;

                // Convert PlayerData to JSON
                string json = JsonUtility.ToJson(playerData, true);

                // Write to file
                File.WriteAllText(SaveFilePath, json);

                Debug.Log($"[SaveLoadManager] Game saved to: {SaveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveLoadManager] Failed to save game: {e.Message}");
            }
        }

        /// <summary>
        /// Load game data from file
        /// </summary>
        public bool LoadGame()
        {
            if (!HasSaveFile())
            {
                Debug.LogWarning("[SaveLoadManager] No save file found!");
                return false;
            }

            try
            {
                // Read from file
                string json = File.ReadAllText(SaveFilePath);

                // Convert JSON to PlayerData
                currentPlayerData = JsonUtility.FromJson<PlayerData>(json);

                Debug.Log($"[SaveLoadManager] Game loaded from: {SaveFilePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveLoadManager] Failed to load game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if save file exists
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(SaveFilePath);
        }

        /// <summary>
        /// Get loaded player data
        /// </summary>
        public PlayerData GetPlayerData()
        {
            return currentPlayerData;
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public void DeleteSave()
        {
            if (HasSaveFile())
            {
                try
                {
                    File.Delete(SaveFilePath);
                    currentPlayerData = null;
                    Debug.Log("[SaveLoadManager] Save file deleted");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SaveLoadManager] Failed to delete save: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Auto-save (called periodically or on important events)
        /// </summary>
        public void AutoSave()
        {
            if (GameManager.Instance?.PlayerData != null)
            {
                SaveGame(GameManager.Instance.PlayerData);
                Debug.Log("[SaveLoadManager] Auto-save complete");
            }
        }

        // Debug: Show save file location
        [ContextMenu("Show Save File Path")]
        private void ShowSavePath()
        {
            Debug.Log($"Save file path: {SaveFilePath}");
            Debug.Log($"Save file exists: {HasSaveFile()}");
        }
    }
}