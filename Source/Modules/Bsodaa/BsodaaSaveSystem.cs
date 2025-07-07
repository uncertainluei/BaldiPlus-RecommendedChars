using System.IO;
using HarmonyLib;
using MTM101BaldAPI;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class ModuleSaveSystem_Bsodaa : ModuleSaveSystem
    {
        internal ModuleSaveSystem_Bsodaa()
        {
            Instance = this;
        }

        internal static ModuleSaveSystem_Bsodaa Instance { get; private set; }

        private bool savHelperDietMode = false;

        public bool helperExhausted = false;
        public bool helperDietMode;

        public override void CoreGameManCreated(CoreGameManager cgm, bool savedGame)
        {
            helperExhausted = false;
            helperDietMode = false;

            if (!savedGame) return;
            helperDietMode = savHelperDietMode;
        }

        public override void Load(BinaryReader reader)
        {
            savHelperDietMode = reader.ReadBoolean();

            helperDietMode = savHelperDietMode;
        }

        public override void Reset()
        {
            savHelperDietMode = false;

            helperDietMode = false;
        }

        public override void Save(BinaryWriter writer)
        {
            savHelperDietMode = helperDietMode;

            writer.Write(savHelperDietMode);
        }
    }
}