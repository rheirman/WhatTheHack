using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(IncidentWorker_ShortCircuit), "TryExecuteWorker")]
    class IncidentWorker_ShortCircuit_TryExcecuteWorker
    {
        static bool Prefix(IncidentWorker_ShortCircuit __instance, IncidentParms parms)
        {
            Map map = (Map) parms.target;
            if (map.spawnedThings.FirstOrDefault((Thing t) => t is Building_RogueAI) is Building_RogueAI controller && controller.managingPowerNetwork)
            {
                controller.DrainMood(controller.moodDrainPreventZzztt);
                Find.LetterStack.ReceiveLetter(label: "WTH_Letter_PreventedShortCircuit_Label".Translate(), text: "WTH_Letter_PreventedShortCircuit_Description".Translate(), textLetterDef: LetterDefOf.NeutralEvent);
                return false;
            }
            return true;
        }
    }
}
