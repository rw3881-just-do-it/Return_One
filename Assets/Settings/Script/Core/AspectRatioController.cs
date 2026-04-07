using UnityEngine;

namespace UnityTV.Core
{
    /// <summary>
    /// Maintains 16:9 aspect ratio with letterboxing if needed
    /// Attach this to Main Camera
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class AspectRatioController : MonoBehaviour
    {
        [Header("Target Aspect Ratio")]
        [SerializeField] private float targetAspect = 16f / 9f; // 16:9

        [Header("Reference Resolution (for UI scaling)")]
        [SerializeField] private int referenceWidth = 1920;
        [SerializeField] private int referenceHeight = 1080;

        [Header("Letterbox Color")]
        [SerializeField] private Color letterboxColor = Color.black;

        private Camera cam;
        private float lastAspect;

        private void Start()
        {
            cam = GetComponent<Camera>();
            UpdateAspectRatio();
        }

        private void Update()
        {
            // Check if aspect ratio changed (window resized)
            float currentAspect = (float)Screen.width / Screen.height;

            if (!Mathf.Approximately(currentAspect, lastAspect))
            {
                UpdateAspectRatio();
            }
        }

        private void UpdateAspectRatio()
        {
            // Calculate current aspect ratio
            float windowAspect = (float)Screen.width / Screen.height;
            float scaleHeight = windowAspect / targetAspect;

            lastAspect = windowAspect;

            if (scaleHeight < 1.0f)
            {
                // Window is too wide - add letterbox on sides
                Rect rect = cam.rect;

                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;

                cam.rect = rect;
            }
            else
            {
                // Window is too tall - add letterbox on top/bottom
                float scaleWidth = 1.0f / scaleHeight;

                Rect rect = cam.rect;

                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;

                cam.rect = rect;
            }

            // Set background color for letterboxes
            cam.backgroundColor = letterboxColor;

            Debug.Log($"[AspectRatio] Window: {Screen.width}x{Screen.height} " +
                      $"(aspect: {windowAspect:F2}), Target: {targetAspect:F2}");
        }

        /// <summary>
        /// Get the current scale factor for UI elements
        /// </summary>
        public float GetScaleFactor()
        {
            float screenAspect = (float)Screen.width / Screen.height;

            if (screenAspect > targetAspect)
            {
                // Screen is wider - scale based on height
                return (float)Screen.height / referenceHeight;
            }
            else
            {
                // Screen is taller - scale based on width
                return (float)Screen.width / referenceWidth;
            }
        }

        // Debug: Show current aspect ratio
        private void OnGUI()
        {
#if UNITY_EDITOR
            if (GUI.Button(new Rect(10, 10, 150, 30), "Log Aspect Info"))
            {
                Debug.Log($"Screen: {Screen.width}x{Screen.height}");
                Debug.Log($"Aspect: {(float)Screen.width / Screen.height:F3}");
                Debug.Log($"Target: {targetAspect:F3}");
                Debug.Log($"Camera Rect: {cam.rect}");
            }
#endif
        }

        /// <summary>
        /// Force update (call this if you change target aspect at runtime)
        /// </summary>
        public void ForceUpdate()
        {
            UpdateAspectRatio();
        }
    }
}