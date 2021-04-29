using System;
using System.Threading.Tasks;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Worker.CInterop;
using Unity.Entities;
using UnityEngine;
using Cubism.Scripts.Config;

namespace Cubism
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    public class ShapeshifterSpawnSystem : ComponentSystem
    {
        private CommandSystem commandSystem;
        private EntityReservationSystem entityReservationSystem;
        private WorkerInWorld worker;

        private EntityQuery shapeshifterCount;
        private int maxSpawns = 20;
        private float spawnProbabilityPerSec = 0.7f;

        public ShapeshifterSpawnSystem(WorkerInWorld worker)
        {
            this.worker = worker;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            commandSystem = World.GetExistingSystem<CommandSystem>();
            entityReservationSystem = World.GetExistingSystem<EntityReservationSystem>();

            shapeshifterCount = GetEntityQuery(
                   ComponentType.ReadOnly<ShapeshifterState.Component>(),
                   ComponentType.ReadOnly<ShapeshifterState.HasAuthority>()
            );
        }

        protected override void OnUpdate()
        {
            MaybeSpawnShapeshifter();
        }

        private void MaybeSpawnShapeshifter()
        {
            // Spawn 'em until we're maxed out
            var countSpawned = shapeshifterCount.CalculateEntityCount();
            if(countSpawned < maxSpawns)
            {
                if(UnityEngine.Random.value < spawnProbabilityPerSec * Time.DeltaTime)
                {
                    SpawnShapeshifter();
                }
            }
        }

        private async void SpawnShapeshifter()
        {
            var entityId = await entityReservationSystem.GetAsync();

            var spawnFieldSize = 10.0f;
            var shapeshifterEntityTemplate = EntityTemplates.CreateShapeshifterEntityTemplate(
                entityId,
                new Vector3(
                    (UnityEngine.Random.value - 0.5f) * 2.0f * spawnFieldSize,
                    0.25f,
                    (UnityEngine.Random.value - 0.5f) * 2.0f * spawnFieldSize
                ),
                worker.Origin,
                Vector3.one * spawnFieldSize
            );

            var entityRequest = new WorldCommands.CreateEntity.Request
            (
                shapeshifterEntityTemplate,
                entityId
            );

            commandSystem.SendCommand(entityRequest);
        }
    }
}
