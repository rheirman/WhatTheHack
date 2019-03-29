using Harmony;
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
        static void Postfix(ref Pawn_JobTracker __instance, ref ThinkResult __result)
        {

            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (!pawn.RaceProps.IsMechanoid)
            {
                return;
            }

            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store != null)
            {
                //de-activate if should auto recharge and power is very low. 
                ExtendedPawnData pawnData = store.GetExtendedDataFor(pawn);
                if (pawn.ShouldRecharge() &&
                    pawn.IsActivated()
                    )
                {
                    pawn.drafter.Drafted = false;
                    pawnData.isActive = false;
                }
            }
            if (pawn.Downed)
            {
                return;
            }
            if(pawn.IsHacked() && pawn.IsActivated() && pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TargetingHackedPoorly))
            {
                HackedPoorlyEvent(pawn);
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
