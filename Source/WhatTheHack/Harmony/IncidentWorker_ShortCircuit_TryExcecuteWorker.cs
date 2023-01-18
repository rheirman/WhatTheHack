using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(IncidentWorker_ShortCircuit), "TryExecuteWorker")]
internal class IncidentWorker_ShortCircuit_TryExcecuteWorker
{
    private static bool Prefix(IncidentParms parms)
    {
        var map = (Map)parms.target;
        if (map.spawnedThings.FirstOrDefault(t => t is Building_RogueAI) is not Building_RogueAI
            {
                managingPowerNetwork: true
            } controller)
        {
            return true;
        }

        controller.DrainMood(controller.moodDrainPreventZzztt);
        Find.LetterStack.ReceiveLetter("WTH_Letter_PreventedShortCircuit_Label".Translate(),
            "WTH_Letter_PreventedShortCircuit_Description".Translate(), LetterDefOf.NeutralEvent);
        return false;
    }
}