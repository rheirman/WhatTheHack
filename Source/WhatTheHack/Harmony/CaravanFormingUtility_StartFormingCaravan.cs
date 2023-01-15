using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CaravanFormingUtility), "StartFormingCaravan")]
internal static class CaravanFormingUtility_StartFormingCaravan
{
    private static void Postfix(List<Pawn> pawns)
    {
        foreach (var pawn in pawns)
        {
            if (!pawn.IsHacked())
            {
                continue;
            }

            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            pawnData.isActive = true;
        }
    }
}