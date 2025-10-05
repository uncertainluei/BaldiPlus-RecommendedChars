using BepInEx.Configuration;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    static class RecommendedCharsConfig
    {
        internal static ConfigEntry<bool> nerfCircle;
        internal static ConfigEntry<bool> nerfMrDaycare;
        internal static ConfigEntry<bool> intendedWiresBehavior;
        internal static ConfigEntry<bool> intendedGifter;

        internal static ConfigEntry<bool> guaranteeSpawnChar;


        internal static void BindConfig(ConfigFile config)
        {
            nerfCircle = config.Bind(
                "Behaviors",
                "Circle",
                true,
                "Nerfs Circle's speed and max jump count, increases his cooldowns, and makes the Nerf Gun cheaper and more common.");
            nerfMrDaycare = config.Bind(
                "Behaviors",
                "MrDaycare",
                true,
                "Nerfs Mr. Daycare's movement speed, guilt sensitivity and timeout times to be overall more manageable.");
            intendedWiresBehavior = config.Bind(
                "Behaviors",
                "IntendedWiresBehavior",
                true,
                "Arts with Wires will not reset their stare time when the player stops looking at them.");
            intendedGifter = config.Bind(
                "Behaviors",
                "IntendedGifter",
                true,
                "LOLdi's Gifter will behave almost exactly like his iteration from LOLdi's Public Alpha (Gifttany from BBRMS).");

            guaranteeSpawnChar = config.Bind(
                "Misc",
                "GuaranteeCharacterSpawn",
                false,
                "All added NPCs will be guaranteed to spawn in their designated floor.");
        }
    }
}
