using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.Duties;

namespace WhatTheHack.Recipes
{
    public class Recipe_HackMechanoid : Recipe_Hacking
    {
        protected new bool allowMultipleParts = false;
        protected override bool CanApplyOn(Pawn pawn)
        {
            return pawn.Faction != Faction.OfPlayer;
        }

        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            pawn.health.AddHediff(WTH_DefOf.WTH_BackupBattery, pawn.TryGetReactor());
            pawn.SetFaction(Faction.OfPlayer);
            if (pawn.relations == null)
            {
                pawn.relations = new Pawn_RelationsTracker(pawn);
            }
            /*
            if (pawn.jobs.curDriver != null)
            {
                pawn.jobs.posture = PawnPosture.LayingOnGroundNormal;
            }
            */
            if (pawn.story == null)
            {
                pawn.story = new Pawn_StoryTracker(pawn);
            }
            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full);
            Find.LetterStack.ReceiveLetter("WTH_Letter_Success_Label".Translate(), "WTH_Letter_Success_Label_Description".Translate(new object[]{billDoer.Name.ToStringShort, pawn.Name}), LetterDefOf.PositiveEvent, pawn);
            billDoer.jobs.jobQueue.EnqueueFirst(new Job(WTH_DefOf.WTH_ClearHackingTable, pawn, pawn.CurrentBed()) {count = 1});
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Power, OpportunityType.Important);
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Maintenance, OpportunityType.Important);

        }


        protected override void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, System.Random r)
        {
            int[] chances = { Base.failureChanceHackPoorly, Base.failureChanceCauseRaid, Base.failureChanceShootRandomDirection, Base.failureChanceHealToStanding, Base.failureChanceNothing };
            int totalChance = chances.Sum();
            int randInt = r.Next(1, totalChance);
            Action<Pawn, BodyPartRecord>[] functions = { HackPoorly, CauseMechanoidRaid, ShootRandomDirection, HealToStanding, Nothing };
            int acc = 0;
            for (int i = 0; i < chances.Count(); i++)
            {
                if (randInt < ((acc + chances[i])))
                {
                    functions[i].Invoke(hackee, part);
                    break;
                }
                acc += chances[i];
            }
            if (!hackee.Downed)
            {
                Log.Message("hackee not downed, hackee:  " + hackee.Label);
            }
            if (hackee.IsHacked())
            {
                Log.Message("hackee is hacked, hackee: " + hackee.Label);
            }

            if (hackee.Downed && !hackee.IsHacked())
            {
                ((Building_HackingTable)hackee.CurrentBed()).TryAddPawnForModification(hackee, recipe);
            }
        }

        //Used to make hacking more powerful mechs more difficult. Capped at 1000 points. At this value, hacking is 50% more difficult.  
        private static float CalcCombatPowerFactorCapped(Pawn hackee)
        {
            float combatPowerFactor = Mathf.Min(hackee.kindDef.combatPower / 1000, 1.0f);
            float maxDifficultyPentaly = 0.5f;
            float combatPowerFactorCapped = 1 - maxDifficultyPentaly * combatPowerFactor;
            return combatPowerFactorCapped;
        }

        private static void Nothing(Pawn pawn, BodyPartRecord part) {
            //nothing
            Find.LetterStack.ReceiveLetter("WTH_Letter_Nothing_Label".Translate(), "WTH_Letter_Nothing_Description".Translate(), LetterDefOf.NeutralEvent, pawn);
            HealthUtility.GiveInjuriesOperationFailureMinor(pawn, part);
        }

        private void HackPoorly(Pawn pawn, BodyPartRecord part)
        {
            pawn.health.AddHediff(WTH_DefOf.WTH_TargetingHackedPoorly, part, null);
            if (this.recipe.GetModExtension<DefModExtension_Recipe>() is DefModExtension_Recipe extension && extension.addsAdditionalHediff != null)
            {
                BodyPartRecord additionalHediffBodyPart = null;
                if (extension.additionalHediffBodyPart != null)
                {
                    additionalHediffBodyPart = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord bpr) => bpr.def == extension.additionalHediffBodyPart);
                }
                pawn.health.AddHediff(extension.addsAdditionalHediff, additionalHediffBodyPart);
            }

            if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule) && pawn.GetComp<CompRefuelable>() == null)
            {
                pawn.InitializeComps();
            }

            pawn.SetFaction(Faction.OfPlayer);
            if (pawn.jobs.curDriver != null)
            {
                pawn.jobs.posture = PawnPosture.LayingOnGroundNormal;
            }
            if (pawn.story == null)
            {
                pawn.story = new Pawn_StoryTracker(pawn);
            }
            Find.LetterStack.ReceiveLetter("WTH_Letter_HackedPoorly_Label".Translate(), "WTH_Letter_HackedPoorly_Description".Translate(), LetterDefOf.NegativeEvent, pawn);
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Power, OpportunityType.Important);
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Maintenance, OpportunityType.Important);
        }

        private static void CauseMechanoidRaid(Pawn pawn, BodyPartRecord part)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
            IntVec3 spawnSpot;
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => pawn.Map.reachability.CanReachColony(c), pawn.Map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
            {
                Nothing(pawn, null);
                return;
            }
            incidentParms.forced = true;
            incidentParms.faction = Faction.OfMechanoids;
            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            incidentParms.spawnCenter = spawnSpot;
            incidentParms.points *= 1.35f;

            QueuedIncident qi = new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, incidentParms), Find.TickManager.TicksGame + new IntRange(1000, 2500).RandomInRange);
            Find.Storyteller.incidentQueue.Add(qi);
            Find.LetterStack.ReceiveLetter("WTH_Letter_CausedMechanoidRaid_Label".Translate(), "WTH_Letter_CausedMechanoidRaid_Description".Translate(), LetterDefOf.ThreatBig, pawn);

        }

        private static void ShootRandomDirection(Pawn pawn, BodyPartRecord part)
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
                Nothing(pawn, part);
                return;
            }
            IntVec3 targetCell = GenRadial.RadialCellsAround(pawn.Position, 7, true).RandomElement();
            Traverse.Create(verb).Field("currentTarget").SetValue(new LocalTargetInfo(targetCell));
            Traverse.Create(verb).Method("TryCastNextBurstShot").GetValue();
            Find.LetterStack.ReceiveLetter("WTH_Letter_ShotRandomDirection_Label".Translate(), "WTH_Letter_ShotRandomDirection_Description".Translate(), LetterDefOf.ThreatSmall, pawn);

        }

        private static void HealToStanding(Pawn pawn, BodyPartRecord part)
        {
            bool shouldStop = false;
            float extraHealth = 100f; //TODO: no magic number;
            extraHealth *= pawn.HealthScale;
            float healPerIteration = 10f;
            float totalExtraHealed = 0f;
            int guard = 0;
            while (totalExtraHealed <= extraHealth && guard < 1000)
            {
                Hediff_Injury hediff_Injury = pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Where(new Func<Hediff_Injury, bool>(HediffUtility.CanHealNaturally)).RandomElement<Hediff_Injury>();
                if (hediff_Injury == null || !pawn.Downed)
                {
                    shouldStop = true;
                }
                hediff_Injury.Heal(healPerIteration);
                if (shouldStop)
                {
                    totalExtraHealed += healPerIteration;
                }
                guard++;
            }
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            if (pawn.GetLord() == null || pawn.GetLord().LordJob == null)
            {
                LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), pawn.Map, new List<Pawn> { pawn });
            }
            Find.LetterStack.ReceiveLetter("WTH_Letter_HealedToStanding_Label".Translate(), "WTH_Letter_HealedToStanding_Description".Translate(), LetterDefOf.ThreatBig, pawn);

        }


    }
    
}
