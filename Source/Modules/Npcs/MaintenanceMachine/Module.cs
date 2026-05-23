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
using System.Linq;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Evil Maintenance Machine"), CaudexModuleSaveTag("Mdl_MaintenanceMachine")]
    [CaudexModuleConfig("Modules", "MaintenanceMachine",
        "It's an evil maintenance machine that's powerful... and evil.", true)]
    public sealed class Module_MaintenanceMachine : RecCharsModule
    {
        internal override byte IconId => 10;

        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddTexturesToAssetMan("MMachineTex/", ["Textures", "Npc", "MaintenanceMachine"]);
            ObjectCreation.AddAudioToAssetMan("MMachineAud/", ["Audio", "Npc", "MaintenanceMachine"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "MaintenanceMachine.json5");

            // Add effect icons
            AssetMan.Add("StatusSpr/SpikeSlowdown", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("MMachineTex/SpikeSlowdownIcon"), 1));
            AssetMan.Add("StatusSpr/PyramidFlip", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("MMachineTex/PyramidFlipIcon"), 1));

            // Load patches
            Hooks.PatchAll(typeof(MaintenanceMachinePatches));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadMaintenanceMachine()
        {
            MaintenanceMachine machine = new NPCBuilder<MaintenanceMachine>(Plugin)
                .SetName("EvilMaintenanceMachine")
                .SetEnum("RecChars_MaintMachine")
                .SetPoster(AssetMan.Get<Texture2D>("MMachineTex/pri_emm"), "PST_PRI_RecChars_MaintMachine1", "PST_PRI_RecChars_MaintMachine2")
                .AddMetaFlag(NPCFlags.Standard & ~NPCFlags.CanSee)
                .SetMetaTags(["adv_exclusion_hammer_weakness"])
                .AddTrigger()
                .Build();

            machine.poster.textData[0].position = new(16, 48);
            machine.poster.textData[0].size = new(224, 32);
            PineDebugNpcIcons.AddIcon([machine], "BorderMaintenanceMachine.png");

            machine.navigator.SetSpeed(machine.normalSpeed);
            machine.navigator.accel = 15f;

            machine.spriteRenderer[0].sprite = AssetLoader.SpriteFromTexture2D(
                AssetMan.Get<Texture2D>(RecommendedCharsPlugin.Plugin ? "MMachineTex/EvilMaintenanceMachine_Party" : "MMachineTex/EvilMaintenanceMachine"), 30f);
            machine.spriteRenderer[0].transform.localPosition = Vector3.down;

            machine.audMan = machine.GetComponent<AudioManager>();
            ((PropagatedAudioManager)machine.audMan).maxDistance = 300;
            machine.audMan.subtitleColor = new(194/255f, 48/255f, 49/255f);
            machine.audClean = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("MMachineAud/EMM_Cleaning"), "Vfx_RecChars_MaintMachine_Cleaning", SoundType.Voice, machine.audMan.subtitleColor);
            machine.audOhno = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("MMachineAud/EMM_OhNo"), "Vfx_RecChars_MaintMachine_OhNo", SoundType.Voice, machine.audMan.subtitleColor);

            LoadDropEntities(machine);

            CharacterRadarColorPatch.colors.Add(machine.character, machine.audMan.subtitleColor);

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_maintmachine", machine);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_maintmachine", machine.Poster);
            ObjMan.Add("Npc/MaintMachine", machine);

            // Add spawn tag for the maintenance level type
            LevelType.Maintenance.GetMeta().tags.Add("recchars:spawns_maintenance_machine");
        }

        private int _collisionLayerMask;
        private void LoadDropEntities(MaintenanceMachine prefab)
        {
            Sprite[] shapeSprites = AssetLoader.SpritesFromSpritesheet(3, 3, 25f, new Vector2(0.5f, 0f), AssetMan.Get<Texture2D>("MMachineTex/PrimitiveShapes"));
            GameObject.DestroyImmediate(shapeSprites[7]);
            GameObject.DestroyImmediate(shapeSprites[8]);
            _collisionLayerMask = ((ITM_NanaPeel)ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value.item).entity.collisionLayerMask;

            // Spike
            PrimitiveDrop_Spike spike = CreateDropEntityBase<PrimitiveDrop_Spike>(shapeSprites[0]);
            spike.audTouch = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "SourPlink.wav"), "Sfx_RecChars_SourPlink", SoundType.Effect, Color.white, 0);
            prefab.entityPres.Add(spike);

            // Cube
            PrimitiveDrop_Cube cube = CreateDropEntityBase<PrimitiveDrop_Cube>(shapeSprites[1]);
            cube.audSquish = AssetFinder.FindAllOfType<Balder_Entity>(true).First().audSquish;
            prefab.entityPres.Add(cube);

            // Cylinder
            PrimitiveDrop_Cylinder cylinder = CreateDropEntityBase<PrimitiveDrop_Cylinder>(shapeSprites[2]);
            cylinder.audSlipLoop = AssetMan.Get<SoundObject>("Sfx/SlipLoop");
            cylinder.audSlipEnd = AssetMan.Get<SoundObject>("Sfx/Slip");
            prefab.entityPres.Add(cylinder);

            // Dodecahedron
            Sprite smallDodecahedron = Sprite.Create(shapeSprites[3].texture, shapeSprites[3].rect, new(0.5f, 0f), 40f, 0, SpriteMeshType.FullRect);
            smallDodecahedron.name = shapeSprites[3].name+"_Small";

            PrimitiveDrop_DodecahedronLarge dodecahedron = CreateDropEntityBase<PrimitiveDrop_DodecahedronLarge>(shapeSprites[3]);
            dodecahedron.smallPre = CreateDropEntityBase<PrimitiveDrop_Dodecahedron>(smallDodecahedron);
            dodecahedron.smallPre.coverCloud = GameObject.Instantiate(AssetFinder.FindOfTypeWithName<CoverCloud>("ChalkCloudStartsOff", true), MTM101BaldiDevAPI.prefabTransform);
            dodecahedron.smallPre.coverCloud.name = "ChalkCloudSmall";
            dodecahedron.smallPre.coverCloud.gameObject.SetActive(true);
            ((BoxCollider)dodecahedron.smallPre.coverCloud.trigger).size = new(2.5f, 10f, 2.5f);
            dodecahedron.smallPre.coverCloud.transform.GetChild(0).GetComponent<BoxCollider>().size = new(2.5f, 10f, 2.5f);
            ParticleSystem.MainModule particles = dodecahedron.smallPre.coverCloud.particles.main;
            particles.startSizeMultiplier = 4f;
            particles.startSizeXMultiplier = 4f;
            particles.maxParticles = 25;
            ParticleSystem.ShapeModule shape = dodecahedron.smallPre.coverCloud.particles.shape;
            shape.scale = new(2.5f, 8f, 2.5f);

            prefab.entityPres.Add(dodecahedron);

            // Rounded Cuboid
            PrimitiveDrop_RoundedCuboid rounded = CreateDropEntityBase<PrimitiveDrop_RoundedCuboid>(shapeSprites[4]);
            rounded.audSlip = AssetMan.Get<SoundObject>("Sfx/Slip");
            rounded.puddleSprite = GameObject.Instantiate(rounded.sprite, rounded.sprite.transform.parent);
            rounded.puddleSprite.name = "Puddle";
            rounded.puddleSprite.sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("MMachineTex/RoundedCuboidPuddle"), 16);
            rounded.puddleSprite.sharedMaterial = AssetMan.Get<Material>("Mat/SpriteNoBillboard");
            rounded.puddleSprite.transform.localEulerAngles = Vector3.right * -90f;
            rounded.puddleSprite.transform.localPosition = Vector3.up * 0.01f;
            rounded.entity.renderer = [rounded.sprite, rounded.puddleSprite];
            prefab.entityPres.Add(rounded);

            // Sphere
            PrimitiveDrop_Sphere sphere = CreateDropEntityBase<PrimitiveDrop_Sphere>(shapeSprites[5]);
            sphere.popPre = AssetFinder.FindOfTypeWithName<QuickExplosion>("QuickPop", true);
            prefab.entityPres.Add(sphere);

            // Pyramid
            PrimitiveDrop_Pyramid pyramid = CreateDropEntityBase<PrimitiveDrop_Pyramid>(shapeSprites[6]);
            prefab.entityPres.Add(pyramid);
        }

        private T CreateDropEntityBase<T>(Sprite sprite) where T : PrimitiveDrop
        {
            Entity entity = new EntityBuilder()
                .SetName(typeof(T).Name)
                .SetBaseRadius(2.5f)
                .SetLayerCollisionMask(_collisionLayerMask)
                .AddTrigger(2.5f)
                .AddDefaultRenderBaseFunction(sprite)
                .Build();
            T drop = entity.gameObject.AddComponent<T>();
            drop.entity = entity;
            drop.sprite = entity.rendererBase.GetComponentInChildren<SpriteRenderer>();
            PropagatedAudioManager audMan = entity.gameObject.AddComponent<PropagatedAudioManager>();
            audMan.maxDistance = 80f;
            drop.audMan = audMan;
            return drop;
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int id, CustomLevelObject lvl)
        {
            if (!title.StartsWith("F") || lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/MaintenanceMachine", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/MaintenanceMachine", GenerationStageFlags.Addend);

            if (lvl.type.GetMeta()?.tags.Contains("recchars:spawns_maintenance_machine") == true)
            {
                lvl.MarkAsNeverUnload();
                lvl.GetPotentialNpcsInclusive().Add(ObjMan.Get<MaintenanceMachine>("Npc/MaintMachine").Weighted(RecommendedCharsConfig.guaranteeSpawnChar ? 9000 : 200));
            }
        }
    }
}
