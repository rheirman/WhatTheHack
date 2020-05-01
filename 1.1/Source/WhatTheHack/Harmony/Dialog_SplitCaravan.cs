using HarmonyLib;
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
        static void Postfix(Dialog_SplitCaravan __instance, ref List<TransferableOneWay> ___transferables)
        {
            Utilities.CalcDaysOfFuel(___transferables);
        }
    }
}
