using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Components;
using UncertainLuei.CaudexLib.Registers;
using UncertainLuei.CaudexLib.Util.Extensions;
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
        [SerializeField] internal SoundObject audBlow, audBuzz, audWow;
        [SerializeField] internal GameObject promptScreen, blackScreen, winScreen;
        [SerializeField] internal TMP_Text promptText;

        private byte paintingCount;
        

        private MovementModifier moveMod = new(default, 0f);
        private PlayerManager liftedPlayer;
        private List<Cell> lights;

        internal Transform partyElevator;
        internal PartyWinRoomFunction cafeteria;
        internal RoomController teleporterRoom;
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
            GameCamera.dijkstraMap.environment = ec;
            CaudexEvents.OnItemUse += TeleporterFeedback;

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
            Debug.LogError("HELLO!");
            SpoilerAreaPatches.surpressDijkstraOOB = false;
            CaudexEvents.OnItemUse -= TeleporterFeedback;
            Entity.physicalHeight = 5f; // Fixes edge case of entities suddenly being all the way up afterwards in certain scenarios
        }

        private IEnumerator WaitForLoader()
        {
            levelLoader.StartGenerate();
            yield return new WaitWhile(() => !levelLoader.levelInProgress);
            yield return new WaitWhile(() => levelLoader.levelInProgress && !levelLoader.levelCreated);

            endingEnvironment.Active = false;
            //endingEnvironment.gameObject.SetActive(false);
            endingEnvironment.height = 30f;
            endingEnvironment.transform.position = Vector3.up * 30f;

            foreach (Pickup pickup in ec.items)
            {
                if (pickup && pickup.item.itemType == ITM_PartySecretTape.itemEnum)
                    pickup.OnItemCollected += (x,y) => StartCoroutine(PromptTransition("Tfx_Enc_RecChars_PartyWin_Tape"));
            }

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

            foreach (Elevator elevate in ec.Elevators)
                elevate.SetState(ElevatorState.NoPower);

            PlayerFileManager.Instance.savedGameData.saveAvailable = false;
            PlayerFileManager.Instance.Save();
        }

        public override void LoadNextLevel()
        {
            GlobalCam.Instance.SetListener(true);
            SubtitleManager.Instance.DestroyAll();
            CoreGameManager.Instance.audMan.PlaySingle(audWow);
            winScreen.SetActive(true);
            GlobalCam.Instance.FadeIn(UiTransition.Dither, 1/30f);
            StartCoroutine(WinTransition());
        }

        private IEnumerator WaitForInteraction()
            => new WaitWhile(() => !Input.anyKeyDown && !InputManager.Instance.GetDigitalInput("MouseSubmit", true) && !InputManager.Instance.GetDigitalInput("Pause", true) && !InputManager.Instance.AnyButton(true));

        private IEnumerator WinTransition()
        {
            yield return new WaitWhile(() => GlobalCam.Instance.TransitionActive);
            yield return WaitForInteraction();
            
            winScreen.SetActive(false);
            blackScreen.SetActive(true);
            CoreGameManager.Instance.ReturnToMenu();
        }

        private IEnumerator PromptTransition(params string[] messages)
        {
            CoreGameManager.Instance.disablePause = true;
            Time.timeScale = 0f;
            promptScreen.SetActive(true);
            promptText.text = messages[0].Localize();
            GlobalCam.Instance.FadeIn(UiTransition.Dither, 1/60f);
            for (int i = 0, c = messages.Length; i < c; i++)
            {
                promptText.text = messages[i].Localize();
                yield return new WaitWhile(() => GlobalCam.Instance.TransitionActive);
                yield return WaitForInteraction();
            }
            GlobalCam.Instance.Transition(UiTransition.Dither, 1/60f);
            promptScreen.SetActive(false);
            yield return new WaitWhile(() => GlobalCam.Instance.TransitionActive);
            CoreGameManager.Instance.disablePause = false;
            Time.timeScale = 1f;
        }

        private void TeleporterFeedback(ItemManager im, ItemObject itm)
        {
            if (itm.itemType != Items.Teleporter) return;
            StartCoroutine(TeleporterUsed());
        }

        private IEnumerator TeleporterUsed()
        {
            CoreGameManager.Instance.disablePause = true;
            yield return new WaitForSeconds(0.2f);
            yield return PromptTransition("Tfx_Enc_RecChars_PartyWin_Teleport_1", "Tfx_Enc_RecChars_PartyWin_Teleport_2", "Tfx_Enc_RecChars_PartyWin_Teleport_3");
        }

        public void Surprise()
        {
            MusicManager.Instance.PlayMidi("DanceV0_5", false);
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

            CoreGameManager.Instance.audMan.PlaySingle(audBlow);

            MusicManager.Instance.StopMidi();
            MusicManager.Instance.StopFile();
            MusicManager.Instance.QueueFile(glambience, true);

            StartCoroutine(UpdateEndingArea());
        }

        private IEnumerator UpdateEndingArea()
        {
            lights = [];  
            cafeteria.room.entitySafeCells.Clear();
            teleporterRoom = endingEnvironment.rooms.First(x => x.name.EndsWith("TeleporterRoom"));

            yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();

            foreach (Door door in teleporterRoom.doors)
                door.Lock(true);

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
                    {
                        lights.Add(light.source);
                        endingEnvironment.lightMap[x,y].AddSource(light.source, light.distance);
                    }

                    yield return null;
                }
            }

            //ec.GetComponent<LightmapModHolder>().ForceUpdateLightmap();

        }

        public void PaintingTouched(PlayerManager player)
        {
            
            paintingCount++;

            if (player.plm.stamina < player.plm.StaminaMax)
                player.plm.stamina = player.plm.StaminaMax;

            switch (paintingCount)
            {
                case 8:
                    return;
                case 7:
                    // Blah blah blah, I know, hardcoded schlock.
                    endingEnvironment.CellFromPosition(new IntVector2(1, 16)).SetShape(0, TileShapeMask.Open);

                    foreach (Door door in teleporterRoom.doors)
                        door.Unlock(); 
                    break;
            }
            StartCoroutine(PromptTransition($"Tfx_Enc_RecChars_PartyWin_{paintingCount}"));
        }
    }
}