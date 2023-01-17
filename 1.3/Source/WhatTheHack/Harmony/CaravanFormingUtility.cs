using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Storage;
using WhatTheHack.ThinkTree;

namespace WhatTheHack.Harmony
{

    [HarmonyPatch(typeof(CaravanFormingUtility), "StartFormingCaravan")]
    static class CaravanFormingUtility_StartFormingCaravan
    {
        static void Postfix(List<Pawn> pawns)
        {
            foreach (var pawn in pawns)
            {
                if (pawn.IsHacked())
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                    ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                    pawnData.isActive = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CaravanFormingUtility), "AllSendablePawns")]
    static class CaravanFormingUtility_AllSendablePawns
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if(instruction.opcode == OpCodes.Isinst && instruction.operand.Equals(typeof(LordJob_VoluntarilyJoinable)))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(CaravanFormingUtility_AllSendablePawns).GetMethod("HasRightLordJob"));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        public static bool HasRightLordJob(LordJob lordJob)
        {
            return lordJob is LordJob_VoluntarilyJoinable || lordJob is LordJob_SearchAndDestroy || lordJob is LordJob_ControlMechanoid;
        }
    }
}
