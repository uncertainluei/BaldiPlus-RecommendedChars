using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{   
    public class MaintenanceMachine : NPC
    {
        public AudioManager audMan;
        public SoundObject audOhno, audClean;
        public List<PrimitiveDrop> entityPres = [];
        public Vector2 delayTimeRange = new(3f, 6f); 
        public Vector2 activeTimeRange = new(30f, 60f); 
        private readonly List<Entity> entitiesSpawned = [];
        private DijkstraMap dijkstraMap;
        private NavigationState_TargetPosition targetState;


        public override void Initialize()
        {
            base.Initialize();
            dijkstraMap = new(ec, PathType.Nav, int.MaxValue, transform);
            targetState = new(this, 0, Vector3.zero);
            behaviorStateMachine.ChangeState(new MaintenanceMachine_WanderCool(this));
        }

        public void SpawnEntity()
        {
            PrimitiveDrop entityToSpawn = entityPres[Random.Range(0,entityPres.Count)];
            entityToSpawn = Instantiate(entityToSpawn, ec.transform);
            entityToSpawn.Spawn(this);
            entitiesSpawned.Add(entityToSpawn.entity);
        }

        private void OnDestroy() // Destroy ALL spawned entities
        {
            while (entitiesSpawned.Count > 0)
            {
                // Destroy if it still exists
                if (entitiesSpawned[0])
                    Destroy(entitiesSpawned[0].gameObject);

                entitiesSpawned.RemoveAt(0);
            }
        }

        public void AddEntity(Entity ent) { if (!entitiesSpawned.Contains(ent)) entitiesSpawned.Add(ent); }
        public void RemoveEntity(Entity ent) { if (entitiesSpawned.Contains(ent)) entitiesSpawned.Remove(ent); }

        public void TryCleanEntity(Entity ent)
        {
            if (!ent || !entitiesSpawned.Contains(ent)) return;
            
            audMan.PlaySingle(audClean);
            entitiesSpawned.Remove(ent);
            Destroy(ent.gameObject);

            if (entitiesSpawned.Count == 0)
                behaviorStateMachine.ChangeState(new MaintenanceMachine_WanderCool(this));
        }

        public void TargetNearestEntity()
        {
            if (entitiesSpawned.Count == 0)
            {
                behaviorStateMachine.ChangeState(new MaintenanceMachine_WanderCool(this));
                return;
            }

            dijkstraMap.Calculate();

            IntVector2 position;
            int nearestDist = int.MaxValue, dist;
            Entity nearestEntity = null;

            foreach (Entity ent in entitiesSpawned)
            {
                position = IntVector2.GetGridPosition(ent.transform.position);
                if (!ec.ContainsCoordinates(position) || (dist = dijkstraMap.Value(position)) == int.MaxValue)
                {
                    RecommendedCharsPlugin.Log.LogWarning("MaintenanceMachine entity is out of bounds! Despawning...");
                    Destroy(ent.gameObject);
                    continue;
                }
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestEntity = ent;
                }
            }
            // No entities found? Go back to wander phase
            if (entitiesSpawned.Count == 0 || !nearestEntity)
            {
                behaviorStateMachine.ChangeState(new MaintenanceMachine_WanderCool(this));
                return;
            }
            targetState.UpdatePosition(nearestEntity.transform.position);
            behaviorStateMachine.ChangeNavigationState(targetState);
        }
    }

    public class MaintenanceMachine_StateBase(MaintenanceMachine machine) : NpcState(machine)
    {
        protected readonly MaintenanceMachine machine = machine;
    }

    public abstract class MaintenanceMachine_TimedState(MaintenanceMachine machine) : MaintenanceMachine_StateBase(machine)
    {
        protected float time;

        public override void Update()
        {
            base.Update();
            time -= Time.deltaTime * npc.TimeScale;
            if (time <= 0f)
                TimeUp();
        }

        protected abstract void TimeUp();
    }

    public class MaintenanceMachine_WanderCool(MaintenanceMachine machine) : MaintenanceMachine_TimedState(machine)
    {
        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0, true));
        }

        public override void Initialize()
        {
            base.Initialize();
            time = Random.Range(machine.delayTimeRange[0], machine.delayTimeRange[1]);
        }

        protected override void TimeUp()
        {
            machine.behaviorStateMachine.ChangeState(new MaintenanceMachine_DropEntities(machine));
        }
    }

    public class MaintenanceMachine_DropEntities(MaintenanceMachine machine) : MaintenanceMachine_TimedState(machine)
    {
        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_WanderRandomDropEntities(machine, 63));
        }

        public override void Initialize()
        {
            base.Initialize();
            time = Random.Range(machine.activeTimeRange[0], machine.activeTimeRange[1]);
        }

        protected override void TimeUp()
        {
            machine.behaviorStateMachine.ChangeState(new MaintenanceMachine_CleanEntities(machine));
        }
    }

    public class NavigationState_WanderRandomDropEntities(MaintenanceMachine machine, int priority) : NavigationState_WanderRandom(machine, priority)
    {
        private readonly MaintenanceMachine machine = machine;
        private readonly float spawnChance = 0.1f;

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (Random.value < spawnChance)
                machine.SpawnEntity();
        }
    }

    public class MaintenanceMachine_CleanEntities(MaintenanceMachine machine) : MaintenanceMachine_StateBase(machine)
    {
        public override void Initialize()
        {
            base.Initialize();
            machine.audMan.PlaySingle(machine.audOhno);
        }

        public override void OnStateTriggerEnter(Entity ent, Collider other, bool validCollision)
        {
            if (!machine.Entity.Squished)
                machine.TryCleanEntity(ent);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            machine.TargetNearestEntity();
        }
    }
}