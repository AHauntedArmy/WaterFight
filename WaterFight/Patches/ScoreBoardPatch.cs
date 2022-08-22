using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;

namespace WaterFight.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class ScoreBoardPatch
    {
        internal static void Postfix(GorillaScoreBoard __instance)
        {
            __instance.boardText.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
        }
    }
}
