using R3;
using UnityEngine;
using StepPet.Core;

namespace StepPet.UI.Views
{
    /// <summary>
    /// View for the bottom expandable menu.
    /// Content changes based on current page (Expenditure on PetCloseup, Shop on SceneOverview).
    /// Slides up when expanded, shows handle when collapsed.
    /// </summary>
    public class BottomMenuView : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float slideSpeed = 10f;
        [SerializeField] private float collapsedYPos = -500f; // How much to hide when collapsed
        [SerializeField] private float expandedYPos = 0f; // How much to hide when collapsed

        [Header("References")]
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private CanvasGroup contentCanvasGroup;

        [Header("Content Panels")]
        [SerializeField] private GameObject expenditureContent; // Shown on PetCloseup
        [SerializeField] private GameObject shopContent; // Shown on SceneOverview

        private bool _isExpanded;
        private UIPage _currentPage;
        private CompositeDisposable _disposables;

        private void Awake()
        {
            _disposables = new CompositeDisposable();
        }

        private void Start()
        {
            if (UIManager.Instance != null)
            {
                // Subscribe to menu expanded state
                UIManager.Instance.IsBottomMenuExpanded
                    .Subscribe(OnExpandedChanged)
                    .AddTo(_disposables);

                // Subscribe to page changes for content switching
                UIManager.Instance.CurrentPage
                    .Subscribe(OnPageChanged)
                    .AddTo(_disposables);
            }

            // Start collapsed
            SetCollapsedImmediate();
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void OnExpandedChanged(bool expanded)
        {
            _isExpanded = expanded;
        }

        private void OnPageChanged(UIPage page)
        {
            _currentPage = page;
            UpdateContentVisibility();

            // Hide menu on Friends List page
            if (page == UIPage.FriendsList)
            {
                UIManager.Instance?.SetBottomMenuExpanded(false);
            }
        }

        private void UpdateContentVisibility()
        {
            // Show expenditure content on PetCloseup, shop content on SceneOverview
            if (expenditureContent != null)
            {
                expenditureContent.SetActive(_currentPage == UIPage.PetCloseup);
            }

            if (shopContent != null)
            {
                shopContent.SetActive(_currentPage == UIPage.SceneOverview);
            }
        }

        private void Update()
        {
            // Hide completely when on Friends List
            bool shouldBeVisible = _currentPage != UIPage.FriendsList;

            if (!shouldBeVisible)
            {
                // Slide completely off screen
                Vector2 hiddenPosition = new Vector2(0, expandedYPos);
                panelTransform.anchoredPosition = Vector2.Lerp(
                    panelTransform.anchoredPosition,
                    hiddenPosition,
                    Time.deltaTime * slideSpeed
                );
                return;
            }

            // Smoothly slide the panel
            panelTransform.anchoredPosition = Vector2.Lerp(
                panelTransform.anchoredPosition,
                new Vector2(0, _isExpanded? expandedYPos : collapsedYPos),
                Time.deltaTime * slideSpeed
            );

            contentCanvasGroup.interactable = _isExpanded;
            contentCanvasGroup.blocksRaycasts = _isExpanded;
        }

        private void SetCollapsedImmediate()
        {
            _isExpanded = false;
            panelTransform.anchoredPosition = new Vector2(0, collapsedYPos);
            contentCanvasGroup.interactable = false;
            contentCanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Called by the handle/drag area to toggle menu
        /// </summary>
        public void OnHandleTapped()
        {
            UIManager.Instance?.ToggleBottomMenu();
        }

        /// <summary>
        /// Called by close button to collapse menu
        /// </summary>
        public void OnCloseButtonClicked()
        {
            UIManager.Instance?.SetBottomMenuExpanded(false);
        }
    }
}
