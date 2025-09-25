using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class MrDaycare : Principal
    {
        internal static Character charEnum = (Character)(-1);

        private readonly int[] lockTimes =
        [
            30,
            60,
            100,
            200,
            500,
            3161
        ];

        public int maxTimeoutLevel = -1;
        public float ruleSensitivityMul = 1.5f;

        public readonly string[] rules =
        [
            "Running",
            "Drinking",
            "Eating",
            "Throwing",
            "LoudSound"
        ];

        public static Dictionary<string, SoundObject> audRuleBreaks = [];
        public SoundObject audSeconds;

        private RoomController daycareRoom;
        private DaycareTimeoutRoomFunction daycareFunction;


        public override void Initialize()
        {
            base.Initialize();
            behaviorStateMachine.ChangeState(new MrDaycare_Waiting(this));
        }

        public void Start()
        {
            if (maxTimeoutLevel < 0)
                maxTimeoutLevel = lockTimes.Length-1;

            Cell currentCell = ec.CellFromPosition(transform.position);
            if (currentCell == null || (daycareRoom = currentCell.room) == null || !daycareRoom.functionObject.TryGetComponent(out daycareFunction))
            {
                RecommendedCharsPlugin.Log.LogError("Mr. Daycare spawned in an invalid location! Despawning...");
                Despawn();
                return;
            }

            daycareFunction.DaycareHolder.AssignMrDaycare(this);
            if (daycareFunction.DaycareHolder.Unlocked) // Free the guy if they spawn after collecting the required quota
                Activate();
        }

        public void Activate()
        {
            navigationStateMachine.currentState.priority = -1;
            Navigator.Entity.SetResistAddend(false);
            Navigator.Entity.SetFrozen(false);
            behaviorStateMachine.ChangeState(new MrDaycare_Wandering(this));
        }

        public new void ObservePlayer(PlayerManager player)
        {
            if (!player.Tagged && DaycareGuiltManager.GetInstance(player).Disobeying)
            {
                timeInSight[player.playerNumber] += Time.deltaTime * TimeScale * ruleSensitivityMul;
                if (timeInSight[player.playerNumber] >= DaycareGuiltManager.GetInstance(player).GuiltSensitivity)
                {
                    targetedPlayer = player;
                    Scold(DaycareGuiltManager.GetInstance(player).RuleBreak);
                    behaviorStateMachine.ChangeState(new MrDaycare_ChasingPlayer(this, player));
                }
            }
        }

        public new void Scold(string ruleBroken)
        {
            if (audRuleBreaks.ContainsKey(ruleBroken))
            {
                audMan.FlushQueue(true);
                audMan.PlaySingle(audRuleBreaks[ruleBroken]);
            }
        }

        public void SendToTimeout(bool canCollide)
        {
            DaycareGuiltManager.GetInstance(targetedPlayer).ClearGuilt();
            if (canCollide)
            {
                targetedPlayer.Teleport(ec.RealRoomMid(daycareRoom));
                Navigator.Entity.Teleport(targetedPlayer.transform.position + targetedPlayer.transform.forward * 10f);
            }
            else
            {
                Navigator.Entity.Teleport(ec.RealRoomMid(daycareRoom));
                DaycareGuiltManager.GetInstance(targetedPlayer).BreakRule("DaycareEscaping", lockTimes[detentionLevel], 0.25f);
            }            
            daycareFunction.Activate(lockTimes[detentionLevel], ec);

            audMan.QueueAudio(audDetention);
            audMan.QueueAudio(audTimes[detentionLevel]);
            audMan.QueueAudio(audSeconds);

            timeInSight[targetedPlayer.playerNumber] = 0f;
            detentionLevel = Math.Min(detentionLevel+1, maxTimeoutLevel);
            ec.MakeNoise(targetedPlayer.transform.position, detentionNoise);
            behaviorStateMachine.ChangeState(new MrDaycare_Timeout(this));
        }

        public void SendToTimeout(NPC npc, bool canCollide)
        {
            if (canCollide)
            {
                npc.transform.position = daycareRoom.RandomEntitySafeCellNoGarbage().FloorWorldPosition + Vector3.up * 9f;
                npc.SentToDetention();
            }
            Navigator.Entity.Teleport(daycareRoom.RandomEntitySafeCellNoGarbage().FloorWorldPosition + Vector3.up * 5f);
        }
    }

    public class MrDaycare_Waiting(MrDaycare daycare) : NpcState(daycare)
    {
        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_DoNothing(npc, 127));
            npc.Navigator.Entity.SetResistAddend(true);
            npc.Navigator.Entity.SetFrozen(true);
        }

        public override void DoorHit(StandardDoor door)
        {
        }
    }

    public class MrDaycare_Wandering(MrDaycare daycare) : Principal_Wandering(daycare)
    {
        private readonly MrDaycare daycare = daycare;

        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_WanderRounds(npc, 0));
        }

        public override void Update()
        {
            if (!npc.Navigator.Entity.Blinded)
            {
                foreach (NPC npc in daycare.ec.Npcs)
                {
                    if (!npc.Disobeying || !daycare.rules.Contains(npc.BrokenRule)) continue;

                    daycare.looker.Raycast(npc.transform, Mathf.Min((daycare.transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude, daycare.looker.distance, npc.ec.MaxRaycast), out var targetSighted);
                    if (targetSighted)
                    {
                        daycare.behaviorStateMachine.ChangeState(new MrDaycare_ChasingNpc(daycare, npc));
                        daycare.Scold(npc.BrokenRule);
                    }
                    break;
                }
            }
        }

        public override void PlayerInSight(PlayerManager player)
        {
            daycare.ObservePlayer(player);
        }

        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            principal.LoseTrackOfPlayer(player);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_WanderRounds(daycare, 0));
        }

        public override void DoorHit(StandardDoor door)
        {
            door.OpenTimedWithKey(door.DefaultTime, false);
        }
    }

    public class MrDaycare_ChasingPlayer(MrDaycare daycare, PlayerManager player) : Principal_ChasingPlayer(daycare, player)
    {
        public override void OnStateTriggerStay(Collider other, bool canCollide)
        {
            if (other.CompareTag("Player") && other.transform == player.transform)
                daycare.SendToTimeout(canCollide);
        }
    }

    public class MrDaycare_ChasingNpc(MrDaycare daycare, NPC targetedNpc) : Principal_ChasingNpc(daycare, targetedNpc)
    {
        private readonly MrDaycare daycare = daycare;

        public override void Resume()
        {
            daycare.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
        }

        public override void OnStateTriggerStay(Collider other, bool canCollide)
        {
            if (other.transform == targetedNpc.transform)
            {
                daycare.SendToTimeout(npc, canCollide);
                daycare.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
            }
        }

        public override void DestinationEmpty()
        {
            daycare.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
        }
    }

    public class MrDaycare_Timeout(MrDaycare daycare) : Principal_Detention(daycare, 0)
    {
        private readonly MrDaycare daycare = daycare;

        public override void Update()
        {
            if (!daycare.audMan.AnyAudioIsPlaying)
                npc.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
        }
    }
}
