using System;
using System.Collections.Generic;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class DaycareRoomFunction : DetentionRoomFunction
    {
        private static event Action OnNotebookCollect;
        public static void NotebookCollected() => OnNotebookCollect?.Invoke();

        public bool Inactive { get; private set; } = true;

        public bool animationsCompat;
        private bool setupInProgress = false;

        private readonly List<MrDaycare> mrDaycares = new List<MrDaycare>();

        public override void Initialize(RoomController room)
        {
            base.Initialize(room);

            // Remove room from Principal's Offices
            room.ec.offices.Remove(room);

            Inactive = true;
            setupInProgress = true;
            OnNotebookCollect += SetupRequirement;
            OnNotebookCollect += NotebookCheck;
        }

        private void OnDestroy()
        {
            if (Inactive)
                OnNotebookCollect -= NotebookCheck;
            if (setupInProgress)
                OnNotebookCollect -= SetupRequirement;
        }

        public override void OnGenerationFinished()
        {
            base.OnGenerationFinished();
            DaycareStandardDoor daycareDoor; 
            for (int i = 0; i < room.doors.Count; i++)
            {
                if (room.doors[i] == null) continue;
                if (room.doors[i] is DaycareStandardDoor) continue;
                if (room.doors[i] is StandardDoor standardDoor)
                {
                    if (animationsCompat)
                        RemoveDoorAnimComponent(standardDoor);

                    daycareDoor = RecommendedCharsPlugin.SwapComponentSimple<StandardDoor, DaycareStandardDoor>(standardDoor);
                    daycareDoor.Setup(room);
                    room.doors[i] = daycareDoor;
                }
            }
        }

        private void RemoveDoorAnimComponent(StandardDoor door)
        {
            DestroyImmediate(door.GetComponent<BBPlusAnimations.Components.StandardDoorExtraMaterials>());
        }

        public int NotebookRequirement { get; private set; } 
        private void SetupRequirement()
        {
            setupInProgress = false;
            OnNotebookCollect -= SetupRequirement;

            NotebookRequirement = Mathf.RoundToInt(BaseGameManager.Instance.NotebookTotal * 0.5f + 0.1f);
            if (BaseGameManager.Instance.NotebookTotal < 5)
                NotebookRequirement = BaseGameManager.Instance.NotebookTotal - 1;

            DaycareStandardDoor daycareDoor;
            int doorCount = room.doors.Count;
            for (int i = 0; i < doorCount; i++)
                if (room.doors[i].TryGetComponent(out daycareDoor))
                    daycareDoor.SetMaterial(NotebookRequirement);
        }

        public override void OnPlayerEnter(PlayerManager player)
        {
            base.OnPlayerEnter(player);
            if (Inactive) // Force-activate if player gets through while inactive
                Activate();
        }

        public void AssignMrDaycare(MrDaycare daycare)
        {
            mrDaycares.Add(daycare);
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

            if (npc is MrDaycare daycare && mrDaycares.Contains(daycare))
                LockNearestDoor(npc);
        }
        public override void OnNpcExit(NPC npc)
        {
            _active = active;
            active = false;

            base.OnNpcExit(npc);

            if (!_active) return;
            active = true;

            if (npc is MrDaycare daycare && mrDaycares.Contains(daycare))
                LockNearestDoor(npc);
        }

        private void NotebookCheck()
        {
            if (BaseGameManager.Instance.FoundNotebooks >= NotebookRequirement)
               Activate();
        }

        private void Activate()
        {
            Inactive = false;
            OnNotebookCollect -= NotebookCheck;

            int doorCount = room.doors.Count;
            for (int i = 0; i < doorCount; i++)
                if (room.doors[i] is DaycareStandardDoor daycareDoor && daycareDoor.IsNotebookGate)
                    daycareDoor.UnlockNotebookGate();

            foreach (MrDaycare daycare in mrDaycares)
                daycare.Activate();
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
