using Harmony;
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
            Log.Message("calling CanPlaceBlueprintAt Postfix");
            if (entDef == WTH_DefOf.WTH_RogueAI)
            {
                if(map.listerBuildings.allBuildingsColonist.FirstOrDefault((Building b) => b is Building_RogueAI) is Building_RogueAI rogueAI)
                {
                    Log.Message("CanPlaceBlueprintAt not allowed");
                    __result = new AcceptanceReport("WTH_Reason_RogueAIExists".Translate());
                }
            }
        }
    }
}
