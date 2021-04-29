using System.Collections;
using System.Collections.Generic;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using UnityEngine;

namespace Cubism
{
    [WorkerType(UnityClientConnector.WorkerType), RequireComponent(typeof(PlayerStateMachine))]
    public class PlayerMovementControl : MonoBehaviour
    {
        // To check we're authoritative over this player
        [Require] private PositionWriter positionWriter;
        [Require] private PlayerAnimatorStateWriter playerAnimatorStateWriter;

        private float defaultSpeed = 7.0f;
        private float alignmentSpeed = 8.0f;
        private float jumpStrength = 10.0f;
        private float descentStrength = 20.0f;

        private Vector3 planarVelocity = Vector3.zero;
        private float verticalVelocity = 0.0f;
        private float verticalVelocityLimit = -100.0f;

        private PlayerStateMachine playerStateMachine;
        private CharacterController сharacterController;
        private Transform currentCameraTransform;

        private void OnEnable()
        {
            playerStateMachine = GetComponent<PlayerStateMachine>();
            сharacterController = GetComponent<CharacterController>();
        }

        public void SetAirborne()
        {
            var stateMachineUpdate = new PlayerAnimatorState.Update
            {
                Airborne = true,
            };
            playerAnimatorStateWriter.SendUpdate(stateMachineUpdate);
        }

        public void JumpTakeOff()
        {
            verticalVelocity = jumpStrength;
        }

        private void Update()
        {
            // Check for landing
            if (playerStateMachine.IsAirborne)
            {
                var castSphereRadius = 0.25f;
                if (Physics.SphereCast(new Ray(transform.position + Vector3.up * 0.05f, Vector3.down), castSphereRadius, 0.1f))
                {
                    // Landed!
                    if(playerStateMachine.IsDescending)
                    {
                        // Smash!
                        Debug.Log("Smash!");
                    }
                    var stateMachineUpdate = new PlayerAnimatorState.Update
                    {
                        Jumping = false,
                        Airborne = false,
                        Descending = false,
                    };
                    playerAnimatorStateWriter.SendUpdate(stateMachineUpdate);
                    Debug.Log("Landed!");
                }
            }

            if (currentCameraTransform == null)
            {
                if (LocalPlayer.Instance.CurrentCamera == null)
                    return;

                currentCameraTransform = LocalPlayer.Instance.CurrentCamera.transform;
            }

            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");

            var localMoveVec = y * Vector3.forward + x * Vector3.right;

            if (localMoveVec.sqrMagnitude > 0)
            {
                // Smoothly align towards the movement direction
                var cameraRelativeDirection = currentCameraTransform.transform.rotation * localMoveVec.normalized;
                var targetAngleY = Mathf.Atan2(cameraRelativeDirection.x, cameraRelativeDirection.z) / Mathf.PI * 180.0f;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, targetAngleY, 0), Mathf.Min(Time.deltaTime * alignmentSpeed, 1.0f));
            }

            var speed = Input.GetKey(KeyCode.LeftShift)
                ? 2 * defaultSpeed
                : defaultSpeed;

            var planarVelocity = speed * (transform.rotation * Vector3.forward * Mathf.Min(localMoveVec.magnitude, 1.0f));

            if(Input.GetKeyDown(KeyCode.Space))
            {
                // Isn't already jumping
                if (!(playerStateMachine.IsJumping || playerStateMachine.IsAirborne))
                {
                    var stateMachineUpdate = new PlayerAnimatorState.Update
                    {
                        Jumping = true,
                    };
                    playerAnimatorStateWriter.SendUpdate(stateMachineUpdate);
                } else if(playerStateMachine.IsAirborne) {
                    // Initiate descent!
                    var stateMachineUpdate = new PlayerAnimatorState.Update
                    {
                        Descending = true,
                    };
                    playerAnimatorStateWriter.SendUpdate(stateMachineUpdate);
                    verticalVelocity = -descentStrength;
                }
            }

            verticalVelocity += Time.deltaTime * Physics.gravity.y;
            verticalVelocity = Mathf.Max(verticalVelocity, verticalVelocityLimit);
            сharacterController.Move(Time.deltaTime * (planarVelocity + Vector3.up * verticalVelocity));
        }
    }
}
