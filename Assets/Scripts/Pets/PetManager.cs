using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using StepPet.Core;

namespace StepPet.Pets
{
    /// <summary>
    /// Manages all pet data and state using reactive properties.
    /// Coordinates between pet instances, UI, and save system.
    /// </summary>
    public class PetManager : MonoBehaviour
    {
        public static PetManager Instance { get; private set; }

        [Header("Animal Definitions")]
        [Tooltip("All available animal types in the game")]
        [SerializeField] private List<AnimalData> animalDefinitions = new();

        /// <summary>
        /// All pet instances (owned and in-progress)
        /// </summary>
        public ReactiveProperty<List<PetInstance>> AllPets { get; } = new(new List<PetInstance>());

        /// <summary>
        /// Only owned pets (for display and daily quota calculation)
        /// </summary>
        public ReadOnlyReactiveProperty<List<PetInstance>> OwnedPets { get; private set; }

        /// <summary>
        /// The main/first pet
        /// </summary>
        public ReadOnlyReactiveProperty<PetInstance> MainPet { get; private set; }

        /// <summary>
        /// Pet currently in unlock challenge (if any)
        /// </summary>
        public ReadOnlyReactiveProperty<PetInstance> ActiveChallengePet { get; private set; }

        /// <summary>
        /// Total daily food requirement from all owned pets
        /// </summary>
        public ReadOnlyReactiveProperty<int> TotalDailyRequirement { get; private set; }

        // Lookup dictionary for animal definitions
        private Dictionary<AnimalType, AnimalData> _animalDataLookup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Build lookup dictionary
            _animalDataLookup = new Dictionary<AnimalType, AnimalData>();
            foreach (var data in animalDefinitions)
            {
                if (data != null && !_animalDataLookup.ContainsKey(data.AnimalType))
                {
                    _animalDataLookup[data.AnimalType] = data;
                }
            }

            // Create derived reactive properties
            OwnedPets = AllPets
                .Select(pets => pets.Where(p => p.IsOwned).ToList())
                .ToReadOnlyReactiveProperty();

            MainPet = AllPets
                .Select(pets => pets.FirstOrDefault(p => p.IsMainPet))
                .ToReadOnlyReactiveProperty();

            ActiveChallengePet = AllPets
                .Select(pets => pets.FirstOrDefault(p => p.IsInUnlockChallenge))
                .ToReadOnlyReactiveProperty();

            TotalDailyRequirement = OwnedPets
                .Select(pets => pets.Sum(p => GetAnimalData(p.AnimalType)?.DailyRequirement ?? 0))
                .ToReadOnlyReactiveProperty();
        }

        private void Start()
        {
            // Sync pet order with UIManager
            SyncPetOrderWithUI();

            // Subscribe to pet list changes to keep UI in sync
            AllPets.Subscribe(_ => SyncPetOrderWithUI());
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            AllPets.Dispose();
        }

        /// <summary>
        /// Get the AnimalData definition for an animal type
        /// </summary>
        public AnimalData GetAnimalData(AnimalType type)
        {
            _animalDataLookup.TryGetValue(type, out var data);
            return data;
        }

        /// <summary>
        /// Get a pet instance by its unique ID
        /// </summary>
        public PetInstance GetPetById(string instanceId)
        {
            return AllPets.Value.FirstOrDefault(p => p.InstanceId == instanceId);
        }

        /// <summary>
        /// Create the player's main pet (called during onboarding)
        /// </summary>
        public PetInstance CreateMainPet(AnimalType type, string customName)
        {
            // Ensure we don't already have a main pet
            if (MainPet.CurrentValue != null)
            {
                Debug.LogWarning("[PetManager] Main pet already exists!");
                return MainPet.CurrentValue;
            }

            var pet = new PetInstance(type, customName, isMainPet: true);
            var pets = new List<PetInstance>(AllPets.Value) { pet };
            AllPets.Value = pets;

            Debug.Log($"[PetManager] Created main pet: {customName} ({type})");
            return pet;
        }

        /// <summary>
        /// Start an unlock challenge for an animal type
        /// </summary>
        public PetInstance StartUnlockChallenge(AnimalType type, string customName = null)
        {
            // Check if we already have an active challenge
            if (ActiveChallengePet.CurrentValue != null)
            {
                Debug.LogWarning("[PetManager] Already have an active unlock challenge!");
                return null;
            }

            var animalData = GetAnimalData(type);
            if (animalData == null)
            {
                Debug.LogError($"[PetManager] No AnimalData found for {type}");
                return null;
            }

            if (animalData.IsStarterPet)
            {
                Debug.LogWarning($"[PetManager] {type} is a starter pet, cannot unlock via challenge");
                return null;
            }

            // Check if we already own this type
            if (AllPets.Value.Any(p => p.AnimalType == type && p.IsOwned))
            {
                Debug.LogWarning($"[PetManager] Already own a {type}");
                return null;
            }

            // Find or create the pet instance
            var existingPet = AllPets.Value.FirstOrDefault(p => p.AnimalType == type);
            if (existingPet != null)
            {
                if (!existingPet.CanStartChallenge)
                {
                    Debug.LogWarning($"[PetManager] Cannot start challenge for {type} - on cooldown or already in progress");
                    return null;
                }

                existingPet.StartUnlockChallenge(animalData.UnlockGoal, animalData.UnlockDays);
                NotifyPetsChanged();
                return existingPet;
            }
            else
            {
                var pet = new PetInstance(type, customName ?? animalData.DisplayName);
                pet.StartUnlockChallenge(animalData.UnlockGoal, animalData.UnlockDays);

                var pets = new List<PetInstance>(AllPets.Value) { pet };
                AllPets.Value = pets;
                return pet;
            }
        }

        /// <summary>
        /// Add overflow steps to the active unlock challenge
        /// </summary>
        public void AddOverflowStepsToChallenge(int steps)
        {
            var challengePet = ActiveChallengePet.CurrentValue;
            if (challengePet == null) return;

            var animalData = GetAnimalData(challengePet.AnimalType);
            if (animalData == null) return;

            challengePet.AddUnlockProgress(steps);

            // Check if challenge is complete
            if (challengePet.UnlockProgress >= animalData.UnlockGoal)
            {
                CompleteUnlockChallenge();
            }
            else
            {
                NotifyPetsChanged();
            }
        }

        /// <summary>
        /// Complete the current unlock challenge
        /// </summary>
        public void CompleteUnlockChallenge()
        {
            var challengePet = ActiveChallengePet.CurrentValue;
            if (challengePet == null) return;

            challengePet.CompleteUnlock();
            NotifyPetsChanged();

            Debug.Log($"[PetManager] Unlock complete! {challengePet.CustomName} is now yours!");
        }

        /// <summary>
        /// Abandon the current unlock challenge
        /// </summary>
        public void AbandonUnlockChallenge()
        {
            var challengePet = ActiveChallengePet.CurrentValue;
            if (challengePet == null) return;

            challengePet.FailUnlockChallenge();
            NotifyPetsChanged();

            Debug.Log($"[PetManager] Challenge abandoned. Banked {challengePet.BankedProgress} steps for next time.");
        }

        /// <summary>
        /// Get all starter pet types (for onboarding selection)
        /// </summary>
        public List<AnimalData> GetStarterPets()
        {
            return animalDefinitions.Where(d => d != null && d.IsStarterPet).ToList();
        }

        /// <summary>
        /// Get all unlockable pet types (for shop)
        /// </summary>
        public List<AnimalData> GetUnlockablePets()
        {
            return animalDefinitions.Where(d => d != null && !d.IsStarterPet).ToList();
        }

        /// <summary>
        /// Force a refresh of derived properties
        /// </summary>
        private void NotifyPetsChanged()
        {
            // Trigger reactive update by reassigning the list
            AllPets.Value = new List<PetInstance>(AllPets.Value);
        }

        /// <summary>
        /// Sync the pet order with UIManager for navigation
        /// </summary>
        private void SyncPetOrderWithUI()
        {
            if (UIManager.Instance == null) return;

            var ownedPetIds = AllPets.Value
                .Where(p => p.IsOwned || p.IsInUnlockChallenge)
                .OrderByDescending(p => p.IsMainPet) // Main pet first
                .ThenBy(p => p.UnlockedAt ?? System.DateTime.MaxValue) // Then by unlock date
                .Select(p => p.InstanceId)
                .ToArray();

            UIManager.Instance.SetPetOrder(ownedPetIds);
        }

        /// <summary>
        /// Load pets from save data (called by save system)
        /// </summary>
        public void LoadPets(List<PetInstance> pets)
        {
            AllPets.Value = pets ?? new List<PetInstance>();
        }

        /// <summary>
        /// Get all pets for saving
        /// </summary>
        public List<PetInstance> GetPetsForSave()
        {
            return new List<PetInstance>(AllPets.Value);
        }
    }
}
