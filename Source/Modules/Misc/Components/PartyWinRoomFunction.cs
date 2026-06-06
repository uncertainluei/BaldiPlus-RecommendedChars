using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PartyWinRoomFunction : RoomFunction
    {
        private bool surpriseTriggered = false;
        private PartyWinManager manager;
        private SurpriseNpcBase[] surpriseNpcs;

        public override void Initialize(RoomController room)
        {
            base.Initialize(room);
            room.offLimits = true; // Do not allow teleporter teleportation in this room (to prevent softlocks)
            if (BaseGameManager.Instance is PartyWinManager partyWin)
                manager = partyWin;
        }

        public override void OnGenerationFinished()
        {
            base.OnGenerationFinished();
            if (!manager)
                RecommendedCharsPlugin.Log.LogError("Party Win Room placed outside the Party Ending scene! Some functionality will be missing!");
            else
            {
                manager.cafeteria = this;
                manager.partyElevator = room.objectObject.transform.Find("PartyElevator(Clone)");
                manager.candle = room.objectObject.transform.Find("ClassicPartyCake_WithFlame(Clone)").GetChild(1).gameObject;
            }

            List<SurpriseNpcVisual> possibleVisuals = new(SurpriseNpc.possibleVisuals);
            RecommendedCharsPlugin.Log.LogDebug($"Party Win Room has {room.objectObject.transform.childCount} children");
            surpriseNpcs = room.objectObject.GetComponentsInChildren<SurpriseNpcBase>(true);
            SurpriseNpc[] npcs = room.objectObject.GetComponentsInChildren<SurpriseNpc>(true);
            int roll;
            for (int i = 0, c = npcs.Length; i < c && possibleVisuals.Count > 0; i++)
            {
                roll = Random.Range(0,possibleVisuals.Count);
                possibleVisuals[roll].Set(npcs[i]);
                possibleVisuals.RemoveAt(roll);
            }
        }

        public override void OnFirstPlayerEnter(PlayerManager player)
        {
            base.OnFirstPlayerEnter(player);
            if (surpriseTriggered) return;
            surpriseTriggered = true;

            foreach (PlayerManager player2 in room.ec.players)
                if (player2 && player2 != player)
                    player2.Teleport(player.transform.position);
            foreach (Door door in room.doors)
                door.Lock(true);
            foreach (SurpriseNpcBase npc in surpriseNpcs)
            {
                npc.transform.forward = (player.transform.position-npc.transform.position).normalized;
                npc.Surprise();
            }

            if (manager)
                manager.Surprise();
        }

        internal void CandleBlown()
        {
            foreach (Cell light in room.lights)
                light.SetLight(false);
            foreach (SurpriseNpcBase npc in surpriseNpcs)
                npc.StartCoroutine(npc.Float());
        }
    }
}
