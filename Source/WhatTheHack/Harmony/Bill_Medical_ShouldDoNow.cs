using HarmonyLib;
using RimWorld;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Bill_Medical), "ShouldDoNow")]
internal class Bill_Medical_ShouldDoNow
{
    private static void Postfix(Bill_Medical __instance, ref bool __result)
    {
        var pawn = __instance.GiverPawn;
        //Traverse.Create(__instance).Property("GiverPawn").GetValue<Pawn>();
        if (__instance.recipe == WTH_DefOf.WTH_HackMechanoid &&
            pawn.CurrentBed() is Building_HackingTable hackingTable && !hackingTable.HasPowerNow())
        {
            __result = false;
        }
        /*
        if (__instance.recipe == WTH_DefOf.WTH_HackMechanoid && pawn.OnHackingTable() && !((Building_HackingTable)pawn.CurrentBed()).HasPowerNow())
        {
            __result = false;
        }
        */
    }
}