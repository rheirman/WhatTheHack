using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(TransferableUtility),"CanStack")]
    class TransferableUtility_CanStack
    {
        static bool Prefix(Thing thing, ref bool __result)
        {
            if(thing is Pawn && ((Pawn)thing).IsHacked())
            {
                __result = false; 
                return false;
            }
            return true;
        }
    }
}
