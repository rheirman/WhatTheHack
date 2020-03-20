using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs
{
    public class JobDriver_MechanoidAbility_Overdrive : JobDriver_MechanoidAbility
    {
        protected override void FailAbility(DefModExtension_Ability modExt)
        {
            base.FailAbility(modExt);
            Action[] actions = { this.GoneTooFar, this.MediumMaintenanceDamage, this.HighMaintenanceDamage};
            List<Action> actionsList = actions.ToList();
            Action action = actionsList.RandomElement();
            action.Invoke();
        }
        private void GoneTooFar()
        {
            pawn.health.AddHediff(WTH_DefOf.WTH_Overdrive_GoneTooFar);
            if(pawn.Faction == Faction.OfPlayer)
            {
                Find.LetterStack.ReceiveLetter("WTH_Letter_OverdiveGoneTooFar_Label".Translate(), "WTH_Letter_OverdiveGoneTooFar_Description".Translate(new object[] { pawn.Name.ToStringShort }), LetterDefOf.NeutralEvent, pawn);
            }

        }
        private void MediumMaintenanceDamage()
        {
            Need_Maintenance need = pawn.needs.TryGetNeed<Need_Maintenance>();
            if(need == null)
            {
                return;
            }
            if (need.CurLevel > 25)
            {
                need.CurLevel = 25;
            }
            else
            {
                return;
            }
            if(pawn.Faction == Faction.OfPlayer)
            {
                Find.LetterStack.ReceiveLetter("WTH_Letter_MediumMaintenanceDamage_Label".Translate(), "WTH_Letter_MediumMaintenanceDamage_Description".Translate(new object[] { pawn.Name.ToStringShort }), LetterDefOf.NegativeEvent, pawn);
            }
        }
        private void HighMaintenanceDamage()
        {
            Need_Maintenance need = pawn.needs.TryGetNeed<Need_Maintenance>();
            if (need == null)
            {
                return;
            }
            if (need.CurLevel > 5)
            {
                need.CurLevel = 5;
            }
            else
            {
                return;
            }
            if(pawn.Faction == Faction.OfPlayer)
            {
                Find.LetterStack.ReceiveLetter("WTH_Letter_HackedPoorlyEvent_Label".Translate(), "WTH_Letter_HighMaintenanceDamage_Description".Translate(new object[] { pawn.Name.ToStringShort }), LetterDefOf.ThreatBig, pawn);
            }
        }
    }
}
