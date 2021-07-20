using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;

/*
namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(CollectionsMassCalculator), "MassUsage")]
    class CollectionsMassCalculator_MassUsage
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction instruction in instructionsList)
            {
                if (instruction.operand == typeof(StatExtension).GetMethod("GetStatValue"))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(CollectionsMassCalculator_MassUsage).GetMethod("GetStatValuePatch"));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        public static float GetStatValuePatch(Thing thing, StatDef stat, bool applyPostProcess)
        {
            if (!(thing is Pawn)){
                return thing.GetStatValue(stat, applyPostProcess);
            }
            else
            {
                return MassUtility.Capacity((Pawn)thing);
            }
        }
    }
}
*/
