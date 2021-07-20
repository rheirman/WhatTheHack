using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{

    [HarmonyPatch(typeof(CaravanFormingUtility), "StartFormingCaravan")]
    static class JobGiver_PrepareCaravan_CollectPawns_AnimalNeedsGathering
    {
        static void Postfix(List<Pawn> pawns)
        {
            foreach(var pawn in pawns)
            {
                if (pawn.IsHacked())
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                    ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                    pawnData.isActive = true;
                }
            }
        }
    }
}
