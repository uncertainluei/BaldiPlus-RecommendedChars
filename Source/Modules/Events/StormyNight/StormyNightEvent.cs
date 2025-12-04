using System.Collections;
using MTM101BaldAPI;
using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class StormyNightEvent : RandomEvent
    {
        public AudioManager audMan;
        public SoundObject audRain;
        public SoundObject[] audThunder;

        public Cubemap nightSkybox;

        private Cubemap lastSky;
        private LightmapModHolder lighting;
        private LightmapMod lightmapModifier;
        private bool transitionActive, hasModifier;

        private IEnumerator stormTimer, endTransitionTimer;

        private float lightLevel, darkLightLevel = 0.12f,
            minBrightLevel = 0.5f, darkBrightLevel = 0.15f;


        public override void Initialize(EnvironmentController controller, System.Random rng)
        {
            base.Initialize(controller, rng);
            lightmapModifier = new(UpdateLightMap, -10);
            lighting = LightmapModHolder.GetInstance(ec);
            transitionActive = false;
        }

        public override void Begin()
        {
            base.Begin();

            if (stormTimer != null)
                StopCoroutine(stormTimer);
            if (endTransitionTimer != null)
                StopCoroutine(endTransitionTimer);
            else
            {
                lastSky = (Cubemap)Shader.GetGlobalTexture("_Skybox");
                Shader.SetGlobalTexture("_Skybox", nightSkybox);
                lightLevel = 1f;

                audMan.volumeMultiplier = 0f;
                audMan.UpdateAudioDeviceVolume();
                audMan.QueueAudio(audRain);
            }

            if (!hasModifier)
            {
                lighting.Add(lightmapModifier, false);
                hasModifier = true;
            }

            stormTimer = StormTimer();
            StartCoroutine(stormTimer);
        }

        public override void End()
        {
            base.End();

        }

        private IEnumerator StormTimer()
        {
            transitionActive = true;
            while (audMan.volumeMultiplier < 1f && lightLevel > darkLightLevel)
            {
                if (lightLevel > darkLightLevel)
                {
                    lightLevel = Mathf.Max(lightLevel - 0.5f * Time.deltaTime * ec.EnvironmentTimeScale, darkLightLevel);
                    lighting.ForceUpdateLightmap();
                }
                audMan.volumeMultiplier = Mathf.Min(1f, audMan.volumeMultiplier + Time.deltaTime * ec.EnvironmentTimeScale);
                audMan.UpdateAudioDeviceVolume();
                yield return null;
            }
            transitionActive = false;

            yield return new WaitForSecondsEnvironmentTimescale(ec, 0.2f);

            while (true)
            {
                audMan.PlaySingle(audThunder[Random.Range(0,audThunder.Length-1)]);
                lightLevel = 3f;
                transitionActive = true;
                while (lightLevel > darkLightLevel)
                {
                    lightLevel = Mathf.Max(lightLevel - 3f * Time.deltaTime * ec.EnvironmentTimeScale, darkLightLevel);
                    lighting.ForceUpdateLightmap();
                    yield return null;
                }
                transitionActive = false;
                yield return new WaitForSecondsEnvironmentTimescale(ec, Random.Range(1f, 15f));
            }
        }

        private IEnumerator EndTransition()
        {
            yield return new WaitWhile(() => transitionActive);

            if (stormTimer != null)
                StopCoroutine(stormTimer);

            Shader.SetGlobalTexture("_Skybox", lastSky);

            float sign = Mathf.Sign(lightLevel) * 2f;
            while (audMan.volumeMultiplier > 0f && Mathf.Abs(lightLevel) < 1f)
            {
                if (Mathf.Abs(lightLevel) < 0.9f)
                {
                    lightLevel -= sign * Time.deltaTime * ec.EnvironmentTimeScale;
                    lighting.ForceUpdateLightmap();

                    if (Mathf.Abs(lightLevel) > 0.95f) lightLevel = 1f;  
                }
                audMan.volumeMultiplier = Mathf.Max(0f, audMan.volumeMultiplier + Time.deltaTime * ec.EnvironmentTimeScale);
                audMan.UpdateAudioDeviceVolume();
                yield return null;
            }

            hasModifier = false;
            lighting.Remove(lightmapModifier);
        }

        private float _level;
        private void UpdateLightMap(ref Color color)
        {
            _level = Mathf.Max(color.r, color.g, color.b);
            color *= lightLevel;

            if (_level > minBrightLevel)
            {
                color.r = Mathf.Max(darkBrightLevel, color.r);
                color.g = Mathf.Max(darkBrightLevel, color.g);
                color.b = Mathf.Max(darkBrightLevel, color.b);
            }
        }
    }
}