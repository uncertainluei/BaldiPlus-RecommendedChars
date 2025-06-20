using MTM101BaldAPI;
using MTM101BaldAPI.Components;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ManMemeCoin : NPC, IClickable<int>
    {
        internal static Dictionary<string, Sprite[]> animation;
        public CustomSpriteAnimator animator;

        public QuickExplosion explosionPre;
        public SoundObject explosionSound;

        private bool dead;

        public override void Initialize()
        {
            base.Initialize();
            dead = false;

            animator.PopulateAnimations(animation, 8);
            animator.SetDefaultAnimation("Spin", 1f);
            behaviorStateMachine.ChangeState(new ManMemeCoin_Idling(this));
        }

        public bool ClickableHidden()
        {
            return dead;
        }

        public bool ClickableRequiresNormalHeight()
        {
            return !Navigator.Entity.squished;
        }
            
        public void ClickableSighted(int player)
        {
        }

        public void ClickableUnsighted(int player)
        {
        }

        public void Clicked(int player)
        {
            if (dead) return;

            WeightedSelection<AbstractManMemeAction>[] events = ManMemeCoinEvents.Events.Where(x => x.selection.ShouldInclude(player)).ToArray();
            WeightedSelection<AbstractManMemeAction>.RandomSelection(events).Invoke(this, player);

            dead = true;
            StartCoroutine(OnBeatingItUp());
            behaviorStateMachine.ChangeState(new ManMemeCoin_Collected(this));
        }

        private IEnumerator OnBeatingItUp()
        {
            QuickExplosion explosion = Instantiate(explosionPre, spriteBase.transform);
            explosion.transform.localPosition += Vector3.forward * 0.015f;
            AudioManager audioManager = GetComponent<AudioManager>();
            audioManager.PlaySingle(explosionSound);
            yield return new WaitForSecondsEnvironmentTimescale(ec, 0.25F);
            spriteRenderer[0].enabled = false;
            yield return new WaitWhile(() => audioManager.AnyAudioIsPlaying);
            Despawn();
        }
    }

    public abstract class ManMemeCoin_StateBase : NpcState
    {
        protected readonly ManMemeCoin coin;

        public ManMemeCoin_StateBase(ManMemeCoin coin) : base(coin)
        {
            this.coin = coin;
        }
    }

    public class ManMemeCoin_Collected : ManMemeCoin_StateBase
    {
        public ManMemeCoin_Collected(ManMemeCoin coin) : base(coin)
        {
        }

        public override void Enter()
        {
            npc.normalLayer = 20;
            npc.navigator.Entity.defaultLayer = npc.normalLayer;
            npc.gameObject.layer = npc.normalLayer;

            npc.navigator.maxSpeed = 0f;
            npc.navigator.speed = 0f;

            npc.navigator.Entity.SetFrozen(true);
            ChangeNavigationState(new NavigationState_DoNothing(npc, 127, true));
        }
    }

    public class ManMemeCoin_Idling : ManMemeCoin_StateBase
    {
        private readonly Transform transform;

        public ManMemeCoin_Idling(ManMemeCoin coin) : base(coin)
        {
            transform = coin.transform;
        }

        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_DoNothing(npc, 0));
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (Vector3.Distance(player.transform.position, transform.position) <= player.pc.reach * 2)
                npc.behaviorStateMachine.ChangeState(new ManMemeCoin_Fleeing(coin, player));
        }
    }

    public class ManMemeCoin_Fleeing : ManMemeCoin_StateBase
    {
        private readonly PlayerManager player;

        private bool playerInSight = true;
        private float fleeTime;

        public ManMemeCoin_Fleeing(ManMemeCoin coin, PlayerManager player) : base(coin)
        {
            this.player = player;
            npc.Navigator.SetSpeed(player.plm.realVelocity);
        }

        public override void Enter()
        {
            base.Enter();
            fleeTime = 10f;
            playerInSight = true;
            ChangeNavigationState(new NavigationState_WanderFlee(npc, 32, player.dijkstraMap));
        }

        private void SetNavigatorSpeed(float target, float multiplier = 0.7f)
        {
            npc.Navigator.SetSpeed(npc.Navigator.Speed - (npc.Navigator.Speed - target) * Mathf.Min(multiplier * Time.deltaTime * npc.TimeScale, 1f));
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (player == this.player)
            {
                playerInSight = true;
                fleeTime = 10f;
                SetNavigatorSpeed(Mathf.Clamp(player.plm.realVelocity, 10f, 75f), 1.3f);
            }
        }

        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            playerInSight = false;
        }

        public override void Update()
        {
            base.Update();

            if (!playerInSight && npc.Navigator.Speed > 10f)
                SetNavigatorSpeed(10f);

            if (fleeTime > 0f)
                fleeTime -= Time.deltaTime * npc.TimeScale;
            else
                npc.behaviorStateMachine.ChangeState(new ManMemeCoin_Idling(coin));
        }
    }
}
