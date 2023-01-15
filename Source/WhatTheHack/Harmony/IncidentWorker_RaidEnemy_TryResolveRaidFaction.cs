using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
internal class IncidentWorker_RaidEnemy_TryResolveRaidFaction
{
    private static bool Prefix(ref IncidentParms parms, ref bool __result)
    {
        var map = (Map)parms.target;

        if (parms.target == null || !parms.target.IncidentTargetTags().Contains(IncidentTargetTagDefOf.Map_RaidBeacon))
        {
            return true;
        }

        foreach (var thing in map.listerThings.ThingsOfDef(WTH_DefOf.WTH_MechanoidBeacon))
        {
            var current = (ThingWithComps)thing;
            var compHibernatable = current.TryGetComp<CompHibernatable>();
            if (compHibernatable == null || compHibernatable.State != HibernatableStateDefOf.Starting ||
                !Rand.Chance(0.85f))
            {
                continue;
            }

            __result = true;
            parms.faction = Faction.OfMechanoids;
            return false;
        }

        return true;
    }
}