using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Gizmo_CaravanInfo), "GizmoOnGUI")]
internal class Gizmo_CaravanInfo_GizmoOnGUI
{
    private static void Postfix(ref Caravan ___caravan)
    {
        var numMechanoids = 0;
        var fuelAmount = 0f;
        var fuelConsumption = 0f;
        var numPlatforms = 0;
        float daysOfFuel = 0;
        var daysOfFuelReason = new StringBuilder();

        foreach (var thing in ___caravan.AllThings)
        {
            if (thing.def.race is { IsMechanoid: true } && thing is Pawn pawn && pawn.IsHacked() &&
                !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
            {
                numMechanoids += thing.stackCount;
            }

            if (thing.def == ThingDefOf.Chemfuel)
            {
                fuelAmount += thing.stackCount;
            }

            if (thing.def == ThingDefOf.MinifiedThing)
            {
                if (thing.GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform)
                {
                    numPlatforms += thing.stackCount;
                }
            }

            if (thing.def == WTH_DefOf.WTH_PortableChargingPlatform)
            {
                numPlatforms += thing.stackCount;
            }
        }

        Utilities.CalcDaysOfFuel(numMechanoids, fuelAmount, ref fuelConsumption, numPlatforms, ref daysOfFuel,
            daysOfFuelReason);
    }
}