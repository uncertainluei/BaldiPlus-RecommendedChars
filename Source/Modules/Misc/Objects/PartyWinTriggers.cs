using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PartyLiftTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            gameObject.SetActive(false);
            BaseGameManager.Instance.CallSpecialManagerFunction(0, other.gameObject);
        }
    }

    public class PartyBlowTrigger : MonoBehaviour, IClickable<int>
    {
        public bool ClickableHidden() => false;
        public bool ClickableRequiresNormalHeight() => false;
        public void ClickableSighted(int player) {}

        public void ClickableUnsighted(int player) {}

        public void Clicked(int player)
            => BaseGameManager.Instance.CallSpecialManagerFunction(1, gameObject);
    }
}
