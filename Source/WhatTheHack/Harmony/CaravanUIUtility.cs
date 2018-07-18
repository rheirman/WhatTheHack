using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(CaravanUIUtility), "AddPawnsSections")]
    class CaravanUIUtility_AddPawnsSections
    {
        static void Postfix(ref TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
        {
            IEnumerable<TransferableOneWay> source = from x in transferables
                                                     where x.ThingDef.category == ThingCategory.Pawn
                                                     select x;


            List<Pawn> pawns = transferables[0].things[0].Map.mapPawns.AllPawnsSpawned;
            widget.AddSection("WTH_MechanoidsSection".Translate(), from x in source
                                                            where ((Pawn)x.AnyThing).RaceProps.IsMechanoid
                                                            select x);
        }
    }
}
