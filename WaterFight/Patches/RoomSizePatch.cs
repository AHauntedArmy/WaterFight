using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaterFight.Patches
{
    [HarmonyPatch(typeof(GorillaNetworking.PhotonNetworkController))]
    [HarmonyPatch("GetRoomSize", MethodType.Normal)]
    internal class RoomSizePatch
    {
        private static void Postfix(ref string gameModeName, ref byte __result)
        {
            if(PhotonNetwork.InRoom) {
                if (!WaterFightPlugin.inRoom) {
                    return;

                } else {
                    __result = WaterFightPlugin.MaxPlayers;
                }
            
            } else if (GorillaComputer.instance.currentGameMode.Contains("WATERFIGHT")) {
                __result = WaterFightPlugin.MaxPlayers;
            }

            // Console.WriteLine("water fight game mode name " + gameModeName);
        }
    }
}
