using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(ShipUtility), "ShipStartupGizmos")]
    class ShipUtility_ShipStartupGizmos
    {
        static void Postfix(ref IEnumerable<Gizmo> __result)
        {
            
            List<Gizmo> modifiedGizmos = new List<Gizmo>();
            bool shouldDisable = false;
            foreach (Map map in Find.Maps)
            {
                foreach (Building_MechanoidBeacon beacon in map.listerBuildings.AllBuildingsColonistOfDef(WTH_DefOf.WTH_MechanoidBeacon).Cast<Building_MechanoidBeacon>())
                {
                    if(beacon.GetComp<CompHibernatable>().State == HibernatableStateDefOf.Starting)
                    {
                        shouldDisable = true;
                    }
                }
            }
            foreach (Gizmo gizmo in __result)
            {
                gizmo.disabled = shouldDisable;
                gizmo.disabledReason = "WTH_Reason_BeaconActive".Translate();
                modifiedGizmos.Add(gizmo);
            }
            __result = modifiedGizmos;
            
        }
    }
}
