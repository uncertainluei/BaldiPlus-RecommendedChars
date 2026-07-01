using System.Collections;
using BaldisBasicsPlusAdvanced.API;
using TMPro;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Components;
using UncertainLuei.CaudexLib.Util;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PartyWinManager : BaseGameManager
    {
        [SerializeField] internal LevelLoader levelLoader;
        [SerializeField] internal EnvironmentController endingEnvironment;
        [SerializeField] internal LevelAsset endingLevel;
        [SerializeField] internal ExtraLevelDataAsset endingLevelExtra;

        [SerializeField] internal LoopingSoundObject glambience;
        [SerializeField] internal SoundObject audBuzz, audWow;
        [SerializeField] internal GameObject promptScreen, blackScreen;
        private TMP_Text promptText;

        private byte paintingCount;

        private MovementModifier moveMod = new(default, 0f);
        private SurpriseNpcBase[] surpriseNpcs;
        private PlayerManager liftedPlayer;

        internal Transform partyElevator;
        internal PartyWinRoomFunction cafeteria;
        internal GameObject candle;

        public override void Initialize()
        {
            if (CoreGameManager.Instance.currentMode == Mode.Free)
            {
                Destroy(ElevatorScreen.Instance.gameObject);
                CoreGameManager.Instance.Quit();
                Destroy(gameObject);
                return;
            }

            base.Initialize();
            CoreGameManager.Instance.SaveEnabled = false;
            CoreGameManager.Instance.readyToStart = false;
            CoreGameManager.Instance.environmentToSpawn = ec;
            SpoilerAreaPatches.surpressDijkstraOOB = true;

            specialManagerFunction =
            [
                (player) => StartCoroutine(Lift(player.GetComponent<PlayerManager>())), // Lift player
                (candleClick) => BlowCandle(candleClick)
            ];

            endingEnvironment = Instantiate(endingEnvironment);
            endingEnvironment.standardDarkLevel = Color.white;
            endingEnvironment.lightingOverride = true;

            levelLoader = Instantiate(levelLoader);
            levelLoader.levelAsset = endingLevel;
            levelLoader.extraAsset = endingLevelExtra;
            levelLoader.Ec = endingEnvironment;

            // Dummy scene object which is literally here to skirt around that dumb 0.12.2 change
            levelLoader.scene = ScriptableObject.CreateInstance<SceneObject>();
            levelLoader.scene.previousLevels = [];
            levelLoader.scene.potentialNPCs = [];
            levelLoader.scene.forcedNpcs = [];

            StartCoroutine(WaitForLoader());
        }

        private new void OnDestroy()
        {
            base.OnDestroy(); // This wouldn't be needed if mystman knew to use the boolean cast!!!
            SpoilerAreaPatches.surpressDijkstraOOB = false;
        }

        private IEnumerator WaitForLoader()
        {
            levelLoader.StartGenerate();
            yield return new WaitWhile(() => !levelLoader.levelInProgress);
            yield return new WaitWhile(() => levelLoader.levelInProgress && !levelLoader.levelCreated);

            endingEnvironment.Active = false;
            endingEnvironment.gameObject.SetActive(false);
            endingEnvironment.height = 30f;
            endingEnvironment.transform.position = Vector3.up * 30f;

            ec.GetComponent<LightmapModHolder>().ForceUpdateLightmap();
            CoreGameManager.Instance.readyToStart = true;
        }
        public override void BeginPlay()
        {
            base.BeginPlay();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            MusicManager.Instance.StopFile();
            MusicManager.Instance.StopMidi();

            foreach (PlayerManager player in ec.players)
            {
                if (!player) continue;
                player.itm.Disable(true);
            }

            PlayerFileManager.Instance.savedGameData.saveAvailable = false;
            PlayerFileManager.Instance.Save();
        }

        public void Surprise()
        {
            MusicManager.Instance.PlayMidi("DanceV0_5", true);
        }

        public IEnumerator Lift(PlayerManager player)
        {
            liftedPlayer = player;
            player.Am.moveMods.Add(moveMod);
            Vector3 pos = partyElevator.position;
            float offset = player.plm.height-pos.y;
            pos.y = player.plm.height;
            player.plm.Entity.Teleport(pos);

            while (player.plm.height < 35f)
            {
                player.plm.height = Mathf.Min(35f, player.plm.height + Time.deltaTime * 10f);
                pos.y = player.plm.height-offset;
                partyElevator.position = pos;
                yield return null;
            }
        }

        public void BlowCandle(GameObject candleClick)
        {
            if (!liftedPlayer) return;

            candleClick.SetActive(false);
            candle.SetActive(false);
            endingEnvironment.gameObject.SetActive(true);
            endingEnvironment.Active = true;
            liftedPlayer.Am.moveMods.Remove(moveMod);
            liftedPlayer.ec = endingEnvironment;
            liftedPlayer.plm.Entity.environmentController = endingEnvironment;
            liftedPlayer.itm.Disable(false);
            cafeteria.CandleBlown();

            liftedPlayer.dijkstraMap.Deactivate();
            liftedPlayer.dijkstraMap.environment = endingEnvironment;
            liftedPlayer.dijkstraMap.Activate();
            liftedPlayer.dijkstraMap.QueueUpdate();
            GameCamera.dijkstraMap.Deactivate();
            GameCamera.dijkstraMap.environment = endingEnvironment;
            GameCamera.dijkstraMap.Activate();
            GameCamera.dijkstraMap.QueueUpdate();

            MusicManager.Instance.StopMidi();
            MusicManager.Instance.StopFile();
            MusicManager.Instance.QueueFile(glambience, true);

            StartCoroutine(UpdateEndingArea());
        }

        private IEnumerator UpdateEndingArea()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();

            foreach (Elevator elevate in endingEnvironment.Elevators)
                elevate.SetState(ElevatorState.NoPower);

            yield return null;

            ec.lightingOverride = true;
            endingEnvironment.lightingOverride = false;
            for (int x = 0; x > ec.levelSize.x; x++)
            {
                for (int y = 0; y > ec.levelSize.z; y++)
                {
                    if (ec.CellFromPosition(x,y).Null) continue;
                    foreach (LightData light in ec.lightMap[x,y].lightSources)
                        endingEnvironment.lightMap[x,y].AddSource(light.source, light.distance);

                    yield return null;
                }
            }

            //ec.GetComponent<LightmapModHolder>().ForceUpdateLightmap();

        }

        public void PaintingTouched()
        {
            paintingCount++;
        }
    }
}