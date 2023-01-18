using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(ShipUtility), "ShipStartupGizmos")]
internal class ShipUtility_ShipStartupGizmos
{
    private static void Postfix(ref IEnumerable<Gizmo> __result)
    {
        var modifiedGizmos = new List<Gizmo>();
        var shouldDisable = false;
        foreach (var map in Find.Maps)
        {
            foreach (var beacon in map.listerBuildings.AllBuildingsColonistOfDef(WTH_DefOf.WTH_MechanoidBeacon)
                         .Cast<Building_MechanoidBeacon>())
            {
                if (beacon.GetComp<CompHibernatable>().State == HibernatableStateDefOf.Starting)
                {
                    shouldDisable = true;
                }
            }
        }

        foreach (var gizmo in __result)
        {
            gizmo.disabled = shouldDisable;
            gizmo.disabledReason = "WTH_Reason_BeaconActive".Translate();
            modifiedGizmos.Add(gizmo);
        }

        __result = modifiedGizmos;
    }
}