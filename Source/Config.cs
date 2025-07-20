using BepInEx.Configuration;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    static class RecommendedCharsConfig
    {
        internal static ConfigEntry<bool> moduleCircle;
        internal static ConfigEntry<bool> moduleGottaBully;
        internal static ConfigEntry<bool> moduleArtsWWires;
        internal static ConfigEntry<bool> moduleCaAprilFools;
        internal static ConfigEntry<bool> moduleMrDaycare;
        internal static ConfigEntry<bool> moduleBsodaa;

        internal static ConfigEntry<bool> nerfCircle;
        internal static ConfigEntry<bool> nerfMrDaycare;
        internal static ConfigEntry<bool> intendedWiresBehavior;

        internal static ConfigEntry<bool> guaranteeSpawnChar;


        internal static void BindConfig(ConfigFile config)
        {
            moduleCircle = config.Bind(
                "Modules",
                "Circle",
                true,
                "Adds Circle and Nerf Gun from TCMG's Basics.");
            moduleGottaBully = config.Bind(
                "Modules",
                "GottaBully",
                true,
                "Adds Gotta Bully from Playtime's Swapped Basics.");
            moduleArtsWWires = config.Bind(
                "Modules",
                "ArtsWWires",
                true,
                "Adds Arts with Wires, based off the Arts and Crafters equivalent in Kinzo/Kracc's 1st Prize Mania and Playtime's Swapped Basics respectively.");
            moduleCaAprilFools = config.Bind(
                "Modules",
                "CaAprilFools",
                true,
                "Adds a few features based on the April Fools updates from the Chaos Awakens Minecraft mod.");
            moduleMrDaycare = config.Bind(
                "Modules",
                "MrDaycare",
                true,
                "Adds Mr. Daycare from Dave's House, as well the Pie and Door Key items.");
            moduleBsodaa = config.Bind(
                "Modules",
                "Bsodaa",
                true,
                "Adds Baldi and Playtime from Eveything is Bsodaa, with their own room and mechanic.");

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
