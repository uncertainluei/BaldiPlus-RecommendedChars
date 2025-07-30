using BepInEx.Configuration;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    static class RecommendedCharsConfig
    {
        internal static ConfigEntry<bool> nerfCircle;
        internal static ConfigEntry<bool> nerfMrDaycare;
        internal static ConfigEntry<bool> intendedWiresBehavior;

        internal static ConfigEntry<bool> guaranteeSpawnChar;


        internal static void BindConfig(ConfigFile config)
        {
            nerfCircle = config.Bind(
                "Nerfs",
                "Circle",
                true,
                "Nerfs Circle's speed and max jump count, increases his cooldowns, and makes the Nerf Gun cheaper and more common.");
            nerfMrDaycare = config.Bind(
                "Nerfs",
                "MrDaycare",
                true,
                "Nerfs Mr. Daycare's movement speed, guilt sensitivity and timeout times to be overall more manageable.");

            intendedWiresBehavior = config.Bind(
                "Misc",
                "IntendedWiresBehavior",
                true,
                "Arts with Wires will not reset their stare time when the player stops looking at it.");
            guaranteeSpawnChar = config.Bind(
                "Misc",
                "GuaranteeCharacterSpawn",
                false,
                "All added NPCs will be guaranteed to spawn in their designated floor.");
        }
    }
}
