using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubism
{
    [WorkerType(UnityClientConnector.WorkerType)]
    public class ShapeshifterBody : MonoBehaviour
    {
        [Require] private ShapeshifterStateReader shapeshifterStateReader;
        private Animator animator;

        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            shapeshifterStateReader.OnUpdate += OnShapeshifterStateUpdated;

            UpdateState();
        }

        private void UpdateState()
        {
            animator.SetBool("restored", shapeshifterStateReader.Data.Restored);
        }

        private void OnShapeshifterStateUpdated(ShapeshifterState.Update update)
        {
            UpdateState();
        }
    }
}
