using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Dialog_SplitCaravan), "CountToTransferChanged")]
    class Dialog_SplitCaravan_CountToTransferChanged
    {
        static void Postfix(Dialog_SplitCaravan __instance)
        {
            List<TransferableOneWay> transferables = Traverse.Create(__instance).Field("transferables").GetValue<List<TransferableOneWay>>();
            Utilities.CalcDaysOfFuel(transferables);
        }
    }
}
