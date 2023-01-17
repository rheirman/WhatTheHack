using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace WhatTheHack.Harmony
{
    //For some unknown reason the overdrive sustainer sometimes is ended before it's ought to. Fixing this properly took too much time, so I'm just surpressing the error (which can't do much harm anyway in this case). 
    [HarmonyPatch(typeof(Verse.Sound.Sustainer), "Maintain")]
    class Sustainer_Maintain
    {
        static bool Prefix(Verse.Sound.Sustainer __instance)
        {
            if(__instance.def == WTH_DefOf.WTH_Sound_Overdrive && __instance.Ended)
            {
                return false;
            }
            return true;
        }
    }
}
