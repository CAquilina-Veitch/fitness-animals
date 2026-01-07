using R3;
using UnityEngine;
using StepPet.Core;

namespace StepPet.Camera
{
    /// <summary>
    /// Controls the main camera by subscribing to CameraManager and UIManager.
    /// Smoothly interpolates camera position and zoom based on reactive state changes.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Transition Settings")]
        [SerializeField] private float positionSmoothTime = 0.3f;
        [SerializeField] private float zoomSmoothTime = 0.3f;

        [Header("Bounds (Optional)")]
        [SerializeField] private bool useBounds;
        [SerializeField] private Vector2 minBounds = new(-10, -10);
        [SerializeField] private Vector2 maxBounds = new(10, 10);

        private UnityEngine.Camera _camera;
        private Vector3 _positionVelocity;
        private float _zoomVelocity;

        private CompositeDisposable _disposables;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _disposables = new CompositeDisposable();
        }

        private void Start()
        {
            // Subscribe to CameraManager changes
            if (CameraManager.Instance != null)
            {
                SubscribeToCameraManager();
            }

            // Subscribe to UIManager for page changes (to trigger camera moves)
            if (UIManager.Instance != null)
            {
                SubscribeToUIManager();
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void SubscribeToCameraManager()
        {
            var cameraManager = CameraManager.Instance;

            // React to target position changes
            cameraManager.TargetPosition
                .Subscribe(targetPos =>
                {
                    // Start transitioning flag
                    cameraManager.IsTransitioning.Value = true;
                })
                .AddTo(_disposables);

            // React to zoom changes
            cameraManager.TargetZoom
                .Subscribe(targetZoom =>
                {
                    cameraManager.IsTransitioning.Value = true;
                })
                .AddTo(_disposables);
        }

        private void SubscribeToUIManager()
        {
            var uiManager = UIManager.Instance;
            var cameraManager = CameraManager.Instance;

            if (cameraManager == null) return;

            // React to page changes
            uiManager.CurrentPage
                .Subscribe(page =>
                {
                    switch (page)
                    {
                        case UIPage.SceneOverview:
                        case UIPage.FriendsList:
                            // Zoom out to scene overview
                            cameraManager.ZoomToSceneOverview();
                            break;

                        case UIPage.PetCloseup:
                            // Will be handled by FocusedPetId change
                            break;
                    }
                })
                .AddTo(_disposables);

            // React to focused pet changes
            uiManager.FocusedPetId
                .Subscribe(petId =>
                {
                    if (!string.IsNullOrEmpty(petId))
                    {
                        // Find the pet and zoom to it
                        ZoomToPetById(petId);
                    }
                })
                .AddTo(_disposables);
        }

        private void ZoomToPetById(string petId)
        {
            // Find the pet GameObject by ID
            // This requires pets to have an IPetIdentifier component
            var pets = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var pet in pets)
            {
                if (pet is StepPet.Input.IPetIdentifier identifier && identifier.PetId == petId)
                {
                    CameraManager.Instance?.ZoomToPet(pet.transform);
                    return;
                }
            }

            // Pet not found - log warning in debug
            Debug.LogWarning($"[CameraController] Could not find pet with ID: {petId}");
        }

        private void Update()
        {
            if (CameraManager.Instance == null) return;

            var cameraManager = CameraManager.Instance;
            Vector3 targetPosition = cameraManager.TargetPosition.Value;
            float targetZoom = cameraManager.TargetZoom.Value;

            // Preserve the camera's Z position (typically -10 for 2D)
            targetPosition.z = transform.position.z;

            // Apply bounds if enabled
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }

            // Smoothly move camera position
            Vector3 newPosition = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _positionVelocity,
                positionSmoothTime
            );
            transform.position = newPosition;

            // Smoothly change zoom (orthographic size)
            float newZoom = Mathf.SmoothDamp(
                _camera.orthographicSize,
                targetZoom,
                ref _zoomVelocity,
                zoomSmoothTime
            );
            _camera.orthographicSize = newZoom;

            // Check if transition is complete
            bool positionReached = Vector3.Distance(transform.position, targetPosition) < 0.01f;
            bool zoomReached = Mathf.Abs(_camera.orthographicSize - targetZoom) < 0.01f;

            if (positionReached && zoomReached && cameraManager.IsTransitioning.Value)
            {
                // Snap to final values
                transform.position = targetPosition;
                _camera.orthographicSize = targetZoom;
                cameraManager.IsTransitioning.Value = false;
            }
        }

        /// <summary>
        /// Immediately snap camera to target (no smooth transition)
        /// </summary>
        public void SnapToTarget()
        {
            if (CameraManager.Instance == null) return;

            var cameraManager = CameraManager.Instance;
            Vector3 targetPosition = cameraManager.TargetPosition.Value;
            targetPosition.z = transform.position.z;

            transform.position = targetPosition;
            _camera.orthographicSize = cameraManager.TargetZoom.Value;
            cameraManager.IsTransitioning.Value = false;

            _positionVelocity = Vector3.zero;
            _zoomVelocity = 0f;
        }
    }
}
