using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
   
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
    class IncidentWorker_RaidEnemy_TryResolveRaidFaction
    {
        static bool Prefix(ref IncidentParms parms, ref bool __result)
        {
            Map map = (Map)parms.target;
            
            if (parms.target != null && parms.target.IncidentTargetTags().Contains(IncidentTargetTagDefOf.Map_RaidBeacon))
            {
                foreach (ThingWithComps current in map.listerThings.ThingsOfDef(WTH_DefOf.WTH_MechanoidBeacon))
                {
                    CompHibernatable compHibernatable = current.TryGetComp<CompHibernatable>();
                    if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Starting && Rand.Chance(0.85f))
                    {
                        __result = true;
                        parms.faction = Faction.OfMechanoids;                      
                        return false;
                    }
                }
            }
            return true;
        }
    }
    
}
