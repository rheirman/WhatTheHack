using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Dialog_SplitCaravan), "CountToTransferChanged")]
internal class Dialog_SplitCaravan_CountToTransferChanged
{
    private static void Postfix(ref List<TransferableOneWay> ___transferables)
    {
        Utilities.CalcDaysOfFuel(___transferables);
    }
}