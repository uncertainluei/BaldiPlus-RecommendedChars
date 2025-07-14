using MidiPlayerTK;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.FragileWindows
{
    internal class DaveWindowlet : ExtWindowletVariant
    {
        internal static Sprite sprHi, sprLo;

        private bool posing;
        private float poseTime;

        private SpriteRenderer spriteRenderer;
        private bool hasMidiEvent;

        protected override void Initialized()
        {
            base.Initialized();
            spriteRenderer = Windowlet.spriteRenderer[0];

            hasMidiEvent = true;
            MusicManager.OnMidiEvent += MidiEvent;
        }

        private void MidiEvent(MPTKEvent midiEvent)
        {
            if (midiEvent.Channel != 9) return; // Is not channel 10
            RecommendedCharsPlugin.Log.LogInfo($"Event index {midiEvent.Value}, time {midiEvent.Length}");
        }

        public override void WanderUpdate() => PoseUpdate();
        public override void HeldUpdate() => PoseUpdate();

        private void PoseUpdate()
        {
        }

        private void DisableMidiEvent()
        {
            if (hasMidiEvent)
                MusicManager.OnMidiEvent -= MidiEvent;
            hasMidiEvent = false;
        }
        private void OnDisable() => DisableMidiEvent();

        public override void Throw(PlayerManager player) => DisableMidiEvent();
        public override bool Shatter()
        {
            RecommendedCharsPlugin.Log.LogInfo("SHATTER");
            Windowlet.ec.MakeNoise(transform.position, 120);
            DisableMidiEvent();
            return true;
        }
    }
}
