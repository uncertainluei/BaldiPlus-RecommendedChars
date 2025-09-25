using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusStudioLevelLoader;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Locks and Bolts"), CaudexModuleSaveTag("Mdl_LockSock")]
    [CaudexModuleConfig("Modules", "LockSock",
        "Adds a padlock sockpuppet.", true)]
    public sealed class Module_LockSock : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Npc", "LockSock"), x => "LSockTex/" + x.name);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "LockSock.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadLockSock()
        {
            LockSock lockSock = new NPCBuilder<LockSock>(Plugin)
                .SetName("LockSock")
                .SetEnum("RecChars_LockSock")
                .SetPoster(AssetMan.Get<Texture2D>("LSockTex/pri_locksock"), "PST_PRI_RecChars_LockSock1", "PST_PRI_RecChars_LockSock2")
                .AddMetaFlag(NPCFlags.Standard & ~NPCFlags.CanSee)
                .SetMetaTags(["adv_exclusion_hammer_weakness"])
                .AddTrigger()
                .Build();

            PineDebugNpcIconPatch.icons.Add(lockSock.character, AssetMan.Get<Texture2D>("LSockTex/BorderLockSock"));

            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 50f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("LSockTex/LockSockSprites"));

            lockSock.navigator.speed = 22;
            lockSock.navigator.maxSpeed = 22;

            lockSock.sprite = lockSock.spriteRenderer[0];
            lockSock.sprite.transform.localPosition = Vector3.zero;

            lockSock.sprite.sprite = sprites[0];
            lockSock.sprNormal = sprites[0];
            lockSock.sprLocking = sprites[1];

            lockSock.audMan = lockSock.GetComponent<AudioManager>();
            lockSock.audMan.subtitleColor = new(84/255f, 75/255f, 65/255f);

            lockSock.audLock = GameObject.Instantiate(((ITM_Acceptable)ItemMetaStorage.Instance.FindByEnum(Items.DoorLock).value.item).audUse);
            lockSock.audLock.subtitle = true;
            lockSock.audLock.soundKey = "Sfx_RecChars_LockSock_Lock";
            lockSock.audLock.name = "SwingDoorLock_WithSubtitle";

            CharacterRadarColorPatch.colors.Add(lockSock.character, lockSock.audMan.subtitleColor);

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_locksock", lockSock);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_locksock", lockSock.Poster);
            ObjMan.Add("Npc_LockSock", lockSock);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(scene, 75, true);
                return;
            }
            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(scene, id < 3 ? 75 : 125);
            }
        }

        private void AddToNpcs(SceneObject scene, int weight, bool endless = false)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                scene.potentialNPCs.Add(ObjMan.Get<LockSock>("Npc_LockSock").Weighted(weight));
            else if (endless || scene.levelNo == 1)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<LockSock>("Npc_LockSock"));
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            }
        }
    }
}
