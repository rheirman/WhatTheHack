using HarmonyLib;
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
using WhatTheHack.ThinkTree;

namespace WhatTheHack.Recipes
{
    public abstract class Recipe_Hacking : RecipeWorker
    {
        protected bool allowMultipleParts = false;
        public virtual bool CanApplyOn(Pawn pawn, out string reason)
        {
            reason = "";
            return true;
        }
        protected virtual bool IsValidPawn(Pawn pawn) {
            return true;
        }
        

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            bool partFound = false;
            if (IsValidPawn(pawn))
            {
                //Copy from vanilla. Much more complex than needed, but does the trick
                for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i++)
                {
                    BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
                    List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
                    for (int j = 0; j < bpList.Count; j++)
                    {
                        BodyPartRecord record = bpList[j];
                        if (record.def == part && !partFound || allowMultipleParts)
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
                                        partFound = true;
                                    }
                                }
                            }
                        }
                    }

                    if (recipe.GetModExtension<DefModExtension_Recipe>() is DefModExtension_Recipe ext && !ext.needsFixedBodyPart && !partFound)
                    {
                        yield return null;
                    }
                }
            }
        }
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            float learnfactor = 1f / recipe.surgerySuccessChanceFactor;

            //We cap the combat power since some mods use extremely high combat power to prevent mechs from being spawned in raids. 
            float combatPowerCapped = pawn.kindDef.combatPower <= 10000 ? pawn.kindDef.combatPower : 300;

            //Let random bad events happen when hacking fails
            if (CheckHackingFail(pawn, billDoer, part))
            {
                learnfactor *= 0.5f;
                if (pawn.Dead)
                {
                    return;
                }
                billDoer.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * learnfactor, false);
                billDoer.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * learnfactor, false);
                return;
            }
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
            {
                billDoer,
                pawn
            });
            if(this.recipe.addsHediff != null)
            {
                pawn.health.AddHediff(this.recipe.addsHediff, part, null);
            }
            billDoer.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * learnfactor, false);
            billDoer.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * learnfactor, false);
            PostSuccessfulApply(pawn, part, billDoer, ingredients, bill);
        }
        protected abstract void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill);

        private bool CheckHackingFail(Pawn hackee, Pawn hacker, BodyPartRecord part)
        {
            float successChance = 1.0f;
            successChance *= recipe.surgerySuccessChanceFactor;
            successChance *= hacker.GetStatValue(WTH_DefOf.WTH_HackingSuccessChance, true);
            System.Random r = new System.Random(DateTime.Now.Millisecond);
            float combatPowerFactorCapped = CalcCombatPowerFactorCapped(hackee);
            successChance *= combatPowerFactorCapped;
            if(recipe.GetModExtension<DefModExtension_Recipe> () is DefModExtension_Recipe ext && ext.surgerySuccessCap > 0)
            {
                if(successChance > 1.0f)
                {
                    successChance = 1.0f; 
                }
                successChance *= ext.surgerySuccessCap;
            }
            if (!Rand.Chance(successChance))
            {
                MoteMaker.ThrowText((hacker.DrawPos + hackee.DrawPos) / 2f, hacker.Map, "WTH_TextMote_OperationFailed".Translate(new object[] { successChance.ToStringPercent() }), 8f);
                if (Rand.Chance(this.recipe.deathOnFailedSurgeryChance))
                {
                    HealthUtility.GiveInjuriesOperationFailureCatastrophic(hackee, part);
                    if (!hackee.Dead)
                    {
                        hackee.Kill(null, null);
                    }
                    Messages.Message("MessageMedicalOperationFailureFatal".Translate(hacker.LabelShort, hackee.LabelShort, this.recipe.LabelCap, hacker.Named("SURGEON"), hackee.Named("PATIENT")), hackee, MessageTypeDefOf.NegativeHealthEvent, true);
                }
                else
                {
                    HackingFailEvent(hacker, hackee, part, r);
                }
                return true;
            }
            return false;
        }

        protected virtual void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, System.Random r) {
            ((Building_HackingTable)hackee.CurrentBed()).TryAddPawnForModification(hackee, recipe);
        }

        //Used to make hacking more powerful mechs more difficult. Capped at 1000 points. At this value, hacking is 50% more difficult.  
        private static float CalcCombatPowerFactorCapped(Pawn hackee)
        {
            float combatPowerFactor = Mathf.Min(hackee.kindDef.combatPower / 1000, 1.0f);
            float maxDifficultyPentaly = 0.5f;
            float combatPowerFactorCapped = 1 - maxDifficultyPentaly * combatPowerFactor;
            return combatPowerFactorCapped;
        }
    }

}
