using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
[HarmonyPriority(Priority.High)]
public static class Pawn_HealthTracker_CheckForStateChange
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var isMechanoid = false;
        var instructionsList = new List<CodeInstruction>(instructions);
        foreach (var instruction in instructionsList)
        {
            if (instruction.operand as MethodInfo == typeof(RaceProperties).GetMethod("get_IsMechanoid"))
            {
                isMechanoid = true;
            }

            if (isMechanoid && instruction.opcode == OpCodes.Ldc_R4)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(Pawn_HealthTracker_CheckForStateChange).GetMethod("GetMechanoidDownChance"));
                //yield return new CodeInstruction(OpCodes.Ldc_R4, 0.5f);//TODO: no magic number? 
                isMechanoid = false;
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static float GetMechanoidDownChance(Pawn pawn)
    {
        if (pawn.Faction != Faction.OfPlayer &&
            !pawn.Faction.HostileTo(Faction
                .OfPlayer)) //make sure allied mechs always die to prevent issues with relation penalties when the player hacks their mechs. 
        {
            return 1.0f;
        }

        return Base.deathOnDownedChance / 100f;
    }
}