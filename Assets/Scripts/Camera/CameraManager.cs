using R3;
using UnityEngine;

namespace StepPet.Camera
{
    /// <summary>
    /// Manages camera state using reactive properties.
    /// CameraController subscribes to these to smoothly animate the camera.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [Header("Zoom Settings")]
        [SerializeField] private float sceneOverviewZoom = 8f;
        [SerializeField] private float petCloseupZoom = 3.5f;

        [Header("Scene Bounds")]
        [SerializeField] private Vector3 sceneOverviewPosition = Vector3.zero;

        /// <summary>
        /// Target camera position (camera will smoothly move to this)
        /// </summary>
        public ReactiveProperty<Vector3> TargetPosition { get; } = new(Vector3.zero);

        /// <summary>
        /// Target orthographic size (camera will smoothly zoom to this)
        /// </summary>
        public ReactiveProperty<float> TargetZoom { get; } = new(8f);

        /// <summary>
        /// Whether the camera is currently transitioning
        /// </summary>
        public ReactiveProperty<bool> IsTransitioning { get; } = new(false);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize with scene overview settings
            TargetPosition.Value = sceneOverviewPosition;
            TargetZoom.Value = sceneOverviewZoom;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            TargetPosition.Dispose();
            TargetZoom.Dispose();
            IsTransitioning.Dispose();
        }

        /// <summary>
        /// Set camera to scene overview (zoomed out, centered)
        /// </summary>
        public void ZoomToSceneOverview()
        {
            TargetPosition.Value = sceneOverviewPosition;
            TargetZoom.Value = sceneOverviewZoom;
        }

        /// <summary>
        /// Zoom camera to focus on a specific pet
        /// </summary>
        public void ZoomToPet(Vector3 petPosition)
        {
            // Keep z position for camera (typically -10 for 2D)
            var targetPos = new Vector3(petPosition.x, petPosition.y, TargetPosition.Value.z);
            TargetPosition.Value = targetPos;
            TargetZoom.Value = petCloseupZoom;
        }

        /// <summary>
        /// Zoom camera to focus on a pet's transform
        /// </summary>
        public void ZoomToPet(Transform petTransform)
        {
            if (petTransform != null)
            {
                ZoomToPet(petTransform.position);
            }
        }

        /// <summary>
        /// Set custom zoom level
        /// </summary>
        public void SetZoom(float zoom)
        {
            TargetZoom.Value = Mathf.Max(0.1f, zoom);
        }

        /// <summary>
        /// Set custom position
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            TargetPosition.Value = position;
        }

        /// <summary>
        /// Get the scene overview zoom level
        /// </summary>
        public float GetSceneOverviewZoom() => sceneOverviewZoom;

        /// <summary>
        /// Get the pet closeup zoom level
        /// </summary>
        public float GetPetCloseupZoom() => petCloseupZoom;
    }
}
