using System.Collections;
using System.Collections.Generic;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using UnityEngine;

namespace Cubism
{
    public class PlayerStateMachine : MonoBehaviour
    {
        [Require] private PlayerAnimatorStateReader playerAnimatorStateReader;

        private Animator animator;
        private PlayerMovementControl playerMovementControl;

        private string JumpingParam = "jumping";
        private string AirborneParam = "airborne";
        private string DescendingParam = "descending";

        public bool IsJumping {
            get { return animator.GetBool(JumpingParam); }
            private set
            {
                animator.SetBool(JumpingParam, value);
            }
        }
        public bool IsAirborne {
            get { return animator.GetBool(AirborneParam); }
            private set
            {
                animator.SetBool(AirborneParam, value);
            }
        }
        public bool IsDescending
        {
            get { return animator.GetBool(DescendingParam); }
            private set
            {
                animator.SetBool(DescendingParam, value);
            }
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            playerMovementControl = GetComponent<PlayerMovementControl>();
            playerAnimatorStateReader.OnUpdate += OnPlayerAnimatorStateUpdated;
        }

        void OnPlayerAnimatorStateUpdated(PlayerAnimatorState.Update update)
        {
            if(update.Jumping.HasValue)
                IsJumping = update.Jumping.Value;
            if (update.Airborne.HasValue)
                IsAirborne = update.Airborne.Value;
            if (update.Descending.HasValue)
                IsDescending = update.Descending.Value;
        }

        /*
         * Animator-triggered event
         */
        public void TriggerSetAirborne()
        {
            if (playerMovementControl != null)
                playerMovementControl.SetAirborne();
        }

        /*
         * Animator-triggered event
         */
        public void TriggerJumpTakeOff()
        {
            if (playerMovementControl != null)
                playerMovementControl.JumpTakeOff();
        }

        void Update()
        {
            //
        }
    }
}
