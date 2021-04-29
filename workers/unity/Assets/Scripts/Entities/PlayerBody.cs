using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubism
{
    [WorkerType(UnityClientConnector.WorkerType)]
    public class PlayerBody : MonoBehaviour
    {
        [Require] private PlayerCharacterReader playerCharacterReader;
        private MeshRenderer bodyRenderer;

        void Start()
        {
            bodyRenderer = GetComponentInChildren<MeshRenderer>();
            playerCharacterReader.OnUpdate += OnPlayerCharacterComponentUpdated;

            UpdateState();
        }

        private void UpdateState()
        {
            var bodyColor = playerCharacterReader.Data.BodyColor;
            bodyRenderer.material.color = new Color(bodyColor.X, bodyColor.Y, bodyColor.Z);
            Debug.Log("body color: " + bodyRenderer.material.color);
        }

        private void OnPlayerCharacterComponentUpdated(PlayerCharacter.Update update)
        {
            UpdateState();
        }
    }
}
