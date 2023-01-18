using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(DaysWorthOfFoodCalculator), "ApproxDaysWorthOfFood")]
[HarmonyPatch(new[]
{
    typeof(List<Pawn>), typeof(List<ThingDefCount>), typeof(int), typeof(IgnorePawnsInventoryMode), typeof(Faction),
    typeof(WorldPath), typeof(float), typeof(int), typeof(bool)
})]
internal class DaysWorthOfFoodCalculator_ApproxDaysWorthOfFood
{
    private static void Prefix(ref List<Pawn> pawns)
    {
        var validPawns = new List<Pawn>();
        foreach (var pawn in pawns)
        {
            if (!pawn.IsHacked())
            {
                validPawns.Add(pawn);
            }
        }

        pawns = validPawns;
    }
}