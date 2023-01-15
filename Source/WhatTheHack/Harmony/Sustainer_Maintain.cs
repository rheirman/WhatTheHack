using HarmonyLib;
using Verse.Sound;

namespace WhatTheHack.Harmony;

//For some unknown reason the overdrive sustainer sometimes is ended before it's ought to. Fixing this properly took too much time, so I'm just surpressing the error (which can't do much harm anyway in this case). 
[HarmonyPatch(typeof(Sustainer), "Maintain")]
internal class Sustainer_Maintain
{
    private static bool Prefix(Sustainer __instance)
    {
        if (__instance.def == WTH_DefOf.WTH_Sound_Overdrive && __instance.Ended)
        {
            return false;
        }

        return true;
    }
}