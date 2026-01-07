using UnityEngine;
using StepPet.Input;

namespace StepPet.Pets
{
    /// <summary>
    /// MonoBehaviour for pet GameObjects in the scene.
    /// Links to a PetInstance (runtime data) and AnimalData (definition).
    /// Handles visuals and wandering behavior.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class Pet : MonoBehaviour, IPetIdentifier
    {
        [Header("Pet Data")]
        [Tooltip("The instance ID this pet represents (set at runtime)")]
        [SerializeField] private string instanceId;

        [Header("Wandering Behavior")]
        [SerializeField] private bool enableWandering = true;
        [SerializeField] private float minWaitTime = 1f;
        [SerializeField] private float maxWaitTime = 3f;

        // Cached references
        private PetInstance _petInstance;
        private AnimalData _animalData;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        // Wandering state
        private Vector3 _homePosition;
        private Vector3 _targetPosition;
        private float _waitTimer;
        private bool _isWaiting = true;

        // IPetIdentifier implementation
        public string PetId => instanceId;

        // Public accessors
        public PetInstance PetInstance => _petInstance;
        public AnimalData AnimalData => _animalData;
        public string DisplayName => _petInstance?.CustomName ?? _animalData?.DisplayName ?? "Pet";
        public bool IsMainPet => _petInstance?.IsMainPet ?? false;
        public bool IsOwned => _petInstance?.IsOwned ?? false;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();

            _homePosition = transform.position;
            _targetPosition = _homePosition;
        }

        private void Start()
        {
            // If we have an instance ID, try to link to PetManager data
            if (!string.IsNullOrEmpty(instanceId))
            {
                LinkToPetInstance(instanceId);
            }

            // Start with a random wait time
            _waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }

        private void Update()
        {
            if (!enableWandering || _animalData == null) return;

            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0)
                {
                    PickNewWanderTarget();
                    _isWaiting = false;
                }
            }
            else
            {
                MoveTowardTarget();
            }
        }

        /// <summary>
        /// Initialize this pet with a PetInstance from PetManager
        /// </summary>
        public void Initialize(PetInstance petInstance)
        {
            if (petInstance == null)
            {
                Debug.LogError("[Pet] Cannot initialize with null PetInstance");
                return;
            }

            _petInstance = petInstance;
            instanceId = petInstance.InstanceId;

            // Get the animal data from PetManager
            if (PetManager.Instance != null)
            {
                _animalData = PetManager.Instance.GetAnimalData(petInstance.AnimalType);
            }

            ApplyVisuals();
        }

        /// <summary>
        /// Link to an existing PetInstance by ID
        /// </summary>
        public void LinkToPetInstance(string petInstanceId)
        {
            if (PetManager.Instance == null)
            {
                Debug.LogWarning("[Pet] PetManager not available, cannot link to instance");
                return;
            }

            var instance = PetManager.Instance.GetPetById(petInstanceId);
            if (instance != null)
            {
                Initialize(instance);
            }
            else
            {
                Debug.LogWarning($"[Pet] Could not find PetInstance with ID: {petInstanceId}");
            }
        }

        /// <summary>
        /// Apply visuals from AnimalData
        /// </summary>
        private void ApplyVisuals()
        {
            if (_animalData == null) return;

            // Apply sprite
            if (_animalData.Sprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _animalData.Sprite;
            }

            // Apply animator controller
            if (_animalData.AnimatorController != null && _animator != null)
            {
                _animator.runtimeAnimatorController = _animalData.AnimatorController;
            }
        }

        private void PickNewWanderTarget()
        {
            float radius = _animalData?.WanderRadius ?? 3f;
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            _targetPosition = _homePosition + new Vector3(randomOffset.x, randomOffset.y, 0);
        }

        private void MoveTowardTarget()
        {
            float speed = _animalData?.WanderSpeed ?? 1f;
            Vector3 direction = (_targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, _targetPosition);

            if (distance > 0.1f)
            {
                transform.position += direction * speed * Time.deltaTime;

                // Flip sprite based on movement direction
                if (_spriteRenderer != null && Mathf.Abs(direction.x) > 0.1f)
                {
                    _spriteRenderer.flipX = direction.x < 0;
                }
            }
            else
            {
                // Reached target, start waiting
                _isWaiting = true;
                _waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
        }

        /// <summary>
        /// Called when the pet is selected (tapped)
        /// </summary>
        public void OnSelected()
        {
            Debug.Log($"[Pet] {DisplayName} selected!");
            // Play selection animation/sound
        }

        /// <summary>
        /// Set home position for wandering
        /// </summary>
        public void SetHomePosition(Vector3 position)
        {
            _homePosition = position;
            transform.position = position;
            _targetPosition = position;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw wander radius in editor
            Gizmos.color = Color.yellow;
            float radius = _animalData?.WanderRadius ?? 3f;
            Vector3 center = Application.isPlaying ? _homePosition : transform.position;
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}
