using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.ThinkTree;

namespace WhatTheHack;

internal class RemoteController : Apparel
{
    public int ControlRadius
    {
        get
        {
            var radius = this.GetStatValue(WTH_DefOf.WTH_ControllerBeltRadius);
            return Mathf.RoundToInt(radius);
        }
    }

    public override IEnumerable<Gizmo> GetWornGizmos()
    {
        if (Find.Selector.SingleSelectedThing != Wearer)
        {
            yield break;
        }

        var linkedPawn = Wearer.RemoteControlLink();
        if (linkedPawn == null)
        {
            yield return GetRemoteControlActivateGizmo(Wearer);
        }
        else
        {
            yield return GetRemoteControlDeActivateGizmo(Wearer);
        }
    }


    private static Gizmo GetRemoteControlDeActivateGizmo(Pawn pawn)
    {
        var command = new Command_Action
        {
            defaultLabel = "WTH_Gizmo_RemoteControlDeactivate_Label".Translate(),
            defaultDesc = "WTH_Gizmo_RemoteControlDeactivate_Description".Translate(),
            icon = TexCommand.CannotShoot,
            action = delegate
            {
                var mech = pawn.RemoteControlLink();
                var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                var mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
                pawnData.remoteControlLink = null;
                mechData.remoteControlLink = null;
                mech.drafter.Drafted = false;
            }
        };
        return command;
    }


    private static Gizmo GetRemoteControlActivateGizmo(Pawn pawn)
    {
        var command_Target = new Command_Target
        {
            defaultLabel = "WTH_Gizmo_RemoteControlActivate_Label".Translate(),
            defaultDesc = "WTH_Gizmo_RemoteControlActivate_Description".Translate(),
            targetingParams = GetTargetingParametersForHacking(),
            hotKey = KeyBindingDefOf.Misc5
        };
        if (pawn.Drafted)
        {
            command_Target.Disable("WTH_Reason_PawnCannotBeDrafted".Translate());
        }

        command_Target.icon = ContentFinder<Texture2D>.Get("Things/MechControllerBelt");
        command_Target.action = delegate(LocalTargetInfo target)
        {
            if (target is not { HasThing: true, Thing: Pawn })
            {
                return;
            }

            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            var mech = (Pawn)target;
            var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            var mechData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mech);
            pawnData.remoteControlLink = mech;
            mechData.remoteControlLink = pawn;
            mechData.isActive = true;
            mech.drafter.Drafted = true;
            Find.Selector.ClearSelection();
            Find.Selector.Select(mech);
            if (pawn.GetLord() != null && pawn.GetLord().LordJob != null)
            {
                return;
            }

            LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_ControlMechanoid(), pawn.Map,
                new List<Pawn> { pawn });
            pawn.mindState.duty.focus = mech;
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
            validator = delegate(TargetInfo targ)
            {
                if (!targ.HasThing)
                {
                    return false;
                }

                var pawn = targ.Thing as Pawn;
                return pawn is { Downed: false } && pawn.IsHacked();
            }
        };
    }
}