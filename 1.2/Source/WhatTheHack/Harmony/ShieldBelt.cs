using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(ShieldBelt), "get_ShouldDisplay")]
    class ShieldBelt_get_ShouldDisplay
    {
        static void Postfix(ShieldBelt __instance, ref bool __result)
        {
            if(__result == false && __instance.Wearer.health != null && __instance.Wearer.health.hediffSet.HasHediff(WTH_DefOf.WTH_BeltModule) && __instance.Wearer.IsHacked() && __instance.Wearer.IsActivated())
            {
                __result = true;
            }
        }
    }
}
