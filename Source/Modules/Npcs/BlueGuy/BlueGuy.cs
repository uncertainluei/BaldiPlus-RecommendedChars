using System.Collections;
using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class BlueGuy : NPC
    {
        public float lookSensitivity = 0.5f, chaseTime = 10f,
            defaultCoolDown = 30f, npcFogTime = 10f, playerFogTime = 20f;

        public SpriteRenderer sprite;
        public Sprite sprNormal, sprAngry;

        public AudioManager audMan;
        public SoundObject audIntro, audLoop;

        public override void Initialize()
        {
            base.Initialize();
            behaviorStateMachine.ChangeState(new BlueGuy_Wander(this));
        }

        public void ToggleAngry(bool angry)
        {
            audMan.FlushQueue(true);
            if (angry)
            {
                sprite.sprite = sprAngry;
                audMan.QueueAudio(audIntro);
                audMan.QueueAudio(audLoop);
                audMan.loop = true;
                return;
            }
            sprite.sprite = sprNormal;
        }
    }

    public class BlueGuy_StateBase(BlueGuy bluGuy) : NpcState(bluGuy)
    {
        protected readonly BlueGuy blueGuy = bluGuy;
    }

    public class BlueGuy_Wander(BlueGuy bluGuy) : BlueGuy_StateBase(bluGuy)
    {
        private float lookTime = 0f;
        private float timeTillLastLook = 0f;

        private bool onCooldown = false;
        private float coolDown = 0f;

        public BlueGuy_Wander(BlueGuy bluGuy, float coolDown) : this(bluGuy)
        {
            onCooldown = true;
            this.coolDown = coolDown;
        }

        public override void Enter()
        {
            base.Enter();
            npc.navigationStateMachine.ChangeState(new NavigationState_WanderRandom(npc, 0));
            lookTime = 0f;
        }

        public override void InPlayerSight(PlayerManager player)
        {
            base.InPlayerSight(player);

            if (!onCooldown)
            {
                lookTime += Time.deltaTime * npc.TimeScale;
                timeTillLastLook = 0.25f;
                if (lookTime > blueGuy.lookSensitivity)
                    npc.behaviorStateMachine.ChangeState(new BlueGuy_Chasing(blueGuy, player,blueGuy.chaseTime));
            }
        }

        public override void Unsighted()
        {
            base.Unsighted();
            lookTime = 0f;
        }

        public override void Update()
        {
            base.Update();
            if (onCooldown)
            {
                coolDown -= Time.deltaTime * npc.TimeScale;
                if (coolDown <= 0f)
                    onCooldown = false;
                return;
            }

            if (timeTillLastLook > 0f)
            {
                timeTillLastLook -= Time.deltaTime * npc.TimeScale;
                if (timeTillLastLook <= 0f)
                    lookTime = 0f;
            }
            
        }
    }

    public class BlueGuy_Chasing : BlueGuy_StateBase
    {
        private readonly PlayerManager targetPlayer;
        private PlayerManager currentPlayer;
        private float chaseTime;
        private readonly NavigationState_TargetPlayer targetState;

        public BlueGuy_Chasing(BlueGuy bluGuy, PlayerManager player, float chaseTime) :
            base(bluGuy)
        {
            targetPlayer = player;
            this.chaseTime = chaseTime;
            targetState = new NavigationState_TargetPlayer(npc, 63, player.transform.position);
        }

        public override void Enter()
        {
            base.Enter();
            currentPlayer = targetPlayer;
            blueGuy.ToggleAngry(true);
            ChangeNavigationState(targetState);
        }

        public override void Exit()
        {
            base.Exit();
            blueGuy.ToggleAngry(false);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            if (player == currentPlayer)
            {
                currentPlayer = null;
                ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
            }
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);

            if (targetPlayer != currentPlayer || player == currentPlayer)
            {
                currentPlayer = player;
                ChangeNavigationState(targetState);
                targetState.UpdatePosition(player.transform.position);
            }
        }

        public override void Update()
        {
            base.Update();
            chaseTime -= Time.deltaTime * npc.TimeScale;
            if (chaseTime <= 0f)
                npc.behaviorStateMachine.ChangeState(new BlueGuy_Wander(blueGuy, blueGuy.defaultCoolDown));
        }

        public override void OnStateTriggerEnter(Collider other, bool canCollide)
        {
            base.OnStateTriggerEnter(other, canCollide);
            if (other.CompareTag("Player"))
            {
                npc.behaviorStateMachine.ChangeState(new BlueGuy_Wander(blueGuy, blueGuy.defaultCoolDown));
                if (canCollide)
                    other.GetComponent<PlayerManager>().ActivateReusableEffect<BlueGuyFog>(blueGuy.playerFogTime);
                return;
            }
            if (other.CompareTag("NPC") && canCollide)
                other.GetComponent<NPC>().ActivateReusableEffect<BlueGuyFog>(blueGuy.npcFogTime);
        }
    }

    public class BlueGuyFog : ReusableEffect
    {
        protected override bool Immune
            => EntType == EntityType.Generic || // Cannot blind Non-"NPC" Entities
            (EntType == EntityType.Npc && Npc.looker == null); // Cannot blind NPCs without lookers
        protected override Sprite GaugeIcon => RecommendedCharsPlugin.AssetMan.Get<Sprite>("StatusSpr/BlueGuyFog");

        private Fog FogPrefab => RecommendedCharsPlugin.ObjMan.Get<Fog>("Fog/BlueGuyFog");
        private Fog fog = null;
        private Coroutine fogClearRoutine;

        protected override void Activated()
        {
            if (EntType != EntityType.Player)
            {
                Entity.SetBlinded(true);
                return;
            }

            if (fog == null)
            {
                Fog fog = FogPrefab;
                this.fog = new()
                {
                    color = fog.color,
                    startDist = fog.startDist,
                    maxDist = fog.maxDist,
                    priority = fog.priority
                };
            }
            else if (fogClearRoutine != null)
            {
                StopCoroutine(fogClearRoutine);
                fogClearRoutine = null;
            }
            fog.strength = FogPrefab.strength;
            Player.ec.AddFog(fog);
        }

        protected override void Reactivated()
        {
            if (EntType == EntityType.Player)
                fog.strength = FogPrefab.strength;
        }

        protected override void ActiveUpdate()
        {
        }

        protected override void Deactivated()
        {
            if (EntType != EntityType.Player)
            {
                Entity.SetBlinded(false);
                return;
            }
            fogClearRoutine = StartCoroutine(FadeOffFog());
        }

        private IEnumerator FadeOffFog()
        {
            fog.strength = FogPrefab.strength;
            Player.ec.UpdateFog();
            while (fog.strength > 0f)
            {
                fog.strength -= 0.5f * Time.deltaTime;
                Player.ec.UpdateFog();
                yield return null;
            }
            fog.strength = 0f;
            Player.ec.RemoveFog(fog);
            fogClearRoutine = null;
        }
    }
}