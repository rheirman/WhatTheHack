using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(DaysWorthOfFoodCalculator), "ApproxDaysWorthOfFood")]
    [HarmonyPatch(new Type[] {typeof(List<Pawn>), typeof(List<ThingDefCount>), typeof(int), typeof(IgnorePawnsInventoryMode), typeof(Faction), typeof(WorldPath), typeof(float), typeof(int), typeof(bool) })]
    class DaysWorthOfFoodCalculator_ApproxDaysWorthOfFood
    {
        static void Prefix(ref List<Pawn> pawns)
        {
            List<Pawn> validPawns = new List<Pawn>();
            foreach(Pawn pawn in pawns)
            {
                if (!pawn.IsHacked())
                {
                    validPawns.Add(pawn);
                }
            }
            pawns = validPawns;
        }

    }
}
