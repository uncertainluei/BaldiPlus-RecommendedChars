using System;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class LockSock : NPC
    {
        public override void Initialize()
        {
            base.Initialize();
            behaviorStateMachine.ChangeState(new NpcState(this));
            behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0));
        }
    }
}