using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class BalloonRoomFunction : RoomFunction
    {
        public int balloonCount = 4;
        public Balloon[] balloonPres;

        public override void OnGenerationFinished()
        {
            base.OnGenerationFinished();
            Quaternion zero = Quaternion.Euler(new());
            for (int i = 0; i < balloonCount; i++)
            {
                Instantiate(balloonPres[Random.Range(0, balloonPres.Length)],
                    room.ec.CellFromPosition(room.entitySafeCells[Random.Range(0, room.entitySafeCells.Count)]).CenterWorldPosition,
                    zero, room.ec.transform).Initialize(room);
            }
        }
    }
}