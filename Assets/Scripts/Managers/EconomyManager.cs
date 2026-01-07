using System;
using R3;

namespace StepPet.Managers
{
    public class EconomyManager
    {
        private static EconomyManager _instance;
        public static EconomyManager Instance => _instance ??= new EconomyManager();

        // State
        private readonly ReactiveProperty<int> _wallet = new(0);
        private readonly ReactiveProperty<int> _todaySteps = new(0);
        private readonly ReactiveProperty<int> _dailyQuota = new(0);
        private readonly ReactiveProperty<string> _todayDate = new(DateTime.Now.ToString("yyyy-MM-dd"));
        private readonly ReactiveProperty<int> _lifetimeSteps = new(0);

        // Public read-only observables
        public ReadOnlyReactiveProperty<int> Wallet => _wallet;
        public ReadOnlyReactiveProperty<int> TodaySteps => _todaySteps;
        public ReadOnlyReactiveProperty<int> DailyQuota => _dailyQuota;
        public ReadOnlyReactiveProperty<string> TodayDate => _todayDate;
        public ReadOnlyReactiveProperty<int> LifetimeSteps => _lifetimeSteps;

        // Computed properties
        public Observable<bool> QuotaMet => _todaySteps.CombineLatest(_dailyQuota, (steps, quota) => steps >= quota);
        public Observable<int> Overflow => _todaySteps.CombineLatest(_dailyQuota, (steps, quota) => Math.Max(0, steps - quota));
        public Observable<int> StepsUntilQuota => _todaySteps.CombineLatest(_dailyQuota, (steps, quota) => Math.Max(0, quota - steps));

        private EconomyManager() { }

        // Setters
        public void SetWallet(int value)
        {
            _wallet.Value = value;
        }

        public void SetTodaySteps(int value)
        {
            _todaySteps.Value = value;
        }

        public void SetDailyQuota(int value)
        {
            _dailyQuota.Value = value;
        }

        public void SetTodayDate(string value)
        {
            _todayDate.Value = value;
        }

        public void SetLifetimeSteps(int value)
        {
            _lifetimeSteps.Value = value;
        }

        public void AddToWallet(int amount)
        {
            _wallet.Value += amount;
        }

        public void AddSteps(int amount)
        {
            _todaySteps.Value += amount;
            _lifetimeSteps.Value += amount;
        }

        public bool TrySpendFromWallet(int amount)
        {
            if (_wallet.Value >= amount)
            {
                _wallet.Value -= amount;
                return true;
            }
            return false;
        }

        public void ResetDaily()
        {
            _todaySteps.Value = 0;
            _todayDate.Value = DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
