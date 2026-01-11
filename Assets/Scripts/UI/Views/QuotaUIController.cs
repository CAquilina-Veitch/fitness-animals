using R3;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StepPet.Core;

namespace StepPet.UI.Views
{
    /// <summary>
    /// Controls the quota progress bar display.
    /// Subscribes to EconomyManager for reactive updates.
    /// </summary>
    public class QuotaUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider quotaProgressBar;
        [SerializeField] private TextMeshProUGUI quotaText;
        [SerializeField] private Image quotaFillImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.4f, 0.7f, 1f);
        [SerializeField] private Color completedColor = new Color(0.4f, 1f, 0.5f);

        private int _currentQuota;
        private int _maxQuota = 1000;

        private void Start()
        {
            var economy = EconomyManager.Instance;
            if (economy == null)
            {
                Debug.LogWarning("[QuotaUIController] EconomyManager not found");
                return;
            }

            economy.CurrentQuota
                .Subscribe(OnCurrentQuotaChanged)
                .AddTo(this);

            economy.MaxQuota
                .Subscribe(OnMaxQuotaChanged)
                .AddTo(this);
        }

        private void OnCurrentQuotaChanged(int value)
        {
            _currentQuota = value;
            UpdateDisplay();
        }

        private void OnMaxQuotaChanged(int value)
        {
            _maxQuota = value;
            UpdateDisplay();
        }

        private void UpdateDisplay()
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
    }
}
