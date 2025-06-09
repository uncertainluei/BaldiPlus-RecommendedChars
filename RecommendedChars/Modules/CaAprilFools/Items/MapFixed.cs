using System;
using System.Collections.Generic;
using System.Text;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_MapFixed : ITM_Map
    {
        public SoundObject ding;

        public override bool Use(PlayerManager pm)
        {
            CoreGameManager.Instance.audMan.PlaySingle(ding);
            pm.ec.map.CompleteMap();
            Destroy(gameObject);
            return true;
        }
    }
}
