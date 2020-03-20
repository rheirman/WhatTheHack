using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using WhatTheHack.ThinkTree;

namespace WhatTheHack.Recipes
{
    class RecipeUtility
    {
        public static IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i++)
            {
                BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
                BodyPartRecord r = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == part);
                if(r != null)
                {
                    yield return r;
                }
            }
        }

        public static void Nothing(Pawn pawn, BodyPartRecord part, RecipeDef recipe)
        {
            //nothing
            Find.LetterStack.ReceiveLetter("WTH_Letter_Nothing_Label".Translate(), "WTH_Letter_Nothing_Description".Translate(), LetterDefOf.NeutralEvent, pawn);
            HealthUtility.GiveInjuriesOperationFailureMinor(pawn, part);
        }

        public static void HackPoorly(Pawn pawn, BodyPartRecord part, RecipeDef recipe)
        {
            pawn.health.AddHediff(WTH_DefOf.WTH_TargetingHackedPoorly, part, null);
            if (recipe.GetModExtension<DefModExtension_Recipe>() is DefModExtension_Recipe extension && extension.addsAdditionalHediff != null)
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

        public static void CauseMechanoidRaidByHackingFailure(Pawn pawn, BodyPartRecord part, RecipeDef recipe)
        {
            CauseMechanoidRaid(pawn, part, recipe);
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(pawn.Map);
            Find.LetterStack.ReceiveLetter("WTH_Letter_CausedMechanoidRaid_Label".Translate(), "WTH_Letter_CausedMechanoidRaid_Description".Translate(), LetterDefOf.ThreatBig, pawn);
        }
        public static void CauseIntendedMechanoidRaid(Pawn pawn, BodyPartRecord part, RecipeDef recipe)
        {
            CauseMechanoidRaid(pawn, part, recipe, 0.9f, 2000, 60000);
            Find.LetterStack.ReceiveLetter("WTH_Letter_CausedIntendedMechanoidRaid_Label".Translate(), "WTH_Letter_CausedIntendedMechanoidRaid_Description".Translate(), LetterDefOf.PositiveEvent, pawn);
            HealthUtility.GiveInjuriesOperationFailureCatastrophic(pawn, part); //Kill mech for balancing purposes. 
            if (!pawn.Dead)
            {
                pawn.Kill(null, null);
            }
        }
        public static void CauseIntendedMechanoidRaidTooLarge(Pawn pawn, BodyPartRecord part, RecipeDef recipe)
        {
            CauseMechanoidRaid(pawn, part, recipe, 1.35f, 2000, 10000);
            Find.LetterStack.ReceiveLetter("WTH_Letter_CausedIntendedMechanoidRaidTooLarge_Label".Translate(), "WTH_Letter_CausedIntendedMechanoidRaidTooLarge_Description".Translate(), LetterDefOf.ThreatBig, pawn);
            HealthUtility.GiveInjuriesOperationFailureCatastrophic(pawn, part); //Kill mech for balancing purposes. 
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(pawn.Map);
            if (!pawn.Dead)
            {
                pawn.Kill(null, null);
            }
        }

        public static void CauseMechanoidRaid(Pawn pawn, BodyPartRecord part, RecipeDef recipe, float points = 1.25f, int minTicks = 2000, int maxTicks = 4000)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, pawn.Map);
            IntVec3 spawnSpot;
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => pawn.Map.reachability.CanReachColony(c), pawn.Map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
            {
                Nothing(pawn, null, recipe);
                return;
            }

            incidentParms.forced = true;
            incidentParms.faction = Faction.OfMechanoids;
            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            incidentParms.spawnCenter = spawnSpot;
            incidentParms.points *= points;

            int delay = new IntRange(minTicks, maxTicks).RandomInRange;
            QueuedIncident qi = new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, incidentParms), Find.TickManager.TicksGame + delay);
            Find.Storyteller.incidentQueue.Add(qi);
            
            Base.Instance.GetExtendedDataStorage().lastEmergencySignalTick = Find.TickManager.TicksGame;
            Base.Instance.GetExtendedDataStorage().lastEmergencySignalDelay = delay;
        }


        public static void ShootRandomDirection(Pawn pawn, BodyPartRecord part, RecipeDef recipe)
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
                Nothing(pawn, part, recipe);
                return;
            }
            IntVec3 targetCell = GenRadial.RadialCellsAround(pawn.Position, 7, true).RandomElement();
            Traverse.Create(verb).Field("currentTarget").SetValue(new LocalTargetInfo(targetCell));
            Traverse.Create(verb).Method("TryCastNextBurstShot").GetValue();
            Find.LetterStack.ReceiveLetter("WTH_Letter_ShotRandomDirection_Label".Translate(), "WTH_Letter_ShotRandomDirection_Description".Translate(), LetterDefOf.ThreatSmall, pawn);

        }

        public static void HealToStanding(Pawn pawn, BodyPartRecord part, RecipeDef recipe)
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
