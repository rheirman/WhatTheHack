using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CaravanUIUtility), "AddPawnsSections")]
internal class CaravanUIUtility_AddPawnsSections
{
    private static void Postfix(ref TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
    {
        var mechs = from x in transferables
            where x.ThingDef.category == ThingCategory.Pawn
                  && ((Pawn)x.AnyThing).IsMechanoid()
                  && ((Pawn)x.AnyThing).IsHacked()
                  && (((Pawn)x.AnyThing).ControllingAI() == null ||
                      !((Pawn)x.AnyThing).ControllingAI().hackedMechs.Contains((Pawn)x.AnyThing))
            select x;

        widget.AddSection("WTH_MechanoidsSection".Translate(), mechs);
        if (mechs.Any())
        {
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Caravanning, OpportunityType.Important);
        }
    }
}