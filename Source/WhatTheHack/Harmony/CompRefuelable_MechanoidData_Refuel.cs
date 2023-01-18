using HarmonyLib;
using RimWorld;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CompRefuelable), "Refuel")]
[HarmonyPatch(new[] { typeof(float) })]
internal class CompRefuelable_MechanoidData_Refuel
{
    private static void Postfix(CompRefuelable __instance, float amount)
    {
        var mechanoidDataComp = __instance.parent.GetComp<CompDataLevel>();
        if (mechanoidDataComp == null)
        {
            return;
        }

        mechanoidDataComp.AccumulateData(amount);
        ((Building_RogueAI)mechanoidDataComp.parent).UpdateGlower();
    }
}