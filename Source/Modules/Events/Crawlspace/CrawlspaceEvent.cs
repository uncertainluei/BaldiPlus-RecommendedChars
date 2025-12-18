using System.Collections;
using System.Collections.Generic;
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
        public EnvironmentController ecPrefab; 
        public RoomController roomPrefab;

        private EnvironmentController crawlspaceEc;

        public override void Initialize(EnvironmentController controller, System.Random rng)
        {
            base.Initialize(controller, rng);

            crawlspaceEc = Object.Instantiate(ecPrefab);
            crawlspaceEc.name = controller.name+"_Crawlspace";
            crawlspaceEc.levelSize = ec.levelSize;
            crawlspaceEc.InitializeCells(crawlspaceEc.levelSize);
            crawlspaceEc.InitializeHeap();
            crawlspaceEc.transform.position -= Vector3.up * 10f;

            // Load an entire empty map lol!
            crawlspaceEc.rooms = [Instantiate(roomPrefab, crawlspaceEc.transform)];
            crawlspaceEc.rooms[0].maxSize = crawlspaceEc.levelSize;
            crawlspaceEc.rooms[0].size = crawlspaceEc.levelSize;
            crawlspaceEc.rooms[0].ec = crawlspaceEc;
            crawlspaceEc.rooms[0].GenerateTextureAtlas();
            for (int y = 0; y < crawlspaceEc.levelSize.z; y++)
            {
                int yOffset = 0;
                if (y == 0)
                    yOffset = 4;
                if (y == crawlspaceEc.levelSize.z - 1)
                    yOffset += 1;

                for (int x = 0; x < crawlspaceEc.levelSize.x; x++)
                {
                    int type = yOffset;
                    if (x == 0)
                        type += 8;
                    if (x == crawlspaceEc.levelSize.x - 1)
                        type += 2;

                    crawlspaceEc.CreateCell(type, new IntVector2(x,y), crawlspaceEc.rooms[0]);
                }
            }

            // Does this work? Idfk
            crawlspaceEc.map = ec.map;
        }

        public override void Begin()
        {
            base.Begin();
        }

        public override void End()
        {
            base.End();
        }
    }
}