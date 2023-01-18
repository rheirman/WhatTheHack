using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(ITab_Pawn_Gear), "InterfaceDrop")]
internal class ITab_Pawn_Gear_InterfaceDrop
{
    private static bool Prefix(ITab_Pawn_Gear __instance, Thing t)
    {
        var pawn = __instance.SelPawnForGear;
        // Traverse.Create(__instance).Property("SelPawnForGear").GetValue<Pawn>();
        if (pawn == null || !pawn.IsHacked() || pawn.equipment == null || pawn.equipment.Primary != t)
        {
            return true;
        }

        Messages.Message("WTH_Message_CannotDrop".Translate(), MessageTypeDefOf.RejectInput);
        return false;
    }
}