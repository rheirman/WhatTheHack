using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

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
                SetPowerComp();
            }
        }

        private void SetPowerComp()
        {
            CompPowerTrader compPower = parent.GetComp<CompPowerTrader>();
            if (compPower != null && parent.Spawned)
            {
                compPower.PowerOn = true;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref mountedTo, "mountedTo");
        }
    }
}
