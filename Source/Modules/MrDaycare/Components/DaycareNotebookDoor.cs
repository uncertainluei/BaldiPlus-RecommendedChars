using System;
using System.Collections;
using UncertainLuei.CaudexLib.Registers;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class DaycareNotebookDoor : StandardDoor
    {
        public bool IsNotebookGate { get; private set; }
        public int NotebookRequirement { get; private set; }
        private bool setupInProgress = false;

        private new void Start()
        {
            base.Start();
            SetMaterial();
            IsNotebookGate = true;
            setupInProgress = true;
            CaudexEvents.OnNotebookCollect += SetupRequirement;
            CaudexEvents.OnNotebookCollect += NotebookCheck;
        }

        private void OnDestroy()
        {
            if (IsNotebookGate)
                CaudexEvents.OnNotebookCollect -= NotebookCheck;
            if (setupInProgress)
                CaudexEvents.OnNotebookCollect -= SetupRequirement;
        }

        private void SetupRequirement()
        {
            setupInProgress = false;
            CaudexEvents.OnNotebookCollect -= SetupRequirement;

            NotebookRequirement = Mathf.RoundToInt(BaseGameManager.Instance.NotebookTotal*0.5f+0.1f);
            if (BaseGameManager.Instance.NotebookTotal < 5)
                NotebookRequirement = BaseGameManager.Instance.NotebookTotal-1;

            lockBlocks = false;
            locked = true; // Set it already to locked so it doesn't do the lock sound
            Lock(true);
            Block(true);
            SetMaterial(NotebookRequirement);
        }

        private void NotebookCheck()
        {
            if (BaseGameManager.Instance.FoundNotebooks >= NotebookRequirement)
                UnlockNotebookGate();
        }

        private void SetMaterial(StandardDoorMats mat)
        {
            overlayOpen[0] = mat.open;
            overlayShut[0] = mat.shut;
            overlayOpen[1] = mat.open;
            overlayShut[1] = mat.shut;
            UpdateTextures();
        }

        public void SetMaterial(int notebookCount) => SetMaterial(DaycareDoorAssets.GetMaterial(notebookCount));
        public void SetMaterial() => SetMaterial(DaycareDoorAssets.template);

        public void UnlockNotebookGate()
        {
            IsNotebookGate = false;
            CaudexEvents.OnNotebookCollect -= NotebookCheck;
            StartCoroutine(ProperlyUnlockDoor(this));
        }

        public override void LockTimed(float time)
        {
            if (!IsNotebookGate)
                base.LockTimed(time);
        }

        public override void Lock(bool cancelTimer)
        {
            base.Lock(cancelTimer);
            if (!IsNotebookGate)
                SetMaterial(DaycareDoorAssets.locked); // Set to locked material
        }

        public override void Unlock()
        {
            if (!IsNotebookGate)
            {
                base.Unlock();
                SetMaterial();
            }
        }

        private static IEnumerator ProperlyUnlockDoor(StandardDoor door)
        {
            yield return new WaitForEndOfFrame();
            door.Unlock();
            door.Block(false);
        }
    }
}
