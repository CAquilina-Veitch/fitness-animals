using R3;
using UnityEngine;
using StepPet.Core;

namespace StepPet.UI.Views
{
    /// <summary>
    /// View for the Friends List panel.
    /// Slides in from the left when CurrentPage is FriendsList.
    /// </summary>
    public class FriendsListView : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float slideSpeed = 8f;
        [SerializeField] private float hiddenXOffset = -400f; // Offset when hidden (off-screen left)

        [Header("References")]
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        private Vector2 _shownPosition;
        private Vector2 _hiddenPosition;
        private bool _isShown;
        private CompositeDisposable _disposables;

        private void Awake()
        {
            _disposables = new CompositeDisposable();

            if (panelTransform == null)
                panelTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // Store the shown position (current position in editor)
            if (panelTransform != null)
            {
                _shownPosition = panelTransform.anchoredPosition;
                _hiddenPosition = new Vector2(_shownPosition.x + hiddenXOffset, _shownPosition.y);
            }
        }

        private void Start()
        {
            // Subscribe to UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CurrentPage
                    .Subscribe(OnPageChanged)
                    .AddTo(_disposables);
            }

            // Start hidden
            SetHiddenImmediate();
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void OnPageChanged(UIPage page)
        {
            bool shouldShow = page == UIPage.FriendsList;

            if (shouldShow != _isShown)
            {
                _isShown = shouldShow;
            }
        }

        private void Update()
        {
            if (panelTransform == null) return;

            // Smoothly slide the panel
            Vector2 targetPosition = _isShown ? _shownPosition : _hiddenPosition;
            panelTransform.anchoredPosition = Vector2.Lerp(
                panelTransform.anchoredPosition,
                targetPosition,
                Time.deltaTime * slideSpeed
            );

            // Update interactability
            if (canvasGroup != null)
            {
                float targetAlpha = _isShown ? 1f : 0f;
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * slideSpeed);
                canvasGroup.interactable = _isShown;
                canvasGroup.blocksRaycasts = _isShown;
            }
        }

        private void SetHiddenImmediate()
        {
            _isShown = false;
            if (panelTransform != null)
            {
                panelTransform.anchoredPosition = _hiddenPosition;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Called by UI button to close the friends list
        /// </summary>
        public void OnCloseButtonClicked()
        {
            UIManager.Instance?.SetPage(UIPage.SceneOverview);
        }
    }
}
