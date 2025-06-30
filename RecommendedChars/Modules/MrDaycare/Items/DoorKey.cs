using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_DoorKey : Item
    {
        public ItemObject nextStage;
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

            foreach (Items itm in keyEnums)
            {
                foreach (IItemAcceptor itemAcceptor in _acceptors)
                {
                    if (itemAcceptor == null) break;
                    if (!itemAcceptor.ItemFits(itm)) continue;

                    if (nextStage)
                    {
                        pm.itm.SetItem(nextStage, pm.itm.selectedItem);
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
