using UnityEngine;
using StepPet.Managers;
using StepPet.Controllers;

namespace StepPet.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        // Controllers - accessible for UIControls
        public EconomyController EconomyController { get; private set; }
        public PetController PetController { get; private set; }

        // Managers - accessible for subscriptions
        public EconomyManager EconomyManager => EconomyManager.Instance;
        public PetManager PetManager => PetManager.Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeControllers();
            InitializeGame();
        }

        private void InitializeControllers()
        {
            EconomyController = new EconomyController(EconomyManager.Instance, PetManager.Instance);
            PetController = new PetController(PetManager.Instance, EconomyManager.Instance);
        }

        private void InitializeGame()
        {
            // Initialize available pets for challenges
            PetController.InitializeAvailablePets();

            // Check for midnight reset
            EconomyController.CheckMidnightReset();

            // Sync daily quota from owned pets
            EconomyController.SyncDailyQuotaFromPets();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // Check for midnight reset when app regains focus
                EconomyController.CheckMidnightReset();
                EconomyController.SyncDailyQuotaFromPets();

                // Check challenge status
                PetController.CheckChallengeCompletion();
            }
        }
    }
}
