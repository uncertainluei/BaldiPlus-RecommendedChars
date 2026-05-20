using BepInEx.Configuration;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    static class RecommendedCharsConfig
    {
        internal static ConfigEntry<bool> onlyOneNpcActivity;
        internal static ConfigEntry<bool> nerfCircle, nerfMrDaycare, intendedWires; // intendedGifter
        //internal static ConfigEntry<bool> intendedGifter;

        internal static ConfigEntry<PartyModeConfigMode> partyMode;
        internal static bool guaranteeSpawnChar;
        internal static ConfigEntry<bool> guaranteeSpawnConf, legacyTextures;


        internal enum PartyModeConfigMode : byte
        {
            DateBased,
            ForceOff,
            ForceOn
        }

        internal static void BindConfig(ConfigFile config)
        {
            onlyOneNpcActivity = config.Bind(
                "Behaviors",
                "OnlyOneNpcActivity",
                true,
                "Enforces that only one in-your-face NPC 'activity' (i.e. Playtime/Circle's jumprope game and AwW's grabbing sequence) can be active at a time.");

            nerfCircle = config.Bind(
                "Behaviors.Nerfs",
                "Circle",
                true,
                "Nerfs Circle's speed and max jump count, increases his cooldowns, and makes the Nerf Gun cheaper and more common.");
            nerfMrDaycare = config.Bind(
                "Behaviors.Nerfs",
                "MrDaycare",
                true,
                "Nerfs Mr. Daycare's movement speed, guilt sensitivity and timeout times to be less frustrating.");
            intendedWires = config.Bind(
                "Behaviors",
                "IntendedWiresBehavior",
                true,
                "Arts with Wires will not reset their stare time when the player stops looking at them.");

            // This currently does NOTHING as Gifter's 'less generic' AI hasn't been implemented
            /*intendedGifter = config.Bind(
                "Behaviors",
                "IntendedGifter",
                true,
                "LOLdi's Gifter will behave almost exactly like his iteration from LOLdi's Public Alpha (Gifttany from BBRMS).");
            */

            partyMode = config.Bind(
                "Misc",
                "PartyMode",
                PartyModeConfigMode.DateBased,
                "Sets how Party Mode will trigger. If \"DateBased\", then it will turn on only in May 1st-22nd.");
            guaranteeSpawnConf = config.Bind(
                "Misc",
                "GuaranteeCharacterSpawn",
                false,
                "All added NPCs will be guaranteed to spawn in their designated floor.");
            guaranteeSpawnChar = guaranteeSpawnConf.Value;
            legacyTextures = config.Bind(
                "Misc",
                "LegacyTextures",
                false,
                "Reverts texture changes made throughout the mod's development to reflect those sent by the requesters.");
        }
    }
}
