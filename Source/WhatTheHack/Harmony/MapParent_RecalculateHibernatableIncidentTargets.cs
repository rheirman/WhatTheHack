using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WhatTheHack.Harmony;

//Make sure mech beacons work.
[HarmonyPatch(typeof(MapParent), "RecalculateHibernatableIncidentTargets")]
internal class MapParent_RecalculateHibernatableIncidentTargets
{
    private static void Postfix(MapParent __instance, ref HashSet<IncidentTargetTagDef> ___hibernatableIncidentTargets)
    {
        foreach (var current in __instance.Map.listerThings.ThingsOfDef(WTH_DefOf.WTH_MechanoidBeacon)
                     .OfType<ThingWithComps>())
        {
            var compHibernatable = current.TryGetComp<CompHibernatable>();
            if (compHibernatable == null || compHibernatable.State != HibernatableStateDefOf.Starting ||
                compHibernatable.Props.incidentTargetWhileStarting == null)
            {
                continue;
            }

            if (___hibernatableIncidentTargets == null)
            {
                ___hibernatableIncidentTargets = new HashSet<IncidentTargetTagDef>();
            }

            ___hibernatableIncidentTargets.Add(compHibernatable.Props.incidentTargetWhileStarting);
        }
    }
}