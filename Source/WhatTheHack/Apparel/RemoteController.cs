using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Duties;
using WhatTheHack.Storage;

namespace WhatTheHack
{
    class RemoteController : Apparel
    {
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == base.Wearer)
            {
                yield return GetRemoteControlActivateGizmo(base.Wearer);
            }
        }
        private static Gizmo GetRemoteControlActivateGizmo(Pawn pawn)
        {
            Command_Target command_Target = new Command_Target();
            command_Target.defaultLabel = "CommandMeleeAttack".Translate();
            command_Target.defaultDesc = "CommandMeleeAttackDesc".Translate();
            command_Target.targetingParams = GetTargetingParametersForHacking();
            command_Target.hotKey = KeyBindingDefOf.Misc5;
            command_Target.icon = TexCommand.Install;
            command_Target.action = delegate (Thing target)
            {
                if(target is Pawn)
                {
                    Pawn mech = (Pawn)target;
                    if (!mech.HasHackedLocomotion())
                    {
                        return;
                    }
                    ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                    ExtendedPawnData mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                    pawnData.remoteControlLink = mech;
                    mechData.remoteControlLink = pawn;
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    if (pawn.GetLord() == null || pawn.GetLord().LordJob == null)
                    {
                        LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_ControlMechanoid(), pawn.Map, new List<Pawn> { pawn });
                        pawn.mindState.duty.focus = mech;
                    }
                    else
                    {
                        Log.Message("lord not null null!");
                    }
                }
            };
            return command_Target;
        }
        private static TargetingParameters GetTargetingParametersForHacking()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }
                    Pawn pawn = targ.Thing as Pawn;
                    return pawn != null && !pawn.Downed && pawn.IsHacked() && pawn.HasHackedLocomotion();
                }
            };
        }
    }
}
