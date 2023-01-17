using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Dialog_LoadTransporters), "TryAccept")]
    class Dialog_LoadTransporters_TryAccept
    {
        static void Postfix(Dialog_LoadTransporters __instance, bool __result, ref List<TransferableOneWay> ___transferables)
        {
            if (__result)
            {
                ___transferables = Utilities.LinkPortablePlatforms(___transferables);
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_LoadTransporters), "CountToTransferChanged")]
    class Dialog_LoadTransporters_CountToTransferChanged
    {
        static void Postfix(Dialog_LoadTransporters __instance)
        {
            List<TransferableOneWay> transferables = Traverse.Create(__instance).Field("transferables").GetValue<List<TransferableOneWay>>();
            Utilities.CalcDaysOfFuel(transferables);
        }
    }
}
