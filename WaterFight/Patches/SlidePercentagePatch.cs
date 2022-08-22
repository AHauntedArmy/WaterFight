using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using WaterFight.GameMode;

namespace WaterFight.Patches
{   
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("GetSlidePercentage", MethodType.Normal)]
    internal class SlidePercentagePatch
    {
        // const float SlipModifier = 0.07f;

        static void Postfix(ref float __result)
        {
            // Console.WriteLine("Slide Percentage is " + __result);
            if (!WaterFightPlugin.inRoom) {
                return;
            }

            if (GorillaGameManager.instance is WaterFightGameMode waterFight) {
                __result += waterFight.GetSlipModifier();
            }
        }
    }
}
