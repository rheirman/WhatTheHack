using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.ThinkTree;

namespace WhatTheHack.Recipes
{
    public class Recipe_ModifyMechanoid : Recipe_Hacking
    {
        public override bool CanApplyOn(Pawn pawn, out string reason)
        {
            reason = "";
            if (recipe.HasModExtension<DefModExtension_Recipe>()) {

                DefModExtension_Recipe ext = recipe.GetModExtension<DefModExtension_Recipe>();
                if(ext.requiredHediff != null && !pawn.health.hediffSet.HasHediff(ext.requiredHediff))
                {
                    reason = "WTH_Reason_MissingHediff".Translate(ext.requiredHediff.label);
                    return false;
                }
                
                bool ignoreBodySize = recipe.addsHediff == WTH_DefOf.WTH_TurretModule && pawn.def.GetModExtension<DefModExtension_TurretModule>() is DefModExtension_TurretModule modExt && modExt.ignoreMinBodySize;
                if (!ignoreBodySize && pawn.BodySize < ext.minBodySize)
                {
                    reason = "WTH_Reason_BodySize".Translate(ext.minBodySize);
                    return false;
                }
            }
            return true;
        }
        protected override bool IsValidPawn(Pawn pawn)
        {
            return pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(recipe.addsHediff);
        }

        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
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
            if (pawn.BillStack.Bills.Count <= 1)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        protected override void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, Random r)
        {
            base.HackingFailEvent(hacker, hackee, part, r);
            Messages.Message("MessageMedicalOperationFailureMinor".Translate(hacker.LabelShort, hackee.def.label, hacker.Named("SURGEON"), hackee.Named("PATIENT")), hackee, MessageTypeDefOf.NegativeHealthEvent, true);
            HealthUtility.GiveInjuriesOperationFailureMinor(hackee, part);
            foreach (IngredientCount ic in recipe.ingredients)
            {
                Thing t = ThingMaker.MakeThing(ic.filter.AnyAllowedDef, null);
                GenSpawn.Spawn(t, hackee.Position, hackee.Map);
            }
        }
    }
}
