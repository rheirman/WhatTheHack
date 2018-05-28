using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{

    //Patch that makes sure controllable mechanoids are selected properly when the dragbox is used to select. 
    [HarmonyPatch]
    public static class Selector_SelectInsideDragbox
    {
        //Since the code we want to change is inside a hidden inner function callod m__0, we get the method to be patched using the harmony access tools. 
        static MethodBase TargetMethod()
        {
            MethodInfo mi = typeof(Selector).GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("m__0"));
            return mi;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if (instruction.operand == typeof(Pawn).GetMethod("get_RaceProps"))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Selector_SelectInsideDragbox), "IsHumanLikeOrHacked", new Type[] {typeof(Pawn)}));//Injected code     
                }             
                else if(instruction.operand == AccessTools.Method(typeof(RaceProperties), "get_Humanlike"))
                {
                    //Ommit this instruction
                }
                else
                {
                    yield return instruction;
                }
                
            }
        }
        static public bool IsHumanLikeOrHacked(Pawn p)
        {
            return p.RaceProps.Humanlike || p.IsHacked();
        }
    }
}
