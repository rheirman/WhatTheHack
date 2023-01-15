using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
{
    private static void Postfix(Pawn pawn)
    {
        //These two flags detect if the creature is part of the colony and if it has the custom class
        var flagIsCreatureMine = pawn.Faction is { IsPlayer: true };
        var flagIsCreatureDraftable = pawn.IsHacked();


        if (flagIsCreatureMine && flagIsCreatureDraftable)
        {
            //If everything goes well, add drafter to the pawn 
            pawn.drafter = new Pawn_DraftController(pawn);
        }
    }
}