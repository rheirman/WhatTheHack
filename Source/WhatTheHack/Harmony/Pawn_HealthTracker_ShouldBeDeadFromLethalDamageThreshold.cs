using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDeadFromLethalDamageThreshold")]
internal static class Pawn_HealthTracker_ShouldBeDeadFromLethalDamageThreshold
{
    private static void Postfix(Pawn_HealthTracker __instance, ref bool __result)
    {
        var pawn = __instance.pawn;
        //Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if (!pawn.IsMechanoid())
        {
            return;
        }

        if (pawn.Faction != Faction.OfPlayer &&
            !pawn.Faction.HostileTo(Faction
                .OfPlayer)) //make sure allied mechs always die to prevent issues with relation penalties when the player hacks their mechs. 
        {
            return;
        }

        if (__result)
        {
            if (__instance.hediffSet.HasHediff(WTH_DefOf.WTH_HeavilyDamaged))
            {
                __result = false;
                return;
            }

            if (!Rand.Chance(Base.downedOnDeathThresholdChance.Value /
                             100f)) //Chance mech goes down instead of dying when lethal threshold is achieved. 
            {
                return;
            }

            __instance.AddHediff(WTH_DefOf.WTH_HeavilyDamaged);
            if (pawn.mindState == null)
            {
                pawn.mindState = new Pawn_MindState(pawn);
            }

            __result = false;
        }
        else
        {
            if (__instance.hediffSet.HasHediff(WTH_DefOf.WTH_HeavilyDamaged))
            {
                __instance.RemoveHediff(__instance.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_HeavilyDamaged));
            }
        }
    }
}