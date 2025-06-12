namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_Mangles : Item
    {
        public ItemObject nextStage;
        public SoundObject audEat;

        public override bool Use(PlayerManager pm)
        {
            CoreGameManager.Instance.audMan.PlaySingle(audEat);
            DaycareGuiltManager.TryBreakRule(pm, "Eating", 0.8f, 0.125f);
            pm.plm.stamina += 50f;

            if (nextStage)
            {
                pm.itm.SetItem(nextStage, pm.itm.selectedItem);
                return false;
            }
            return true;
        }
    }
}
