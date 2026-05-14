using System.Collections;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_BaldiCake : Item
    {
        public SoundObject audEat;
        public Sprite gaugeSprite;
        public float time = 10f;

        private readonly MovementModifier eatMoveMod = new(new(), 0.5f);
        private readonly ValueModifier speedMul = new(1.6f), staminaDropMul = new(2f), staminaRiseMul = new(0.6f);

        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;
            CoreGameManager.Instance.audMan.PlaySingle(audEat);
            DaycareGuiltManager.GetInstance(pm).BreakRule("Eating", 0.8f, 0.25f);
            StartCoroutine(EatRoutine());
            return true;
        }

        private IEnumerator EatRoutine()
        {
            pm.Am.moveMods.Add(eatMoveMod);
            
            for (float t = 2; t > 0f; t -= Time.deltaTime * pm.PlayerTimeScale)
                yield return null;

            if (pm.plm.stamina < pm.plm.StaminaMax)
                pm.plm.AddStamina(pm.plm.StaminaMax*0.5f, true);

            pm.Am.moveMods.Remove(eatMoveMod);
            PlayerMovementStatModifier stats = pm.GetComponent<PlayerMovementStatModifier>();
            stats.AddModifier("runSpeed", speedMul);
            stats.AddModifier("staminaDrop", staminaDropMul);
            stats.AddModifier("staminaRise", staminaRiseMul);
            HudGauge gauge = CoreGameManager.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, time);
            for (float t = time; t > 0f; t -= Time.deltaTime * pm.PlayerTimeScale)
            {
                gauge.SetValue(time, t);
                yield return null;
            }
            stats.RemoveModifier(speedMul);
            stats.RemoveModifier(staminaDropMul);
            stats.RemoveModifier(staminaRiseMul);
            gauge.Deactivate();
            Destroy(gameObject);
        }
    }
}
