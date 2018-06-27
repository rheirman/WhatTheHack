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

namespace WhatTheHack.Recipes
{
    class Recipe_HackMechanoid : Recipe_Surgery
    {

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            Log.Message("recipe.SuccessFactor: " + recipe.surgerySuccessChanceFactor);
            if (brain != null && (!pawn.health.hediffSet.HasHediff(recipe.addsHediff) || (pawn.IsHacked() && pawn.Faction != Faction.OfPlayer)))
            {
                yield return brain;
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                //Let random bad events happen when hacking fails
                if (CheckHackingFail(pawn, billDoer, part))
                {
                    if (pawn.Dead)
                    {
                        return;
                    }
                    //Re-add surgery bill
                    Building_HackingTable.TryAddPawnForModification(pawn, WTH_DefOf.WTH_HackMechanoid);

                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                Find.LetterStack.ReceiveLetter("WTH_Letter_Success_Label".Translate(), "WTH_Letter_Success_Label_Description".Translate(), LetterDefOf.PositiveEvent, pawn);
            }
            if (!pawn.IsHacked())
            {
                pawn.health.AddHediff(this.recipe.addsHediff, part, null);
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

        }

        private bool CheckHackingFail(Pawn hackee, Pawn hacker, BodyPartRecord part)
        {
            float failureChance = 1.0f;
            failureChance *= recipe.surgerySuccessChanceFactor;
            failureChance *= hacker.GetStatValue(WTH_DefOf.WTH_HackingSuccessChance, true);
            Random r = new Random(DateTime.Now.Millisecond);

            if (!Rand.Chance(failureChance))
            {
                if (Rand.Chance(this.recipe.deathOnFailedSurgeryChance))
                {
                    HealthUtility.GiveInjuriesOperationFailureCatastrophic(hackee, part);
                    if (!hackee.Dead)
                    {
                        hackee.Kill(null, null);
                    }
                    Messages.Message("MessageMedicalOperationFailureFatal".Translate(new object[]
                    {
                hacker.LabelShort,
                hacker.LabelShort,
                this.recipe.LabelCap
                    }), hackee, MessageTypeDefOf.NegativeHealthEvent, true);
                }
                else
                {
                    int randInt = r.Next(1, 100);
                    //Applying syntactic sugar. Short, but not very readable.
                    int[] chances = { Base.failureChanceHackPoorly, Base.failureChanceCauseRaid, Base.failureChanceShootRandomDirection, Base.failureChanceHealToStanding, Base.failureChanceNothing };
                    Action<Pawn, BodyPartRecord>[] functions = { HackPoorly, CauseMechanoidRaid, ShootRandomDirection, HealToStanding, Nothing };
                    int totalChance = chances.Sum();
                    int acc = 0;
                    for (int i = 0; i < chances.Count(); i++)
                    {
                        if (randInt < ((acc + chances[i]) * totalChance) / 100)
                        {
                            functions[i].Invoke(hackee, part);
                            break;
                        }
                        acc += chances[i];
                    }
                }
                return true;
            }
            return false;



        }

        private static void Nothing(Pawn pawn, BodyPartRecord part) {
            //nothing
            Find.LetterStack.ReceiveLetter("WTH_Letter_Nothing_Label".Translate(), "WTH_Letter_Nothing_Description".Translate(), LetterDefOf.NeutralEvent, pawn);
            HealthUtility.GiveInjuriesOperationFailureMinor(pawn, part);
        }

        private static void HackPoorly(Pawn pawn, BodyPartRecord part)
        {
            pawn.health.AddHediff(WTH_DefOf.WTH_TargetingHackedPoorly, part, null);
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
            while (shouldStop && totalExtraHealed <= extraHealth)
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
