using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    // Patch that prevents a null reference error when a pawn doesn't have psychic entropy (which is always the case for mechs). 
    [HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
    static class Caravan_NeedsTracker_TrySatisfyPawnNeeds
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            Label label = ilg.DefineLabel();

            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                yield return instruction;

                if (instruction.operand as FieldInfo == AccessTools.Field(typeof(Pawn), "psychicEntropy"))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(Pawn_PsychicEntropyTracker));
                    instructionsList[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Brtrue, label);
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ret);
                }

            }
        }
    }


}
