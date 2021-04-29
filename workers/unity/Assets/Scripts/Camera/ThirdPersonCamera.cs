using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubism {
    public class ThirdPersonCamera : MonoBehaviour
    {
        private float distance = 5.0f;
        private Vector2 angles = new Vector2(0, 30.0f);
        private float speed = 30.0f;

        private LocalPlayer targetPlayer;
        private Vector3 targetFocusPos = Vector3.zero;

        void Start()
        {

        }

        void Update()
        {
            if(targetPlayer == null)
            {
                // Well, we might drift around.. but we won't
                if (LocalPlayer.Instance == null)
                    return;

                targetPlayer = LocalPlayer.Instance;
                targetPlayer.CurrentCamera = GetComponent<Camera>();
            }
            
            var dx = Input.GetAxisRaw("Mouse X");
            var dy = Input.GetAxisRaw("Mouse Y");
            angles.x = Mathf.Clamp(angles.x - dy, -90.0f, 90.0f);
            angles.y += dx;

            targetFocusPos = Vector3.Lerp(targetFocusPos, LocalPlayer.Instance.transform.position, Mathf.Min(Time.deltaTime * speed, 1.0f));
            //targetFocusPos = LocalPlayer.Instance.transform.position;

            var targetOrientation = Quaternion.Euler(angles.x, angles.y, 0);
            var targetPosition = targetFocusPos + targetOrientation * Vector3.back * distance;
            transform.rotation = targetOrientation;
            transform.position = targetPosition;
        }
    }
}
