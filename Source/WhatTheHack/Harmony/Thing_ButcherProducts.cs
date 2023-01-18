using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Thing), "ButcherProducts")]
internal static class Thing_ButcherProducts
{
    private static void Postfix(Thing __instance, ref IEnumerable<Thing> __result, float efficiency)
    {
        if (__instance is not Pawn pawn || !pawn.IsMechanoid())
        {
            return;
        }

        __result = GenerateExtraButcherProducts(__result, pawn, efficiency);
    }

    private static IEnumerable<Thing> GenerateExtraButcherProducts(IEnumerable<Thing> things, Pawn pawn,
        float efficiency)
    {
        foreach (var thing in things)
        {
            yield return thing;
        }

        var random = new Random(DateTime.Now.Millisecond);

        var combatpowerCapped = pawn.kindDef.combatPower <= 10000 ? pawn.kindDef.combatPower : 300;
        var baseSpawnRateParts = combatpowerCapped * GetDifficultyFactor() * Base.partDropRateModifier;
        var baseSpawnRateChips = combatpowerCapped * GetDifficultyFactor() * Base.chipDropRateModifier;

        var partsCount = random.Next(GenMath.RoundRandom(baseSpawnRateParts * 0.04f * efficiency),
            GenMath.RoundRandom(baseSpawnRateParts * 0.065f * efficiency)); //TODO: no magic number
        if (partsCount > 0)
        {
            var parts = ThingMaker.MakeThing(WTH_DefOf.WTH_MechanoidParts);
            parts.stackCount = partsCount;
            yield return parts;
        }

        var chipCount =
            random.Next(0, GenMath.RoundRandom(baseSpawnRateChips * 0.012f * efficiency)); //TODO: no magic number
        if (chipCount > 0)
        {
            var chips = ThingMaker.MakeThing(WTH_DefOf.WTH_MechanoidChip);
            chips.stackCount = chipCount;
            yield return chips;
        }

        foreach (var hediff in pawn.health.hediffSet.hediffs)
        {
            if (hediff.def.GetModExtension<DefModextension_Hediff>() is { extraButcherProduct: { } } ext)
            {
                yield return ThingMaker.MakeThing(ext.extraButcherProduct);
            }
        }
    }

    private static float GetDifficultyFactor()
    {
        var difficultyFactor = 1.0f;
        if (Find.Storyteller.difficultyDef == WTH_DefOf.Peaceful)
        {
            difficultyFactor = 2.0f;
        }
        else if (Find.Storyteller.difficultyDef == WTH_DefOf.Easy)
        {
            difficultyFactor = 1.5f;
        }
        else if (Find.Storyteller.difficultyDef == WTH_DefOf.Medium)
        {
            difficultyFactor = 1.35f;
        }
        else if (Find.Storyteller.difficultyDef == WTH_DefOf.Rough)
        {
            difficultyFactor = 1.2f;
        }
        else if (Find.Storyteller.difficultyDef == WTH_DefOf.Hard)
        {
            difficultyFactor = 1.1f;
        }

        return difficultyFactor;
    }
}