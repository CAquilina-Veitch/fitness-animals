using System.Collections.Generic;
using R3;
using StepPet.Models;

namespace StepPet.Managers
{
    public class PetManager
    {
        private static PetManager _instance;
        public static PetManager Instance => _instance ??= new PetManager();

        // State
        private readonly ReactiveProperty<List<PetData>> _ownedPets = new(new List<PetData>());
        private readonly ReactiveProperty<int> _currentPetIndex = new(0);
        private readonly ReactiveProperty<PetData> _activeChallenge = new(null);
        private readonly ReactiveProperty<List<PetData>> _availablePets = new(new List<PetData>());

        // Public read-only observables
        public ReadOnlyReactiveProperty<List<PetData>> OwnedPets => _ownedPets;
        public ReadOnlyReactiveProperty<int> CurrentPetIndex => _currentPetIndex;
        public ReadOnlyReactiveProperty<PetData> ActiveChallenge => _activeChallenge;
        public ReadOnlyReactiveProperty<List<PetData>> AvailablePets => _availablePets;

        // Computed properties
        public Observable<PetData> CurrentPet => _currentPetIndex.CombineLatest(_ownedPets, (index, pets) =>
            pets.Count > 0 && index >= 0 && index < pets.Count ? pets[index] : null);

        public Observable<int> TotalDailyRequirement => _ownedPets.Select(pets =>
        {
            int total = 0;
            foreach (var pet in pets)
            {
                if (pet.IsOwned)
                    total += pet.DailyRequirement;
            }
            return total;
        });

        public Observable<bool> HasActiveChallenge => _activeChallenge.Select(c => c != null);

        private PetManager() { }

        // Setters
        public void SetOwnedPets(List<PetData> pets)
        {
            _ownedPets.Value = pets;
        }

        public void SetCurrentPetIndex(int index)
        {
            var pets = _ownedPets.Value;
            if (pets.Count > 0 && index >= 0 && index < pets.Count)
            {
                _currentPetIndex.Value = index;
            }
        }

        public void SetActiveChallenge(PetData pet)
        {
            _activeChallenge.Value = pet;
        }

        public void SetAvailablePets(List<PetData> pets)
        {
            _availablePets.Value = pets;
        }

        public void AddPet(PetData pet)
        {
            var pets = new List<PetData>(_ownedPets.Value) { pet };
            _ownedPets.Value = pets;
        }

        public void UpdatePet(PetData updatedPet)
        {
            var pets = new List<PetData>(_ownedPets.Value);
            for (int i = 0; i < pets.Count; i++)
            {
                if (pets[i].Id == updatedPet.Id)
                {
                    pets[i] = updatedPet;
                    break;
                }
            }
            _ownedPets.Value = pets;
        }

        public void ClearActiveChallenge()
        {
            _activeChallenge.Value = null;
        }

        public void AddChallengeProgress(int steps)
        {
            if (_activeChallenge.Value != null)
            {
                _activeChallenge.Value.UnlockProgress += steps;
                // Trigger update by reassigning
                _activeChallenge.Value = _activeChallenge.Value;
            }
        }
    }
}
