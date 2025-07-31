namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_Mangles : Item
    {
        public SoundObject audEat;

        public override bool Use(PlayerManager pm)
        {
            CoreGameManager.Instance.audMan.PlaySingle(audEat);
            DaycareGuiltManager.TryBreakRule(pm, "Eating", 0.8f, 0.25f);
            pm.plm.stamina += 50f;
            Destroy(gameObject);
            return true;
        }
    }
}
