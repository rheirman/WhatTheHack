using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

//This patch improves compatibility with other mods. For mechs added by some mods HasAnyLovePartner is called when the mech lies down at a mechanoid platform, causing errors. 
[HarmonyPatch(typeof(LovePartnerRelationUtility), "HasAnyLovePartner")]
internal class LovePartnerRelationUtility_HasAnyLovePartner
{
    private static bool Prefix(Pawn pawn)
    {
        if (pawn.IsMechanoid() && pawn.IsHacked())
        {
            return false;
        }

        return true;
    }
}