using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    //This patch improves compatibility with other mods. For mechs added by some mods HasAnyLovePartner is called when the mech lies down at a mechanoid platform, causing errors. 
    [HarmonyPatch(typeof(LovePartnerRelationUtility), "HasAnyLovePartner")]
    class LovePartnerRelationUtility_HasAnyLovePartner
    {
        static bool Prefix(Pawn pawn)
        {
            if(pawn.RaceProps.IsMechanoid && pawn.IsHacked())
            {
                return false;
            }
            return true;
        }
    }
}
