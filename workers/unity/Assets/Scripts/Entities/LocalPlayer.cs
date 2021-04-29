using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubism
{
    /*
     * Hold a static reference to the local player instance
     */
    [WorkerType(UnityClientConnector.WorkerType)]
    public class LocalPlayer : MonoBehaviour
    {
        // Execute only on the owned player
        [Require] private PositionWriter positionWriter;

        public static LocalPlayer Instance;
        private Camera currentCamera;
        public Camera CurrentCamera { get; set; }

        void Start()
        {
            Instance = this;
        }
    }
}
