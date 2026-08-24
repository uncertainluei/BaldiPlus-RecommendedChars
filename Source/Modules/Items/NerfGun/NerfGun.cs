namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_NerfGun : Item
    {
        public override bool Use(PlayerManager pm)
        {
            Destroy(gameObject);

            if (pm.jumpropes.Count == 0) return false;

            bool used = false;
            for (int i = pm.jumpropes.Count-1; i >= 0; i--)
            {
                if (pm.jumpropes[i] is CircleJumprope)
                {
                    used = true;
                    pm.jumpropes[i].End(false);
                }
            }
            
            return used;
        }
    }
}
