using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(CompRefuelable), "Refuel")]
    [HarmonyPatch(new Type[] {typeof(float)})]
    class CompRefuelable_MechanoidData_Refuel
    {
        static void Postfix(CompRefuelable __instance, float amount)
        {
            CompDataLevel mechanoidDataComp = __instance.parent.GetComp<CompDataLevel>();
            if(mechanoidDataComp != null)
            {
                mechanoidDataComp.AccumulateData(amount);
                ((Building_RogueAI)(mechanoidDataComp.parent)).UpdateGlower();
            }
        }
    }
}
