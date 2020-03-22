using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(SelfDefenseUtility), "ShouldStartFleeing")]
    class SelfDefenceUtility_ShouldStartFleeing
    {
        static bool Prefix(Pawn pawn, ref bool __result)
        {
            if(pawn.RemoteControlLink() != null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
