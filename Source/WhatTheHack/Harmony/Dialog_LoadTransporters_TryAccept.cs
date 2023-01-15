using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Dialog_LoadTransporters), "TryAccept")]
internal class Dialog_LoadTransporters_TryAccept
{
    private static void Postfix(bool __result, ref List<TransferableOneWay> ___transferables)
    {
        if (__result)
        {
            ___transferables = Utilities.LinkPortablePlatforms(___transferables);
        }
    }
}