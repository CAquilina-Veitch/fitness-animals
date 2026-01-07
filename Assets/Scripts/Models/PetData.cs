using System;

namespace StepPet.Models
{
    [Serializable]
    public class PetData
    {
        public string Id;
        public string Name;
        public string AnimalType;
        public int DailyRequirement;
        public bool IsMainPet;
        public bool IsOwned;

        // Challenge fields
        public int UnlockGoal;
        public int UnlockProgress;
        public int BankedProgress;
        public DateTime? UnlockDeadline;
        public DateTime? CooldownUntil;

        // Equipped accessories
        public string[] EquippedAccessories;

        public PetData()
        {
            Id = Guid.NewGuid().ToString();
            EquippedAccessories = Array.Empty<string>();
        }

        public PetData(string animalType, string name, int dailyRequirement, bool isMainPet = false)
        {
            Id = Guid.NewGuid().ToString();
            AnimalType = animalType;
            Name = name;
            DailyRequirement = dailyRequirement;
            IsMainPet = isMainPet;
            IsOwned = isMainPet;
            EquippedAccessories = Array.Empty<string>();
        }
    }
}
