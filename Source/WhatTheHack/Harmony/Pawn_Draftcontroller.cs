using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    static class Pawn_Draftcontroller_set_Drafted
    {
        static void Postfix(Pawn_DraftController __instance)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store != null)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
                pawnData.isActive = true;
            }
        }
    }
    //TODO: finish following patch!
    /*
    [HarmonyPatch]
    static class Pawn_DraftController_GetGizmos
    {
        static MethodBase TargetMethod()
        {
            var predicateClass = typeof(RCellFinder).GetNestedTypes(AccessTools.all)
                .FirstOrDefault(t => t.FullName.Contains("c__Iterator0"));
            return predicateClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("MoveNext"));
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];

                if (instructionsList[i].opcode == OpCodes.Ldloc_3)
                {
                }
            }
        }
    }
    */
}

