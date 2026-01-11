using R3;
using UnityEngine;
using StepPet.Core;

namespace StepPet.UI.Views
{
    /// <summary>
    /// Container view for the top HUD overlay.
    /// Manages visibility (hide on FriendsList) and fade animation.
    /// Child components (QuotaUIController, WalletUIController) handle their own data.
    /// </summary>
    public class TopHUDView : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private CanvasGroup canvasGroup;

        private bool _isVisible = true;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CurrentPage
                    .Subscribe(OnPageChanged)
                    .AddTo(this);
            }
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
    }
}
