using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class CharacterConfusionEvent : RandomEvent
    {
        private static CharacterConfusionEvent _instance;

        private Dictionary<CharacterShuffleData, CharacterShuffleData> npcSwaps = [];

        public override void Begin()
        {
            if (_instance && _instance != this)
            {
                base.End();
                Destroy(gameObject);
                throw new System.InvalidOperationException("The same CharacterConfusionEvent can't occupy the same space at the same time.");
            }

            base.Begin();
            _instance = this;

            List<NPC> npcs = new(ec.Npcs);
            npcs.RemoveAll(x => !x || x.spriteRenderer == null || x.spriteRenderer.Length == 0 || !x.spriteRenderer[0] ||
                x.GetMeta()?.tags.Contains("recchars:character_confusion_blacklist") != false);
            List<NPC> npcsToSwap = new(npcs);
            while (npcsToSwap.Count > 0)
            {
                NPC a = npcsToSwap[crng.Next(npcsToSwap.Count)];
                npcsToSwap.Remove(a);
                CharacterShuffleData shuffledA = a.gameObject.AddComponent<CharacterShuffleData>();
                shuffledA.Npc = a;
                shuffledA.SpriteParent = a.spriteRenderer[0].transform.parent;

                NPC b = null;
                CharacterShuffleData shuffledB = null;
                //swap twice with a random NPC if we are out
                if (npcsToSwap.Count == 0)
                {
                    do b = npcs[crng.Next(npcs.Count)]; while (b == a);
                    shuffledB = b.GetComponent<CharacterShuffleData>();
                    npcSwaps[shuffledB] = shuffledA;
                }
                else
                {
                    b = npcsToSwap[crng.Next(npcsToSwap.Count)];
                    npcsToSwap.Remove(b);

                    shuffledB = b.gameObject.AddComponent<CharacterShuffleData>();
                    shuffledB.Npc = b;
                    shuffledB.SpriteParent = b.spriteRenderer[0].transform.parent;
                    shuffledB.ToSwap = shuffledA;
                    npcSwaps.Add(shuffledB, shuffledA);
                }
                
                npcSwaps.Add(shuffledA, shuffledB);
                shuffledA.ToSwap = shuffledB;
            }
            foreach (var toSwap in npcSwaps.Keys)
                toSwap.SwapSprites();
        }

        public override void End()
        {
            base.End();
            if (_instance != this)
            {
                Destroy(gameObject);
                throw new System.InvalidOperationException("The same CharacterConfusionEvent can't occupy the same space at the same time.");
            }

            _instance = null;
            foreach (var pair in npcSwaps.Keys)
                if (pair) // Character appearance destroyed midway (i.e. NPC despawn) will be ignored
                   DestroyImmediate(pair);

            npcSwaps.Clear();
        }

        private void OnDestroy()
        {
            if (_instance == this && Active)
                End();
        }
    }

    public class CharacterShuffleData : MonoBehaviour
    {
        public NPC Npc {get; internal set;}
        public CharacterShuffleData ToSwap {get; internal set;}
        public Transform SpriteParent {get; internal set;}

        // Unlike Dark Magic Pack, this is used to just copy the subtitle color
        //public AudioManager AudioManagerToCopy {get; internal set;}

        public virtual void SwapSprites()
        {
            for (int i = 0; i < Npc.spriteRenderer.Length; i++)
                if (Npc.spriteRenderer[i] && Npc.spriteRenderer[i].transform.parent == SpriteParent)
                    Npc.spriteRenderer[i].transform.SetParent(ToSwap.SpriteParent, false);
        }

        public virtual void Revert()
        {
            enabled = false;
            for (int i = 0; i < Npc.spriteRenderer.Length; i++)
                if (Npc.spriteRenderer[i] && Npc.spriteRenderer[i].transform.parent == ToSwap.SpriteParent)
                    Npc.spriteRenderer[i].transform.SetParent(SpriteParent, false);
        }

        private void OnDestroy()
        {
            if (enabled)
            {
                Revert();
                if (ToSwap && ToSwap.enabled && ToSwap.ToSwap == this)
                    DestroyImmediate(ToSwap);
            }
        }
    }
}