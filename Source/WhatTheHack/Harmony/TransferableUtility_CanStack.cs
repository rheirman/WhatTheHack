using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(TransferableUtility), "CanStack")]
internal class TransferableUtility_CanStack
{
    private static bool Prefix(Thing thing, ref bool __result)
    {
        if (thing is Pawn pawn && pawn.IsHacked() || thing is Building_PortableChargingPlatform)
        {
            __result = false;
            return false;
        }

        if (thing is not ThingWithComps twc || twc.TryGetComp<CompMountable>() is not { mountedTo: { } })
        {
            return true;
        }

        __result = false;
        return false;
    }
}