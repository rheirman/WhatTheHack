using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_CallTracker), "DoCall")]
    class Pawn_CallTracker_DoCall
    {
        static bool Prefix(Pawn_CallTracker __instance)
        {
            if(__instance.pawn.IsHacked() && __instance.pawn.OnBaseMechanoidPlatform() || __instance.pawn.OnHackingTable())
            {
                return false;
            }
            return true;
        }
    }
}
