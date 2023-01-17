using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(StrippableUtility), "CanBeStrippedByColony")]
    class WorkGiver_Strip_HasJobOnThing
    {
        static bool Prefix(Thing th, ref bool __result)
        {
            if(th is Pawn){
                Pawn pawn = (Pawn)th;
                if(pawn.RaceProps.IsMechanoid)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
