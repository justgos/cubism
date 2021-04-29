using System.Collections;
using System.Collections.Generic;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using UnityEngine;


namespace Cubism
{
    [WorkerType(UnityGameLogicConnector.WorkerType)]
    public class ShapeshifterBehavior : MonoBehaviour
    {
        [Require] private ShapeshifterStateWriter shapeshifterStateWriter;

        private float revertProbabilityPerSec = 0.2f;
        private Vector3 velocity = Vector3.zero;

        private float driftSpeed = 100.0f;

        void Start()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Trigger enter: " + other.gameObject);
            // If collider with the player - restore the shapeshifter's original form
            if (other.GetComponentInParent<PlayerTag>() != null && !shapeshifterStateWriter.Data.Restored)
            {
                var stateUpdate = new ShapeshifterState.Update
                {
                    Restored = true,
                };
                shapeshifterStateWriter.SendUpdate(stateUpdate);
            }
        }

        void Update()
        {
            if (shapeshifterStateWriter.Data.Restored)
            {
                if (UnityEngine.Random.value < revertProbabilityPerSec * Time.deltaTime)
                {
                    var stateUpdate = new ShapeshifterState.Update
                    {
                        Restored = false,
                    };
                    shapeshifterStateWriter.SendUpdate(stateUpdate);
                }
            } else {
                // Decay velocity
                velocity *= 0.99f;
                // Drift around!
                velocity += Time.deltaTime * UnityEngine.Random.value * new Vector3(
                    (UnityEngine.Random.value - 0.5f) * 2.0f * driftSpeed,
                    0.0f,
                    (UnityEngine.Random.value - 0.5f) * 2.0f * driftSpeed
                );
                // Stay with the specified bounds
                transform.position = new Vector3(
                    Mathf.Clamp(
                        transform.position.x + Time.deltaTime * velocity.x,
                        shapeshifterStateWriter.Data.DriftAreaCenter.X - shapeshifterStateWriter.Data.DriftAreaExtents.X,
                        shapeshifterStateWriter.Data.DriftAreaCenter.X + shapeshifterStateWriter.Data.DriftAreaExtents.X
                    ),
                    transform.position.y,
                    Mathf.Clamp(
                        transform.position.z + Time.deltaTime * velocity.z,
                        shapeshifterStateWriter.Data.DriftAreaCenter.Z - shapeshifterStateWriter.Data.DriftAreaExtents.Z,
                        shapeshifterStateWriter.Data.DriftAreaCenter.Z + shapeshifterStateWriter.Data.DriftAreaExtents.Z
                   )
                );
            }
        }
    }
}
