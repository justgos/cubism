using Cubism.Scripts.Config;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Representation;
using Improbable.Gdk.GameObjectCreation;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.TransformSynchronization;
using Improbable.Worker.CInterop;
using UnityEngine;
using Unity.Entities;

namespace Cubism
{
    public class UnityGameLogicConnector : WorkerConnector
    {
        [SerializeField] private EntityRepresentationMapping entityRepresentationMapping = default;

        public const string WorkerType = "UnityGameLogic";

        [SerializeField] private GameObject levelPrefab;
        private GameObject level;

        private async void Start()
        {
            // Using a custom async player creation flow
            //PlayerLifecycleConfig.CreatePlayerEntityTemplate = EntityTemplates.CreatePlayerEntityTemplate;
            AsyncHandleCreatePlayerRequestSystem.CreatePlayerEntityTemplate = EntityTemplates.CreatePlayerEntityTemplate;

            IConnectionFlow flow;
            ConnectionParameters connectionParameters;

            if (Application.isEditor)
            {
                flow = new ReceptionistFlow(CreateNewWorkerId(WorkerType));
                connectionParameters = CreateConnectionParameters(WorkerType);
            }
            else
            {
                flow = new ReceptionistFlow(CreateNewWorkerId(WorkerType),
                    new CommandLineConnectionFlowInitializer());
                connectionParameters = CreateConnectionParameters(WorkerType,
                    new CommandLineConnectionParameterInitializer());
            }

            var builder = new SpatialOSConnectionHandlerBuilder()
                .SetConnectionFlow(flow)
                .SetConnectionParameters(connectionParameters);

            await Connect(builder, new ForwardingDispatcher()).ConfigureAwait(false);
        }

        public static void AddServerSystems(World world)
        {
            world.GetOrCreateSystem<EntityReservationSystem>();

            // Using a custom async player creation flow
            world.GetOrCreateSystem<AsyncHandleCreatePlayerRequestSystem>();

            world.GetOrCreateSystem<PlayerHeartbeatInitializationSystem>();
            world.GetOrCreateSystem<SendPlayerHeartbeatRequestSystem>();
            world.GetOrCreateSystem<HandlePlayerHeartbeatResponseSystem>();
        }

        protected override void HandleWorkerConnectionEstablished()
        {
            //PlayerLifecycleHelper.AddServerSystems(Worker.World);
            AddServerSystems(Worker.World);
            GameObjectCreationHelper.EnableStandardGameObjectCreation(Worker.World, entityRepresentationMapping);
            TransformSynchronizationHelper.AddServerSystems(Worker.World);
            if(Worker.World.GetExistingSystem<ShapeshifterSpawnSystem>() == null)
                Worker.World.AddSystem(new ShapeshifterSpawnSystem(Worker));

            if (levelPrefab == null)
            {
                Debug.LogError("Cannot instantiate the level - `levelPrefab` is not set!");
                return;
            }

            level = Instantiate(levelPrefab, transform.position, transform.rotation);
        }

        public override void Dispose()
        {
            if (level != null)
            {
                Destroy(level);
            }

            base.Dispose();
        }
    }
}
