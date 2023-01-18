using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony;

//Don't allow multiple belts for mechs with belt modules. Mechs often don't have a waist, and vanilla CanWearTogether therefore always returns true for belt items.
//Almost literal copy of JobDriver_Wear_TryUnequipSomething, but applies for hacked mechs only, and disallows multiple belts. 
[HarmonyPatch(typeof(JobDriver_Wear), "TryUnequipSomething")]
internal class JobDriver_Wear_TryUnequipSomething
{
    private static bool Prefix(ref JobDriver_Wear __instance)
    {
        if (!__instance.pawn.IsHacked())
        {
            return true;
        }

        //var apparel = Traverse.Create(__instance).Property("Apparel").GetValue<Apparel>();
        var wornApparel = __instance.pawn.apparel.WornApparel;
        foreach (var wornApp in wornApparel)
        {
            if (!BothBelts(__instance.Apparel.def, wornApp.def))
            {
                continue;
            }

            var forbid = __instance.pawn.Faction != null && __instance.pawn.Faction.HostileTo(Faction.OfPlayer);
            if (!__instance.pawn.apparel.TryDrop(wornApp, out _, __instance.pawn.PositionHeld, forbid))
            {
                Log.Error($"{__instance.pawn} could not drop {wornApp.ToStringSafe()}");
                __instance.EndJobWith(JobCondition.Errored);
                return false;
            }

            break;
        }

        return true;
    }

    private static bool BothBelts(ThingDef A, ThingDef B)
    {
        return Utilities.IsBelt(A.apparel) && Utilities.IsBelt(B.apparel);
    }
}