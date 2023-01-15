using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_JobTracker), "DetermineNextJob")]
internal static class Pawn_JobTracker_DetermineNextJob
{
    private static void Postfix(ref Pawn ___pawn)
    {
        if (___pawn.IsHacked() && ___pawn.IsActivated() &&
            ___pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TargetingHackedPoorly))
        {
            HackedPoorlyEvent(___pawn);
        }
    }

    private static void HackedPoorlyEvent(Pawn pawn)
    {
        var rand = new Random(DateTime.Now.Millisecond);
        var rndInt = rand.Next(1, 1000);
        if (rndInt > 4) //TODO: no magic number
        {
            return;
        }

        var need = pawn.needs.TryGetNeed<Need_Maintenance>();
        need.CurLevel = 0;
        Find.LetterStack.ReceiveLetter("WTH_Letter_HackedPoorlyEvent_Label".Translate(),
            "WTH_Letter_HackedPoorlyEvent_Description".Translate(), LetterDefOf.ThreatBig, pawn);
    }
}