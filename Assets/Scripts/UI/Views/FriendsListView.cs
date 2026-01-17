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
        [SerializeField] private MinMax normalizedX = new MinMax { min = -1f, max = 0f }; // min = hidden, max = shown (-1 = left edge, 0 = center, 1 = right edge)

        [Header("References")]
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas rootCanvas;

        private Vector2 _shownPosition;
        private Vector2 _hiddenPosition;
        private bool _isShown;
        private CompositeDisposable _disposables;
        private float _canvasWidth;

        private void Awake()
        {
            _disposables = new CompositeDisposable();

            // Get canvas width for normalized position calculations
            if (rootCanvas != null)
            {
                RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
                _canvasWidth = canvasRect.rect.width;
            }
            else
            {
                // Fallback: try to find canvas in parent hierarchy
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                    _canvasWidth = canvasRect.rect.width;
                }
                else
                {
                    _canvasWidth = Screen.width;
                }
            }

            // Calculate positions from normalized values (assumes center anchor)
            // -1 = one screen width left, 0 = center, 1 = one screen width right
            float baseY = panelTransform.anchoredPosition.y;
            _shownPosition = new Vector2(normalizedX.max * _canvasWidth, baseY);
            _hiddenPosition = new Vector2(normalizedX.min * _canvasWidth, baseY);
        }

        private void Start()
        {
            UIManager.Instance.CurrentPage
                .Subscribe(OnPageChanged)
                .AddTo(_disposables);

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
            _isShown = shouldShow;
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
