using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using UnityEngine;

namespace WaterFight.Tools
{
    internal class Prefabs
    {
        public static bool Loaded => prefabsLoaded;
        public static bool FailedToLoad => prefabsLoaded == false && loadingPrefabs == null;
        private static bool prefabsLoaded = false;

        public static GameObject SuperSoakerPrefab => superSoakerPrefab;
        private static GameObject superSoakerPrefab = null;

        public static GameObject RightHandOffsetPrefab => rightHandOffsetPrefab;
        private static GameObject rightHandOffsetPrefab = null;

        public static GameObject LeftHandOffsetPrefab => leftHandOffsetPrefab;
        private static GameObject leftHandOffsetPrefab = null;

        public static Coroutine LoadingPrefabs => loadingPrefabs;
        private static Coroutine loadingPrefabs = null;
        
        public static void LoadPrefabs(MonoBehaviour host)
        {
            if ( host == null || prefabsLoaded || loadingPrefabs != null) {
                return;
            }

            loadingPrefabs = host.StartCoroutine(LoadPrefabsAsnyc());
        }

        private static IEnumerator LoadPrefabsAsnyc()
        {
            Stream assetStream;
            AssetBundleCreateRequest assetLoader;
            try {
                assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WaterFight.Resources.waterfightassets");
                assetLoader = AssetBundle.LoadFromStreamAsync(assetStream);

            } catch {
                Debug.LogError("WaterFight: Failed to load assets from stream");

                loadingPrefabs = null;
                yield break;
            }

            yield return assetLoader;

            var assetBundle = assetLoader.assetBundle;
            if (assetBundle == null) {
                Debug.Log("failed to load assets");

                loadingPrefabs = null;
                yield break;
            }

            superSoakerPrefab = assetBundle.LoadAsset<GameObject>("SuperSoaker");
            if (superSoakerPrefab == null) {
                Debug.LogWarning("failed to load supersoaker from the asset bundle");

            } else {
                GameObject.DontDestroyOnLoad(superSoakerPrefab);
            }

            var handOffsets = assetBundle.LoadAsset<GameObject>("HandParents")?.GetComponent<ParentOffsets>();
            if (handOffsets != null) {
                rightHandOffsetPrefab = handOffsets.RightHandParent;
                leftHandOffsetPrefab = handOffsets.LeftHandParent;

                GameObject.DontDestroyOnLoad(rightHandOffsetPrefab);
                GameObject.DontDestroyOnLoad(leftHandOffsetPrefab);
            
            } else {
                Debug.LogError("failed to load hand offsets from assetbundle");
            }

            assetBundle.Unload(false);

            loadingPrefabs = null;
            prefabsLoaded = true;
            yield return null;
        }
    }
}
