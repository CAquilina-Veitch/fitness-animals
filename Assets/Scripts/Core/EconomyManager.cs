using R3;
using UnityEngine;

namespace StepPet.Core
{
    /// <summary>
    /// Manages the economy state using reactive properties.
    /// UI components subscribe to these properties to react to changes.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        /// <summary>
        /// Current wallet balance (saved food for spending)
        /// </summary>
        public ReactiveProperty<int> Wallet { get; } = new(0);

        /// <summary>
        /// Current daily quota progress (steps/food earned today)
        /// </summary>
        public ReactiveProperty<int> CurrentQuota { get; } = new(0);

        /// <summary>
        /// Maximum daily quota required (sum of all owned pets' requirements)
        /// </summary>
        public ReactiveProperty<int> MaxQuota { get; } = new(1000);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            Wallet.Dispose();
            CurrentQuota.Dispose();
            MaxQuota.Dispose();
        }

        /// <summary>
        /// Add food to the wallet
        /// </summary>
        public void AddToWallet(int amount)
        {
            if (amount <= 0) return;

            var previous = Wallet.Value;
            Wallet.Value += amount;
            Log($"Wallet: {previous} + {amount} = {Wallet.Value}");
        }

        /// <summary>
        /// Spend food from the wallet. Returns true if successful.
        /// </summary>
        public bool SpendFromWallet(int amount)
        {
            if (amount <= 0 || Wallet.Value < amount)
                return false;

            var previous = Wallet.Value;
            Wallet.Value -= amount;
            Log($"Wallet: {previous} - {amount} = {Wallet.Value}");
            return true;
        }

        /// <summary>
        /// Set the current quota progress
        /// </summary>
        public void SetQuotaProgress(int current)
        {
            var previous = CurrentQuota.Value;
            CurrentQuota.Value = Mathf.Max(0, current);
            Log($"Quota: {previous} -> {CurrentQuota.Value}/{MaxQuota.Value}");
        }

        /// <summary>
        /// Set the maximum quota (called when pets are added/removed)
        /// </summary>
        public void SetMaxQuota(int max)
        {
            var previous = MaxQuota.Value;
            MaxQuota.Value = Mathf.Max(1, max);
            Log($"MaxQuota: {previous} -> {MaxQuota.Value}");
        }

        /// <summary>
        /// Check if daily quota is met
        /// </summary>
        public bool IsQuotaMet => CurrentQuota.Value >= MaxQuota.Value;

        /// <summary>
        /// Get overflow amount (steps above quota)
        /// </summary>
        public int GetOverflow() => Mathf.Max(0, CurrentQuota.Value - MaxQuota.Value);

        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[EconomyManager] {message}");
        }
    }
}
