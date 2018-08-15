using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    class JobDriver_Explode : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.Do(delegate {
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 4.5f, DamageDefOf.Bomb, pawn, DamageDefOf.Bomb.defaultDamage, DamageDefOf.Bomb.defaultArmorPenetration, DamageDefOf.Bomb.soundExplosion, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
                pawn.jobs.startingNewJob = false;
                BodyPartRecord reactorPart = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord r) => r.def.defName == "Reactor");
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Bomb, reactorPart.def.GetMaxHealth(pawn), 9999, -1, null, reactorPart));
            });
        }
    }
}
