using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Components;
using UncertainLuei.CaudexLib.Util;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    // TBA
    public class CrawlspaceEvent : RandomEvent
    {
        public static CrawlspaceEvent Instance {get; private set;}

        // Prefabs
        public EnvironmentController ecPrefab; 
        public LevelLoader lvlLoaderPre;
        public LevelDataContainer lvlData;
        public Material[] roomMaterials;
        public MeshRenderer tilePrefab;


        private LevelLoader lvlLoader;
        public Texture2D transparentTexture, darkLightmap;

        public EnvironmentController CrawlspaceEc {get; private set;}
        private readonly Dictionary<RoomController, HoleMaterialPair> holeAtlases = [];
        private readonly List<Cell> openCells = [], potentialFallCells = [];

        private readonly struct HoleMaterialPair(Material normalMat, Material posterMat)
        {
            public Material NormalMat {get;} = normalMat;
            public Material PosterMat {get;} = posterMat;
        }

        public override void Initialize(EnvironmentController controller, System.Random rng)
        {
            base.Initialize(controller, rng);
            Instance = this;

            CrawlspaceEc = Instantiate(ecPrefab);
            CrawlspaceEc.transform.position -= Vector3.up * 20f;
            CrawlspaceEc.transform.localScale = new Vector3(1,1,1);
            CrawlspaceEc.standardDarkLevel = Color.white;
            CrawlspaceEc.height = -20;

            lvlLoader = Instantiate(lvlLoaderPre);
            lvlLoader.name = "CrawlspaceLevelLoader";

            ElevatorScreen elevate = FindObjectOfType<ElevatorScreen>();
            if (elevate)
                elevate.QueueEnumerator(WaitForLoader(elevate));
            else
                StartCoroutine(WaitForLoader(elevate));

            // Dummy scene object which is literally here to skirt around that dumb 0.12.2 change
            lvlLoader.scene = ScriptableObject.CreateInstance<SceneObject>();
            lvlLoader.scene.previousLevels = [];
            lvlLoader.scene.potentialNPCs = [];
            lvlLoader.scene.forcedNpcs = [];
            lvlLoader.ec = CrawlspaceEc;
            lvlLoader.levelContainer = lvlData;
            lvlData.levelSize = ec.levelSize;
            lvlData.tile = FillArea();
        }

        private bool _setup = false;
        public override void AfterUpdateSetup(System.Random rng)
            => _setup = true;
        public override void PremadeSetup()
            => _setup = true;

        private IEnumerator WaitForLoader(ElevatorScreen elevate)
        {
            yield return new WaitWhile(() => lvlData.tile == null);
            lvlLoader.StartGenerate();
            yield return new WaitWhile(() => !lvlLoader.levelInProgress);
            yield return new WaitWhile(() => lvlLoader.levelInProgress && !_setup);
            DestroyImmediate(lvlLoader.scene);

            CrawlspaceEc.name += "_Crawlspace";
            CrawlspaceEc.mainHall = CrawlspaceEc.rooms[0];
            
            try
            {
                // Re-initialize the EC's lightmap
                CrawlspaceEc.standardDarkLevel = Color.white;
                CrawlspaceEc.InitializeLighting();
                LightmapModHolder.GetInstance(ec).ForceUpdateLightmap();
                CoreGameManager.Instance.updateLightMap = false;

                // Setup areas
                SetupArea(CrawlspaceEc.rooms[0], true);

                if (ec.mainHall)
                    SetupArea(ec.mainHall);
                foreach (RoomController room in ec.rooms)
                {
                    if (room.type == RoomType.Hall)
                        SetupArea(room);
                }
            }
            catch (Exception e)
            {
                RecommendedCharsPlugin.Log.LogError(e);

                // Auto-trigger error
                lvlLoader.enabled = true;
                lvlLoader.levelInProgress = true;
                lvlLoader.framesSinceLastYield = 120;
                yield break;
            }

            
            Destroy(lvlLoader.gameObject);
            if (elevate)
                elevate.busy = false;
        }

        private void SetupArea(RoomController area, bool ceiling = false)
        {
            RecommendedCharsPlugin.Log.LogInfo($"Crawlspace: found area {area.name}");
            if (holeAtlases.ContainsKey(area)) return;

            Material baseMat = area.baseMat, pstMat = area.posterMat, holeMat = null;
            Texture2D florTex = area.florTex, ceilTex = area.ceilTex;

            area.baseMat = new Material(baseMat);
            area.posterMat = new Material(pstMat);
            area.ceilTex = transparentTexture;

            if (!ceiling)
            {
                area.florTex = transparentTexture;
                area.GenerateTextureAtlas();
                holeMat = area.baseMat;

                area.ceilTex = ceilTex;
                area.baseMat = new Material(baseMat);
            }
            area.GenerateTextureAtlas();

            holeAtlases.Add(area, new(area.baseMat, area.posterMat));

            area.baseMat = baseMat;
            area.posterMat = pstMat;
            area.florTex = florTex;
            area.ceilTex = ceilTex;

            if (ceiling) return;

            int type;
            foreach (var cell in area.cells)
            {
                if (cell.HardCoverageBin%16 == 15 || !IsCellEntitySafe(cell)) continue;

                type = cell.ConstBin | (cell.HardCoverageBin%16);
                if (!IsBitAvailable(type, 0) && (cell.position.z == ec.levelSize.z-1 || !IsCellConnected(cell, ec.CellFromPosition(cell.position.x,cell.position.z+1))))
                    type |= 1;
                if (!IsBitAvailable(type, 1) && (cell.position.x == ec.levelSize.x-1 || !IsCellConnected(cell, ec.CellFromPosition(cell.position.x+1,cell.position.z))))
                    type |= 2;
                if (!IsBitAvailable(type, 2) && (cell.position.z == 0 || !IsCellConnected(cell, ec.CellFromPosition(cell.position.x,cell.position.z-1))))
                    type |= 4;
                if (!IsBitAvailable(type, 3) && (cell.position.x == 0 || !IsCellConnected(cell, ec.CellFromPosition(cell.position.x-1,cell.position.z))))
                    type |= 8;

                potentialFallCells.Add(cell);

                if (type != 0) // Is not completely open (there is no point in displaying a completely invisible object)
                {
                    MeshRenderer tileVisual = Instantiate(tilePrefab, cell.Tile.transform);
                    tileVisual.GetComponent<MeshFilter>().sharedMesh = ec.TileMesh(type);
                    tileVisual.sharedMaterial = holeMat;
                    tileVisual.transform.localPosition = Vector3.up * -10f;
                }

                CrawlspaceEc.CellFromPosition(cell.position.x,cell.position.z).SetBase(holeAtlases[CrawlspaceEc.rooms[0]].NormalMat);
            }
        }

        private bool IsBitAvailable(int x, int bit) => (x >> bit) % 2 == 1;
        private bool IsCellConnected(Cell a, Cell b) => !b.Null && a.room == b.room && !b.offLimits && b.HardCoverageBin%16 != 15 && IsCellEntitySafe(b);
        public bool IsCellEntitySafe(Cell a) => a.room.entitySafeCells.Count > 0 || a.room.entitySafeCells.Contains(a.position);
        public bool IsCellOpen(Cell cell) => openCells.Count > 0 && openCells.Contains(cell);

        public void UpdateHallCell(int x, int y, bool open)
        {
            Cell cell = ec.CellFromPosition(x,y), crawlspaceCell = CrawlspaceEc.CellFromPosition(x,y);
            if (crawlspaceCell == null || crawlspaceCell.Null ||
                cell == null || cell.Null || !cell.room || !holeAtlases.ContainsKey(cell.room)) return;

            if (!open)
            {
                if (!openCells.Contains(cell)) return;
                openCells.Remove(cell);

                cell.SetBase(cell.Tile.MeshRenderer.sharedMaterial == holeAtlases[cell.room].NormalMat ? cell.room.baseMat : cell.room.posterMat);
                crawlspaceCell.SetBase(CrawlspaceEc.rooms[0].baseMat);
                return;
            }

            if (openCells.Contains(cell)) return;
            openCells.Add(cell);

            cell.SetBase(cell.Tile.MeshRenderer.sharedMaterial == cell.room.baseMat ? holeAtlases[cell.room].NormalMat : holeAtlases[cell.room].PosterMat);
            crawlspaceCell.SetBase(holeAtlases[CrawlspaceEc.rooms[0]].NormalMat);
        }

        private CellData[] FillArea()
        {
            List<CellData> cells = [];
            for (int y = 0; y < ec.levelSize.z; y++)
            {
                int yOffset = 0;
                if (y == 0)
                    yOffset = 4;
                if (y == ec.levelSize.z - 1)
                    yOffset += 1;

                for (int x = 0; x < ec.levelSize.x; x++)
                {
                    int type = yOffset;
                    if (x == 0)
                        type += 8;
                    if (x == ec.levelSize.x - 1)
                        type += 2;

                    cells.Add(new() { pos = new(x,y), roomId = 0, type = type});
                }
            }
            return cells.ToArray();
        }

        public override void Begin()
        {
            base.Begin();
            CrawlspaceEc.Active = true;
            StartCoroutine(TilesStartDisappearingCusWhyNot());
        }

        private IEnumerator TilesStartDisappearingCusWhyNot()
        {
            while (active && potentialFallCells.Count > 0)
            {
                //yield return new WaitForSecondsEnvironmentTimescale(ec, UnityEngine.Random.Range(0f,1f));
                Cell cellThatFell = potentialFallCells[UnityEngine.Random.Range(0,potentialFallCells.Count)];
                potentialFallCells.Remove(cellThatFell);
                UpdateHallCell(cellThatFell.position.x, cellThatFell.position.z, true);
                yield return null;
            }
        }

        public override void End()
        {
            base.End();
        }
    }
}