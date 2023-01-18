using System.Linq;
using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), "SetDead")]
internal static class Pawn_HealthTracker_SetDead
{
    private static void Postfix(Pawn_HealthTracker __instance)
    {
        var removedHediffs = __instance.hediffSet.hediffs.FindAll(h =>
            h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
            Rand.Chance(modExt.destroyOnDeathChance));
        foreach (var hediff in removedHediffs)
        {
            __instance.AddHediff(WTH_DefOf.WTH_DestroyedModule, hediff.Part);
        }

        __instance.hediffSet.hediffs = __instance.hediffSet.hediffs.Except(removedHediffs).ToList();
    }
}

//Make sure mechanoids can be downed like other pawns. 

//Deactivates mechanoid when downed
//Recharge and repair mechanoid when on platform
//TODO: refactor. Move all needs related stuff to something needs related