using System.Collections;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class DaycareStandardDoor : StandardDoor
    {
        public bool IsNotebookGate { get; private set; }

        private bool aIsDaycare = false;
        private bool bIsDaycare = false;

        private new void Start()
        {
        }

        public void Setup(RoomController room)
        {
            if (bTile.room == room || IsMiscRoom(bTile.room))
            {
                aIsDaycare = true;
                mask[0] = DaycareDoorAssets.mask;
            }
            if (aTile.room == room || IsMiscRoom(aTile.room))
            {
                bIsDaycare = true;
                mask[1] = DaycareDoorAssets.mask;
            }

            audDoorOpen = DaycareDoorAssets.open;
            audDoorShut = DaycareDoorAssets.shut;
            audDoorUnlock = DaycareDoorAssets.unlock;

            IsNotebookGate = true;
            lockBlocks = false;
            locked = true; // Set it already to locked so it doesn't do the lock sound

            Lock(true);
            Block(true);
        }

        private bool IsMiscRoom(RoomController room)
        {
            if (room.type == RoomType.Hall) return true; // Is a hallway
            if (room.category != RoomCategory.Class && room.doorMats.name == "ClassDoorSet") return true; // Is not a classroom and has the 'classroom' doors
            return false;
        }

        private void SetMaterial(StandardDoorMats mat)
        {
            if (aIsDaycare)
            {
                overlayOpen[0] = mat.open;
                overlayShut[0] = mat.shut;
            }
            if (bIsDaycare)
            {
                overlayOpen[1] = mat.open;
                overlayShut[1] = mat.shut;
            }
            UpdateTextures();
        }

        public void SetMaterial(int notebookCount) => SetMaterial(DaycareDoorAssets.GetMaterial(notebookCount));
        private void SetMaterial() => SetMaterial(DaycareDoorAssets.template);

        public void UnlockNotebookGate()
        {
            IsNotebookGate = false;
            StartCoroutine(ProperlyUnlockDoor(this));
        }

        public override void Lock(bool cancelTimer)
        {
            base.Lock(cancelTimer);
            if (!IsNotebookGate)
                SetMaterial(DaycareDoorAssets.locked); // Set to locked material
        }

        public override void Unlock()
        {
            base.Unlock();
            SetMaterial(); // Set to locked material
        }

        private static IEnumerator ProperlyUnlockDoor(StandardDoor door)
        {
            yield return new WaitForEndOfFrame();
            door.Unlock();
            door.Block(false);
        }
    }
}
