using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class Noongus : NPC
    {
        public AudioManager audMan;
        public SoundObject audIdle, audSpotted, audThrow;

        public SpriteRenderer sprite;
        public Sprite sprIdle, sprSpot, sprThrow;

        public ITM_NanaPeel[] brickPre;

        public Vector2 brickThrowSpeedRange = new(20f, 50f);
        public float coolDown = 20f, idleSoundChance = 0.09f;

        internal DijkstraMap dijkstraMap;

        public override void Initialize()
        {
            base.Initialize();
            dijkstraMap = new(ec, PathType.Nav, 5, transform);
            behaviorStateMachine.ChangeState(new Noongus_Wander(this));
        }

        private float idleSoundDelay = 1f;
        public void IdleSoundChance()
        {
            idleSoundDelay -= Time.deltaTime * TimeScale;
            if (idleSoundDelay > 0f || audMan.AnyAudioIsPlaying) return;

            idleSoundDelay = 1f;
            if (Random.value <= idleSoundChance)
                audMan.PlaySingle(audIdle);
        }

        internal IEnumerator SpeakSpriteRoutine()
        {
            sprite.sprite = sprSpot;
            while (audMan.AnyAudioIsPlaying)
            {
                if (sprite.sprite != sprSpot)
                    yield break;

                yield return null;
            }
            sprite.sprite = sprThrow;
        }

        public void ThrowBricks(Vector3 forward)
        {
            audMan.PlaySingle(audThrow);
            Vector3 right = new(forward.z, 0, forward.x);

            List<Collider> colliders = [];
            int brickCount = Random.Range(4, 10);

            dijkstraMap.targets.Clear();
            dijkstraMap.targetPosition = new IntVector2[brickCount];

            for (int i = 0; i < brickCount; i++)
            {
                ITM_NanaPeel brick = Instantiate(brickPre[Random.Range(0, brickPre.Length)], ec.transform);
                brick.Spawn(ec, transform.position + right*4f*(Random.value - 0.5f),
                    (forward + right*0.5f*(Random.value - 0.5f)).normalized, Random.Range(brickThrowSpeedRange.x, brickThrowSpeedRange.y));
                dijkstraMap.targets.Add(brick.transform);

                foreach (Collider collider in colliders)
                {
                    Physics.IgnoreCollision(collider, brick.entity.collider);
                    Physics.IgnoreCollision(collider, brick.entity.trigger);
                }
                colliders.Add(brick.entity.collider);
                colliders.Add(brick.entity.trigger);
            }
        }
    }

    public class Noongus_StateBase(Noongus noon) : NpcState(noon)
    {
        protected readonly Noongus noon = noon;
    }

    public class Noongus_Wander(Noongus noon, float coolDown = 0f) : Noongus_StateBase(noon)
    {
        private float coolDown = coolDown;

        public override void Enter()
        {
            base.Enter();
            noon.sprite.sprite = noon.sprIdle;
            ChangeNavigationState(new NavigationState_WanderRandom(noon, 0));
        }

        public override void Update()
        {
            base.Update();
            if (coolDown > 0f)
            {
                coolDown -= Time.deltaTime * noon.TimeScale;
                return;
            }
            noon.IdleSoundChance();
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (coolDown <= 0f)
                noon.behaviorStateMachine.ChangeState(new Noongus_PlayerSpotted(noon, player));
        }
    }

    public class Noongus_PlayerSpotted(Noongus noon, PlayerManager player) : Noongus_StateBase(noon)
    {
        private PlayerManager player = player;
        private Vector3 targetPos = player.transform.position;
        private NavigationState_TargetPlayer targetState = new(noon, 63, player.transform.position, false);

        public override void Initialize()
        {
            base.Initialize();
            noon.audMan.PlaySingle(noon.audSpotted);
            noon.StartCoroutine(noon.SpeakSpriteRoutine());
            noon.dijkstraMap.targets.Clear();
            noon.dijkstraMap.targets.Add(noon.transform);
            noon.dijkstraMap.targetPosition = [new()];
        }

        public override void Enter()
        {
            base.Enter();
            targetPos = player.transform.position;
            ChangeNavigationState(targetState);
        }

        public override void Update()
        {
            base.Update();
            if (player.dijkstraMap.Value(IntVector2.GetGridPosition(noon.transform.position)) <= 2)
                noon.behaviorStateMachine.ChangeState(new Noongus_ThrowBricks(noon, targetPos));
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            this.player = player;
            targetPos = player.transform.position;
            targetState.UpdatePosition(targetPos);
        }

        public override void PlayerLost(PlayerManager player)
        {
            base.PlayerLost(player);
            noon.behaviorStateMachine.ChangeState(new Noongus_Wander(noon));
        }
    }

    public class Noongus_ThrowBricks(Noongus noon, Vector3 targetPos) : Noongus_StateBase(noon)
    {
        private Vector3 direction = targetPos;
        private bool bricksThrown = false;
        private float time = 2f;

        public override void Initialize()
        {
            base.Initialize();
            direction = Directions.DirFromVector3(direction-noon.transform.position, 45).ToVector3();
            ChangeNavigationState(new NavigationState_DoNothing(noon, 0));
        }

        public override void Resume()
        {
            base.Resume();
            noon.behaviorStateMachine.ChangeState(new Noongus_Wander(noon));
        }

        public override void Update()
        {
            base.Update();
            if (!bricksThrown)
            {
                if (noon.audMan.AnyAudioIsPlaying)
                    return;

                noon.ThrowBricks(direction);
                bricksThrown = true;
            }
            time -= Time.deltaTime * noon.TimeScale;
            if (time <= 0f)
                noon.behaviorStateMachine.ChangeState(new Noongus_Flee(noon));
        }
    }

    public class NavigationState_WanderFleeTriggerState(NPC npc, int priority, DijkstraMap dijkstraMap) : NavigationState_WanderFlee(npc, priority, dijkstraMap)
    {
        public override void DestinationEmpty() => npc.behaviorStateMachine.CurrentState.DestinationEmpty();
    }

    public class Noongus_Flee(Noongus noon) : Noongus_StateBase(noon)
    {
        public override void Enter()
        {
            base.Enter();
            noon.sprite.sprite = noon.sprIdle;
            ChangeNavigationState(new NavigationState_WanderFleeTriggerState(noon, 0, noon.dijkstraMap));
        }

        private void Wander() => noon.behaviorStateMachine.ChangeState(new Noongus_Wander(noon, noon.coolDown));

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            Wander();
        }

        public override void Resume()
        {
            base.Resume();
            Wander();
        }
    }
}