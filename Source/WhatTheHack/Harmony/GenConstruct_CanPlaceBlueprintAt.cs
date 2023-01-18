using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintAt")]
internal class GenConstruct_CanPlaceBlueprintAt
{
    private static void Postfix(ref AcceptanceReport __result, BuildableDef entDef, Map map)
    {
        if (entDef != WTH_DefOf.WTH_RogueAI)
        {
            return;
        }

        if (map.listerBuildings.allBuildingsColonist.FirstOrDefault(b => b is Building_RogueAI) is Building_RogueAI)
        {
            __result = new AcceptanceReport("WTH_Reason_RogueAIExists".Translate());
        }
    }
}