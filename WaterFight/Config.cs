using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;

using WaterFight.GameMode.WaterGun;
using WaterFight.Tools;
using Unity.Mathematics;
using System.ComponentModel;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace WaterFight
{
    internal static class Config
    {
        private const string WaterSettingsFileName = "WaterSettings.Json";
        private const string WaterSettingsURL = "https://raw.githubusercontent.com/AHauntedArmy/WaterFight/master/WaterFight/WaterSettings.Json";
        private static WaterSettingsContainer waterSettings = null;

        private static bool configLoaded = false;

        public static bool Loaded => configLoaded;
        public static WaterSettingsContainer WaterSettings => waterSettings;

        public static async Task Load()
        {
            string serializedWaterSettings = null;
            string downloadedWaterSettings = await FetchWaterSettings();

            if (downloadedWaterSettings != null) {
                serializedWaterSettings = downloadedWaterSettings;
            
            } else {
                Debug.LogWarning("failed to download settings, loading from file instead");

                serializedWaterSettings = FileTools.ReadAllText(WaterSettingsFileName);

                if (serializedWaterSettings == null) {
                    Debug.LogError("WaterFight: water settings are missing");
                    configLoaded = false;
                    return;
                }
            }


            // Debug.Log("config static construct called");
            // WaterSettingsContainer waterSettings = new WaterSettingsContainer();
            // string serializedSettings = JsonConvert.SerializeObject(waterSettings, Formatting.Indented);
            // Debug.Log(serializedSettings);

            try {
                var deserializedSettings = JsonConvert.DeserializeObject<WaterSettingsContainer>(serializedWaterSettings);
                waterSettings = new WaterSettingsContainer(deserializedSettings);

            } catch (Exception ex) {
                Debug.LogError(ex.ToString());
                configLoaded = false;
            }

            if (FileTools.WriteAllText(WaterSettingsFileName, serializedWaterSettings)) {
                configLoaded = true;
            
            } else {
                configLoaded = false;
            }
        }

        public static async Task<string> FetchWaterSettings()
        {
            try {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

                string serializedSettings = await httpClient.GetStringAsync(WaterSettingsURL);
                //Debug.Log("testing async 1 2 3");
                return serializedSettings;
            
            } catch {
                Debug.LogWarning("WaterFight: failed to download settings");
                return null;
            }
        }


    }
}
