using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace WhatTheHack.Harmony;

//Make sure mechanoid temple quest is started when minerals are found when the scanner is tuned to mech parts.  
[HarmonyPatch(typeof(CompLongRangeMineralScanner), "DoFind")]
public static class CompLongRangeMineralScanner_Foundminerals
{
    private const int MinDistance = 6;
    private const int MaxDistance = 22;
    private static readonly IntRange TimeoutDaysRange = new IntRange(25, 50);

    private static bool Prefix(CompLongRangeMineralScanner __instance, Pawn worker, ref ThingDef ___targetMineable)
    {
        if (__instance == null)
        {
            return true;
        }

        if (___targetMineable == null)
        {
            return true;
        }

        if (___targetMineable != WTH_DefOf.WTH_MineableMechanoidParts)
        {
            return true;
        }

        //    Traverse.Create(__instance).Field("daysWorkingSinceLastMinerals").SetValue(0f);
        if (!TileFinder.TryFindNewSiteTile(out _, MinDistance, MaxDistance, true))
        {
            return false;
        }

        var slate = new Slate();
        slate.Set("map", worker.Map);
        slate.Set("targetMineable", ___targetMineable);
        slate.Set("worker", worker);
        if (!WTH_DefOf.WTH_LongRangeMineralScannerMechParts.CanRun(slate))
        {
            return true;
        }

        var quest = QuestUtility.GenerateQuestAndMakeAvailable(WTH_DefOf.WTH_LongRangeMineralScannerMechParts,
            slate);
        Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null,
            quest);
        return false;
    }
}