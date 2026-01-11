using R3;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StepPet.Core;

namespace StepPet.UI.Views
{
    /// <summary>
    /// Controls the wallet amount display.
    /// Subscribes to EconomyManager for reactive updates.
    /// </summary>
    public class WalletUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI walletText;
        [SerializeField] private Image walletIcon;

        private void Start()
        {
            var economy = EconomyManager.Instance;
            if (economy == null)
            {
                Debug.LogWarning("[WalletUIController] EconomyManager not found");
                return;
            }

            economy.Wallet
                .Subscribe(OnWalletChanged)
                .AddTo(this);
        }

        private void OnWalletChanged(int amount)
        {
            if (walletText != null)
            {
                walletText.text = amount.ToString("N0");
            }
        }
    }
}
