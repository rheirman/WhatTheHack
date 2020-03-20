using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{

    
    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class PawnComponentsUtility_AddAndRemoveDynamicComponents
    {    
        static void Postfix(Pawn pawn)
        {
            //These two flags detect if the creature is part of the colony and if it has the custom class
            bool flagIsCreatureMine = pawn.Faction != null && pawn.Faction.IsPlayer;
            bool flagIsCreatureDraftable = (pawn.IsHacked());


            if (flagIsCreatureMine && flagIsCreatureDraftable)
            {
                //If everything goes well, add drafter to the pawn 
                pawn.drafter = new Pawn_DraftController(pawn);
            }
        }
    }

    
}
