using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Comps
{
    public class CompMountable : ThingComp
    {
        public Pawn mountedTo = null;
        public float drawOffset = 0;

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
            SetDrawOffset();
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            Uninstall();
            base.PostDestroy(mode, previousMap);
        }
        public void Uninstall()
        {
            if (this.mountedTo != null && mountedTo.health != null)
            {
                if (mountedTo.health.hediffSet.HasHediff(WTH_DefOf.WTH_MountedTurret))
                {
                    mountedTo.health.RemoveHediff(mountedTo.health.hediffSet.GetFirstHediffOfDef(WTH_DefOf.WTH_MountedTurret));
                }
                Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(mountedTo).turretMount = null;
                mountedTo = null;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!Active)
            {
                return;
            }           
            Configure();
            LetMountedToWaitIfReserved();

            if(mountedTo.health != null && !mountedTo.health.hediffSet.HasHediff(WTH_DefOf.WTH_TurretModule))
            {
                Uninstall();
            }
        }

        private void LetMountedToWaitIfReserved()
        {
            Building_TurretGun turret = (Building_TurretGun)parent;
            if (turret.Map != null && turret.Map.reservationManager.IsReservedByAnyoneOf(turret, Faction.OfPlayer))
            {
                if (mountedTo.CurJobDef != JobDefOf.Wait_Combat && mountedTo.CurJobDef != WTH_DefOf.WTH_Mechanoid_Rest)
                {
                    mountedTo.jobs.StartJob(new Job(JobDefOf.Wait_Combat) { expiryInterval = 10000}, JobCondition.InterruptForced);
                    mountedTo.jobs.curDriver.AddFailCondition(delegate { return !turret.Map.reservationManager.IsReservedByAnyoneOf(turret, Faction.OfPlayer); });
                }
            }
        }

        private void Configure()
        {
            if ((mountedTo.Map == null && parent.Spawned) || mountedTo.Downed || (mountedTo.Spawned && OutOfBounds(mountedTo, parent)))
            {
                ToInventory();
            }
            else if (mountedTo.Map != parent.Map && mountedTo.Spawned && !OutOfBounds(mountedTo, parent.GetInnerIfMinified()))
            {
                OutOfInventory();
            }
            SetPowerComp();
            LinkToMountedTo();
            if (parent.Spawned)
            {
                parent.Position = mountedTo.Position;
            }
            if (parent.Faction != mountedTo.Faction)
            {
                parent.SetFaction(mountedTo.Faction);
            }
        }

        private void LinkToMountedTo()
        {
            if (Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store && mountedTo != null)
            {
                store.GetExtendedDataFor(mountedTo).turretMount = this;
            }
        }

        private bool OutOfBounds(Pawn mountedTo, Thing parent)
        {

            CellRect cellRect = GenAdj.OccupiedRect(mountedTo.Position, mountedTo.Rotation, parent.def.Size + new IntVec2(1,1));
            CellRect.CellRectIterator iterator = cellRect.GetIterator();
            while (!iterator.Done())
            {
                IntVec3 current = iterator.Current;
                if (!current.InBounds(mountedTo.Map))
                {
                    return true;
                }
                iterator.MoveNext();
            }
            return false;
        }


        public void OutOfInventory()
        {
            parent = (ThingWithComps)parent.SplitOff(1);
            GenSpawn.Spawn(parent, mountedTo.Position, mountedTo.Map, Rot4.North, WipeMode.Vanish, false);
        }

        public void ToInventory()
        {
            parent = (ThingWithComps)parent.SplitOff(1);
            mountedTo.inventory.innerContainer.TryAdd(parent, 1);
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
            Scribe_Values.Look(ref drawOffset, "drawOffset");
        }

        private void SetDrawOffset()
        {
            
            PawnKindLifeStage curKindLifeStage = mountedTo.ageTracker.CurKindLifeStage;
            Texture2D unreadableTexture = curKindLifeStage.bodyGraphicData.Graphic.MatEast.mainTexture as Texture2D;
            Texture2D t = unreadableTexture.GetReadableTexture();
            int backHeight = GetBackHeight(t);
            float backHeightRelative = (float)backHeight / (float)t.height;
            float textureHeight = curKindLifeStage.bodyGraphicData.drawSize.y;
            //If animal texture does not fit in a tile, take this into account
            float extraOffset = textureHeight > 1f ? (textureHeight - 1f) / 2f : 0;
            //Small extra offset, you don't want to draw pawn exactly on back
            extraOffset += 0.5f; 
            drawOffset = (textureHeight * backHeightRelative - extraOffset);
        }

        private static int GetBackHeight(Texture2D t)
        {

            int middle = t.width / 2;
            int backHeight = 0;
            bool inBody = false;
            float threshold = 0.8f;


            for (int i = 0; i < t.height; i++)
            {
                Color c = t.GetPixel(middle, i);
                if (inBody && c.a < threshold)
                {
                    backHeight = i;
                    break;
                }
                if (c.a >= threshold)
                {
                    inBody = true;
                }
            }
            return backHeight;
        }
    }
}
