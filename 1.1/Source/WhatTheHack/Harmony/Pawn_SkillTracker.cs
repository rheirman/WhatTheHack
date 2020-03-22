using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_SkillTracker), "Learn")]
    class Pawn_SkillTracker_Learn
    {
        static bool Prefix(Pawn_SkillTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.IsHacked())
            {
                return false;
            }
            return true;
        }
    }
}
