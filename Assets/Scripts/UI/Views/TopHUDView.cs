using R3;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StepPet.Core;

namespace StepPet.UI.Views
{
    /// <summary>
    /// View for the top HUD overlay.
    /// Displays daily quota progress bar and wallet amount.
    /// Always visible except on Friends List.
    /// </summary>
    public class TopHUDView : MonoBehaviour
    {
        [Header("Quota Bar")]
        [SerializeField] private Slider quotaProgressBar;
        [SerializeField] private TextMeshProUGUI quotaText; // e.g., "600/1000 food (daily)"
        [SerializeField] private Image quotaFillImage;
        [SerializeField] private Color normalColor = new Color(0.4f, 0.7f, 1f); // Light blue
        [SerializeField] private Color completedColor = new Color(0.4f, 1f, 0.5f); // Green

        [Header("Wallet")]
        [SerializeField] private TextMeshProUGUI walletText; // e.g., "4,000"
        [SerializeField] private Image walletIcon;

        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private CanvasGroup canvasGroup;

        private bool _isVisible = true;
        private CompositeDisposable _disposables;

        // These will be driven by an EconomyManager later
        // For now, we'll have public setters for testing
        private int _currentQuota;
        private int _maxQuota = 1000;
        private int _walletAmount;

        private void Awake()
        {
            _disposables = new CompositeDisposable();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CurrentPage
                    .Subscribe(OnPageChanged)
                    .AddTo(_disposables);
            }

            // Initialize with default values
            UpdateQuotaDisplay();
            UpdateWalletDisplay();
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void OnPageChanged(UIPage page)
        {
            // Hide on Friends List page
            _isVisible = page != UIPage.FriendsList;
        }

        private void Update()
        {
            if (canvasGroup == null) return;

            // Fade in/out based on visibility
            float targetAlpha = _isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            canvasGroup.interactable = _isVisible;
            canvasGroup.blocksRaycasts = _isVisible;
        }

        private void UpdateQuotaDisplay()
        {
            if (quotaProgressBar != null)
            {
                quotaProgressBar.maxValue = _maxQuota;
                quotaProgressBar.value = _currentQuota;
            }

            if (quotaText != null)
            {
                quotaText.text = $"{_currentQuota:N0}/{_maxQuota:N0} food (daily)";
            }

            if (quotaFillImage != null)
            {
                bool isComplete = _currentQuota >= _maxQuota;
                quotaFillImage.color = isComplete ? completedColor : normalColor;
            }
        }

        private void UpdateWalletDisplay()
        {
            if (walletText != null)
            {
                walletText.text = _walletAmount.ToString("N0");
            }
        }

        #region Public Setters (will be replaced by EconomyManager subscriptions)

        /// <summary>
        /// Set the current quota progress
        /// </summary>
        public void SetQuota(int current, int max)
        {
            _currentQuota = current;
            _maxQuota = max;
            UpdateQuotaDisplay();
        }

        /// <summary>
        /// Set the wallet amount
        /// </summary>
        public void SetWallet(int amount)
        {
            _walletAmount = amount;
            UpdateWalletDisplay();
        }

        /// <summary>
        /// Animate food particles from quota bar to wallet (called when overflow happens)
        /// </summary>
        public void AnimateOverflowToWallet(int amount)
        {
            // TODO: Implement particle animation
            // For now, just update the wallet directly
            _walletAmount += amount;
            UpdateWalletDisplay();
        }

        #endregion
    }
}
