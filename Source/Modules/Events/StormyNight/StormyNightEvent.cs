using System.Collections;
using MTM101BaldAPI;
using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class StormyNightEvent : RandomEvent
    {
        public AudioManager audMan, rainAudMan;
        public SoundObject audRain;
        public SoundObject[] audThunder;

        public Cubemap nightSkybox;

        private Cubemap lastSky;
        private LightmapModHolder lighting;
        private LightmapMod lightmapModifier;
        private bool transitionActive, hasModifier;

        private IEnumerator stormTimer, endTransitionTimer;

        private float lightLevel, darkLightLevel = 0.15f,
            minBrightLevel = 0.5f, darkBrightLevel = 0.10f;


        public override void Initialize(EnvironmentController controller, System.Random rng)
        {
            base.Initialize(controller, rng);
            lightmapModifier = new(ChangeLightLevel, -10);
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

                rainAudMan.volumeMultiplier = 0f;
                rainAudMan.UpdateAudioDeviceVolume();
                rainAudMan.QueueAudio(audRain);
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
            endTransitionTimer = EndTransition();
            StartCoroutine(endTransitionTimer);
        }

        private IEnumerator StormTimer()
        {
            /*transitionActive = true;
            while (audMan.volumeMultiplier < 1f && lightLevel > darkLightLevel)
            {
                if (lightLevel > darkLightLevel)
                {
                    lightLevel = Mathf.Max(lightLevel - 0.5f * Time.deltaTime * ec.EnvironmentTimeScale, darkLightLevel);
                    lighting.ForceUpdateLightmap();
                }
                
                yield return null;
            }
            transitionActive = false;

            yield return new WaitForSecondsEnvironmentTimescale(ec, 0.2f);*/

            while (true)    
            {
                audMan.PlaySingle(audThunder[Random.Range(0,audThunder.Length-1)]);
                lightLevel = 3f;
                transitionActive = true;
                while (lightLevel > darkLightLevel)
                {
                    if (rainAudMan.volumeMultiplier < 1f)
                    {
                        rainAudMan.volumeMultiplier = Mathf.Min(1f, rainAudMan.volumeMultiplier + Time.deltaTime * ec.EnvironmentTimeScale);
                        rainAudMan.UpdateAudioDeviceVolume();
                    }

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

            float sign = Mathf.Sign(lightLevel);
            while (rainAudMan.volumeMultiplier > 0f && Mathf.Abs(lightLevel) < 1f)
            {
                if (Mathf.Abs(lightLevel) < 1f)
                {
                    lightLevel += sign * Time.deltaTime * ec.EnvironmentTimeScale;
                    lighting.ForceUpdateLightmap();

                    if (Mathf.Abs(lightLevel) > 0.95f) lightLevel = 1f;
                }
                rainAudMan.volumeMultiplier = Mathf.Max(0f, rainAudMan.volumeMultiplier - Time.deltaTime * ec.EnvironmentTimeScale);
                rainAudMan.UpdateAudioDeviceVolume();
                yield return null;
            }
            rainAudMan.FlushQueue(true);

            hasModifier = false;
            lighting.Remove(lightmapModifier);
        }

        private float _level, _modLevel;
        private void ChangeLightLevel(ref Color color)
        {
            _level = float.MinValue;
            for (byte i = 0; i < 3; i++)
            {
                if (color[i] >= _level)
                {
                    _level = color[i];
                }
            }

            _modLevel = _level * lightLevel;
            if (lightLevel > 1f)
            {
                // Try to not wash out the color if possible
                color *= Mathf.Min(1, _modLevel)/_level;
                return;
            }
            if (_level > minBrightLevel && _modLevel < darkBrightLevel)
            {
                color *= darkBrightLevel/_level;
                return;
            }

            color *= lightLevel;
        }
    }
}