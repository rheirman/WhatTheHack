using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Duties;

namespace WhatTheHack.Recipes
{
    class Recipe_HackMechanoid : Recipe_Surgery
    {

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            if (brain != null && !pawn.health.hediffSet.HasHediff(recipe.addsHediff))
            {
                yield return brain;
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    //TODO actions here when hacking fails!

                    pawn.health.AddHediff(WTH_DefOf.TargetingHackedPoorly, part, null);
                    pawn.SetFaction(Faction.OfPlayer);
                    if (pawn.playerSettings == null)
                    {
                        Log.Message("pawn playersettings were null");

                    }
                    else
                    {
                        Log.Message("pawn playersettings medcare: " + pawn.playerSettings.medCare.GetLabel());
                        Log.Message("pawn playersettings medcare tostring: " + pawn.playerSettings.medCare);

                    }
                    if (pawn.jobs.curDriver != null)
                    {
                        pawn.jobs.curDriver.layingDown = LayingDownState.LayingSurface;
                    }
                    if (pawn.story == null)
                    {
                        pawn.story = new Pawn_StoryTracker(pawn);
                    }

                    //CauseMechanoidRaid(pawn);
                    //FireShotRandomly(pawn);
                    //HealUntilStanding(pawn);
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
            }
            pawn.health.AddHediff(this.recipe.addsHediff, part, null);
            pawn.SetFaction(Faction.OfPlayer);
            if(pawn.playerSettings == null)
            {
                Log.Message("pawn playersettings were null");

            }
            else
            {
                Log.Message("pawn playersettings medcare: " + pawn.playerSettings.medCare.GetLabel());
                Log.Message("pawn playersettings medcare tostring: " + pawn.playerSettings.medCare);

            }

            if (pawn.jobs.curDriver != null)
            {
                pawn.jobs.curDriver.layingDown = LayingDownState.LayingSurface;
            }
            if (pawn.story == null)
            {
                pawn.story = new Pawn_StoryTracker(pawn);
            }

        }

        private static void CauseMechanoidRaid(Pawn pawn)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, IncidentCategory.ThreatBig, pawn.Map);
            IntVec3 spawnSpot;
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => pawn.Map.reachability.CanReachColony(c), pawn.Map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
            {
                //return;
            }
            incidentParms.forced = true;
            incidentParms.faction = Faction.OfMechanoids;
            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            incidentParms.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;
            incidentParms.spawnCenter = spawnSpot;
            incidentParms.points *= 1.35f;

            QueuedIncident qi = new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, incidentParms), Find.TickManager.TicksGame + new IntRange(1000, 2500).RandomInRange);
            Find.Storyteller.incidentQueue.Add(qi);
        }

        private static void FireShotRandomly(Pawn pawn)
        {
            Verb verb = null;

            foreach (Verb v in pawn.equipment.AllEquipmentVerbs)
            {
                if (!v.IsMeleeAttack)
                {
                    verb = v;
                }
            }
            if (verb == null)
            {
                return;
            }
            IntVec3 targetCell = GenRadial.RadialCellsAround(pawn.Position, 7, true).RandomElement();
            Traverse.Create(verb).Field("currentTarget").SetValue(new LocalTargetInfo(targetCell));
            Traverse.Create(verb).Method("TryCastNextBurstShot").GetValue();
        }

        private static void HealUntilStanding(Pawn pawn)
        {
            bool shouldStop = false;
            while (!shouldStop)
            {
                Hediff_Injury hediff_Injury = pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Where(new Func<Hediff_Injury, bool>(HediffUtility.CanHealNaturally)).RandomElement<Hediff_Injury>();
                if (hediff_Injury == null || !pawn.Downed)
                {
                    shouldStop = true;
                    continue;
                }
                hediff_Injury.Heal(10);
            }
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            if (pawn.GetLord() == null || pawn.GetLord().LordJob == null)
            {
                LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), pawn.Map, new List<Pawn> { pawn });
            }
            else
            {
                Log.Message("lord was not null");
            }
        }
    }
    
}
