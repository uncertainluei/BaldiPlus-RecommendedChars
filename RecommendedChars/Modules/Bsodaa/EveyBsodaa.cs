using System;
using System.Collections;
using System.Collections.Generic;

using MTM101BaldAPI;
using MTM101BaldAPI.Components;

using UncertainLuei.BaldiPlus.RecommendedChars;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class EveyBsodaa : NPC
    {
        internal static Dictionary<string, Sprite[]> animations;
        public CustomSpriteAnimator animator;

        public AudioManager audMan;
        public EveyBsodaaSpray projectilePre;

        public SoundObject audCharging;
        public SoundObject[] audSuccess;
        public SoundObject audReloaded;

        public float ChargeTime { get; private set; } = 2f;

        public override void Initialize()
        {
            base.Initialize();

            animator.PopulateAnimations(animations, 8);
            animator.Play("Idle",1f);

            behaviorStateMachine.ChangeState(new EveyBsodaa_Wandering(this));
        }

        public void BsodaFinished(bool hit)
        {
            animator.Play("Upset", 1f); // Set to upset sprite if no NPC/Player was hit
            if (hit) 
            {
                animator.Play("Happy", 1f); // Set to happy sprite
                audMan.QueueAudio(audSuccess[UnityEngine.Random.Range(0, audSuccess.Length)]); // Play one of the happy lines
            }

            behaviorStateMachine.ChangeState(new EveyBsodaa_TimedRestock(this, 10f));
        }

        public void Shoot(Transform target)
        {
            EveyBsodaaSpray bsoda = Instantiate(projectilePre);
            bsoda.bsodaa = this;
            bsoda.transform.rotation = Directions.DirFromVector3(target.position-transform.position, 45f).ToRotation();
            bsoda.transform.position = transform.position + bsoda.transform.forward * 2f;

            behaviorStateMachine.ChangeState(new EveyBsodaa_Waiting(this));
            animator.Play("Shoot", 1f);
        }

        public void Charge()
        {
            audMan.FlushQueue(true);
            audMan.PlaySingle(audCharging);
            animator.Play("Charge", 1f);
        }

        public bool NpcInSight(NPC npc)
        {
            if (Navigator.Entity.Blinded)
                return false;

            looker.Raycast(npc.transform, Mathf.Min((transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude, looker.distance, npc.ec.MaxRaycast), out bool sighted);
            return sighted;
        }
    }

    public class EveyBsodaa_Statebase : NpcState
    {
        protected readonly EveyBsodaa bsodaa;
        public EveyBsodaa_Statebase(EveyBsodaa bsodaa) : base(bsodaa)
        {
            this.bsodaa = bsodaa;
        }
    }

    public class EveyBsodaa_Wandering : EveyBsodaa_Statebase
    {
        public EveyBsodaa_Wandering(EveyBsodaa bsodaa) : base(bsodaa)
        {
        }

        public override void Enter()
        {
            base.Enter();
            bsodaa.animator.Play("Idle", 1f);
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        public override void InPlayerSight(PlayerManager player)
        {
            base.InPlayerSight(player);
            npc.behaviorStateMachine.ChangeState(new EveyBsodaa_PreCharge(bsodaa, player));
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }
    }

    public class EveyBsodaa_PreCharge : EveyBsodaa_Statebase
    {
        private readonly Transform targetTransform;

        private readonly NPC targetNpc;
        private readonly PlayerManager player;

        private readonly Func<bool> InSight;
        private EveyBsodaa_PreCharge(EveyBsodaa bsodaa, Transform target) : base(bsodaa)
        {
            targetTransform = target;
        }
        public EveyBsodaa_PreCharge(EveyBsodaa bsodaa, PlayerManager target) : this(bsodaa, target.transform)
        {
            player = target;
            InSight = PlayerInSight;
        }
        public EveyBsodaa_PreCharge(EveyBsodaa bsodaa, NPC target) : this(bsodaa, target.transform)
        {
            targetNpc = target;
            InSight = NpcInSight;
        }

        private bool PlayerInSight() => npc.looker.PlayerInSight(player);
        private bool NpcInSight() => bsodaa.NpcInSight(npc);

        public override void Enter()
        {
            base.Enter();
            npc.Navigator.FindPath(npc.transform.position, targetTransform.position);
            ChangeNavigationState(new NavigationState_TargetPosition(npc, 63, npc.Navigator.NextPoint));
        }

        public override void DestinationEmpty()
        {
            if (InSight())
            {
                base.DestinationEmpty();
                npc.behaviorStateMachine.ChangeState(new EveyBsodaa_Charging(bsodaa, targetTransform, bsodaa.ChargeTime));
                return;
            }
            npc.behaviorStateMachine.ChangeState(new EveyBsodaa_Wandering(bsodaa));
        }
    }

    public class EveyBsodaa_Charging : EveyBsodaa_Statebase
    {
        private readonly Transform targetTransform;
        private float timeLeft;

        public EveyBsodaa_Charging(EveyBsodaa bsodaa, Transform target, float chargeTime) : base(bsodaa)
        {
            targetTransform = target;
            timeLeft = chargeTime;
        }

        public override void Enter()
        {
            base.Enter();
            bsodaa.Charge();
            ChangeNavigationState(new NavigationState_DoNothing(npc, 0));
        }

        public override void Update()
        {
            base.Update();
            timeLeft -= Time.deltaTime * npc.TimeScale;
            if (timeLeft <= 0f)
            {
                bsodaa.Shoot(targetTransform);
                npc.behaviorStateMachine.ChangeState(new EveyBsodaa_Waiting(bsodaa));
            }
        }
    }

    public class EveyBsodaa_Waiting : EveyBsodaa_Statebase
    {
        public EveyBsodaa_Waiting(EveyBsodaa bsodaa) : base(bsodaa)
        {
        }
    }

    // Placeholder, used before implementing Bsodaa Helper
    public class EveyBsodaa_TimedRestock : EveyBsodaa_Statebase
    {
        private float timeLeft;
        public EveyBsodaa_TimedRestock(EveyBsodaa bsodaa, float time) : base(bsodaa)
        {
            timeLeft = time;
        }

        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        public override void Update()
        {
            base.Update();
            timeLeft -= Time.deltaTime * npc.TimeScale;
        }
    }

    public class EveyBsodaa_OutForRestock : EveyBsodaa_Statebase
    {

        public EveyBsodaa_OutForRestock(EveyBsodaa bsodaa) : base(bsodaa)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }
    }
}
