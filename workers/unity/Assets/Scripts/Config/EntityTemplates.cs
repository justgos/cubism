using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.QueryBasedInterest;
using Improbable.Gdk.TransformSynchronization;
using UnityEngine;
using System.Threading.Tasks;

namespace Cubism.Scripts.Config
{
    public static class EntityTemplates
    {
        public static async Task<EntityTemplate> CreatePlayerEntityTemplate(EntityId entityId, string workerId, byte[] serializedArguments)
        {
            var clientAttribute = EntityTemplate.GetWorkerAccessAttribute(workerId);
            var serverAttribute = UnityGameLogicConnector.WorkerType;

            var playerAuthRequest = PlayerAuthRequest.Deserialize(serializedArguments);
            Debug.Log("playerAuthRequest.SessionKey: " + playerAuthRequest.SessionKey);

            // That's the place player data loading should happen
            await Task.Delay(500);

            var position = new Vector3(0, 0.25f, 0);
            var coords = Coordinates.FromUnityVector(position);
            var bodyColor = new Vector3Float(0.0f, 1.0f, 1.0f);

            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(coords), clientAttribute);
            template.AddComponent(new PlayerAnimatorState.Snapshot(false, false, false), clientAttribute);
            template.AddComponent(new PlayerCharacter.Snapshot(bodyColor), serverAttribute);
            template.AddComponent(new Metadata.Snapshot("Player"), serverAttribute);

            PlayerLifecycleHelper.AddPlayerLifecycleComponents(template, workerId, serverAttribute);
            TransformSynchronizationHelper.AddTransformSynchronizationComponents(template, clientAttribute, position);

            const int serverRadius = 500;
            var clientRadius = workerId.Contains(MobileClientWorkerConnector.WorkerType) ? 100 : 500;

            var serverQuery = InterestQuery.Query(Constraint.RelativeCylinder(serverRadius));
            var clientQuery = InterestQuery.Query(Constraint.RelativeCylinder(clientRadius));

            var interest = InterestTemplate.Create()
                .AddQueries<Metadata.Component>(serverQuery)
                .AddQueries<Position.Component>(clientQuery)
                .AddQueries<PlayerCharacter.Component>(clientQuery);
            template.AddComponent(interest.ToSnapshot(), serverAttribute);

            template.SetReadAccess(UnityClientConnector.WorkerType, MobileClientWorkerConnector.WorkerType, serverAttribute);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, serverAttribute);

            return template;
        }

        public static EntityTemplate CreateShapeshifterEntityTemplate(EntityId entityId, Vector3 position, Vector3 driftAreaCenter, Vector3 driftAreaExtents)
        {
            var serverAttribute = UnityGameLogicConnector.WorkerType;

            var coords = Coordinates.FromUnityVector(position);

            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(coords), serverAttribute);
            template.AddComponent(new ShapeshifterState.Snapshot(
                false,
                new Vector3Float(driftAreaCenter.x, driftAreaCenter.y, driftAreaCenter.z),
                new Vector3Float(driftAreaExtents.x, driftAreaExtents.y, driftAreaExtents.z)
            ), serverAttribute);
            template.AddComponent(new Metadata.Snapshot("Shapeshifter"), serverAttribute);

            TransformSynchronizationHelper.AddTransformSynchronizationComponents(template, serverAttribute, position);

            const int serverRadius = 500;
            var serverQuery = InterestQuery.Query(Constraint.RelativeCylinder(serverRadius));

            var interest = InterestTemplate.Create()
                .AddQueries<Metadata.Component>(serverQuery)
                .AddQueries<Position.Component>(serverQuery)
                .AddQueries<ShapeshifterState.Component>(serverQuery);
            template.AddComponent(interest.ToSnapshot(), serverAttribute);

            template.SetReadAccess(UnityClientConnector.WorkerType, MobileClientWorkerConnector.WorkerType, serverAttribute);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, serverAttribute);

            return template;
        }
    }
}
