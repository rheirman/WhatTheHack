using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Bill_Medical), "ShouldDoNow")]
    class Bill_Medical_ShouldDoNow
    {
        static void Postfix(Bill_Medical __instance, ref bool __result)
        {
            Pawn pawn = Traverse.Create(__instance).Property("GiverPawn").GetValue<Pawn>();
            if(__instance.recipe == WTH_DefOf.HackMechanoid && pawn.OnHackingTable() && !((Building_HackingTable)pawn.CurrentBed()).HasPowerNow())
            {
                __result = false;
            }
        }
    }
}
