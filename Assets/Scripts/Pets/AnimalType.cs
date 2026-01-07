namespace StepPet.Pets
{
    /// <summary>
    /// All animal types available in the game.
    /// Used to identify ScriptableObject definitions.
    /// </summary>
    public enum AnimalType
    {
        // Starter pets (chosen at onboarding)
        Puppy,
        Kitten,
        Bunny,

        // Unlock challenge pets
        Duckling,
        BabyGiraffe,
        BabyElephant,
        BabyPenguin,
        BabyPanda
    }
}