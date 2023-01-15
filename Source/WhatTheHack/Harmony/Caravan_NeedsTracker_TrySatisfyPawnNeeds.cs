using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WhatTheHack.Harmony;

// Patch that prevents a null reference error when a pawn doesn't have psychic entropy (which is always the case for mechs). 
[HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
internal static class Caravan_NeedsTracker_TrySatisfyPawnNeeds
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        var label = ilg.DefineLabel();

        var instructionsList = new List<CodeInstruction>(instructions);
        for (var i = 0; i < instructionsList.Count; i++)
        {
            var instruction = instructionsList[i];
            yield return instruction;

            if (instruction.operand as FieldInfo != AccessTools.Field(typeof(Pawn), "psychicEntropy"))
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Dup);
            yield return new CodeInstruction(OpCodes.Isinst, typeof(Pawn_PsychicEntropyTracker));
            instructionsList[i + 1].labels.Add(label);
            yield return new CodeInstruction(OpCodes.Brtrue, label);
            yield return new CodeInstruction(OpCodes.Pop);
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}