using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StepPet.Core;
using R3;

namespace StepPet.Debugging
{
    /// <summary>
    /// Debug panel for testing navigation during development.
    /// Shows current state and provides buttons to test navigation.
    /// Remove or disable in production builds.
    /// </summary>
    public class DebugNavigationPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI currentPageText;
        [SerializeField] private TextMeshProUGUI focusedPetText;
        [SerializeField] private TextMeshProUGUI bottomMenuText;
        [SerializeField] private TextMeshProUGUI cameraZoomText;

        [Header("Buttons")]
        [SerializeField] private Button friendsListButton;
        [SerializeField] private Button sceneOverviewButton;
        [SerializeField] private Button petCloseupButton;
        [SerializeField] private Button toggleMenuButton;
        [SerializeField] private Button nextPetButton;

        private CompositeDisposable _disposables;

        private void Awake()
        {
            _disposables = new CompositeDisposable();
        }

        private void Start()
        {
            SetupButtonListeners();
            SubscribeToManagers();
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void SetupButtonListeners()
        {
            if (friendsListButton != null)
                friendsListButton.onClick.AddListener(() => UIManager.Instance?.SetPage(UIPage.FriendsList));

            if (sceneOverviewButton != null)
                sceneOverviewButton.onClick.AddListener(() => UIManager.Instance?.SetPage(UIPage.SceneOverview));

            if (petCloseupButton != null)
                petCloseupButton.onClick.AddListener(GoToPetCloseup);

            if (toggleMenuButton != null)
                toggleMenuButton.onClick.AddListener(() => UIManager.Instance?.ToggleBottomMenu());

            if (nextPetButton != null)
                nextPetButton.onClick.AddListener(() => UIManager.Instance?.NavigateToNextPet());
        }

        private void SubscribeToManagers()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CurrentPage
                    .Subscribe(_ => UpdateDisplay())
                    .AddTo(_disposables);

                UIManager.Instance.FocusedPetId
                    .Subscribe(_ => UpdateDisplay())
                    .AddTo(_disposables);

                UIManager.Instance.IsBottomMenuExpanded
                    .Subscribe(_ => UpdateDisplay())
                    .AddTo(_disposables);
            }

            if (Camera.CameraManager.Instance != null)
            {
                Camera.CameraManager.Instance.TargetZoom
                    .Subscribe(_ => UpdateDisplay())
                    .AddTo(_disposables);
            }
        }

        private void UpdateDisplay()
        {
            if (UIManager.Instance != null)
            {
                if (currentPageText != null)
                    currentPageText.text = $"Page: {UIManager.Instance.CurrentPage.Value}";

                if (focusedPetText != null)
                    focusedPetText.text = $"Pet: {UIManager.Instance.FocusedPetId.Value ?? "None"}";

                if (bottomMenuText != null)
                    bottomMenuText.text = $"Menu: {(UIManager.Instance.IsBottomMenuExpanded.Value ? "Open" : "Closed")}";
            }

            if (Camera.CameraManager.Instance != null)
            {
                if (cameraZoomText != null)
                    cameraZoomText.text = $"Zoom: {Camera.CameraManager.Instance.TargetZoom.Value:F1}";
            }
        }

        private void GoToPetCloseup()
        {
            var uiManager = UIManager.Instance;
            if (uiManager == null) return;

            var pets = uiManager.PetOrder.Value;
            if (pets != null && pets.Length > 0)
            {
                uiManager.FocusPet(pets[0]);
            }
            else
            {
                Debug.LogWarning("[DebugPanel] No pets available to focus on");
            }
        }
    }
}
