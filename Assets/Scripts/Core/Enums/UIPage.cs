namespace StepPet.Core
{
    /// <summary>
    /// Represents the current navigation page in the app.
    /// </summary>
    public enum UIPage
    {
        /// <summary>
        /// Friends list panel (Canvas overlay, slides from left)
        /// </summary>
        FriendsList,

        /// <summary>
        /// Zoomed out view of entire scene with all pets (World space)
        /// </summary>
        SceneOverview,

        /// <summary>
        /// Zoomed in view of a single pet (World space)
        /// </summary>
        PetCloseup
    }
}
