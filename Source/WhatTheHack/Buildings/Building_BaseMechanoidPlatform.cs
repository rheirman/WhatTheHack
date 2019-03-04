using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WhatTheHack.Buildings
{
    public abstract class Building_BaseMechanoidPlatform : Building_Bed
    {
        protected bool regenerateActive = true;
        protected bool repairActive = true;
        public CompRefuelable refuelableComp;
        public const int SLOTINDEX = 1;

        public virtual bool RegenerateActive { get => regenerateActive; }
        public virtual bool RepairActive { get => repairActive;}

        public abstract bool CanHealNow();
        public abstract bool HasPowerNow();

        //make sure if you place the platform on a downed mech, the mech will get an opportunity to start the rest job again. 
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                Pawn curOccupant = GetCurOccupant(SLOTINDEX);
                if(curOccupant != null && curOccupant.IsHacked())
                {
                    curOccupant.jobs.StartJob(new Job(WTH_DefOf.WTH_Mechanoid_Rest, this) {count = 1}, JobCondition.InterruptForced);
                }
            }
        }
        /*
        public new IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if (!base.Spawned)
                {
                    return Enumerable.Empty<Pawn>();
                }
                Log.Message("called AssigningCandidates");
                return base.Map.mapPawns.AllPawns.Where((Pawn p) => p.IsHacked());
            }
        }
        */
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return new Command_Action
            {
                defaultLabel = "CommandBedSetOwnerLabel".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner", true),
                defaultDesc = "WTH_Gizmo_SetMechanoidOwner_Description".Translate(),
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_AssignBuildingOwner(this));
                },
                hotKey = KeyBindingDefOf.Misc3
            };
        }

    }
}
