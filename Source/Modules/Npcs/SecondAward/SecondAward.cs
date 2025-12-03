using UnityEngine;
using UncertainLuei.CaudexLib.Components;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class SecondAward : FirstPrize
    {
        internal static Character charEnum = (Character)(-1);
        public SoundObject audBroken;

        public float stunTime = 8f;

        public void Stunned()
            => audMan.PlaySingle(audBroken);

        public void StunSound()
            => audMan.PlaySingle(audBang);

        public override void Initialize()
        {
            base.Initialize();
            behaviorStateMachine.ChangeState(new SecondAward_Active(this));
        }
    }

    public class SecondAward_Active(SecondAward award) : FirstPrize_Active(award)
    {
        private readonly SecondAward award = award;

        public override void PlayerInSight(PlayerManager player)
        {
            if (!player.HasActiveReusableEffect<SecondAwardStun>() && !player.Tagged)
            {
                base.PlayerInSight(player);
                currentPlayer = player;
            }
        }

        public override void OnStateTriggerEnter(Collider other, bool validCollision)
        {
            if (!validCollision) return;
            if (other.CompareTag("NPC"))
            {
                award.StunSound();
                other.GetComponent<NPC>().ActivateReusableEffect<SecondAwardStun>(award.stunTime);
                return;
            }
            if (!other.CompareTag("Player"))
                return;

            PlayerManager player = null;
            if (currentPlayer != null && other.transform == currentPlayer.transform)
            {
                player = currentPlayer;
                currentPlayer = null;
                ResetPath();
            }
            player ??= other.GetComponent<PlayerManager>();
            award.StunSound();
            player.ActivateReusableEffect<SecondAwardStun>(award.stunTime);
        }

        public override void OnStateTriggerExit(Collider other, bool validCollision)
        {
        }

        private void ResetPath()
        {
            currentStandardTargetPos.x = 0;
            currentStandardTargetPos.z = 0;
            currentWindowTargetPos.x = 0;
            currentWindowTargetPos.z = 0;
            if (npc.Navigator.speed <= firstPrize.wanderSpeed + 1f)
                ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));

            npc.Navigator.maxSpeed = firstPrize.wanderSpeed;
        }
    }

    public class SecondAward_Stunned(SecondAward award, float time) : FirstPrize_Stunned(award, time)
    {
        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
            if (time <= 0f)
                npc.behaviorStateMachine.ChangeState(new SecondAward_Active(award));
        }
    }

    public class SecondAwardStun : ReusableEffect
    {
        protected override bool Immune
            => EntType == EntityType.Generic || // Cannot stun Non-"NPC" Entities
            (EntType == EntityType.Npc && Npc.Character == Character.Null && Npc is Balder_Entity); // Cannot stun Balders
        protected override Sprite GaugeIcon => RecommendedCharsPlugin.AssetMan.Get<Sprite>("StatusSpr/ElectricalStun");

        private bool blinded;
        private readonly MovementModifier moveMod = new(default, 0f);

        protected override void Activated()
        {
            moveMod.movementMultiplier = 0f;
            Entity.ExternalActivity.moveMods.Add(moveMod);

            if (EntType == EntityType.Player)
            {
                Player.itm.Disable(true);
                return;
            }
            if (Npc is not FirstPrize prize)
            {
                blinded = true;
                Entity.SetBlinded(true);
                return;
            }
            StunFirstPrize(prize);
        }

        protected override void Reactivated()
        {
            if (EntType == EntityType.Npc && Npc is FirstPrize prize)
                StunFirstPrize(prize);
        }

        private void StunFirstPrize(FirstPrize prize)
        {
            NpcState state = Npc.behaviorStateMachine.CurrentState;
            if (state is FirstPrize_Stunned stunState && timeLeft > stunState.time)
            {
                stunState.time = timeLeft;
                return;
            }
            float time = prize.cutTime;
            prize.CutWires();
            prize.cutTime = time; 
        }

        protected override void ActiveUpdate()
        {
            moveMod.movementMultiplier = Mathf.Clamp(0.5f-timeLeft/SetTime, 0f, 0.5f);
        }

        protected override void Deactivated()
        {
            Entity.ExternalActivity.moveMods.Remove(moveMod);

            if (EntType == EntityType.Player)
            {
                Player.itm.Disable(false);
                return;
            }
            if (blinded)
            {
                blinded = false;
                Entity.SetBlinded(false);
            }
        }
    }
}