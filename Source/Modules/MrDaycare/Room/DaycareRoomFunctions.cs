using System;
using System.Collections.Generic;
using UncertainLuei.CaudexLib.Registers;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class MrDaycareHolderFunction : RoomFunction
    {
        private readonly List<MrDaycare> mrDaycares = [];
        public bool Unlocked {get; private set;} = false;

        public void FixedUpdate()
        {
            if (Unlocked)
                return;

            foreach (Door door in room.doors)
            {
                if (door == null || door.locked)
                    continue;
                    
                Unlock();
                return;
            }
        }

        public override void OnPlayerEnter(PlayerManager player)
        {
            base.OnPlayerEnter(player);
            if (!Unlocked) // Force-activate if player gets through while inactive
                Unlock();
        }

        public void AssignMrDaycare(MrDaycare daycare)
            => mrDaycares.Add(daycare);

        public bool ContainsMrDaycare(MrDaycare daycare)
            => mrDaycares.Contains(daycare);

        private void Unlock()
        {
            Unlocked = true;
            foreach (MrDaycare daycare in mrDaycares)
                daycare.Activate();
        }
    }

    [RequireComponent(typeof(MrDaycareHolderFunction))]
    public class DaycareTimeoutRoomFunction : DetentionRoomFunction
    {
        public MrDaycareHolderFunction DaycareHolder {get; private set;}

        public override void Initialize(RoomController room)
        {
            base.Initialize(room);
            DaycareHolder = GetComponent<MrDaycareHolderFunction>();

            // Remove room from Principal's Offices
            room.ec.offices.Remove(room);
        }

        private void RemoveDoorAnimComponent(StandardDoor door)
        {
            DestroyImmediate(door.GetComponent<BBPlusAnimations.Components.StandardDoorExtraMaterials>());
        }

        private bool _active;
        public override void OnPlayerExit(PlayerManager player)
        {
            _active = active;
            active = false;

            base.OnPlayerExit(player);

            if (_active)
            {
                DaycareGuiltManager.GetInstance(player).BreakRule("DaycareEscaping", time, 0.25f);
                active = true;
            }
        }
        public override void OnNpcEnter(NPC npc)
        {
            _active = active;
            active = false;

            base.OnNpcEnter(npc);

            if (!_active) return;
            active = true;

            if (npc is MrDaycare daycare && DaycareHolder.ContainsMrDaycare(daycare))
                LockNearestDoor(npc);
        }
        public override void OnNpcExit(NPC npc)
        {
            _active = active;
            active = false;

            base.OnNpcExit(npc);

            if (!_active) return;
            active = true;

            if (npc is MrDaycare daycare && DaycareHolder.ContainsMrDaycare(daycare))
                LockNearestDoor(npc);
        }
    }

    public class DaycareRuleFreeZone : RoomFunction
    {
        public bool excludeEscaping = true;

        public override void OnPlayerStay(PlayerManager player)
        {
            base.OnPlayerStay(player);
            DaycareGuiltManager.GetInstance(player).ClearGuilt(excludeEscaping);
        }
    }
}
