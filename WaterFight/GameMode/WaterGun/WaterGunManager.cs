using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Photon.Pun;

#if GAME
using WaterFight.Input;
#endif

namespace WaterFight.GameMode.WaterGun
{
    internal class WaterGunManager : MonoBehaviour, IPunObservable
    {
        [SerializeField] private WaterStreamController waterController = null;
        [SerializeField] private PreasurePumpController waterPump = null;
        [SerializeField] private Renderer gunRenderer = null;
        [SerializeField] private Material waterTankMat = null;
        [SerializeField] private Color tankDeadColour;
        [SerializeField] private Color tankLiveColour;

#if GAME

        // bitmask to save gun state to cut down on data and messages sent
        private static class GunState
        {
            public static readonly byte Shooting = 1 << 0;
            public static readonly byte Pumping = 1 << 1;
        }

        private byte gunState = 0;
        private bool inputsAssigned = false;
        private bool isLocalPlayer = false;


        public bool IsLocalPlayer => isLocalPlayer;
        public WaterStreamController WaterController => waterController;
        public PreasurePumpController WaterPump => waterPump;

        // functions
        public void SetWaterTankHealth(float healthPercent) => waterTankMat.color = Vector4.MoveTowards(tankLiveColour, tankDeadColour, healthPercent);
        void Awake()
        {
            // creating new material instances for the tank
            var renderMats = gunRenderer.materials;
            if(renderMats.Length < 5) {
                Debug.LogError("WaterFight: not enough materials on the gun??");
                return;
            }

            waterTankMat = UnityEngine.Object.Instantiate(waterTankMat);
            renderMats[3] = waterTankMat;

            // assign the new material instance back to the renderer
            gunRenderer.materials = renderMats;
        }

        void OnEnable()
        {
            // water fight player controller only exists when in the gamemode
            if (this.gameObject.GetComponentInParent<VRRig>() is VRRig myRig) {
                if (myRig.isOfflineVRRig || (myRig.photonView != null && myRig.photonView.IsMine)) {
                    InputEvents.RightController.Trigger += ShootTrigger;
                    InputEvents.LeftController.Grip += GrabPump;
                    inputsAssigned = true;
                    isLocalPlayer = myRig.photonView != null;
                }
            }
        }

        void OnDisable()
        {
            WaterPump.Grab(false);
            waterController.Shoot(false);
            gunState = 0;

            isLocalPlayer = false;

            if (!inputsAssigned) {
                return;
            }

            InputEvents.RightController.Trigger -= ShootTrigger;
            InputEvents.LeftController.Grip -= GrabPump;
            inputsAssigned = false;
        }

        void Start()
        {
            // force our serialize view to run after vrrig's
            var photonView = this.gameObject.GetComponentInParent<VRRig>()?.photonView;
            if (photonView != null) {
                if (photonView.ObservedComponents.Contains(this)) {
                    photonView.ObservedComponents.Remove(this);
                    photonView.ObservedComponents.Add(this);
                } else {
                    photonView.ObservedComponents.Add(this);
                }

            } else {
                Debug.Log("photon view is null");
            }
        }

        void ShootTrigger(object sender, InputEventArgs inputs)
        {
            if (inputs.Pressed) {
                if (waterController.Shoot(true)) {
                    gunState |= GunState.Shooting;
                
                } else {
                    gunState &= (byte)~GunState.Shooting;
                }
            }

            if (inputs.Released) {
                waterController.Shoot(false);
                gunState &= (byte)~GunState.Shooting;
            }

            // Debug.Log($"gun state bitmask: {Convert.ToString(gunState, toBase: 2).PadLeft(8, '0')}");
        }

        void GrabPump(object sender, InputEventArgs inputs)
        {
            if(inputs.Pressed) {
                if (waterPump.Grab(true)) {
                    gunState |= GunState.Pumping;
                
                } else {
                    gunState &= (byte)~GunState.Pumping;
                }
            }

            if (inputs.Released) {
                waterPump.Grab(false);
                gunState &= (byte)~GunState.Pumping;
            }

            // Debug.Log($"gun state bitmask: {Convert.ToString(gunState, toBase: 2).PadLeft(8, '0')}");
        }

#endif
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
#if GAME
            // Debug.Log("gun onserialize view running");
            try {
                if (WaterFightGameMode.instance == null || !(GorillaGameManager.instance is WaterFightGameMode wfGameMode) || !wfGameMode.ModRunning) {
                    // Debug.Log("people are missing the mod");
                    return;
                }

                if (stream.IsWriting) {
                    stream.SendNext(waterController.WaterSpeed);
                    stream.SendNext(gunState);
                    return;
                }

                waterController.SetWaterPressure((float)stream.ReceiveNext());

                byte state = (byte)stream.ReceiveNext();

                // Debug.Log($"gun state bitmask: {Convert.ToString(state, toBase: 2).PadLeft(8, '0')}");
                // Debug.Log("shooting");

                waterController.Shoot((state & GunState.Shooting) == GunState.Shooting);

                // Debug.Log($"gun state bitmask: {Convert.ToString(state, toBase: 2).PadLeft(8, '0')}");
                // Debug.Log("grabbing");

                if ((state & GunState.Pumping) == GunState.Pumping) {
                    // Debug.Log("starting to pump");
                    waterPump.StartGrab();

                } else {
                    waterPump.Grab(false);
                }
            
            } catch { }
#endif
        }
    }
}
