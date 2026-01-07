using System;

namespace StepPet.Pets
{
    /// <summary>
    /// Runtime data for an individual pet instance.
    /// This is NOT a MonoBehaviour - it's pure data that can be serialized/saved.
    /// </summary>
    [Serializable]
    public class PetInstance
    {
        /// <summary>Unique ID for this pet instance (GUID)</summary>
        public string InstanceId;

        /// <summary>The type of animal (references AnimalData ScriptableObject)</summary>
        public AnimalType AnimalType;

        /// <summary>Custom name given by the player</summary>
        public string CustomName;

        /// <summary>Whether this is the player's main/first pet</summary>
        public bool IsMainPet;

        /// <summary>Whether this pet is fully owned (vs in-progress unlock)</summary>
        public bool IsOwned;

        /// <summary>Equipped accessory IDs</summary>
        public string[] EquippedAccessories;

        /// <summary>When the pet was unlocked (null if not owned)</summary>
        public DateTime? UnlockedAt;

        // Unlock challenge state (only relevant if IsOwned == false)
        /// <summary>Current progress toward unlock goal</summary>
        public int UnlockProgress;

        /// <summary>Banked progress from previous attempts</summary>
        public int BankedProgress;

        /// <summary>Deadline for current unlock attempt</summary>
        public DateTime? UnlockDeadline;

        /// <summary>Cooldown end time (if recently failed/abandoned)</summary>
        public DateTime? CooldownUntil;

        /// <summary>
        /// Create a new pet instance
        /// </summary>
        public PetInstance(AnimalType animalType, string customName, bool isMainPet = false)
        {
            InstanceId = Guid.NewGuid().ToString();
            AnimalType = animalType;
            CustomName = customName;
            IsMainPet = isMainPet;
            IsOwned = isMainPet; // Main pet is owned immediately
            EquippedAccessories = Array.Empty<string>();

            if (isMainPet)
            {
                UnlockedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Check if this pet is currently in an unlock challenge
        /// </summary>
        public bool IsInUnlockChallenge =>
            !IsOwned &&
            UnlockDeadline.HasValue &&
            UnlockDeadline.Value > DateTime.UtcNow;

        /// <summary>
        /// Check if this pet is on cooldown
        /// </summary>
        public bool IsOnCooldown =>
            !IsOwned &&
            CooldownUntil.HasValue &&
            CooldownUntil.Value > DateTime.UtcNow;

        /// <summary>
        /// Check if this pet is available to start an unlock challenge
        /// </summary>
        public bool CanStartChallenge =>
            !IsOwned &&
            !IsInUnlockChallenge &&
            !IsOnCooldown;

        /// <summary>
        /// Start an unlock challenge for this pet
        /// </summary>
        public void StartUnlockChallenge(int goalSteps, int days)
        {
            if (!CanStartChallenge) return;

            UnlockProgress = BankedProgress; // Start with banked progress
            UnlockDeadline = DateTime.UtcNow.AddDays(days);
            CooldownUntil = null;
        }

        /// <summary>
        /// Add progress to the unlock challenge
        /// </summary>
        public void AddUnlockProgress(int steps)
        {
            if (!IsInUnlockChallenge) return;
            UnlockProgress += steps;
        }

        /// <summary>
        /// Complete the unlock challenge (pet is now owned)
        /// </summary>
        public void CompleteUnlock()
        {
            IsOwned = true;
            UnlockedAt = DateTime.UtcNow;
            UnlockProgress = 0;
            BankedProgress = 0;
            UnlockDeadline = null;
            CooldownUntil = null;
        }

        /// <summary>
        /// Fail or abandon the unlock challenge
        /// </summary>
        public void FailUnlockChallenge(int cooldownDays = 14)
        {
            // Bank 10% of progress
            BankedProgress += UnlockProgress / 10;
            UnlockProgress = 0;
            UnlockDeadline = null;
            CooldownUntil = DateTime.UtcNow.AddDays(cooldownDays);
        }
    }
}
