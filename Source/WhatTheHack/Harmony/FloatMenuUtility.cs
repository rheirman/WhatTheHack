using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    //Transpilers to make sure a drafted mechanoid have a melee and ranged attack option. Vanilla only allows humanlike pawns to do that. 
    /*
    [HarmonyPatch(typeof(FloatMenuUtility))]
    [HarmonyPatch("GetMeleeAttackAction")]
    class FloatMenuUtility_GetMeleeAttackAction
    {
        //Detour to FloatMenuMakerMap CanTakeOrder, which is patched to make sure hacked mechanoids can also be controlled
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count - 1; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                
                if (instructionsList[i].operand == typeof(Pawn).GetMethod("get_IsColonistPlayerControlled"))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Extensions), "CanTakeOrder"));//Injected code     
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
    [HarmonyPatch(typeof(FloatMenuUtility))]
    [HarmonyPatch("GetRangedAttackAction")]
    class FloatMenuUtility_GetRangedAttackAction
    {
        //Detour to FloatMenuMakerMap CanTakeOrder, which is patched to make sure hacked mechanoids can also be controlled
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count - 1; i++)
            {
                CodeInstruction instruction = instructionsList[i];

                if (instructionsList[i].operand == typeof(Pawn).GetMethod("get_IsColonistPlayerControlled"))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Extensions), "CanTakeOrder"));//Injected code     
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
    */
}