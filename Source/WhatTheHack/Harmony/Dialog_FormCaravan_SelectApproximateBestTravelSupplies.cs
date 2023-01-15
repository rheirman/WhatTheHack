using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Dialog_FormCaravan), "SelectApproximateBestTravelSupplies")]
internal static class Dialog_FormCaravan_SelectApproximateBestTravelSupplies
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        for (var i = 0; i < instructionsList.Count; i++)
        {
            var instruction = instructionsList[i];
            if (instruction.operand as MethodInfo == AccessTools.Method(typeof(WildManUtility), "AnimalOrWildMan"))
            {
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(Dialog_FormCaravan_SelectApproximateBestTravelSupplies)
                        .GetMethod("AnimalOrWildManOrHacked"));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static bool AnimalOrWildManOrHacked(Pawn pawn)
    {
        return pawn.AnimalOrWildMan() || pawn.IsHacked();
    }
}