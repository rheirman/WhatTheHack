using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(LordToil_Siege), "CanBeBuilder")]
    class LordToil_Siege_CanBeBuilder
    {
        static bool Prefix(Pawn p, ref bool __result)
        {
            if (p.IsHacked())
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
