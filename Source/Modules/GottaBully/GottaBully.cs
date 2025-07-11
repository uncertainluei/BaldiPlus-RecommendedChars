﻿using System.Collections.Generic;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class GottaBully : GottaSweep
    {
        public override void Initialize()
        {
            base.Initialize();
            behaviorStateMachine.ChangeState(new GottaBully_Wait(this));
        }

        public override void VirtualOnTriggerEnter(Collider other)
        {
        }

        public override void VirtualOnTriggerExit(Collider other)
        {
        }

        public void BackHome()
        {
            transform.position = home;
            behaviorStateMachine.ChangeState(new GottaBully_Wait(this));
        }

        private readonly List<int> slotsToSteal = [];
        public Bully bullyReference;
        public void StealItem(PlayerManager pm)
        {
            audMan.PlaySingle(audSweep);

            slotsToSteal.Clear();
            for (int i = 0; i <= pm.itm.maxItem; i++)
                if (!bullyReference.itemsToReject.Contains(pm.itm.items[i].itemType))
                    slotsToSteal.Add(i);

            if (slotsToSteal.Count > 0)
            {
                pm.itm.RemoveItem(slotsToSteal[Random.Range(0, slotsToSteal.Count)]);
                behaviorStateMachine.ChangeState(new GottaBully_Stealing(this));
                return;
            }

            pm.plm.Entity.AddForce(new Force((pm.transform.position - transform.position + navigator.Velocity).normalized, speed, -speed));
        }
    }

    public class GottaBully_Wait(GottaBully gottaBully) : GottaSweep_Wait(gottaBully, gottaBully)
    {
        public override void Enter()
        {
            base.Enter();
            // Disable addend movemods so it doesn't get pushed elsewhere
            npc.Navigator.Entity.SetResistAddend(true);
        }

        public override void Exit()
        {
            base.Exit();
            npc.Navigator.Entity.SetResistAddend(false);
        }

        public override void Update()
        {
            waitTime -= Time.deltaTime * npc.TimeScale;
            if (waitTime <= 0f)
                npc.behaviorStateMachine.ChangeState(new GottaBully_BullyingTime(gottaBully));
        }
    }

    public class GottaBully_Stealing(GottaBully gottaBully) : GottaSweep_StateBase(gottaBully, gottaBully)
    {
        public override void Update()
        {
            if (!gottaBully.audMan.AnyAudioIsPlaying)
                gottaBully.BackHome();
        }
    }

    public class GottaBully_BullyingTime(GottaBully gottaBully) : GottaSweep_SweepingTime(gottaBully, gottaBully)
    {
        public override void OnStateTriggerEnter(Collider other)
        {
            base.OnStateTriggerEnter(other);

            if (other.isTrigger && other.GetComponent<Principal>() != null)
            {
                gottaBully.BackHome();
                return;
            }

            if (other.CompareTag("Player"))
            {
                PlayerManager pm = other.GetComponent<PlayerManager>();
                if (!pm.Tagged && !pm.plm.entity.resistAddend)
                    gottaBully.StealItem(pm);
            }
        }

        public override void Update()
        {
            sweepTime -= Time.deltaTime * npc.TimeScale;
            if (sweepTime <= 0f)
                npc.behaviorStateMachine.ChangeState(new GottaBully_Returning(gottaBully));
        }
    }

    public class GottaBully_Returning(GottaBully gottaBully) : GottaSweep_Returning(gottaBully, gottaBully)
    {
        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (!gottaSweep.IsHome)
                npc.behaviorStateMachine.CurrentNavigationState.UpdatePosition(gottaSweep.home);
            else
                npc.behaviorStateMachine.ChangeState(new GottaBully_Wait(gottaBully));
        }
    }
}
