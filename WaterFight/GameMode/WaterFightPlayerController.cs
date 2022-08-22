using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using WaterFight.GameMode.WaterGun;

namespace WaterFight.GameMode
{
    internal class WaterFightPlayerController : MonoBehaviour
    {
        private WaterFightGameMode waterFightGameMode;
        private WaterStreamController waterController;
        private WaterGunManager waterGunManager;
        private Player myOwner;

        private const float HitCooldown = 0.5f;
        private static float lastHitTime = 0f; // only local player can send hit events, other instances of this class need to know this value

        [SerializeField] private GameObject rightHandOffsetReference = null;
        [SerializeField] private GameObject leftHandOffsetReference = null;

        public Transform RightPalmReference => rightHandOffsetReference?.transform?.parent;
        public Transform LeftPalmReference => leftHandOffsetReference?.transform?.parent;
        public GameObject RightHandOffsetReference {
            get => rightHandOffsetReference;
            set {
                if (rightHandOffsetReference == null) {
                    rightHandOffsetReference = value;
                }
            }
        }

        public GameObject LeftHandOffsetReference {
            get => leftHandOffsetReference;
            set {
                if (leftHandOffsetReference == null) {
                    leftHandOffsetReference = value;
                }
            }
        }


        void OnEnable() => this.StartCoroutine(CheckHealth());
        void OnDisbale() => this.StopAllCoroutines();

        void Awake()
        {
            waterGunManager = this.gameObject.GetComponentInChildren<WaterGunManager>();
            waterController = waterGunManager?.WaterController;
            myOwner = this.gameObject.GetComponent<VRRig>()?.photonView?.Owner;
        }

        void Start()
        {
            if (GorillaGameManager.instance is WaterFightGameMode waterFIghtGM) {
                waterFightGameMode = waterFIghtGM;
                Debug.Log("set water fight gamemode");
            
            } else {
                Debug.LogError("WaterFight: Instantiated networked water gun for the wrong game type!!");
            }
        }

        void OnParticleCollision(GameObject particle)
        {
             // Debug.Log(particle.name);

            if (waterController == null) {
                return;
            }

            var gunManager = particle.GetComponent<WaterGunManager>();
            if (gunManager == null) {
                return;
            }

            // take 2% of the shooters water pressure
            waterController.SetWaterPressure(waterController.WaterSpeed + (gunManager.WaterController.WaterSpeed * 0.02f));

            // if the person hitting this player is the local player, report the hit
            if (waterFightGameMode != null && gunManager.IsLocalPlayer) {
                // Debug.Log("attempting to send hit event");

                // cooldown to prevent network event spam
                float time = Time.time;
                if (time > lastHitTime && waterFightGameMode.CanHitTarget(this.myOwner)) {
                   //  Debug.Log("sending hit event");
                    lastHitTime = time + HitCooldown;
                    Vector3 hitPoint = this.gameObject.transform.position;
                    waterFightGameMode.photonView.RPC("ReportTagRPC", RpcTarget.MasterClient, new object[] { this.myOwner, hitPoint.x, hitPoint.y, hitPoint.z });
                }
            }
        }
        private IEnumerator CheckHealth()
        {
            while(true) {
                yield return new WaitForSeconds(0.1f);
                if (waterFightGameMode == null) {
                    Debug.LogWarning("WaterFight: water fight gamemode is null");
                    continue;
                }

                waterGunManager.SetWaterTankHealth(waterFightGameMode.GetPlayerHealth(myOwner.ActorNumber));
            }
        }
    }
}
