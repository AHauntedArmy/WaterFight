using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

#if GAME
using WaterFight.Input;
#endif

namespace WaterFight.GameMode.WaterGun
{
    internal class PreasurePumpController : MonoBehaviour
    {
        [SerializeField] private Transform pumpCenter = null;
        [SerializeField] private WaterStreamController waterController = null;
#if GAME
        private Transform handReference = null;
        
        private Vector3 grabOffset;
        private Vector3 lastLocalPosition;

        private bool grabbingPump = false;

        private const float InterpolateSpeed = 0.1f;
        private const float MaxYPosition = 0.0225f;
        private const float StaticXPosition = -0.0024f;

        void Awake()
        {
           //  Debug.Log("pressure pump controller percentage " + Percentage);
            // this.gameObject.layer = LayerMask.NameToLayer("GorillaInteractable");
            var playerController = this.gameObject.GetComponentInParent<WaterFightPlayerController>();
            if (playerController != null) {
                // Debug.Log("found player controller");
                this.handReference = playerController.LeftPalmReference;
                if (this.handReference == null) {
                    Debug.LogError("could find hand reference");
                }
            
            } else {
                handReference = GorillaLocomotion.Player.Instance.leftHandFollower;
            }
        }

        void OnDisable()
        {
            grabbingPump = false;
        }

        void Update()
        {
            Vector3 currentPosition = this.transform.position;
            if (grabbingPump) {
                if (Vector3.Distance(currentPosition, handReference.position) > 0.3f) {
                    grabbingPump = false;
                    return;
                }

                // find the direction position
                Vector3 dir = handReference.position - (currentPosition + grabOffset);

                // clamp the direction to the "forward" axis. (model has up as forward)
                Vector3 clampedDir = Vector3.Project(dir, this.transform.up);

                // add the direction to the position
                this.transform.position += clampedDir;

                // clam the local Y position to not go below 0
                Vector3 currentLocalPosition = this.transform.localPosition;
                currentLocalPosition.x = StaticXPosition; // pump seemed to be moving sideways slightly, probably float precision error
                currentLocalPosition.y = Mathf.Clamp(currentLocalPosition.y, 0f, 1f);
                this.transform.localPosition = currentLocalPosition;

                if ((lastLocalPosition - currentLocalPosition).y > 0) {
                    // Debug.Log("real magnitude " + clampedDir.magnitude / Time.deltaTime);
                    waterController.AddWaterPressure(clampedDir.magnitude / Time.deltaTime);
                }

                lastLocalPosition = currentLocalPosition;
            
            // pump position shows pressure
            } else {
                Vector3 localPosition = this.transform.localPosition;
                float currentY = localPosition.y;
                float pressurePercent = WaterStreamController.PressurePercentage * waterController.WaterSpeed;
                currentY = Mathf.MoveTowards(currentY, MaxYPosition * pressurePercent, InterpolateSpeed * Time.deltaTime);
                localPosition.y = currentY;
                this.transform.localPosition = localPosition;
            }
        }

        public bool Grab(bool grabbing)
        {
            // Debug.Log("grabbed called");

            bool result = false;

            if (grabbing) {
                float distance = Vector3.Distance(pumpCenter.transform.position, handReference.position);
                // Debug.Log(distance);

                if (distance < 0.15f) {
                    StartGrab();
                    result = true;
                }

            } else {
                grabbingPump = false;
                result = false;
            }

            return result;
        }

        public void StartGrab()
        {
            grabOffset = handReference.position - this.transform.position;
            lastLocalPosition = this.transform.localPosition;
            grabbingPump = true;
        }
#endif
    }
}
