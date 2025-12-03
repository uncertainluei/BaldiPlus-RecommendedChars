using MidiPlayerTK;
using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.FragileWindows
{
    internal class DaveWindowlet : ExtWindowletVariant
    {
        private static readonly Dictionary<int, bool> percussionStates = new()
        {
            {27,true}, {28,false}, {29,false}, {30,true}, {31,true},
            {33,false}, {34,true}, {35,false}, {36,false}, {37,true},
            {38,false}, {40,true}, {41,false}, {42,true}, {43,false},
            {44,true}, {45,false}, {46,true}, {47,false}, {48,false},
            {49,true}, {50,false}, {51,true}, {52,true}, {53,true},
            {54,true}, {55,true}, {56,true}, {57,true}, {59,true},
            {60,true}, {61,true}, {62,false}, {63,false}, {64,false},
            {65,true}, {66,false}, {67,true}, {68,true}, {71,true},
            {72,true}, {75,true}, {76,true}, {77,false}, {78,true},
            {79,false}, {83,true}, {85,true}, {86,false}, {87,false},
        };

        internal static Sprite sprHi, sprLo;

        private bool posing;
        private float poseTime;

        private SpriteRenderer spriteRenderer;
        private bool eventsEnabled;

        protected override void Initialized()
        {
            base.Initialized();
            spriteRenderer = Windowlet.spriteRenderer[0];

            eventsEnabled = true;
            MusicManager.OnMidiEvent += MidiEvent;
            Baldi.OnBaldiSlap += BaldiSlap;
        }

        private void MidiEvent(MPTKEvent midiEvent)
        {
            if (midiEvent.Channel != 9) return; // Only sounds from channel 10 are allowed
            if (!percussionStates.ContainsKey(midiEvent.Value)) return;

            posing = true;
            poseTime = Mathf.Max(0.15f, midiEvent.Length * midiEvent.MPTK_DeltaTimeMillis * 0.002f);
            spriteRenderer.sprite = percussionStates[midiEvent.Value] ? sprHi : sprLo;
        }
        private void BaldiSlap(Baldi bal)
        {
            if (bal.behaviorStateMachine.CurrentState is Baldi_Chase_Broken ||
                bal.behaviorStateMachine.CurrentState is Baldi_Chase_Tutorial) return;

            float num = Vector3.Distance(bal.transform.position, transform.position);
            if (num > 100f) return;

            posing = true;
            poseTime = 0.4f;
            spriteRenderer.sprite = (num < 50f) ? sprHi : sprLo;
        }

        public override void WanderUpdate() => PoseUpdate();
        public override void HeldUpdate() => PoseUpdate();

        private void PoseUpdate()
        {
            if (!posing) return;

            poseTime -= Time.deltaTime;
            if (poseTime <= 0f)
            {
                posing = false;
                spriteRenderer.sprite = Windowlet.me.mainSprite;
            }
        }

        private void DisableEvents()
        {
            if (eventsEnabled)
            {
                MusicManager.OnMidiEvent -= MidiEvent;
                Baldi.OnBaldiSlap -= BaldiSlap;
            }
            eventsEnabled = false;
        }
        private void OnDisable() => DisableEvents();

        public override void Throw(PlayerManager player) => DisableEvents();
        public override bool Shatter()
        {
            DisableEvents();
            return true;
        }
    }
}
