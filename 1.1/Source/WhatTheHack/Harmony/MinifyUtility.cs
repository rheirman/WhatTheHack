using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(MinifyUtility), "Uninstall")]
    class MinifyUtility_Uninstall
    {
        static void Prefix(ref Thing th)
        {
            if (th.TryGetComp<CompMountable>() is CompMountable comp && comp.Active)
            {
                comp.Uninstall();
            }
        }
    }
}
