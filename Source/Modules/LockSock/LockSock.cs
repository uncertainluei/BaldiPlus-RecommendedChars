using System.Collections;
using System.Collections.Generic;
using MTM101BaldAPI;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{   
    public class LockSock : NPC
    {
        public AudioManager audMan;
        public SoundObject audLock;


        public SpriteRenderer sprite;
        public Sprite sprNormal, sprLocking;

        public float doorLockChance = 0.45f;

        private readonly List<Door> doorQueue = [];
        public byte maxDoorQueueSize = 3;

        public override void Initialize()
        {
            base.Initialize();
            behaviorStateMachine.ChangeState(new LockSock_Wander(this));
        }

        private Cell currentCell;
        public void CheckCellForDoors(Cell cell)
        {
            if (currentCell == cell || Blinded)
                return;
            
            currentCell = cell;
            if (cell.doors != null && cell.doors.Count > 0)
            {
                foreach (Door door in cell.doors)
                {
                    if (ShouldInspectDoor(door))
                    {
                        behaviorStateMachine.ChangeState(new LockSock_Locking(this,door));
                        return;
                    }
                }
            }
        }

        public void LockDoor(Door door)
        {
            audMan.PlaySingle(audLock);
            if (door.IsOpen)
                door.Shut();
            door.LockTimed(7f);
        }

        public bool ShouldInspectDoor(Door door)
        {
            if (door == null || doorQueue.Contains(door) ||
                door.locked || Random.value >= doorLockChance)
                return false;

            if (doorQueue.Count == maxDoorQueueSize)
                doorQueue.RemoveAt(0);
            doorQueue.Add(door);
            return true;
        }
    }

    public class LockSock_StateBase(LockSock sock) : NpcState(sock)
    {
        protected readonly LockSock sock = sock;
    }

    public class LockSock_Wander(LockSock sock) : LockSock_StateBase(sock)
    {
        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        public override void Update()
        {
            base.Update();
            sock.CheckCellForDoors(npc.ec.CellFromPosition(npc.transform.position));   
        }
    }

    public class LockSock_Locking(LockSock sock, Door target) : LockSock_StateBase(sock)
    {
        private Door door = target;
        private Vector3 targetPosition;
        private IEnumerator waitRoutine;

        public override void Enter()
        {
            base.Enter();
            targetPosition = npc.transform.position;
            Vector3 aPos = door.aTile.CenterWorldPosition;
            Vector3 bPos = door.bTile.CenterWorldPosition;
            targetPosition = Vector3.Distance(targetPosition, aPos) <= Vector3.Distance(targetPosition, bPos) ? aPos : bPos;
            targetPosition += (aPos+(bPos-aPos)/2-targetPosition) * 0.3f;
            ChangeNavigationState(new NavigationState_TargetPosition(npc, 31, targetPosition));
        }

        public override void Exit()
        {
            base.Exit();
            sock.sprite.sprite = sock.sprNormal;

            if (waitRoutine != null)
                sock.StopCoroutine(waitRoutine);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            waitRoutine = Wait();
            sock.StartCoroutine(waitRoutine);
        }

        private IEnumerator Wait()
        {
            npc.behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(npc, 0));
            sock.sprite.sprite = sock.sprLocking;
            sock.LockDoor(door);
            yield return new WaitForSecondsNPCTimescale(sock, 0.4f);
            npc.behaviorStateMachine.ChangeState(new LockSock_Wander(sock));
        }
    }
}