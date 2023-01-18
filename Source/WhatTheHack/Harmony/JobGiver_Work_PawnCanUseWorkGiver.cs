using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(JobGiver_Work), "PawnCanUseWorkGiver")]
internal class JobGiver_Work_PawnCanUseWorkGiver
{
    //Make mechs with work modules able to do work. Some duplicate code here in regards to vanilla, but I fear a transpiler would mess up compatibility with other mods. 
    private static void Postfix(ref bool __result, Pawn pawn, WorkGiver giver)
    {
        if (__result == false)
        {
            __result =
                (giver.def.nonColonistsCanDo || pawn.IsColonist || pawn.IsHacked() && pawn.workSettings != null) &&
                (pawn.story == null || !pawn.WorkTypeIsDisabled(giver.def.workType)) && !giver.ShouldSkip(pawn) &&
                giver.MissingRequiredCapacity(pawn) == null;
        }
    }
}