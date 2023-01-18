using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WhatTheHack.Harmony;

//Make sure only the summary of the character card is shown for hacked mechanoids (and not backstories/skills etc.). 
[HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard")]
public class CharacterCardUtility_DrawCharacterCard
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        var hasHackedMech = false;
        var i = 0;
        var label = ilg.DefineLabel();
        foreach (var instruction in instructionsList)
        {
            if (instruction.operand as MethodInfo == typeof(Pawn).GetMethod("get_IsColonist"))
            {
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(CharacterCardUtility_DrawCharacterCard).GetMethod("IsColonistOrHackedMech"));
                hasHackedMech = true;
            }
            else
            {
                yield return instruction;
            }

            if (hasHackedMech && instructionsList[i - 1].opcode == OpCodes.Sub)
            {
                instructionsList[i + 1].labels.Add(label);
                yield return new CodeInstruction(OpCodes.Ldarg_1); //Load "pawn" argument. 
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(CharacterCardUtility_DrawCharacterCard).GetMethod("ShouldReturn")); //Check if pawn is hacked
                yield return
                    new CodeInstruction(OpCodes.Brfalse, label); //if pawn is not hacked, continue normal flow. 
                yield return new CodeInstruction(OpCodes.Ret); //else, return the function
                hasHackedMech = false;
            }

            i++;
        }
    }

    public static bool IsColonistOrHackedMech(Pawn pawn)
    {
        return pawn.IsColonist || pawn.IsHacked();
    }

    public static bool ShouldReturn(Pawn pawn)
    {
        if (!pawn.IsHacked())
        {
            return false;
        }

        GUI.EndGroup();
        return true;
    }
}