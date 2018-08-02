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
            for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i++)
            {
                BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
                List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
                for (int j = 0; j < bpList.Count; j++)
                {
                    BodyPartRecord record = bpList[j];
                    if (record.def == part)
                    {
                        IEnumerable<Hediff> diffs = from x in pawn.health.hediffSet.hediffs
                                                    where x.Part == record
                                                    select x;
                        if (diffs.Count<Hediff>() != 1 || diffs.First<Hediff>().def != recipe.addsHediff)
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

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                //Let random bad events happen when hacking fails
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                //Find.LetterStack.ReceiveLetter("LetterSuccess_Label".Translate(), "LetterSuccess_Label_Description".Translate(), LetterDefOf.PositiveEvent, pawn);
            }
            pawn.health.AddHediff(this.recipe.addsHediff, part, null);
            /*
            Log.Message("Recipe_ModifyMechanoid.ApplyOnPawn called");
            if(this.recipe.addsHediff == WTH_DefOf.WTH_RepairModule)
            {
                Log.Message("pawn.InitializeComps() called");
                pawn.InitializeComps();
            }
            */
        }
    }
}
