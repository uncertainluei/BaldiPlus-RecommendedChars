using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class MrDaycare : Principal
    {
        internal static Character charEnum = (Character)(-1);

        private readonly int[] lockTimes = new int[]
        {
            30,
            60,
            100,
            200,
            500,
            3161
        };

        public SoundObject audSeconds;
        public readonly string[] rules = new string[]
        {
            "Running",
            "Drinking",
            "Eating", // Used by Lots Of Items
            "Throwing",
            "LoudSound"
        };

        public static Dictionary<string, SoundObject> audRuleBreaks = new Dictionary<string, SoundObject>();

        private RoomController daycareRoom;
        private DaycareRoomFunction daycareFunction;

        public override void Initialize()
        {
            base.Initialize();
            behaviorStateMachine.ChangeState(new MrDaycare_Waiting(this));
        }

        public void Start()
        {
            Cell currentCell = ec.CellFromPosition(transform.position);
            if (currentCell == null || (daycareRoom = currentCell.room) == null || !daycareRoom.functionObject.TryGetComponent(out daycareFunction))
            {
                RecommendedCharsPlugin.Log.LogError("Mr. Daycare spawned in an invalid location! Despawning...");
                Despawn();
                return;
            }

            daycareFunction.AssignMrDaycare(this);
            if (!daycareFunction.Inactive) // Free the guy if they spawn after collecting the required quota
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
                timeInSight[player.playerNumber] += Time.deltaTime * TimeScale;
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

        public void SendToTimeout()
        {
            targetedPlayer.Teleport(ec.RealRoomMid(daycareRoom));
            DaycareGuiltManager.GetInstance(targetedPlayer).ClearGuilt();
            transform.position = targetedPlayer.transform.position + targetedPlayer.transform.forward * 10f;
            daycareFunction.Activate(lockTimes[detentionLevel], ec);

            audMan.QueueAudio(audDetention);
            audMan.QueueAudio(audTimes[detentionLevel]);
            audMan.QueueAudio(audSeconds);

            timeInSight[targetedPlayer.playerNumber] = 0f;
            detentionLevel = Math.Min(detentionLevel + 1, lockTimes.Length-1);
            ec.MakeNoise(targetedPlayer.transform.position, detentionNoise);
            behaviorStateMachine.ChangeState(new MrDaycare_Timeout(this));
        }

        public void SendToTimeout(NPC npc)
        {
            npc.transform.position = daycareRoom.RandomEntitySafeCellNoGarbage().FloorWorldPosition + Vector3.up * 9f;
            npc.SentToDetention();
        }
    }

    public class MrDaycare_Waiting : NpcState
    {
        public MrDaycare_Waiting(MrDaycare daycare) : base(daycare)
        {
        }

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

    public class MrDaycare_Wandering : Principal_Wandering
    {
        private readonly MrDaycare daycare;
        public MrDaycare_Wandering(MrDaycare daycare) : base(daycare)
        {
            this.daycare = daycare;
        }

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

    public class MrDaycare_ChasingPlayer : Principal_ChasingPlayer
    {
        private readonly MrDaycare daycare;
        private int currentNoiseVal = 0;

        public MrDaycare_ChasingPlayer(MrDaycare daycare, PlayerManager player) : base(daycare, player)
        {
            this.daycare = daycare;
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            currentNoiseVal = 0;
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (this.player == player)
                currentNoiseVal = 0;
        }

        public override void Hear(GameObject source, Vector3 position, int value)
        {
            base.Hear(source, position, value);
            if (!npc.looker.PlayerInSight(player) && currentNoiseVal <= value)
            {
                currentNoiseVal = value;
                ChangeNavigationState(targetState);
                targetState.UpdatePosition(position);
            }
        }

        public override void OnStateTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && other.transform == player.transform)
                daycare.SendToTimeout();
        }
    }

    public class MrDaycare_ChasingNpc : Principal_ChasingNpc
    {
        private readonly MrDaycare daycare;

        public MrDaycare_ChasingNpc(MrDaycare daycare, NPC targetedNpc) : base(daycare, targetedNpc)
        {
            this.daycare = daycare;
        }

        public override void Resume()
        {
            daycare.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
        }

        public override void OnStateTriggerStay(Collider other)
        {
            if (other.transform == targetedNpc.transform)
            {
                daycare.SendToTimeout(npc);
                daycare.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
            }
        }

        public override void DestinationEmpty()
        {
            daycare.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
        }
    }

    public class MrDaycare_Timeout : Principal_Detention
    {
        private readonly MrDaycare daycare;
        public MrDaycare_Timeout(MrDaycare daycare) : base(daycare, 0)
        {
            this.daycare = daycare;
        }

        public override void Update()
        {
            if (!daycare.audMan.AnyAudioIsPlaying)
                npc.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
        }
    }
}
