using UnityEngine;
using StepPet.Pets;

namespace StepPet.Core
{
    /// <summary>
    /// Main game manager that handles app lifecycle and initialization.
    /// This is the entry point for the application.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Manager References (Optional - will find if not set)")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Camera.CameraManager cameraManager;
        [SerializeField] private PetManager petManager;

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool createTestPetOnStart = true;

        /// <summary>
        /// Whether the game has finished initializing
        /// </summary>
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Optional: persist across scenes
            // DontDestroyOnLoad(gameObject);

            // Set target frame rate for mobile
            Application.targetFrameRate = 60;

            // Lock to portrait orientation
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            Log("GameManager initializing...");

            // Find managers if not assigned
            if (uiManager == null)
                uiManager = FindAnyObjectByType<UIManager>();

            if (cameraManager == null)
                cameraManager = FindAnyObjectByType<Camera.CameraManager>();

            if (petManager == null)
                petManager = FindAnyObjectByType<PetManager>();

            // Validate required components
            if (uiManager == null)
            {
                LogError("UIManager not found! Please add UIManager to the scene.");
                return;
            }

            if (cameraManager == null)
            {
                LogError("CameraManager not found! Please add CameraManager to the scene.");
                return;
            }

            if (petManager == null)
            {
                LogError("PetManager not found! Please add PetManager to the scene.");
                return;
            }

            // Initialize default state
            InitializeDefaultState();

            IsInitialized = true;
            Log("GameManager initialized successfully.");
        }

        private void InitializeDefaultState()
        {
            // Start at Scene Overview
            uiManager.SetPage(UIPage.SceneOverview);

            // TODO: Load saved data from Firebase/local storage
            // For now, create a test pet if none exist
            if (createTestPetOnStart && petManager.MainPet.CurrentValue == null)
            {
                CreateTestMainPet();
            }
        }

        private void CreateTestMainPet()
        {
            // Create a test main pet for development
            var testPet = petManager.CreateMainPet(AnimalType.Puppy, "Buddy");

            if (testPet != null)
            {
                Log($"Created test main pet: {testPet.CustomName} ({testPet.AnimalType})");

                // Spawn the pet GameObject in the scene
                SpawnPetInScene(testPet);
            }
        }

        private void SpawnPetInScene(PetInstance petInstance)
        {
            // Create a simple pet GameObject
            var petGO = new GameObject($"Pet_{petInstance.CustomName}");
            petGO.transform.position = Vector3.zero;

            // Add required components
            var spriteRenderer = petGO.AddComponent<SpriteRenderer>();
            petGO.AddComponent<BoxCollider2D>();

            // Add Pet component and initialize
            var pet = petGO.AddComponent<Pet>();
            pet.Initialize(petInstance);

            // Set a placeholder color if no sprite
            var animalData = petManager.GetAnimalData(petInstance.AnimalType);
            if (animalData == null || animalData.Sprite == null)
            {
                // Create a simple colored square as placeholder
                spriteRenderer.sprite = CreatePlaceholderSprite();
                spriteRenderer.color = GetPlaceholderColor(petInstance.AnimalType);
            }

            Log($"Spawned pet GameObject: {petGO.name}");
        }

        private Sprite CreatePlaceholderSprite()
        {
            // Create a simple 64x64 white square texture
            var texture = new Texture2D(64, 64);
            var colors = new Color[64 * 64];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        private Color GetPlaceholderColor(AnimalType type)
        {
            return type switch
            {
                AnimalType.Puppy => new Color(0.8f, 0.6f, 0.4f),      // Brown
                AnimalType.Kitten => new Color(1f, 0.8f, 0.6f),       // Orange
                AnimalType.Bunny => new Color(0.9f, 0.9f, 0.9f),      // White
                AnimalType.Duckling => new Color(1f, 1f, 0.4f),       // Yellow
                AnimalType.BabyGiraffe => new Color(1f, 0.9f, 0.5f),  // Tan
                AnimalType.BabyElephant => new Color(0.7f, 0.7f, 0.7f), // Gray
                AnimalType.BabyPenguin => new Color(0.2f, 0.2f, 0.2f),  // Dark gray
                AnimalType.BabyPanda => new Color(1f, 1f, 1f),          // White
                _ => Color.magenta
            };
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App is being paused (backgrounded)
                Log("App paused - saving state...");
                // TODO: Save game state
            }
            else
            {
                // App is resuming
                Log("App resumed");
                // TODO: Sync steps, refresh data
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Log("App gained focus");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #region Logging Helpers

        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[GameManager] {message}");
        }

        #endregion
    }
}
