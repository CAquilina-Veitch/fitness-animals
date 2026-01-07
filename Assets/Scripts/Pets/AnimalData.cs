using UnityEngine;

namespace StepPet.Pets
{
    /// <summary>
    /// ScriptableObject defining an animal type's base stats and visuals.
    /// Create one asset per animal type (Puppy, Kitten, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "New Animal", menuName = "Step Pet/Animal Data")]
    public class AnimalData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private AnimalType animalType;
        [SerializeField] private string displayName = "Baby Animal";
        [SerializeField, TextArea] private string description;

        [Header("Stats")]
        [Tooltip("Daily food requirement once owned")]
        [SerializeField] private int dailyRequirement = 300;

        [Header("Unlock Challenge (0 = starter pet, no challenge needed)")]
        [Tooltip("Total steps needed to unlock (0 for starter pets)")]
        [SerializeField] private int unlockGoal;
        [Tooltip("Days to complete the challenge")]
        [SerializeField] private int unlockDays = 14;

        [Header("Visuals")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite sprite;
        [SerializeField] private RuntimeAnimatorController animatorController;

        [Header("Wandering")]
        [SerializeField] private float wanderSpeed = 1f;
        [SerializeField] private float wanderRadius = 3f;

        // Public accessors
        public AnimalType AnimalType => animalType;
        public string DisplayName => displayName;
        public string Description => description;
        public int DailyRequirement => dailyRequirement;
        public int UnlockGoal => unlockGoal;
        public int UnlockDays => unlockDays;
        public bool IsStarterPet => unlockGoal == 0;
        public Sprite Icon => icon;
        public Sprite Sprite => sprite;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public float WanderSpeed => wanderSpeed;
        public float WanderRadius => wanderRadius;
    }
}
