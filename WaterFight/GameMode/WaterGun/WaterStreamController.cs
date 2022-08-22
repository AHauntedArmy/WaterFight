using Oculus.Platform;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

using UnityEngine;

#if GAME
using WaterFight.Input;
#endif

namespace WaterFight.GameMode.WaterGun
{
    internal class WaterStreamController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles = null;
        [SerializeField] private GameObject splashPrefab = null;
        [SerializeField] private AudioSource shootingSound = null;

#if GAME
        private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        private ParticleSystem.MainModule mainParticleSettings;

        private float waterSpeed = 0f;

        public float WaterSpeed => waterSpeed;

        void Awake()
        {
            this.enabled = false;
            // Debug.Log("water pressure decay rate " + DecayRate);
            // this.DecayRate = MaxSpeed / DecaySeconds;
            // InputEvents.RightController.Trigger += Shoot;

            // Debug.Log("particle controller awake");
            if (particles == null || splashPrefab == null)
            {
                enabled = false;
                Debug.Log($"particles is {particles} splashprefab is {splashPrefab}");
                return;
            }

            mainParticleSettings = particles.main;

        }

        void OnDisable()
        {
            // Debug.Log("wat stream controller being disbaled");
            particles.Stop();
            shootingSound.Stop();
        }

        void Update()
        {
            if (waterSpeed == 0f) {
                this.enabled = false;
                return;
            }

            waterSpeed -= DecayRate * Time.deltaTime;
            waterSpeed = Mathf.Max(waterSpeed, 0f);
            // Debug.Log("water speed " + waterSpeed);
            
            // setting the particle speed
            mainParticleSettings.startSpeed = waterSpeed;
        }

        void OnParticleCollision(GameObject surface)
        {
            // Debug.Log("on particle collision called, script is enabled " + this.enabled);

            if (splashPrefab == null)
            {
                return;
            }

            int index = 0;
            int numCollision = particles.GetCollisionEvents(surface, collisionEvents);

            while (index < numCollision)
            {
                var splash = Instantiate(splashPrefab);
                Destroy(splash, 0.2f);

                splash.transform.position = collisionEvents[index].intersection;
                splash.transform.rotation = Quaternion.LookRotation(collisionEvents[index].normal, Vector3.up);

                ++index;
            }
        }

        public bool Shoot(bool shooting)
        {
            // Debug.Log("input event called");

            if (shooting && waterSpeed > 0f) {
                if (!this.enabled) {
                    this.enabled = true;
                    particles.Play();
                    shootingSound.Play();
                    return true;
                }

            } else if (this.enabled) {
                // Debug.Log("stopping shooting");
                this.enabled = false;
                return false;
            }

            return false;
        }

        public void AddWaterPressure(float pressure) => SetWaterPressure(waterSpeed + (pressure * PressureModifier * Time.deltaTime));
        public void SetWaterPressure(float pressure)
        {
            waterSpeed = Mathf.Min(pressure, currentMaxSpeed);
            mainParticleSettings.startSpeed = waterSpeed;
        }

        // static stuff
        private const float DecaySeconds = 15f;
        private const float MaxSpeed = 20f;
        private const float MaxPressureModifier = 7f;
        private static float PressureModifier = 2.5f;
        private static float DecayRate = MaxSpeed / DecaySeconds;
        private static float currentMaxSpeed = MaxSpeed;
        private static float pressurePercentage = 1f / WaterStreamController.MaxSpeed;
        public static float PressurePercentage => pressurePercentage;

        // can't think of a good way of doing this once for all instances on events instead of checking each frame
        public static void SetMaxWaterPressure(WaterFightGameMode instance)
        {
            if (instance == null) {
                return;
            }

            float roomSizeSpeedModifier = 12f;
            float roomSizePressureModifier = 4.5f;
            float roomSizePercent = (1f / 10f) * (float)PhotonNetwork.PlayerList.Length;

            roomSizeSpeedModifier *= roomSizePercent;
            roomSizePressureModifier *= roomSizePercent;

            currentMaxSpeed = MaxSpeed - roomSizeSpeedModifier;
            PressureModifier = MaxPressureModifier - roomSizePressureModifier;
            // Debug.Log("max pressure " + currentMaxSpeed);

            pressurePercentage = 1f / currentMaxSpeed;
            DecayRate = currentMaxSpeed / DecaySeconds;
        }
#endif
    }
}
