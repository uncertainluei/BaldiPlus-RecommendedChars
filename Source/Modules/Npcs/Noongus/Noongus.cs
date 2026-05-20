using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class Noongus : NPC
    {
        public AudioManager audMan;
        public SoundObject audIdle, audSpotted, audThrow;

        public SpriteRenderer sprite;
        public Sprite sprIdle, sprSpot, sprThrow;

        public override void Initialize()
        {
            behaviorStateMachine.ChangeState(new Noongus_Wander(this));
        }
    }

    public class Noongus_StateBase(Noongus noon) : NpcState(noon)
    {
        protected readonly Noongus noon = noon;
    }

    public class Noongus_Wander(Noongus noon) : Noongus_StateBase(noon)
    {
        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_WanderRandom(noon, 0));
        }
    }

    public class Noongus_PlayerSpotted(Noongus noon) : Noongus_StateBase(noon)
    {
        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_DoNothing(noon, 63, false));
        }
    }
}