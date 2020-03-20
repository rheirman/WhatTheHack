using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Harmony
{
    //Make sure only the summary of the character card is shown for hacked mechanoids (and not backstories/skills etc.). 
    [HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard")]
    public class CharacterCardUtility_DrawCharacterCard
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            bool flag = false;
            int i = 0;
            Label label = ilg.DefineLabel();
            foreach (CodeInstruction instruction in instructionsList)
            {
                if(instruction.operand == typeof(Pawn).GetMethod("get_IsColonist"))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(CharacterCardUtility_DrawCharacterCard).GetMethod("IsColonistOrHackedMech"));
                }
                else
                {
                    yield return instruction;
                }
                if (instruction.operand == typeof(Pawn).GetMethod("MainDesc"))
                {
                    flag = true;
                }
                if(flag && instruction.operand == typeof(TooltipHandler).GetMethod("TipRegion", new Type[] {typeof(Rect), typeof(Func < string >), typeof(int)}))
                {
                    instructionsList[i+1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);//Load "pawn" argument. 
                    yield return new CodeInstruction(OpCodes.Call, typeof(CharacterCardUtility_DrawCharacterCard).GetMethod("ShouldReturn"));//Check if pawn is hacked
                    yield return new CodeInstruction(OpCodes.Brfalse, label);//if pawn is not hacked, continue normal flow. What argument to provide here? 
                    yield return new CodeInstruction(OpCodes.Ret);//else, return the function
                    flag = false;
                }
                i++;
            }
        }

        public static bool IsColonistOrHackedMech(Pawn pawn)
        {
            if (pawn.IsColonist || pawn.IsHacked())
            {
                return true;
            }
            return false;
        }

        public static bool ShouldReturn(Pawn pawn)
        {
            if (pawn.IsHacked())
            {
                GUI.EndGroup();
                return true;
            }
            return false;
        }
    }
}
