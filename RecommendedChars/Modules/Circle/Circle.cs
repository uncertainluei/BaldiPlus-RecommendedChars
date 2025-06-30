using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    class CircleNpc : Playtime
    {
        public SpriteRenderer sprite;
        public Sprite sprNormal;
        public Sprite sprSad;

        internal static Character charEnum = (Character)(-1);
    }

    class CircleJumprope : Jumprope
    {
        private const string startKey = "Hud_RecChars_CircleRope_Start";
        private const string continueKey = "Hud_RecChars_CircleRope_Continue";
        private const string failKey = "Hud_RecChars_CircleRope_Fail";

        internal static Dictionary<string, Sprite[]> ropeAnimation;
        public CustomSpriteAnimator ropeAnimator;

        private new void Start()
        {
            base.Start();

            // Stop the RopeTimer routine
            StopAllCoroutines();

            animator.enabled = false;

            ropeDelay = 0f;
            ropeAnimator.PopulateAnimations(ropeAnimation, 15);

            countTmp.text = $"{jumps}/{LocalizationManager.Instance.GetLocalizedText(startKey)}";
            StartCoroutine(CircleRopeTimer());
        }

        private void CircleRopeDown()
        {
            ropeDelay = 0f;

            if (height > jumpBuffer)
            {
                jumps++;

                if (jumps < 10)
                    playtime.Count(jumps);

                countTmp.text = $"{jumps}/{LocalizationManager.Instance.GetLocalizedText(continueKey)}";
                return;
            }

            playtime.ec.MakeNoise(playtime.transform.position, noiseValue);
            jumps = 0;
            ropeDelay = 2f;
            playtime.JumpropeHit();

            totalPoints += penaltyVal;
            if (totalPoints < 20)
                totalPoints = 20;

            countTmp.text = $"{jumps}/{LocalizationManager.Instance.GetLocalizedText(failKey)}";
        }

        private IEnumerator CircleRopeTimer()
        {
            while (jumps < maxJumps)
            {
                float delay = ropeDelay;
                while (delay > 0f)
                {
                    delay -= Time.deltaTime;
                    yield return null;
                }

                ropeAnimator.Play("JumpRope", 1F/ropeTime);
                float hitTime = ropeTime;
                while (hitTime > 0f)
                {
                    hitTime -= Time.deltaTime;
                    yield return null;
                }

                CircleRopeDown();
            }

            while (height > 0f)
            {
                yield return null;
            }

            End(success: true);
        }
    }
}