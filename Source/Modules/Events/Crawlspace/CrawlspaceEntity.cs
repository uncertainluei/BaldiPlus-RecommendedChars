using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Mono.Cecil;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Components;
using UncertainLuei.CaudexLib.Util;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{

    public class CrawlspaceEntity : MonoBehaviour
    {
        private enum LocationState
        {
            Above,
            Falling,
            Below
        }

        private EntityType entType;
        private LocationState location;

        private Entity entity;
        private NPC npc;

        private PlayerManager player;

        private EntityOverrider overrider = new();


        private readonly MovementModifier moveModCrawlspace = new(default, 0.5f),
            moveModStationary = new(default, 0, int.MaxValue);

        private void Awake()
        {
            entType = EntityType.Generic;
            entity = GetComponent<Entity>();
            if (CompareTag("Player"))
            {
                entType = EntityType.Player;
                player = GetComponent<PlayerManager>();
            }
            else if (CompareTag("NPC"))
            {
                entType = EntityType.Npc;
                npc = GetComponent<NPC>();
            }

            location = entity.Ec == CrawlspaceEvent.Instance.CrawlspaceEc ? LocationState.Below : LocationState.Above;
        }

        private Cell _cell;
        private void FixedUpdate()
        {
            if (!CrawlspaceEvent.Instance || location == LocationState.Falling ||
                !entity.active || !entity.InBounds || !entity.Grounded)
                return;

            _cell = entity.Ec.CellFromPosition(transform.position);
            if (_cell == null || _cell.Null) return;
            if (location == LocationState.Above && CrawlspaceEvent.Instance.IsCellOpen(_cell))
            {
                location = LocationState.Falling;
                StartCoroutine(FallTransition());
            }
        }

        private IEnumerator FallTransition()
        {
            overrider.Override(entity);
            overrider.SetInteractionState(false);
            overrider.SetFrozen(true);
            overrider.SetInBounds(false);
            overrider.SetGrounded(false);
            entity.SetTrigger(false);

            switch (entType)
            {
                case EntityType.Player:
                    player.itm.Disable(true);
                    player.plm.Entity.ExternalActivity.moveMods.Add(moveModStationary);
                    break;
                default:
                    overrider.SetBlinded(true);
                    break;
            }

            float gravity = 0f;
            Vector3 center = entity.Ec.CellFromPosition(transform.position).CenterWorldPosition;
            EntityHeightFixer heightComp = EntityHeightFixer.GetInstance(entity);

            while (heightComp.heightDifference > -20f)
            {
                if (Time.timeScale == 0f || entity.Ec.EnvironmentTimeScale == 0f)
                {
                    yield return null;
                    continue;
                }

                center.y = transform.position.y;
                if (Vector3.Distance(transform.position, center) > 0.5f)
                    transform.position = Vector3.Lerp(transform.position, center, 3f * Time.deltaTime * entity.Ec.EnvironmentTimeScale);
                else
                    transform.position = center;

                gravity -= 40f * Time.deltaTime * entity.Ec.EnvironmentTimeScale;
                heightComp.heightDifference = Mathf.Max(-20f, heightComp.heightDifference + gravity * Time.deltaTime * entity.Ec.EnvironmentTimeScale);
                yield return null;
            }

            overrider.SetInteractionState(true);
            overrider.SetFrozen(false);
            entity.SetTrigger(true);

            switch (entType)
            {
                case EntityType.Player:
                    player.plm.Entity.ExternalActivity.moveMods.Remove(moveModStationary);
                    player.itm.Disable(false);
                    break;
                default:
                    overrider.SetBlinded(false);
                    break;
            }

            overrider.Release();
            SetEnvironmentController(CrawlspaceEvent.Instance.CrawlspaceEc);
        }

        public void SetEnvironmentController(EnvironmentController ec)
        {
            location = ec == CrawlspaceEvent.Instance.CrawlspaceEc ? LocationState.Below : LocationState.Above;
            
            entity.environmentController = ec;
            entity.CullRenderer(false);
            entity.UpdateHeightAndScale();
            switch (entType)
            {
                case EntityType.Player:
                    if (player.ec == ec)
                        break;

                    List<PlayerManager> players = new(player.ec.Players);
                    players.Remove(player);
                    player.ec.players = players.ToArray();

                    players = new(ec.Players);
                    players.Add(player);
                    ec.players = players.ToArray();

                    player.ec = ec;
                    GameCamera.dijkstraMap.Deactivate();
                    GameCamera.dijkstraMap.environment = ec;
                    GameCamera.dijkstraMap.Activate();
                    GameCamera.dijkstraMap.QueueUpdate();
                    break;
                case EntityType.Npc:
                    if (npc.ec == ec)
                        break;

                    npc.ec.Npcs.Remove(npc);
                    ec.Npcs.Add(npc);

                    npc.transform.parent = ec.transform;
                    npc.ec = ec;
                    if (npc.Navigator)
                    {
                        npc.Navigator.Initialize(ec);
                        if (npc.Navigator._navMeshPath != null)
                        {
                            npc.Navigator.recalculatePath = true;
                            npc.behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
                            npc.Navigator.CheckPath();
                        }
                    }
                    break;
            }
            
            foreach (var audMan in GetComponentsInChildren<PropagatedAudioManager>())
            {
                audMan.environment = ec;
                audMan.propagationPosition = transform.position;
                audMan.propagationSource.transform.SetParent(ec.soundPropagationTransform);
            }

            return;

            if (location == LocationState.Above)
            {
                if (entity.ExternalActivity.moveMods.Contains(moveModCrawlspace))
                    entity.ExternalActivity.moveMods.Remove(moveModCrawlspace);

                return;
            }
            if (!entity.ExternalActivity.moveMods.Contains(moveModCrawlspace))
                entity.ExternalActivity.moveMods.Add(moveModCrawlspace);
        }
    }
}