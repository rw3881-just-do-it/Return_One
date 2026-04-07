using UnityEngine;

namespace UnityTV.Core
{
    /// <summary>
    /// Audio Manager - handles all game audio (music and sound effects)
    /// TODO: Implement full audio system
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Settings")]
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1.0f;

        private void Awake()
        {
            // Create audio sources if they don't exist
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = musicVolume;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }
        }

        /// <summary>
        /// Play background music by name
        /// </summary>
        public void PlayMusic(string musicName)
        {
            Debug.Log($"[AudioManager] Playing music: {musicName}");
            // TODO: Load and play music clip
            // AudioClip clip = Resources.Load<AudioClip>($"Audio/Music/{musicName}");
            // if (clip != null && musicSource != null)
            // {
            //     musicSource.clip = clip;
            //     musicSource.Play();
            // }
        }

        /// <summary>
        /// Play sound effect by name
        /// </summary>
        public void PlaySFX(string sfxName)
        {
            Debug.Log($"[AudioManager] Playing SFX: {sfxName}");
            // TODO: Load and play SFX clip
            // AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
            // if (clip != null && sfxSource != null)
            // {
            //     sfxSource.PlayOneShot(clip);
            // }
        }

        /// <summary>
        /// Stop background music
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        /// <summary>
        /// Set SFX volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }
    }
}