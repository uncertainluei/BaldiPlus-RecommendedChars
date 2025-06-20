﻿using BepInEx.Configuration;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    static class RecommendedCharsConfig
    {
        internal static ConfigEntry<bool> moduleCircle;
        internal static ConfigEntry<bool> moduleGottaBully;
        internal static ConfigEntry<bool> moduleArtsWWires;
        internal static ConfigEntry<bool> moduleCaAprilFools;
        internal static ConfigEntry<bool> moduleMrDaycare;

        internal static ConfigEntry<bool> moduleExp;

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
                "Adds Arts with Wires, character sprites and audio from Kinzo/Kracc's 1st Prize Mania and Playtime's Swapped Basics respectively.");
            moduleCaAprilFools = config.Bind(
                "Modules",
                "CaAprilFools",
                true,
                "Adds a few features from the April Fools updates from the Chaos Awakens Minecraft mod.");
            moduleMrDaycare = config.Bind(
                "Modules",
                "MrDaycare",
                true,
                "Adds Mr. Daycare and the Pie item from Dave's House.");

            moduleExp = config.Bind(
                "Modules",
                "Experimental",
                false,
                "Experimental modules. TBA");

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
