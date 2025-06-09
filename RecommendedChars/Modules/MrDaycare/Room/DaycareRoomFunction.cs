using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class DaycareRoomFunction : DetentionRoomFunction
    {
        private static event Action OnNotebookCollect;
        public static void NotebookCollected() => OnNotebookCollect?.Invoke();

        public bool Inactive { get; private set; } = true;
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
            
            DaycareNotebookGate daycareDoor;
            foreach (Door door in room.doors)
            {
                if (!(door is StandardDoor standardDoor)) continue;
                if (!door.TryGetComponent(out daycareDoor))
                    daycareDoor = door.gameObject.AddComponent<DaycareNotebookGate>();

                daycareDoor.Setup(standardDoor, room);
            }
        }

        public int NotebookRequirement { get; private set; } 
        private void SetupRequirement()
        {
            setupInProgress = false;
            OnNotebookCollect -= SetupRequirement;

            NotebookRequirement = Mathf.RoundToInt(BaseGameManager.Instance.NotebookTotal * 0.5f + 0.1f);
            if (BaseGameManager.Instance.NotebookTotal < 5)
                NotebookRequirement = BaseGameManager.Instance.NotebookTotal - 1;

            DaycareNotebookGate daycareDoor;
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
                player.RuleBreak("DaycareEscaping", time, 0.25f);
                active = true;
            }
        }
        public override void OnNpcEnter(NPC npc)
        {
            _active = active;
            active = false;

            base.OnNpcEnter(npc);

            if (_active && npc is MrDaycare daycare && mrDaycares.Contains(daycare))
            {
                LockNearestDoor(npc);
                active = true;
            }
        }
        public override void OnNpcExit(NPC npc)
        {
            _active = active;
            active = false;

            base.OnNpcExit(npc);

            if (_active && npc is MrDaycare daycare && mrDaycares.Contains(daycare))
            {
                LockNearestDoor(npc);
                active = true;
            }
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

            DaycareNotebookGate daycareDoor;
            int doorCount = room.doors.Count;
            for (int i = 0; i < doorCount; i++)
                if (room.doors[i].TryGetComponent(out daycareDoor))
                    daycareDoor.Unlock();

            foreach (MrDaycare daycare in mrDaycares)
                daycare.Activate();
        }
    }

    public class DaycareNotebookGate : MonoBehaviour
    {
        private StandardDoor door;

        private bool aIsDaycare = false;
        private bool bIsDaycare = false;

        public void Setup(StandardDoor door, RoomController room)
        {
            this.door = door;

            if (door.bTile.room == room || door.bTile.room.type == RoomType.Hall)
            {
                aIsDaycare = true;
                door.mask[0] = DaycareDoorAssets.mask;
            }
            if (door.aTile.room == room || door.aTile.room.type == RoomType.Hall)
            {
                bIsDaycare = true;
                door.mask[1] = DaycareDoorAssets.mask;
            }

            door.audDoorOpen = DaycareDoorAssets.open;
            door.audDoorShut = DaycareDoorAssets.shut;

            door.locked = true; // Set it already to locked so it doesn't do the lock sound
            door.Lock(true);
            door.Block(true);
        }

        private void SetMaterial(StandardDoorMats mat)
        {
            if (aIsDaycare)
            {
                door.overlayOpen[0] = mat.open;
                door.overlayShut[0] = mat.shut;
            }
            if (bIsDaycare)
            {
                door.overlayOpen[1] = mat.open;
                door.overlayShut[1] = mat.shut;
            }
            door.UpdateTextures();
        }

        public void SetMaterial(int notebookCount) => SetMaterial(DaycareDoorAssets.GetMaterial(notebookCount));
        private void SetMaterial() => SetMaterial(DaycareDoorAssets.template);

        public void Unlock()
        {
            SetMaterial(); // Set to default material
            door.StartCoroutine(ProperlyUnlockDoor(door));
            DestroyImmediate(this);
        }

        private static IEnumerator ProperlyUnlockDoor(StandardDoor door)
        {
            yield return new WaitForEndOfFrame();
            door.Unlock();
            door.Block(false);
        }
    }
}
