using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class Painting : MonoBehaviour, IClickable<int>
    {
        public SpriteRenderer sprite;
        public SoundObject audShatter;
        public PaintingParticle particlePre;

        public void Clicked(int player)
        {
            PlayerManager pm = CoreGameManager.Instance.GetPlayer(player);
            CoreGameManager.Instance.audMan.PlaySingle(audShatter);

            if (BaseGameManager.Instance is PartyWinManager partyWin)
                partyWin.PaintingTouched(pm);

            // Anger Baldi and make noise as if the player got an activity incorrect
            BaseGameManager.Instance.AngerBaldi(1f);
            pm.ec.MakeNoise(transform.position, 126);

            for (int c = Random.Range(10,16); c > 0; c--) //10-15 shards
                Instantiate(particlePre, transform.parent).Initialize(transform.position, sprite.sprite, pm.ec);
            Destroy(gameObject);
        }

        public void ClickableSighted(int _) {}
        public void ClickableUnsighted(int _) {}

        public bool ClickableHidden() => false;
        public bool ClickableRequiresNormalHeight() => true;
    }
}