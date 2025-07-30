using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class DaycareGuiltManager : MonoBehaviour
    {
        private static PlayerManager _player;
        private static DaycareGuiltManager _instance;
        public static DaycareGuiltManager GetInstance(PlayerManager player)
        {
            if (_player == player && _instance != null)
                return _instance;

            _player = player;
            if (!_player.TryGetComponent(out _instance))
                _instance = _player.gameObject.AddComponent<DaycareGuiltManager>();

            return _instance;
        }

        public static void TryBreakRule(PlayerManager player, string rule, float linger, float sensitivity = 1f)
        {
            if (RecommendedCharsConfig.moduleMrDaycare.Value)
                GetInstance(player).BreakRule(rule, linger, sensitivity);
        }

        private PlayerManager player;

        public string RuleBreak { get; private set; }
        public bool Disobeying { get; private set; }
        public float GuiltTime { get; private set; }
        public float GuiltSensitivity { get; private set; }

        private void Start()
        {
            player = GetComponent<PlayerManager>();
        }

        public void BreakRule(string rule, float linger, float sensitivity = 1f)
        {
            if (linger >= GuiltTime)
            {
                RuleBreak = rule;
                Disobeying = true;
                GuiltTime = linger;
                GuiltSensitivity = sensitivity;
            }
        }

        public void ClearGuilt(bool excludeExcaping = false)
        {
            if (!excludeExcaping || !Disobeying || RuleBreak != "DaycareEscaping")
            {
                RuleBreak = "";
                Disobeying = false;
                GuiltTime = 0f;
            }
        }

        private void Update()
        {
            if (GuiltTime > 0f)
                GuiltTime -= Time.deltaTime * player.PlayerTimeScale;
            else if (Disobeying)
            {
                Disobeying = false;
                GuiltTime = 0f;
            }
        }
    }
}
