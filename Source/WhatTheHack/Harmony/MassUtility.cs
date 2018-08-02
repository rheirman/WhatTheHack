using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(MassUtility), "Capacity")]
    class MassUtility_Capacity
    {
        static void Postfix(Pawn p, ref StringBuilder explanation, ref float __result)
        {
            if(p.def.HasModExtension<DefModExtension_PawnMassCapacity>())
            {
                __result += p.def.GetModExtension<DefModExtension_PawnMassCapacity>().bonusMassCapacity;
                if(explanation != null)
                {
                    explanation.AppendLine("WTH_Explanation_BonusCapacity".Translate() + ": " + __result);
                }
            }
        }
    }
}
