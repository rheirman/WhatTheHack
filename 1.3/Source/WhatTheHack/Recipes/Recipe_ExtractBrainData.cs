using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Recipes
{
    class Recipe_ExtractBrainData : Recipe_Surgery
    {

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Where((BodyPartRecord bpr) => bpr.def == recipe.appliedOnFixedBodyParts.First());
            foreach (BodyPartRecord part in parts)
            {
                yield return part;
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            bool flag = MedicalRecipesUtility.IsClean(pawn, part);
            bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                Thing md = ThingMaker.MakeThing(WTH_DefOf.WTH_ExtractedBrainData);
                md.stackCount = 3 + pawn.skills.GetSkill(SkillDefOf.Intellectual).Level;
                GenPlace.TryPlaceThing(md, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
            DamageDef surgicalCut = DamageDefOf.SurgicalCut;
            float amount = 99999f;
            float armorPenetration = 999f;
            pawn.TakeDamage(new DamageInfo(surgicalCut, amount, armorPenetration, -1f, null, part, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
            if (flag)
            {
                if (pawn.Dead)
                {
                    ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.OrganHarvesting);
                }
                ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
            }
            if (flag2 && pawn.Faction != null && billDoer != null && billDoer.Faction != null)
            {
                Faction arg_120_0 = pawn.Faction;
                Faction faction = billDoer.Faction;
                int goodwillChange = -15;
                GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(pawn);
                arg_120_0.TryAffectGoodwillWith(faction, goodwillChange, true, true, HistoryEventDefOf.PerformedHarmfulSurgery, lookTarget);
            }
        }
    }
}
