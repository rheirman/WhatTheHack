using HarmonyLib;
using RimWorld;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Dialog_LoadTransporters), "CountToTransferChanged")]
internal class Dialog_LoadTransporters_CountToTransferChanged
{
    private static void Postfix(Dialog_LoadTransporters __instance)
    {
        //Traverse.Create(__instance).Field("transferables").GetValue<List<TransferableOneWay>>();
        Utilities.CalcDaysOfFuel(__instance.transferables);
    }
}