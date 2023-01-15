using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

//Patch that makes sure controllable mechanoids are selected properly when the dragbox is used to select. 
[HarmonyPatch]
public static class Selector_SelectInsideDragbox
{
    //Since the code we want to change is inside a hidden inner function callod b__0, we get the method to be patched using the harmony access tools. 
    private static MethodBase TargetMethod()
    {
        return typeof(Selector).GetNestedTypes(AccessTools.all).FirstOrDefault(c => c.Name == "<>c")
            ?.GetMethods(AccessTools.all)
            .FirstOrDefault(m => m.Name.Contains("SelectInsideDragBox") && m.Name.EndsWith("_1"));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        var found = false;
        foreach (var instruction in instructionsList)
        {
            if (!found && instruction.operand as MethodInfo == typeof(Pawn).GetMethod("get_RaceProps"))
            {
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Selector_SelectInsideDragbox), "IsHumanLikeOrHacked",
                        new[] { typeof(Pawn) })); //Injected code     
                found = true;
            }
            else if (instruction.operand as MethodInfo == AccessTools.Method(typeof(RaceProperties), "get_Humanlike"))
            {
                //Ommit this instruction
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static bool IsHumanLikeOrHacked(Pawn p)
    {
        return p.RaceProps.Humanlike || p.IsHacked() && !MechLikelyMounted(p);
    }

    //returns true when a humanlike is on the same square as a mechanoid
    private static bool MechLikelyMounted(Pawn pawn)
    {
        if (!pawn.IsMechanoid())
        {
            return false;
        }

        var humanLikeOnPawnPosition = pawn.Map.thingGrid.ThingsAt(pawn.Position)
            .FirstOrDefault(t => t is Pawn currentPawn && currentPawn.RaceProps.Humanlike) != null;
        return humanLikeOnPawnPosition;
    }
}