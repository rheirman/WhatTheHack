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
    [HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintAt")]
    class GenConstruct_CanPlaceBlueprintAt
    {
        static void Postfix(ref AcceptanceReport __result, BuildableDef entDef, Map map)
        {
            if (entDef == WTH_DefOf.WTH_RogueAI)
            {
                if(map.listerBuildings.allBuildingsColonist.FirstOrDefault((Building b) => b is Building_RogueAI) is Building_RogueAI rogueAI)
                {
                    __result = new AcceptanceReport("WTH_Reason_RogueAIExists".Translate());
                }
            }
        }
    }
}
