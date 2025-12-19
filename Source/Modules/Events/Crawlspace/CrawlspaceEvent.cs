using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        // Assigned textures
        public EnvironmentController ecPrefab; 
        public RoomController roomPrefab;
        public Texture2D transparent;

        private EnvironmentController crawlspaceEc;

        public override void Initialize(EnvironmentController controller, System.Random rng)
        {
            try
            {
            base.Initialize(controller, rng);

            crawlspaceEc = Instantiate(ecPrefab);
            crawlspaceEc.name = controller.name+"_Crawlspace";
            crawlspaceEc.levelSize = ec.levelSize;
            crawlspaceEc.InitializeCells(crawlspaceEc.levelSize);
            crawlspaceEc.InitializeHeap();
            crawlspaceEc.InitializeLighting();
            crawlspaceEc.transform.position -= Vector3.up * 10f;

            // Instantiate dummy room and set stuff up
            crawlspaceEc.rooms = [Instantiate(roomPrefab, crawlspaceEc.transform)];
            crawlspaceEc.rooms[0].maxSize = crawlspaceEc.levelSize;
            crawlspaceEc.rooms[0].size = crawlspaceEc.levelSize;
            crawlspaceEc.rooms[0].ec = crawlspaceEc;
            crawlspaceEc.rooms[0].GenerateTextureAtlas();
            // Cover the entire section with cells
            FillArea(ec, crawlspaceEc.rooms[0]);

            // Initialize lighting (it's still required despite the game using ONE lightmap texture)
            crawlspaceEc.standardDarkLevel = Color.white;
            crawlspaceEc.InitializeLighting();
            LightmapModHolder.GetInstance(ec).ForceUpdateLightmap();
            CoreGameManager.Instance.updateLightMap = false;

            // Does this work? Idfk
            crawlspaceEc.map = ec.map;
            }
            catch (Exception e)
            {
                RecommendedCharsPlugin.Log.LogError(e);
            }
        }

        private void FillArea(EnvironmentController ec, RoomController room)
        {
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

                    crawlspaceEc.CreateCell(type, new IntVector2(x,y), room);
                }
            }
        }

        public override void Begin()
        {
            base.Begin();
            PlayerManager player = CoreGameManager.Instance.GetPlayer(0);
            player.ec = crawlspaceEc;

            player.plm.entity.environmentController = crawlspaceEc;
            EntityHeightFixer.GetInstance((PlayerEntity)player.plm.entity).heightDifference = -10;
        }

        public override void End()
        {
            base.End();
        }

        private class PullYourArseDownThere : MonoBehaviour
        {
            private Vector3 _position;

            private void LateUpdate()
            {
                _position = transform.position;
                _position.y -= 10f;
                transform.position = _position;
            }
        }
    }
}