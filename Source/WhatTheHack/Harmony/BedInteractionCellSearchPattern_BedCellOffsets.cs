using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(BedInteractionCellSearchPattern), "BedCellOffsets")]
internal static class BedInteractionCellSearchPattern_BedCellOffsets
{
    private static bool Prefix(List<IntVec3> offsets, IntVec2 size)
    {
        if (size != new IntVec2(3, 3))
        {
            return true;
        }

        offsets.Add(IntVec3.Zero);

        return false;
    }
}