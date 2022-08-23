using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

using WaterFight.GameMode;
using System.Collections;

namespace WaterFight.Tools
{
    internal class PrefabManager : MonoBehaviour
    {
        private const string gorillaPrefabName = "GorillaPrefabs/Gorilla Player Networked";
        private const string gorillaRigRightHandName = "rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R";
        private const string gorillaRigLeftHandName = "rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L";

        private GameObject gorillaPrefab = null;
        private GameObject gorillaPrefabRightHandOffset = null;
        private GameObject gorillaPrefabLeftHandOffset = null;
        private GameObject gorillaPrefabWaterGun = null;
        private GameObject waterGunCosmetic = null;

        public GameObject WaterGunCosmetic => waterGunCosmetic;

        void Awake()
        {
            Prefabs.LoadPrefabs(this);

            // preloading the vrrig into the resource cache
            if (PhotonNetwork.PrefabPool is DefaultPool pool) {
                if (!pool.ResourceCache.ContainsKey(gorillaPrefabName)) {
                    gorillaPrefab = Resources.Load<GameObject>(gorillaPrefabName);

                    if (gorillaPrefab != null) {
                        pool.ResourceCache.Add(gorillaPrefabName, gorillaPrefab);
                    }

                } else {
                    gorillaPrefab = pool.ResourceCache[gorillaPrefabName];
                }
            }

            foreach (var collider in gorillaPrefab.GetComponentsInChildren<Collider>()) {
                collider.isTrigger = false;
            }

            this.StartCoroutine(LoadSuperSoaker());
        }

        IEnumerator LoadSuperSoaker()
        {
            if (Prefabs.FailedToLoad) {
                yield break;
            }

            if (!Prefabs.Loaded) {
                yield return Prefabs.LoadingPrefabs;
            }

            yield return null;

            if (gorillaPrefab != null) {
                gorillaPrefabRightHandOffset = GameObject.Instantiate(Prefabs.RightHandOffsetPrefab);
                gorillaPrefabLeftHandOffset = GameObject.Instantiate(Prefabs.LeftHandOffsetPrefab);

                var rigRightHand = gorillaPrefab.transform.Find(gorillaRigRightHandName);
                var rigLeftHand = gorillaPrefab.transform.Find(gorillaRigLeftHandName);

                if(rigRightHand == null) {
                    Debug.LogError("couldn't find rig right hand palm");
                }

                if (rigLeftHand == null) {
                    Debug.LogError("couldn't find rig left hand palm");
                }

                gorillaPrefabRightHandOffset.transform.SetParent(rigRightHand, false);
                gorillaPrefabLeftHandOffset.transform.SetParent(rigLeftHand, false);
            }

            var gorillaHand = GorillaTagger.Instance.offlineVRRig.transform.Find(gorillaRigRightHandName);
            // Debug.Log($"hand name = {gorillaHand?.gameObject?.name}");

            var handParent = GameObject.Instantiate(Prefabs.RightHandOffsetPrefab);
            handParent.transform.SetParent(gorillaHand.transform, false);

            waterGunCosmetic = GameObject.Instantiate(Prefabs.SuperSoakerPrefab);
            waterGunCosmetic.transform.SetParent(handParent.transform, false);
            // waterGunCosmetic.SetActive(false);
            waterGunCosmetic.SetActive(WaterFightPlugin.EnableCosmetics);

            yield return null;
        }

        public void AttachPrefabs()
        {
            var wfPlayercontroller = gorillaPrefab.AddComponent<WaterFightPlayerController>();
            wfPlayercontroller.LeftHandOffsetReference = gorillaPrefabLeftHandOffset;
            wfPlayercontroller.RightHandOffsetReference = gorillaPrefabRightHandOffset;
            
            gorillaPrefabWaterGun = GameObject.Instantiate(Prefabs.SuperSoakerPrefab);
            gorillaPrefabWaterGun.transform.SetParent(gorillaPrefabRightHandOffset.transform, false);
            gorillaPrefabWaterGun.SetActive(true);
        }

        public void RemovePrefabs()
        {
            if (gorillaPrefabWaterGun != null) {
                GameObject.Destroy(gorillaPrefabWaterGun);
                gorillaPrefabWaterGun = null;
            }

            var hitHandler = gorillaPrefab.GetComponent<WaterFightPlayerController>();
            if (hitHandler != null) {
                GameObject.Destroy(hitHandler);
            }
        }

    }
}
