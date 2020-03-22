using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(ITab_Pawn_Character), "get_IsVisible")]
    class ITab_Pawn_Character_IsVisible
    {
        static bool Prefix(ITab_Pawn_Character __instance, ref bool __result)
        {
            Pawn pawn = Traverse.Create(__instance).Property("PawnToShowInfoAbout").GetValue<Pawn>();
            if (pawn.IsHacked())
            {
                //__result = false;
                //return false;
            }
            return true;
        }
    }
}
