using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_DoorKey : Item
    {
        public LayerMaskObject layerMask;
        internal static Items[] keyEnums;

        private RaycastHit _hit;
        private IItemAcceptor[] _acceptors;

        public override bool Use(PlayerManager pm)
        {
            Destroy(gameObject);
            if (!Physics.Raycast(pm.transform.position, CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward, out _hit, pm.pc.reach, layerMask.mask))
                return false;

            _acceptors = _hit.transform.GetComponents<IItemAcceptor>();
            foreach (IItemAcceptor itemAcceptor in _acceptors)
            {
                foreach (Items itm in keyEnums)
                {
                    if (!itemAcceptor.ItemFits(itm)) continue;
                    itemAcceptor.InsertItem(pm, pm.ec);
                    return true;
                }
            }
            return false;
        }
    }
}
