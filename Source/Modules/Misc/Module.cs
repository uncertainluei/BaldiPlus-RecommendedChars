using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusStudioLevelLoader;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelLoader;
using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;
using UnityEngine.AI;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Miscellaneous"), CaudexModulePriority(10)]
    public sealed class Module_Misc : RecCharsModule
    {
        internal override byte IconId => 19;

        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddTexturesToAssetMan("CakeTex/", ["Textures", "Environment", "Structure", "Cake"]);
            ObjectCreation.AddTexturesToAssetMan("PartyElevateTex/", ["Textures", "Environment", "Structure", "PartyElevator"]);
        }

        private WeightedRoomAsset[] newCafeterias;

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            // bee poster
            ObjectCreation.CreatePoster(AssetLoader.TextureFromMod(BasePlugin, "Textures", "Environment", "Poster", "bee.png"), "bee");

            // BBCR Party Style Objects
            _baseMat = AssetMan.Get<Material>("Mat/TileBase");
            _baseShader = AssetFinder.FindOfTypeWithName<Shader>("Shader Graphs/Standard", true);

            LoadCakeObj(
                CreateMaterial("CakeTex/CakeSide"),
                CreateMaterial("CakeTex/CakeTop"),
                CreateMaterial("CakeTex/Candle")
            );
            LoadPartyElevatorObj(
                CreateMaterial(AssetFinder.FindOfTypeWithName<Texture2D>("DiamongPlateFloor", true), "DiamondPlateFloor"),
                CreateMaterial("PartyElevateTex/PantographSide"),
                CreateMaterial("PartyElevateTex/PantographFront"),
                CreateMaterial("PartyElevateTex/MetalFence")
            );
            LoadArtPaintings();

            Balloon[] balloons = ((PartyEvent)RandomEventMetaStorage.Instance.Get(RandomEventType.Party).value).balloon;

            // Party Cafeteria
            RoomAsset cafeteria = Resources.FindObjectsOfTypeAll<RoomAsset>().First(x => x.GetInstanceID() >= 0 && x.roomFunctionContainer != null && x.roomFunctionContainer.name.StartsWith("Cafeteria"));
            CaudexRoomBlueprint cafeBlueprint = new(Plugin, "PartyCafeteria", cafeteria);
            ObjMan.Add("Room/CafeParty", cafeBlueprint);
            cafeBlueprint.functionContainer = GameObject.Instantiate(cafeBlueprint.functionContainer, MTM101BaldiDevAPI.prefabTransform);
            cafeBlueprint.functionContainer.name = "CafeteriaPartyRoomFunction";
            BalloonRoomFunction balloonFunction = cafeBlueprint.functionContainer.AddFunction<BalloonRoomFunction>();
            balloonFunction.balloonCount = 10;
            balloonFunction.balloonPres = balloons;
            LevelLoaderCompatHelper.AddRoom(cafeBlueprint);

            newCafeterias = ObjectCreation.RoomAssetsFromDirectory(cafeBlueprint, Path.Combine("Cafeteria", "Party"));

            LevelLoaderCompatHelper.AddRoom(cafeBlueprint, "recchars_partycafeterianonanas");
            RoomFunctionContainer noNanasFunction = GameObject.Instantiate(cafeBlueprint.functionContainer, MTM101BaldiDevAPI.prefabTransform);
            noNanasFunction.name = "CafeteriaPartyRoomFunction_NoNanas";
            noNanasFunction.RemoveFunction<NanaPeelRoomFunction>();
            LevelLoaderPlugin.Instance.roomSettings["recchars_partycafeterianonanas"].container = noNanasFunction;
        }

        private Material _baseMat;
        private Shader _baseShader;
        private Material CreateMaterial(Texture2D tex, string name)
        {
            Material newMat = new(_baseMat) { name = name };
            newMat.shader = _baseShader;
            newMat.SetMainTexture(tex);
            return newMat;
        }
        private Material CreateMaterial(string path)
            => CreateMaterial(AssetMan.Get<Texture2D>(path), Path.GetFileNameWithoutExtension(path));

        private void LoadCakeObj(params Material[] mats)
        {
            // Cake model
            Dictionary<string, Material> materials = new()
            {
                { "ClassicCakeSide", mats[0]},
                { "ClassicCakeTop", mats[1] },
                { "ClassicCakeCandle", mats[2] }
            };
            GameObject cakeObject = AssetLoader.ModelFromModManualMaterials(BasePlugin, materials, "Meshes", "ClassicPartyCake.obj");
            cakeObject.ConvertToPrefab(true);
            CapsuleCollider cakeCollider = cakeObject.AddComponent<CapsuleCollider>();
            cakeCollider.radius = 18f;
            cakeCollider.height = 50;
            NavMeshObstacle obstacle = cakeObject.AddComponent<NavMeshObstacle>();
            obstacle.carveOnlyStationary = true;
            obstacle.carving = true;
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.radius = 18f;
            obstacle.height = 25f;
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_cake", cakeObject);

            // Candle flame
            Sprite candleSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CakeTex/CandleFlame"), 10f);
            GameObject candleObject = ObjectCreation.CreateSpriteBillboard(candleSprite, default, MTM101BaldiDevAPI.prefabTransform, "Flame").gameObject;

            cakeObject = GameObject.Instantiate(cakeObject, MTM101BaldiDevAPI.prefabTransform);
            cakeObject.name = "ClassicPartyCake_WithFlame";
            candleObject = GameObject.Instantiate(candleObject, cakeObject.transform);
            candleObject.transform.position = Vector3.up * 33f;
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_cakewcandle", cakeObject);
        }

        private void LoadPartyElevatorObj(params Material[] mats)
        {
            Dictionary<string, Material> materials = new()
            {
                { "DiamondPlateFloor", mats[0]},
                { "PantographSide", mats[1] },
                { "PantographFront", mats[2] },
                { "MetalFence", mats[3] }
            };
            GameObject elevatorObject = AssetLoader.ModelFromModManualMaterials(BasePlugin, materials, "Meshes", "PartyElevator.obj");
            elevatorObject.ConvertToPrefab(true);
            BoxCollider elevatorCollider = elevatorObject.AddComponent<BoxCollider>();
            elevatorCollider.center = Vector3.up * 15f;
            elevatorCollider.size = new(10f,30f,10f);
            NavMeshObstacle obstacle = elevatorObject.AddComponent<NavMeshObstacle>();
            obstacle.carveOnlyStationary = true;
            obstacle.carving = true;
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = Vector3.up * 15f;
            obstacle.size = new(10f,30f,10f);
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_partyelevator", elevatorObject);
        }

        private void LoadArtPaintings()
        {
            Sprite[] paintingSprites = AssetLoader.SpritesFromSpritesheet(
                2, 4, 70f, Vector2.one*0.5f,
                AssetLoader.TextureFromMod(BasePlugin,"Textures","Environment","Room","Classroom","ArtPaintings.png")
            );

            Notebook notebookObj = GameObject.Instantiate(AssetFinder.FindAllOfType<Notebook>(true).First(), MTM101BaldiDevAPI.prefabTransform);
            Painting paintingObj = notebookObj.gameObject.AddComponent<Painting>();
            paintingObj.name = "Painting_0";
            paintingObj.sprite = notebookObj.sprite;
            GameObject.DestroyImmediate(notebookObj);
            paintingObj.sprite.sprite = paintingSprites[0];
            paintingObj.audShatter = AssetMan.Get<SoundObject>("Sfx/FakeShatter");
            paintingObj.particlePre = ObjectCreation.CreateSpriteBillboard(null, default, MTM101BaldiDevAPI.prefabTransform, "PaintingParticle")
                .gameObject.AddComponent<PaintingParticle>();
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_painting0", paintingObj.gameObject);

            for (int i = 1; i < paintingSprites.Length; i++)
            {
                paintingObj = GameObject.Instantiate(paintingObj, MTM101BaldiDevAPI.prefabTransform);
                paintingObj.name = "Painting_"+i;
                paintingObj.sprite.sprite = paintingSprites[i];
                LevelLoaderPlugin.Instance.basicObjects.Add("recchars_painting"+i, paintingObj.gameObject);
            }
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Misc", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Misc", GenerationStageFlags.Addend);
            lvl.MarkAsNeverUnload();

            // Spawn the BEE in literally any floor
            lvl.posters = lvl.posters.AddToArray(ObjMan.Get<PosterObject>("Pst/bee").Weighted(1));
        }

        [CaudexGenModEvent(GenerationModType.Finalizer)]
        private void FloorFinalizerLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Misc", GenerationStageFlags.Finalizer))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Misc", GenerationStageFlags.Finalizer);

            if (!RecommendedCharsPlugin.PartyMode) return;

            // Replace cafeterias with cake variants
            List<WeightedRoomAsset> specialRooms = lvl.potentialSpecialRooms.ToList();

            int totalWeight = 0;
            for (int i = 0; i < specialRooms.Count; i++)
            {
                if (!specialRooms[i].selection.roomFunctionContainer ||
                    !specialRooms[i].selection.roomFunctionContainer.name.StartsWith("Cafeteria"))
                    continue;

                totalWeight += specialRooms[i].weight;
                specialRooms.RemoveAt(i);
                i--;
            }
            if (totalWeight == 0)
                return;

            int weightPerCafe = totalWeight/newCafeterias.Length;
            foreach (WeightedRoomAsset cafeteria in newCafeterias)
                specialRooms.Add(cafeteria.selection.Weighted(weightPerCafe)); // Add the party cafeterias
                
            lvl.potentialSpecialRooms = specialRooms.ToArray();
        }
    }
}
