using System;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public enum EntityType : byte
    {
        Generic,
        Player,
        Npc
    }

    public static class ReusableEffectHelper
    {
        private static Type _lastEffectType;
        private static GameObject _lastObject;
        private static ReusableEffect _lastInstance;

        private static bool TryGetReusableEffect<T>(GameObject obj, out T effect) where T : ReusableEffect
        {
            Type type = typeof(T);
            if (_lastEffectType == type && _lastObject == obj && _lastInstance != null)
            {
                effect = (T)_lastInstance;
                return true;
            }
            if (obj.TryGetComponent(out effect))
            {
                _lastEffectType = type;
                _lastObject = obj;
                _lastInstance = effect;
                return true;
            }
            return false;
        }

        private static bool HasActiveReusableEffect<T>(GameObject obj) where T : ReusableEffect
            => TryGetReusableEffect(obj, out T effect) && effect.Active;

        public static bool HasActiveReusableEffect<T>(this PlayerManager plyr) where T : ReusableEffect
            => HasActiveReusableEffect<T>(plyr.gameObject);
        public static bool HasActiveReusableEffect<T>(this NPC npc) where T : ReusableEffect
            => HasActiveReusableEffect<T>(npc.gameObject);
        public static bool HasActiveReusableEffect<T>(this Entity ent) where T : ReusableEffect
            => HasActiveReusableEffect<T>(ent.gameObject);

        private static T GetOrAddReusableEffect<T>(GameObject obj, EntityType type, Entity ent = null, PlayerManager plyr = null, NPC npc = null) where T : ReusableEffect
        {
            if (TryGetReusableEffect<T>(obj, out T effect))
                return effect;

            _lastEffectType = typeof(T);
            _lastObject = obj;
            _lastInstance = obj.AddComponent<T>();
            switch (type)
            {
                default:
                    _lastInstance.AssignToEntity(ent);
                    break;
                case EntityType.Player:
                    _lastInstance.AssignToPlayer(plyr, ent);
                    break;
                case EntityType.Npc:
                    _lastInstance.AssignToNpc(npc, ent);
                    break;
            }
            return (T)_lastInstance;
        }

        public static bool ActivateReusableEffect<T>(this PlayerManager plyr, float time) where T : ReusableEffect
            => GetOrAddReusableEffect<T>(plyr.gameObject, EntityType.Player, null, plyr).Activate(time);
        public static bool ActivateReusableEffect<T>(this NPC npc, float time) where T : ReusableEffect
            => GetOrAddReusableEffect<T>(npc.gameObject, EntityType.Npc, null, null, npc).Activate(time);
        public static bool ActivateReusableEffect<T>(this Entity ent, float time) where T : ReusableEffect
            => GetOrAddReusableEffect<T>(ent.gameObject, EntityType.Generic, ent).Activate(time);

        private static void DeactivateReusableEffect<T>(GameObject obj) where T : ReusableEffect
        {
            if (TryGetReusableEffect<T>(obj, out T effect))
                effect.Deactivate();
        }

        public static void RemoveReusableEffect<T>(this PlayerManager plyr, float time) where T : ReusableEffect
            => DeactivateReusableEffect<T>(plyr.gameObject);
        public static void RemoveReusableEffect<T>(this NPC npc, float time) where T : ReusableEffect
            => DeactivateReusableEffect<T>(npc.gameObject);
        public static void RemoveReusableEffect<T>(this Entity ent, float time) where T : ReusableEffect
            => DeactivateReusableEffect<T>(ent.gameObject);
    }

    public abstract class ReusableEffect : MonoBehaviour
    {
        // Effect duration
        public bool Active { get; private set; } = false;
        protected float SetTime { get; private set;}
        protected float timeLeft;

        // Properties
        protected virtual bool Immune => false;
        protected virtual Sprite GaugeIcon => null;

        // Entity properties
        protected EntityType EntType { get; private set; }

        protected Entity Entity { get; private set; }
        protected PlayerManager Player { get; private set; }
        protected NPC Npc { get; private set; }

        // Gauges (for players with HUDs)
        private HudGaugeManager gaugeMan;
        private HudGauge gauge;

        private bool _valid = false;

        // Ran on the first frame after its creation, so this is a failsave incase stuff goes HORRIBLY wrong
        private void Start()
        {
            if (!_valid)
            {
                RecommendedCharsPlugin.Log.LogError($"Status effect {GetType().FullName} could not be applied to Object {name}! Destroying component...");
                Destroy(this);
            }
        }

        public void AssignToPlayer(PlayerManager plyr = null, Entity ent = null)
        {
            _valid = true;
            EntType = EntityType.Player;

            Player = plyr ?? GetComponent<PlayerManager>();
            Entity = ent ?? Player.plm.Entity;

            if (CoreGameManager.Instance.huds.Length > Player.playerNumber &&
                CoreGameManager.Instance.GetHud(Player.playerNumber) != null)
                gaugeMan = CoreGameManager.Instance.GetHud(Player.playerNumber).gaugeManager;
        }

        public void AssignToNpc(NPC npc = null, Entity ent = null)
        {
            _valid = true;
            EntType = EntityType.Npc;

            Npc = npc ?? GetComponent<NPC>();

            Entity = ent;
            if (Npc.Navigator != null)
                Entity ??= Npc.Navigator.Entity;
        }

        public void AssignToEntity(Entity ent)
        {
            _valid = true;
            EntType = EntityType.Generic;
            Entity = ent;

            if (CompareTag("NPC"))
            {
                AssignToNpc(null, ent);
                return;
            }
            if (CompareTag("Player"))
                AssignToPlayer(null, ent);
        }

        public bool Activate(float time)
        {
            // Do not activate if it is immune
            if (Immune) return false;

            if (!Active)
            {
                Active = true;
                SetTime = timeLeft = time;

                Activated();

                if (EntType != EntityType.Player || gaugeMan == null)
                    return true;

                // Create or update new gauge respectively
                if (gauge == null && GaugeIcon != null)
                    gauge = gaugeMan.ActivateNewGauge(GaugeIcon, SetTime);
                else
                    gauge.SetValue(SetTime, timeLeft);

                return true;
            }

            if (SetTime < time)
                SetTime = time;
            if (timeLeft < time)
                timeLeft = time;

            gauge?.SetValue(SetTime, timeLeft);
            Reactivated();
            return true;
        }

        public void Deactivate()
        {
            if (!Active) return;

            Active = false;
            Deactivated();

            gauge?.SetValue(SetTime, 0f);
            gauge?.Deactivate();
            gauge = null;
        }

        private void Update()
        {
            if (!Active) return;
            ActiveUpdate();

            timeLeft -= Time.deltaTime * GetTimeScale();
            gauge?.SetValue(SetTime, timeLeft);
            if (timeLeft <= 0f)
                Deactivate();
        }

        protected virtual float GetTimeScale() // Dictates how fast time ticks down
        {
            return EntType switch
            {
                EntityType.Player => Player.PlayerTimeScale,
                EntityType.Npc => Npc.TimeScale,
                _ => Entity.Ec.EnvironmentTimeScale,
            };
        }

        protected abstract void Activated(); // When activated (re-applications do not count)
        protected abstract void Reactivated(); // When timer gets updated
        protected abstract void Deactivated(); // When deactivated
        protected abstract void ActiveUpdate(); // Update loop
    }
}