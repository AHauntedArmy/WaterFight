using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace WaterFight.GameMode.WaterGun
{
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class WaterSettingsContainer
    {
        private float waterDecayRate;
        [JsonProperty] private float waterDecaySeconds = 15f;

        private float calculatedMaxWaterSpeed = 20f;
        private float waterSpeedPercentage;
        [JsonProperty] private float maxWaterSpeed = 20f;
        [JsonProperty] private float minWaterSpeed = 10f;

        private float calculatePressureModifier = 3f;
        [JsonProperty] private float maxPressureModifier = 7f;
        [JsonProperty] private float minPressureModifier = 3f;

        public float WaterSpeedPercentage => waterSpeedPercentage;
        public float MaxWaterSpeed => calculatedMaxWaterSpeed;
        public float PressureModifier => calculatePressureModifier;
        public float WaterDecayRate => waterDecayRate;

        [JsonConstructor]
        public WaterSettingsContainer() => CalculateRates();
        public WaterSettingsContainer(WaterSettingsContainer source)
        {
            waterDecaySeconds = 0f;
            waterDecaySeconds = source.waterDecaySeconds;
            
            maxWaterSpeed = source.maxWaterSpeed;
            minWaterSpeed = source.minWaterSpeed;

            maxPressureModifier = source.maxPressureModifier;
            minPressureModifier = source.minPressureModifier;

            CalculateRates();
        }

        private void CalculateRates()
        {
            waterDecayRate = calculatedMaxWaterSpeed / waterDecaySeconds;
            waterSpeedPercentage = 1f /calculatedMaxWaterSpeed;
        }

        public void ModifyByPercent(WaterFightGameMode gameModeInstance, float percent)
        {
            // so only the game mode can modify the settings
            if (gameModeInstance == null) {
                return;
            }

            float waterSpeedDifference = maxWaterSpeed - minWaterSpeed;
            waterSpeedDifference *= percent;
            calculatedMaxWaterSpeed = maxWaterSpeed - waterSpeedDifference;

            float pressureModifierDifference = maxPressureModifier - minPressureModifier;
            pressureModifierDifference *= percent;
            calculatePressureModifier = maxPressureModifier - pressureModifierDifference;

            CalculateRates();

            // Console.WriteLine("calculated pressure " + calculatePressureModifier);
            // Console.WriteLine("calculated water speed " + calculatedMaxWaterSpeed);
        }
    }
}
