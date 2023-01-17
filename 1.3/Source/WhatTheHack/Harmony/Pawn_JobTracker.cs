using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.ThinkTree;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "DetermineNextJob")]
    static class Pawn_JobTracker_DetermineNextJob
    {
        static void Postfix(ref Pawn_JobTracker __instance, ref ThinkResult __result, ref Pawn ___pawn)
        {

            if(___pawn.IsHacked() && ___pawn.IsActivated() && ___pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TargetingHackedPoorly))
            {
                HackedPoorlyEvent(___pawn);
            }
        }

        private static void HackedPoorlyEvent(Pawn pawn)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            int rndInt = rand.Next(1, 1000);
            if (rndInt <= 4) //TODO: no magic number
            {
                Need_Maintenance need = pawn.needs.TryGetNeed<Need_Maintenance>();
                need.CurLevel = 0;
                Find.LetterStack.ReceiveLetter("WTH_Letter_HackedPoorlyEvent_Label".Translate(), "WTH_Letter_HackedPoorlyEvent_Description".Translate(), LetterDefOf.ThreatBig, pawn);
            }
        }
    }
}
