using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    class JobGiver_AIFightEnemy_TryGiveJob
    {
        static void Postfix(Pawn pawn, Job __result)
        {
            if (pawn.RaceProps.IsMechanoid)
            {
                Log.Message("tryGiveJob for " + pawn.Name);
                if(__result != null)
                {
                    Log.Message("jobdef was: " + __result.def.defName);
                }
            }
        }
    }
}
