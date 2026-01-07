using System;
using System.Collections.Generic;
using StepPet.Managers;
using StepPet.Models;

namespace StepPet.Controllers
{
    public class PetController
    {
        private readonly PetManager _petManager;
        private readonly EconomyManager _economyManager;

        // Pet definitions - daily requirements and unlock goals
        private static readonly Dictionary<string, (int dailyReq, int unlockGoal)> PetDefinitions = new()
        {
            // Starter pets
            { "Puppy", (300, 0) },
            { "Kitten", (300, 0) },
            { "Bunny", (400, 0) },
            // Unlock challenge pets
            { "Duckling", (400, 8000) },
            { "BabyGiraffe", (600, 15000) },
            { "BabyElephant", (800, 20000) },
            { "BabyPenguin", (700, 25000) },
            { "BabyPanda", (1000, 35000) }
        };

        private const int ChallengeDurationDays = 14;
        private const int CooldownDurationDays = 14;
        private const float BankPercentage = 0.10f;

        public PetController(PetManager petManager, EconomyManager economyManager)
        {
            _petManager = petManager;
            _economyManager = economyManager;
        }

        public void SelectMainPet(string animalType, string name)
        {
            if (!PetDefinitions.TryGetValue(animalType, out var definition))
            {
                return;
            }

            var mainPet = new PetData(animalType, name, definition.dailyReq, isMainPet: true);
            _petManager.AddPet(mainPet);
        }

        public void NavigateToPet(int index)
        {
            _petManager.SetCurrentPetIndex(index);
        }

        public void NavigateToNextPet()
        {
            int currentIndex = _petManager.CurrentPetIndex.CurrentValue;
            int petCount = _petManager.OwnedPets.CurrentValue.Count;

            if (petCount > 0)
            {
                int nextIndex = (currentIndex + 1) % petCount;
                _petManager.SetCurrentPetIndex(nextIndex);
            }
        }

        public void NavigateToPreviousPet()
        {
            int currentIndex = _petManager.CurrentPetIndex.CurrentValue;
            int petCount = _petManager.OwnedPets.CurrentValue.Count;

            if (petCount > 0)
            {
                int prevIndex = (currentIndex - 1 + petCount) % petCount;
                _petManager.SetCurrentPetIndex(prevIndex);
            }
        }

        public bool StartChallenge(string animalType)
        {
            // Can't start if already have an active challenge
            if (_petManager.ActiveChallenge.CurrentValue != null)
            {
                return false;
            }

            if (!PetDefinitions.TryGetValue(animalType, out var definition))
            {
                return false;
            }

            // Check if already owned
            foreach (var pet in _petManager.OwnedPets.CurrentValue)
            {
                if (pet.AnimalType == animalType && pet.IsOwned)
                {
                    return false;
                }
            }

            // Check cooldown from available pets
            foreach (var pet in _petManager.AvailablePets.CurrentValue)
            {
                if (pet.AnimalType == animalType)
                {
                    if (pet.CooldownUntil.HasValue && pet.CooldownUntil.Value > DateTime.Now)
                    {
                        return false;
                    }

                    // Start challenge with banked progress
                    var challengePet = new PetData
                    {
                        Id = pet.Id,
                        AnimalType = animalType,
                        Name = animalType, // Placeholder until unlocked
                        DailyRequirement = definition.dailyReq,
                        IsMainPet = false,
                        IsOwned = false,
                        UnlockGoal = definition.unlockGoal,
                        UnlockProgress = pet.BankedProgress,
                        BankedProgress = pet.BankedProgress,
                        UnlockDeadline = DateTime.Now.AddDays(ChallengeDurationDays)
                    };

                    _petManager.SetActiveChallenge(challengePet);
                    _petManager.AddPet(challengePet);
                    return true;
                }
            }

            // New challenge pet
            var newChallengePet = new PetData
            {
                AnimalType = animalType,
                Name = animalType,
                DailyRequirement = definition.dailyReq,
                IsMainPet = false,
                IsOwned = false,
                UnlockGoal = definition.unlockGoal,
                UnlockProgress = 0,
                BankedProgress = 0,
                UnlockDeadline = DateTime.Now.AddDays(ChallengeDurationDays)
            };

            _petManager.SetActiveChallenge(newChallengePet);
            _petManager.AddPet(newChallengePet);
            return true;
        }

        public void AbandonChallenge()
        {
            var challenge = _petManager.ActiveChallenge.CurrentValue;
            if (challenge == null)
            {
                return;
            }

            // Bank 10% of progress
            int banked = (int)(challenge.UnlockProgress * BankPercentage);
            challenge.BankedProgress += banked;
            challenge.CooldownUntil = DateTime.Now.AddDays(CooldownDurationDays);
            challenge.UnlockProgress = 0;
            challenge.UnlockDeadline = null;
            challenge.IsOwned = false;

            // Remove from owned pets, update available pets
            var ownedPets = new List<PetData>(_petManager.OwnedPets.CurrentValue);
            ownedPets.RemoveAll(p => p.Id == challenge.Id);
            _petManager.SetOwnedPets(ownedPets);

            // Update available pets with new banked progress
            var availablePets = new List<PetData>(_petManager.AvailablePets.CurrentValue);
            bool found = false;
            for (int i = 0; i < availablePets.Count; i++)
            {
                if (availablePets[i].AnimalType == challenge.AnimalType)
                {
                    availablePets[i] = challenge;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                availablePets.Add(challenge);
            }
            _petManager.SetAvailablePets(availablePets);

            _petManager.ClearActiveChallenge();
        }

        public void CheckChallengeCompletion()
        {
            var challenge = _petManager.ActiveChallenge.CurrentValue;
            if (challenge == null)
            {
                return;
            }

            // Check if deadline passed
            if (challenge.UnlockDeadline.HasValue && DateTime.Now > challenge.UnlockDeadline.Value)
            {
                // Failed - treat as abandon
                AbandonChallenge();
                return;
            }

            // Check if goal reached
            if (challenge.UnlockProgress >= challenge.UnlockGoal)
            {
                CompleteChallenge();
            }
        }

        public void CompleteChallenge()
        {
            var challenge = _petManager.ActiveChallenge.CurrentValue;
            if (challenge == null)
            {
                return;
            }

            // Mark as owned
            challenge.IsOwned = true;
            challenge.UnlockDeadline = null;
            challenge.BankedProgress = 0;

            // Update in owned pets
            _petManager.UpdatePet(challenge);

            // Remove from available pets
            var availablePets = new List<PetData>(_petManager.AvailablePets.CurrentValue);
            availablePets.RemoveAll(p => p.AnimalType == challenge.AnimalType);
            _petManager.SetAvailablePets(availablePets);

            _petManager.ClearActiveChallenge();
        }

        public void InitializeAvailablePets()
        {
            var available = new List<PetData>();

            foreach (var kvp in PetDefinitions)
            {
                if (kvp.Value.unlockGoal > 0) // Only challenge pets
                {
                    available.Add(new PetData
                    {
                        AnimalType = kvp.Key,
                        Name = kvp.Key,
                        DailyRequirement = kvp.Value.dailyReq,
                        UnlockGoal = kvp.Value.unlockGoal,
                        IsOwned = false,
                        IsMainPet = false
                    });
                }
            }

            _petManager.SetAvailablePets(available);
        }
    }
}
