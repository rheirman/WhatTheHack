using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.Duties;
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
                Need_Power powerNeed = pawn.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power) as Need_Power;

                if (powerNeed != null && powerNeed.CurCategory >= PowerCategory.LowPower && pawnData.shouldAutoRecharge && pawn.IsActivated())
                {
                    pawn.drafter.Drafted = false;
                    pawnData.isActive = false;
                }
            }

            if (pawn.Faction == Faction.OfPlayer && pawn.IsHacked() && pawn.OnMechanoidPlatform() && pawn.CanReserve(pawn.CurrentBed()))
            {
                Job job = new Job(WTH_DefOf.Mechanoid_Rest, pawn.CurrentBed());
                job.count = 1;
                __result = new ThinkResult(job, __result.SourceNode, __result.Tag, false);
            }

            if (pawn.Downed)
            {
                return;
            }


            if(pawn.IsHacked() && pawn.IsActivated() && pawn.health.hediffSet.HasHediff(WTH_DefOf.TargetingHackedPoorly))
            {
                UnHackMechanoid(pawn);
            }
            if (pawn.Faction == Faction.OfPlayer && pawn.IsHacked() && !pawn.IsActivated() && !pawn.OnMechanoidPlatform())
            { 
                Building_MechanoidPlatform closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(pawn, pawn);
                if(closestAvailablePlatform != null && pawn.CanReserve(closestAvailablePlatform))
                {
                    Job job = new Job(WTH_DefOf.Mechanoid_Rest, closestAvailablePlatform);
                    __result = new ThinkResult(job, __result.SourceNode, __result.Tag, false);
                }                
            }

        }

        private static void UnHackMechanoid(Pawn pawn)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            int rndInt = rand.Next(1, 1000);
            if (rndInt <= 5) //TODO: no magic number
            {
                pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.TargetingHackedPoorly));
                pawn.SetFaction(Faction.OfMechanoids);
                pawn.story = null;
                if (pawn.GetLord() == null || pawn.GetLord().LordJob == null)
                {
                    LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), pawn.Map, new List<Pawn> { pawn });
                }
                else
                {
                    Log.Message("lord was not null");
                }
            }
            else
            {
                Log.Message("mechnoid stays hacked");
            }
        }
    }
}
