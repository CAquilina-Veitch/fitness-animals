using R3;
using StepPet.Core;
using StepPet.Input;
using UnityEngine;

namespace StepPet
{
    /// <summary>
    /// Handles swipe navigation between pages according to the design:
    ///
    /// [Friends List] <--> [Scene Overview] <--> [Animal Close-ups]
    ///
    /// From Friends List:
    ///   - Swipe RIGHT → Scene Overview
    ///
    /// From Scene Overview:
    ///   - Swipe LEFT → Friends List
    ///   - Swipe RIGHT → Pet Closeup (first or last viewed)
    ///
    /// From Pet Closeup:
    ///   - Swipe LEFT → Always returns to Scene Overview
    ///   - Swipe RIGHT → Next pet
    /// </summary>
    public class UISwipeController : MonoBehaviour
    {
        [SerializeField] private SwipeController swipeController;

        private UIManager uiManager;

        private void Start()
        {
            uiManager = UIManager.Instance;

            swipeController.OnSwipeLeft.Subscribe(_ => OnSwipeLeft()).AddTo(this);
            swipeController.OnSwipeRight.Subscribe(_ => OnSwipeRight()).AddTo(this);
            swipeController.OnSwipeUp.Subscribe(_ => OnSwipeUp()).AddTo(this);
            swipeController.OnSwipeDown.Subscribe(_ => OnSwipeDown()).AddTo(this);
        }

        private void OnSwipeRight()
        {
            switch (uiManager.CurrentPage.Value)
            {
                case UIPage.SceneOverview:
                    // Scene Overview: swipe left goes to Friends List
                    uiManager.SetPage(UIPage.FriendsList);
                    break;

                case UIPage.PetCloseup:
                    // Pet Closeup: swipe left ALWAYS returns to Scene Overview
                    uiManager.SetPage(UIPage.SceneOverview);
                    break;

                case UIPage.FriendsList:
                    // Friends List: swipe left does nothing (already leftmost)
                    break;
            }
        }

        private void OnSwipeLeft()
        {
            switch (uiManager.CurrentPage.Value)
            {
                case UIPage.FriendsList:
                    // Friends List: swipe right goes to Scene Overview
                    uiManager.SetPage(UIPage.SceneOverview);
                    break;

                case UIPage.SceneOverview:
                    // Scene Overview: swipe right goes to Pet Closeup
                    uiManager.NavigateToPetCloseup();
                    break;

                case UIPage.PetCloseup:
                    // Pet Closeup: swipe right goes to next pet
                    uiManager.NavigateToNextPet();
                    break;
            }
        }

        private void OnSwipeUp() => uiManager.SetBottomMenuExpanded(true);

        private void OnSwipeDown() => uiManager.SetBottomMenuExpanded(false);
    }
}
