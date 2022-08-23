using System;
using System.Collections;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using PhotonHashTable = ExitGames.Client.Photon.Hashtable;

using BepInEx;

using Utilla;

using WaterFight.Patches;
using WaterFight.GameMode;
using WaterFight.Input;
using WaterFight.Tools;

namespace WaterFight
{
    [ModdedGamemode("WATERFIGHT", "Water Fight", typeof(WaterFightGameMode))]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class WaterFightPlugin : BaseUnityPlugin
    {
        public const int MaxPlayers = 10;      
        public static bool inRoom = false;
        public static bool EnableCosmetics = true;

        private PrefabManager prefabManager;

        async void Awake()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;            
            HarmonyPatches.ApplyHarmonyPatches();

            await WaterFight.Config.Load();
        }

        void OnEnable()
        {
            EnableCosmetics = true;

            if (!inRoom) {
                prefabManager?.WaterGunCosmetic?.SetActive(true);
            }
        }

        void OnDisable()
        {
            EnableCosmetics = false;
            prefabManager?.WaterGunCosmetic?.SetActive(false);
            // HarmonyPatches.RemoveHarmonyPatches();
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            // so we can verify people joining have the mod installed
            PhotonHashTable property = new PhotonHashTable();
            property.Add(WaterFightGameMode.GameModeKey, (byte)1);
            PhotonNetwork.LocalPlayer.SetCustomProperties(property);

            this.gameObject.AddComponent<InputHandler>();
            prefabManager = this.gameObject.AddComponent<PrefabManager>();           
        }    

        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            // if important assets aren't loaded, forcefully disconnect.
            if(Prefabs.FailedToLoad || !WaterFight.Config.Loaded) {
                PhotonNetwork.Disconnect();
                return;
            }

            prefabManager?.AttachPrefabs();
            prefabManager?.WaterGunCosmetic?.SetActive(false);

            inRoom = true;
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            inRoom = false;

            prefabManager?.RemovePrefabs();
            prefabManager?.WaterGunCosmetic?.SetActive(this.enabled);
        }
    }
}
