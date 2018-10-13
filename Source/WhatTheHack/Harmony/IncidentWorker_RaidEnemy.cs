using Harmony;
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
            if(parms.target != null && parms.target.IncidentTargetTags().Contains(IncidentTargetTagDefOf.Map_RaidBeacon))
            {
                Log.Message("TryResolveRaidFaction patch called");
                __result = true;
                parms.faction = Faction.OfMechanoids;
                return false;
            }
            return true;
        }
    }
}
