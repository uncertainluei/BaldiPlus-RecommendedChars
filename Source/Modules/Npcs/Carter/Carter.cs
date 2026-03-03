using System.Collections;
using System.Collections.Generic;
using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class Carter : NPC
    {
        public SpriteRenderer sprite;
        public Sprite sprNormal, sprAngry, sprScreech;

        public AudioManager audMan;
        public SoundObject[] audLost, audItms, audHelp, audLeave;
        public SoundObject audCoords, audThanks, audIntro, audLoop;

        public ItemObject[] possibleItems;

        public override void Initialize()
        {
            base.Initialize();
            behaviorStateMachine.ChangeState(new Carter_Wander(this));
        }
    }

    public class Carter_StateBase(Carter carter) : NpcState(carter)
    {
        protected readonly Carter carter = carter;
    }

    public class Carter_Wander(Carter carter) : Carter_StateBase(carter)
    {

        public override void Enter()
        {
            base.Enter();
            npc.navigationStateMachine.ChangeState(new NavigationState_WanderRandom(npc, 0));
        }
    }
}