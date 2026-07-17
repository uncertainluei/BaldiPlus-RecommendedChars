using System.Collections;
using System.Collections.Generic;
using System.IO;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using UncertainLuei.CaudexLib.Util;
using UnityCipher;
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

    public class ITM_PartySecretTape : Item
    {
        private RaycastHit hit;
        internal static Items itemEnum;
        internal static SoundObject speech;

        public override bool Use(PlayerManager pm)
        {
            Destroy(gameObject);
            if (!Physics.Raycast(pm.transform.position, CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.Reach, pm.pc.ClickLayers))
                return false;
                
            IItemAcceptor component = hit.transform.GetComponent<IItemAcceptor>();
            if (component is not TapePlayer || !component.ItemFits(Items.Tape)) return false;
            TapePlayer tape = (TapePlayer)component;
            tape.StartCoroutine(SecretMessage(tape));
            return true;
        }

        private static IEnumerator SecretMessage(TapePlayer tape)
        {
            tape.audMan.FlushQueue(true);
            tape.audMan.PlaySingle(tape.audInsert);
            tape.audMan.usesVfx = true;
            tape.audMan.audioSourceManager.vfxSource.enabled = true;
            
            if (tape.changeOnUse)
                tape.spriteToChange.sprite = tape.usedSprite;

            # if DEBUG
            string path = Path.Combine(AssetLoader.GetModPath(RecommendedCharsPlugin.Plugin), "Audio", "Sfx", "SecretMessage.ogg");
            if (!File.Exists(path+"_enc"))
            {
                byte[] data = RijndaelEncryption.Encrypt(File.ReadAllBytes(path), "1999");
                File.WriteAllBytes(path+"_enc", data);
            }
            #endif

            if (!speech)
            {
                RecommendedCharsPlugin.Log.LogError("Tried playing the secret message when it doesn't exist. You are not slick, buddy.");
                yield break;
            }
            if (!speech.soundClip)
            {
                string pathToDecrypt = Path.Combine(AssetLoader.GetModPath(RecommendedCharsPlugin.Plugin), "Audio", "Sfx", "SecretMessage.ogg_enc");
                speech.soundClip = RijndaelEncryption.Decrypt(File.ReadAllBytes(pathToDecrypt), "1999").ToAudioClip("SecretMessage", AudioType.OGGVORBIS);
                yield return new WaitForEndOfFrame();
            }
            tape.audMan.PlaySingle(speech);
            yield return new WaitWhile(() => tape.audMan.AnyAudioIsPlaying);

            GlobalCam.Instance.SetListener(true);
            SubtitleManager.Instance.DestroyAll();
            CoreGameManager.Instance.ReturnToMenu();
        }
    }
}
