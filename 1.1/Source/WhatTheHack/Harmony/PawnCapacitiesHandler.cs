using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(PawnCapacitiesHandler), "CapableOf")]
    class PawnCapacitiesHandler_CapableOf
    {
        static void Postfix(PawnCapacitiesHandler __instance, PawnCapacityDef capacity, ref bool __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if(capacity.defName == "Moving" && pawn.RaceProps.IsMechanoid && pawn.def.HasModExtension<DefModExtension_PawnCapacity>())
            {
                __result = __instance.GetLevel(capacity) > pawn.def.GetModExtension<DefModExtension_PawnCapacity>().minForCapableMoving;
            }
        }
    }
}
