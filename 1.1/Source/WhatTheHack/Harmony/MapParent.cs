using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    //Make sure mech beacons work.
    [HarmonyPatch(typeof(MapParent), "RecalculateHibernatableIncidentTargets")]
    class MapParent_RecalculateHibernatableIncidentTargets
    {
        static void Postfix(MapParent __instance)
        {
            foreach (ThingWithComps current in __instance.Map.listerThings.ThingsOfDef(WTH_DefOf.WTH_MechanoidBeacon).OfType<ThingWithComps>())
            {
                CompHibernatable compHibernatable = current.TryGetComp<CompHibernatable>();
                if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Starting && compHibernatable.Props.incidentTargetWhileStarting != null)
                {
                    HashSet<IncidentTargetTagDef> hibernatableIncidentTargets = Traverse.Create(__instance).Field("hibernatableIncidentTargets").GetValue<HashSet<IncidentTargetTagDef>>();
                    if (hibernatableIncidentTargets == null)
                    {
                        hibernatableIncidentTargets = new HashSet<IncidentTargetTagDef>();
                    }
                    hibernatableIncidentTargets.Add(compHibernatable.Props.incidentTargetWhileStarting);
                    Traverse.Create(__instance).Field("hibernatableIncidentTargets").SetValue(hibernatableIncidentTargets);
                }
            }
        }
    }
}
