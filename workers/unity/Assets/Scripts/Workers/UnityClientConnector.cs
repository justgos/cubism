using System;
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
    public class UnityClientConnector : WorkerConnector
    {
        [SerializeField] private EntityRepresentationMapping entityRepresentationMapping = default;

        public const string WorkerType = "UnityClient";

        [SerializeField] private GameObject levelPrefab;
        private GameObject level;


        private async void Start()
        {
            var connParams = CreateConnectionParameters(WorkerType);

            var builder = new SpatialOSConnectionHandlerBuilder()
                .SetConnectionParameters(connParams);

            if (!Application.isEditor)
            {
                var initializer = new CommandLineConnectionFlowInitializer();
                switch (initializer.GetConnectionService())
                {
                    case ConnectionService.Receptionist:
                        builder.SetConnectionFlow(new ReceptionistFlow(CreateNewWorkerId(WorkerType), initializer));
                        break;
                    case ConnectionService.Locator:
                        builder.SetConnectionFlow(new LocatorFlow(initializer));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                builder.SetConnectionFlow(new ReceptionistFlow(CreateNewWorkerId(WorkerType)));
            }

            await Connect(builder, new ForwardingDispatcher()).ConfigureAwait(false);
        }

        protected override void HandleWorkerConnectionEstablished()
        {
            PlayerLifecycleHelper.AddClientSystems(Worker.World, autoRequestPlayerCreation: false);
            GameObjectCreationHelper.EnableStandardGameObjectCreation(Worker.World, entityRepresentationMapping);
            TransformSynchronizationHelper.AddClientSystems(Worker.World);

            if (levelPrefab == null)
            {
                Debug.LogError("Cannot instantiate the level - `levelPrefab` is not set!");
                return;
            }

            level = Instantiate(levelPrefab, transform.position, transform.rotation);

            CallPlayerCreation();
        }

        public void CallPlayerCreation()
        {
            var playerCreationSystem = Worker.World.GetExistingSystem<SendCreatePlayerRequestSystem>();
            var playerAuthRequest = new PlayerAuthRequest
            {
                SessionKey = "arara!",
            };
            playerCreationSystem.RequestPlayerCreation(serializedArguments: playerAuthRequest.Serialize());
        }

        private void OnCreatePlayerResponse(PlayerCreator.CreatePlayer.ReceivedResponse response)
        {
            if (response.StatusCode != StatusCode.Success)
            {
                Debug.LogWarning($"Error: {response.Message}");
            }
        }

        public override void Dispose()
        {
            if(level != null)
            {
                Destroy(level);
            }

            base.Dispose();
        }
    }
}
