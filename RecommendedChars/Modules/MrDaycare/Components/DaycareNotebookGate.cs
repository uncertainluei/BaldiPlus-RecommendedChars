using System.Collections;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
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

            if (RecommendedCharsPlugin.AnimationsCompat)
                AnimationsCompat();

            door.audDoorOpen = DaycareDoorAssets.open;
            door.audDoorShut = DaycareDoorAssets.shut;

            door.locked = true; // Set it already to locked so it doesn't do the lock sound
            door.Lock(true);
            door.Block(true);
        }

        private void AnimationsCompat()
        {
            Destroy(GetComponent<BBPlusAnimations.Components.StandardDoorExtraMaterials>());
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
