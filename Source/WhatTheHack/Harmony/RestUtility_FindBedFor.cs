using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(RestUtility), "FindBedFor")]
[HarmonyPatch(new[] { typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(GuestStatus) })]
internal class RestUtility_FindBedFor
{
    private static bool Prefix(Pawn sleeper, Pawn traveler, ref Building_Bed __result)
    {
        if (!sleeper.IsHacked())
        {
            return true;
        }

        if (!HealthAIUtility.ShouldSeekMedicalRest(sleeper))
        {
            return true;
        }

        if (sleeper.OnBaseMechanoidPlatform())
        {
            __result = sleeper.CurrentBed();
            return false;
        }

        __result = Utilities.GetAvailableMechanoidPlatform(traveler, sleeper);
        return false;
    }
}

//Make sure only mechanoids can use hacking table and mechanoid platforms a bed
//"Wake up" mechanoids when forming a caravan