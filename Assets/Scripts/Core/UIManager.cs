using System;
using R3;
using UnityEngine;

namespace StepPet.Core
{
    /// <summary>
    /// Manages UI navigation state using reactive properties.
    /// Other components subscribe to these properties to react to state changes.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        /// <summary>
        /// Current navigation page (FriendsList, SceneOverview, or PetCloseup)
        /// </summary>
        public ReactiveProperty<UIPage> CurrentPage { get; } = new(UIPage.SceneOverview);

        /// <summary>
        /// ID of the currently focused pet (null when in SceneOverview or FriendsList)
        /// </summary>
        public ReactiveProperty<string> FocusedPetId { get; } = new(null);

        /// <summary>
        /// Whether the bottom menu is expanded
        /// </summary>
        public ReactiveProperty<bool> IsBottomMenuExpanded { get; } = new(false);

        /// <summary>
        /// List of pet IDs in order for navigation (swipe through pets)
        /// </summary>
        public ReactiveProperty<string[]> PetOrder { get; } = new(System.Array.Empty<string>());

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) 
                Instance = null;

            CurrentPage.Dispose();
            FocusedPetId.Dispose();
            IsBottomMenuExpanded.Dispose();
            PetOrder.Dispose();
        }

        /// <summary>
        /// Navigate to a specific page
        /// </summary>
        public void SetPage(UIPage page)
        {
            var previousPage = CurrentPage.Value;

            // When leaving PetCloseup, clear focused pet
            if (page != UIPage.PetCloseup)
            {
                FocusedPetId.Value = null;
            }

            // Close bottom menu when changing pages
            if (previousPage != page)
            {
                IsBottomMenuExpanded.Value = false;
            }

            CurrentPage.Value = page;

            Log($"Page: {previousPage} → {page}");
        }

        public void NavigateToPetCloseup()
        {
            var pets = PetOrder.CurrentValue;
            // If there was a previously focused pet, go to that one
            // Otherwise go to the first pet
            var lastFocused = FocusedPetId.Value;
            if (!string.IsNullOrEmpty(lastFocused) && Array.IndexOf(pets, lastFocused) >= 0)
            {
                FocusPet(lastFocused);
            }
            else
            {
                FocusPet(pets[0]);
            }
        }
        
        /// <summary>
        /// Focus on a specific pet (navigates to PetCloseup)
        /// </summary>
        public void FocusPet(string petId)
        {
            var previousPetId = FocusedPetId.Value;
            FocusedPetId.Value = petId;
            CurrentPage.Value = UIPage.PetCloseup;

            Log($"FocusPet: {previousPetId ?? "none"} → {petId} (now on PetCloseup)");
        }

        /// <summary>
        /// Toggle the bottom menu expanded state
        /// </summary>
        public void ToggleBottomMenu()
        {
            var newState = !IsBottomMenuExpanded.Value;
            IsBottomMenuExpanded.Value = newState;

            Log($"BottomMenu: toggled → {(newState ? "OPEN" : "CLOSED")}");
        }

        /// <summary>
        /// Set bottom menu expanded state
        /// </summary>
        public void SetBottomMenuExpanded(bool expanded)
        {
            if (IsBottomMenuExpanded.Value != expanded)
            {
                IsBottomMenuExpanded.Value = expanded;
                Log($"BottomMenu: → {(expanded ? "OPEN" : "CLOSED")}");
            }
        }

        /// <summary>
        /// Navigate to next pet in order (for swipe right on PetCloseup)
        /// </summary>
        public void NavigateToNextPet()
        {
            var pets = PetOrder.Value;
            if (pets == null || pets.Length == 0)
            {
                Log("NavigateToNextPet: No pets in order!");
                return;
            }

            var currentId = FocusedPetId.Value;
            var currentIndex = System.Array.IndexOf(pets, currentId);

            if (currentIndex < 0)
            {
                // Not found, go to first pet
                Log($"NavigateToNextPet: Current pet not in order, going to first pet");
                FocusPet(pets[0]);
            }
            else if (currentIndex < pets.Length - 1)
            {
                // Go to next pet
                Log($"NavigateToNextPet: {currentIndex + 1}/{pets.Length} → {currentIndex + 2}/{pets.Length}");
                FocusPet(pets[currentIndex + 1]);
            }
            else
            {
                Log($"NavigateToNextPet: Already at last pet ({currentIndex + 1}/{pets.Length})");
            }
        }

        /// <summary>
        /// Navigate to previous pet in order (optional, for symmetry)
        /// Note: Per design doc, swipe left always goes back to SceneOverview
        /// </summary>
        public void NavigateToPreviousPet()
        {
            var pets = PetOrder.Value;
            if (pets == null || pets.Length == 0)
            {
                Log("NavigateToPreviousPet: No pets in order!");
                return;
            }

            var currentId = FocusedPetId.Value;
            var currentIndex = System.Array.IndexOf(pets, currentId);

            if (currentIndex > 0)
            {
                Log($"NavigateToPreviousPet: {currentIndex + 1}/{pets.Length} → {currentIndex}/{pets.Length}");
                FocusPet(pets[currentIndex - 1]);
            }
            else
            {
                Log($"NavigateToPreviousPet: Already at first pet");
            }
        }
        
        

        public void SetPetOrder(string[] petIds)
        {
            PetOrder.Value = petIds ?? System.Array.Empty<string>();
            Log($"PetOrder updated: {PetOrder.Value.Length} pets");
        }

        private void Log(string message)
        {
            if (enableDebugLogs) 
                Debug.Log($"[UIManager] {message}");
        }
    }
}
