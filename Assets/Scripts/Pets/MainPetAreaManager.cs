using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace StepPet.Pets
{
    /// <summary>
    /// Manages the onscreen pet prefabs in the main pet area.
    /// Holds references to instantiated pets for camera movement and other systems.
    /// </summary>
    public class MainPetAreaManager : MonoBehaviour
    {
        public static MainPetAreaManager Instance { get; private set; }

        [Header("Pet Area Settings")]
        [SerializeField] private Transform petContainer;
        [SerializeField] private Vector2 areaSize = new(10f, 6f);
        [SerializeField] private Vector2 areaCenter = Vector2.zero;

        [Header("Movement Settings")]
        [SerializeField] private float minSpacing = 2f;

        /// <summary>
        /// All active Pet instances in the scene
        /// </summary>
        public ReactiveProperty<List<Pet>> ActivePets { get; } = new(new List<Pet>());

        /// <summary>
        /// Dictionary for quick pet lookup by instance ID
        /// </summary>
        private Dictionary<string, Pet> _petLookup = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            petContainer = transform;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            ActivePets.Dispose();
        }

        /// <summary>
        /// Register a pet that exists in the scene
        /// </summary>
        public void RegisterPet(Pet pet)
        {
            if (pet == null || string.IsNullOrEmpty(pet.PetId)) return;

            if (!_petLookup.ContainsKey(pet.PetId))
            {
                _petLookup[pet.PetId] = pet;
                var pets = new List<Pet>(ActivePets.Value) { pet };
                ActivePets.Value = pets;
                Debug.Log($"[MainPetAreaManager] Registered pet: {pet.DisplayName} ({pet.PetId})");
            }
        }

        /// <summary>
        /// Unregister a pet from the manager
        /// </summary>
        public void UnregisterPet(Pet pet)
        {
            if (pet == null || string.IsNullOrEmpty(pet.PetId)) return;

            if (_petLookup.Remove(pet.PetId))
            {
                var pets = new List<Pet>(ActivePets.Value);
                pets.Remove(pet);
                ActivePets.Value = pets;
                Debug.Log($"[MainPetAreaManager] Unregistered pet: {pet.DisplayName}");
            }
        }

        /// <summary>
        /// Get a pet by its instance ID
        /// </summary>
        public Pet GetPetById(string petId)
        {
            _petLookup.TryGetValue(petId, out var pet);
            return pet;
        }

        /// <summary>
        /// Get the transform of a pet by ID (for camera targeting)
        /// </summary>
        public Transform GetPetTransform(string petId)
        {
            return GetPetById(petId)?.transform;
        }

        /// <summary>
        /// Get all pet transforms (for camera bounds calculation)
        /// </summary>
        public IEnumerable<Transform> GetAllPetTransforms()
        {
            return ActivePets.Value.Select(p => p.transform);
        }

        /// <summary>
        /// Get a random position within the pet area bounds
        /// </summary>
        public Vector3 GetRandomPositionInArea()
        {
            float x = Random.Range(areaCenter.x - areaSize.x / 2f, areaCenter.x + areaSize.x / 2f);
            float y = Random.Range(areaCenter.y - areaSize.y / 2f, areaCenter.y + areaSize.y / 2f);
            return new Vector3(x, y, 0);
        }

        /// <summary>
        /// Get a position that maintains spacing from other pets
        /// </summary>
        public Vector3 GetSpacedPosition()
        {
            const int maxAttempts = 10;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 candidate = GetRandomPositionInArea();
                bool hasSpace = true;

                foreach (var pet in ActivePets.Value)
                {
                    if (Vector3.Distance(candidate, pet.transform.position) < minSpacing)
                    {
                        hasSpace = false;
                        break;
                    }
                }

                if (hasSpace)
                    return candidate;
            }

            return GetRandomPositionInArea();
        }

        /// <summary>
        /// Calculate the center point of all pets (for camera overview)
        /// </summary>
        public Vector3 GetPetsCenterPoint()
        {
            var pets = ActivePets.Value;
            if (pets.Count == 0)
                return areaCenter;

            Vector3 sum = Vector3.zero;
            foreach (var pet in pets)
            {
                sum += pet.transform.position;
            }
            return sum / pets.Count;
        }

        /// <summary>
        /// Calculate bounds encompassing all pets
        /// </summary>
        public Bounds GetPetsBounds()
        {
            var pets = ActivePets.Value;
            if (pets.Count == 0)
                return new Bounds(areaCenter, areaSize);

            Bounds bounds = new Bounds(pets[0].transform.position, Vector3.zero);
            foreach (var pet in pets)
            {
                bounds.Encapsulate(pet.transform.position);
            }

            // Add padding
            bounds.Expand(2f);
            return bounds;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize.x, areaSize.y, 0));
        }
    }
}
