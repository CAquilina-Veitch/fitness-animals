using System;
using StepPet.Managers;

namespace StepPet.Controllers
{
    public class EconomyController
    {
        private readonly EconomyManager _economyManager;
        private readonly PetManager _petManager;

        public EconomyController(EconomyManager economyManager, PetManager petManager)
        {
            _economyManager = economyManager;
            _petManager = petManager;
        }

        public void RecordSteps(int newSteps)
        {
            _economyManager.AddSteps(newSteps);

            // If we have overflow and an active challenge, add to challenge progress
            int currentSteps = _economyManager.TodaySteps.CurrentValue;
            int quota = _economyManager.DailyQuota.CurrentValue;

            if (currentSteps > quota && _petManager.ActiveChallenge.CurrentValue != null)
            {
                int previousSteps = currentSteps - newSteps;
                int previousOverflow = Math.Max(0, previousSteps - quota);
                int currentOverflow = currentSteps - quota;
                int newOverflowSteps = currentOverflow - previousOverflow;

                if (newOverflowSteps > 0)
                {
                    _petManager.AddChallengeProgress(newOverflowSteps);
                }
            }
        }

        public void ProcessOverflow()
        {
            int steps = _economyManager.TodaySteps.CurrentValue;
            int quota = _economyManager.DailyQuota.CurrentValue;
            int overflow = Math.Max(0, steps - quota);

            if (overflow > 0)
            {
                _economyManager.AddToWallet(overflow);
            }
        }

        public bool SpendFood(int amount)
        {
            // Can only spend if quota is met
            int steps = _economyManager.TodaySteps.CurrentValue;
            int quota = _economyManager.DailyQuota.CurrentValue;

            if (steps < quota)
            {
                return false;
            }

            return _economyManager.TrySpendFromWallet(amount);
        }

        public void CheckMidnightReset()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string storedDate = _economyManager.TodayDate.CurrentValue;

            if (today != storedDate)
            {
                // Process any remaining overflow before reset
                ProcessOverflow();
                _economyManager.ResetDaily();
            }
        }

        public void SyncDailyQuotaFromPets()
        {
            int totalRequirement = 0;
            foreach (var pet in _petManager.OwnedPets.CurrentValue)
            {
                if (pet.IsOwned)
                {
                    totalRequirement += pet.DailyRequirement;
                }
            }
            _economyManager.SetDailyQuota(totalRequirement);
        }
    }
}
