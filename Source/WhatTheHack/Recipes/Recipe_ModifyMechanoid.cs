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
    class Recipe_ModifyMechanoid : Recipe_Surgery
    {

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(recipe.addsHediff))
            {
                //Copy from vanilla code. Much more complex than needed, but does the trick
                for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i++) //for each allowed body part
                {
                    BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
                    List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
                    for (int j = 0; j < bpList.Count; j++) //for each body part of pawn
                    {
                        BodyPartRecord record = bpList[j];
                        if (record.def == part)
                        {
                            IEnumerable<Hediff> hediffs = from x in pawn.health.hediffSet.hediffs
                                                        where x.Part == record
                                                        select x;
                            if (hediffs.Count<Hediff>() != 1 || hediffs.First<Hediff>().def != recipe.addsHediff)
                            {
                                if (record.parent == null || pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Contains(record.parent))
                                {
                                    if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record))
                                    {
                                        yield return record;
                                    }
                                }
                            }
                        }
                    }
                }
            } 
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            float learnfactor = 1.0f;
            float xp = 125.0f;
            if (billDoer != null)
            {
                //Let random bad events happen when hacking fails
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    learnfactor = 0.5f;
                }
                else
                {
                    TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]{billDoer, pawn});
                    pawn.health.AddHediff(this.recipe.addsHediff, part, null);
                }
                billDoer.skills.Learn(SkillDefOf.Crafting, xp * learnfactor, false);
                billDoer.skills.Learn(SkillDefOf.Intellectual, xp * learnfactor, false);
                //Find.LetterStack.ReceiveLetter("LetterSuccess_Label".Translate(), "LetterSuccess_Label_Description".Translate(), LetterDefOf.PositiveEvent, pawn);
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
            if(this.recipe.addsHediff == WTH_DefOf.WTH_RepairModule)
            {
                pawn.InitializeComps();
            }
            
        }
    }
}
