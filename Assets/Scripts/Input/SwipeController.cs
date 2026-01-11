using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using StepPet.Core;

namespace StepPet.Input
{
    /// <summary>
    /// Handles swipe gesture detection and navigation.
    /// Uses the Input System for touch/mouse position tracking.
    /// Calls UIManager methods based on swipe direction and current page.
    /// </summary>
    public class SwipeController : MonoBehaviour
    {
        [Header("Swipe Settings")]
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxSwipeTime = 0.5f;
        [SerializeField] private float swipeDirectionThreshold = 0.7f; // How horizontal the swipe must be (0-1)

        [Header("Tap Settings")]
        [SerializeField] private float maxTapDistance = 20f;
        [SerializeField] private float maxTapTime = 0.3f;

        [Header("References")]
        [SerializeField] private UnityEngine.Camera mainCamera;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Input Actions - created programmatically for portability
        private InputAction _pointerPositionAction;
        private InputAction _pointerPressAction;

        // Swipe tracking
        private Vector2 _touchStartPosition;
        private float _touchStartTime;
        private bool _isTouching;

        // R3 Subjects for events (other controllers can subscribe)
        private readonly Subject<Vector2> _onSwipeLeft = new();
        private readonly Subject<Vector2> _onSwipeRight = new();
        private readonly Subject<Vector2> _onSwipeUp = new();
        private readonly Subject<Vector2> _onSwipeDown = new();
        private readonly Subject<Vector2> _onTap = new();

        /// <summary>Fired when user swipes left (from right to left)</summary>
        public Observable<Vector2> OnSwipeLeft => _onSwipeLeft;

        /// <summary>Fired when user swipes right (from left to right)</summary>
        public Observable<Vector2> OnSwipeRight => _onSwipeRight;

        /// <summary>Fired when user swipes up</summary>
        public Observable<Vector2> OnSwipeUp => _onSwipeUp;

        /// <summary>Fired when user swipes down</summary>
        public Observable<Vector2> OnSwipeDown => _onSwipeDown;

        /// <summary>Fired when user taps (screen position)</summary>
        public Observable<Vector2> OnTap => _onTap;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
            }

            // Create input actions programmatically (works with both mouse and touch)
            _pointerPositionAction = new InputAction(
                name: "PointerPosition",
                type: InputActionType.Value,
                binding: "<Pointer>/position"
            );

            _pointerPressAction = new InputAction(
                name: "PointerPress",
                type: InputActionType.Button,
                binding: "<Pointer>/press"
            );
        }

        private void OnEnable()
        {
            _pointerPositionAction.Enable();
            _pointerPressAction.Enable();

            _pointerPressAction.started += OnTouchStarted;
            _pointerPressAction.canceled += OnTouchEnded;
        }

        private void OnDisable()
        {
            _pointerPressAction.started -= OnTouchStarted;
            _pointerPressAction.canceled -= OnTouchEnded;

            _pointerPositionAction.Disable();
            _pointerPressAction.Disable();
        }

        private void OnDestroy()
        {
            _pointerPositionAction?.Dispose();
            _pointerPressAction?.Dispose();

            _onSwipeLeft.Dispose();
            _onSwipeRight.Dispose();
            _onSwipeUp.Dispose();
            _onSwipeDown.Dispose();
            _onTap.Dispose();
        }

        private void OnTouchStarted(InputAction.CallbackContext context)
        {
            _touchStartPosition = _pointerPositionAction.ReadValue<Vector2>();
            _touchStartTime = Time.time;
            _isTouching = true;
        }

        private void OnTouchEnded(InputAction.CallbackContext context)
        {
            if (!_isTouching) return;
            _isTouching = false;

            Vector2 touchEndPosition = _pointerPositionAction.ReadValue<Vector2>();
            float touchDuration = Time.time - _touchStartTime;
            Vector2 swipeDelta = touchEndPosition - _touchStartPosition;
            float swipeDistance = swipeDelta.magnitude;

            // Check for tap
            if (swipeDistance < maxTapDistance && touchDuration < maxTapTime)
            {
                HandleTap(touchEndPosition);
                return;
            }

            // Check for swipe
            if (swipeDistance >= minSwipeDistance && touchDuration <= maxSwipeTime)
            {
                HandleSwipe(swipeDelta, touchEndPosition);
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            _onTap.OnNext(screenPosition);

            Log($"TAP at ({screenPosition.x:F0}, {screenPosition.y:F0})");

            // Handle tap based on current page
            var uiManager = UIManager.Instance;
            if (uiManager == null) return;

            var currentPage = uiManager.CurrentPage.Value;

            if (currentPage == UIPage.SceneOverview)
            {
                // Tap on scene overview - check if tapped on a pet
                TrySelectPetAtPosition(screenPosition);
            }
            // Other tap handling can be added here
        }

        private void HandleSwipe(Vector2 swipeDelta, Vector2 endPosition)
        {
            Vector2 direction = swipeDelta.normalized;

            // Check if swipe is horizontal enough
            bool isHorizontal = Mathf.Abs(direction.x) > swipeDirectionThreshold;
            bool isVertical = Mathf.Abs(direction.y) > swipeDirectionThreshold;

            if (isHorizontal)
            {
                if (direction.x > 0)
                {
                    Log($"SWIPE RIGHT (delta: {swipeDelta.magnitude:F0}px)");
                    _onSwipeRight.OnNext(endPosition);
                }
                else
                {
                    Log($"SWIPE LEFT (delta: {swipeDelta.magnitude:F0}px)");
                    _onSwipeLeft.OnNext(endPosition);
                }
            }
            else if (isVertical)
            {
                if (direction.y > 0)
                {
                    Log($"SWIPE UP (delta: {swipeDelta.magnitude:F0}px)");
                    _onSwipeUp.OnNext(endPosition);
                }
                else
                {
                    Log($"SWIPE DOWN (delta: {swipeDelta.magnitude:F0}px)");
                    _onSwipeDown.OnNext(endPosition);
                }
            }
            else
                Log($"Swipe ignored - not horizontal or vertical enough (dir: {direction})");
        }
        

        private void TrySelectPetAtPosition(Vector2 screenPosition)
        {
            if (mainCamera == null) return;

            // Raycast to find pets
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            if (hit.collider != null)
            {
                // Check if we hit a pet (look for a component that identifies pets)
                var petIdentifier = hit.collider.GetComponent<IPetIdentifier>();
                if (petIdentifier != null)
                {
                    Log($"Tapped on pet: {petIdentifier.PetId}");
                    UIManager.Instance?.FocusPet(petIdentifier.PetId);
                }
                else
                {
                    Log($"Tapped on collider '{hit.collider.name}' but it's not a pet");
                }
            }
            else
            {
                Log("Tapped on empty space (no collider hit)");
            }
        }

        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SwipeController] {message}");
            }
        }
    }

    /// <summary>
    /// Interface to identify pet GameObjects for tap selection.
    /// Implement this on pet prefabs.
    /// </summary>
    public interface IPetIdentifier
    {
        string PetId { get; }
    }
}
