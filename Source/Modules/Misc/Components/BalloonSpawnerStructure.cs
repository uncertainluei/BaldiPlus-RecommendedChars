using System.Linq;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    internal class BalloonSpawnerStructure : StructureBuilder
    {
        private Balloon[] balloonPres;
        private IntVector2 minMax;

        private static readonly RoomCategory[] whitelist = [RoomCategory.Class, RoomCategory.Faculty, RoomCategory.Office];

        public override void Initialize(EnvironmentController ec, StructureParameters parameters)
        {
            base.Initialize(ec, parameters);
            minMax = parameters.minMax[0];

            int c = parameters.prefab.Length;
            balloonPres = new Balloon[c];
            for (int i = 0; i < c; i++)
                balloonPres[i] = parameters.prefab[i].selection.GetComponent<Balloon>();
        }

        private void Generate(System.Random rng)
        {
            foreach (RoomController room in ec.rooms)
                if (whitelist.Contains(room.category))
                    SpawnBalloons(room, rng);
        }

        private void SpawnBalloons(RoomController room, System.Random rng)
        {
            int count = rng.Next(minMax.x, minMax.z);
            Quaternion zero = Quaternion.Euler(new());
            for (int i = 0; i < count; i++)
            {
                Instantiate(balloonPres[rng.Next(balloonPres.Length)],
                    room.ec.CellFromPosition(room.entitySafeCells[rng.Next(room.entitySafeCells.Count)]).CenterWorldPosition,
                    zero, room.ec.transform).Initialize(room);
            }
        }

        public override void Generate(LevelGenerator lg, System.Random rng)
        {
            base.Generate(lg, rng);
            Generate(rng);
        }

        public override void GenerateInPremadeMap(System.Random rng)
        {
            base.GenerateInPremadeMap(rng);
            Generate(rng);
        }
    }
}
