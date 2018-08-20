using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Comps
{
    class CompMountable : ThingComp
    {
        public Pawn mountedTo = null;

        public Pawn MountedTo {
            get
            {
                return this.MountedTo;
            }
        }
        public bool Active
        {
            get
            {
                return this.mountedTo != null && !parent.Destroyed;
            }
        }
        public void MountToPawn(Pawn pawn)
        {
            this.mountedTo = pawn;
            SetPowerComp();
            Configure();
            //parent.holdingOwner = pawn.inventory.innerContainer;
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            Uninstall();
            base.PostDestroy(mode, previousMap);
        }
        public void Uninstall()
        {
            if (Active && mountedTo.health != null)
            {
                if (mountedTo.health.hediffSet.HasHediff(WTH_DefOf.WTH_MountedTurret))
                {
                    mountedTo.health.RemoveHediff(mountedTo.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_MountedTurret));
                }
                mountedTo = null;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Active)
            {
                if (parent.Spawned)
                {
                    parent.Position = mountedTo.Position;
                }
                Configure();
            }
            if (Active)
            {
                Building_TurretGun turret = (Building_TurretGun)parent;
                if (turret.Map.reservationManager.IsReservedByAnyoneOf(turret, Faction.OfPlayer))
                {
                    if(mountedTo.CurJobDef != JobDefOf.Wait_Combat)
                    {
                        mountedTo.jobs.StartJob(new Job(JobDefOf.Wait_Combat) { expiryInterval = 20 }, JobCondition.InterruptForced);
                    }
                }
            }
        }
        public override void CompTickRare()
        {
            base.CompTickRare();

        }

        private void Configure()
        {
            if ((mountedTo.Map == null && parent.Spawned) || mountedTo.Downed)
            {
                parent = (ThingWithComps)parent.SplitOff(1);
                mountedTo.inventory.innerContainer.TryAdd(parent, 1);
            }
            else if (mountedTo.Map != parent.Map)
            {
                parent = (ThingWithComps)parent.SplitOff(1);
                //mountedTo.Map.spawnedThings.TryAdd(parent, 1);
                GenSpawn.Spawn(parent, mountedTo.Position, mountedTo.Map, Rot4.North, WipeMode.Vanish, false);
            }
            SetPowerComp();

        }

        private void SetPowerComp()
        {
            CompPowerTrader compPower = parent.GetComp<CompPowerTrader>();
            if (compPower != null && parent.Spawned)
            {
                CompFlickable compFlickable = parent.GetComp<CompFlickable>();
                if (compFlickable != null && compFlickable.SwitchIsOn)
                {
                    compPower.PowerOn = true;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref mountedTo, "mountedTo");
        }
    }
}
