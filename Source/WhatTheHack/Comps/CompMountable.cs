using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Comps
{
    class CompMountable : ThingComp
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
            //parent.holdingOwner = pawn.inventory.innerContainer;
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
                mountedTo = null;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Active)
            {
                Configure();
            }
            if (Active)
            {
                LetMountedToWaitIfReserved();
            }
            if (Active && parent.IsHashIntervalTick(120))
            {
                ConsumePowerIfNeeded();
            }
        }

        private void ConsumePowerIfNeeded()
        {
            CompPowerTrader compPower = parent.GetComp<CompPowerTrader>();
            CompFlickable compFlickable = parent.GetComp<CompFlickable>();
            if (compPower != null && compFlickable != null && compFlickable.SwitchIsOn && parent.Spawned)
            {
                if (mountedTo.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power) is Need_Power powerNeed && !powerNeed.DirectlyPowered(mountedTo))
                {
                    powerNeed.CurLevel -= 0.48f + compPower.Props.basePowerConsumption * 0.002f;
                }
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
            if (parent.Spawned)
            {
                parent.Position = mountedTo.Position;
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
            Texture2D t = GetReadableTexture(unreadableTexture);
            int backHeight = GetBackHeight(t);
            float backHeightRelative = (float)backHeight / (float)t.height;
            float textureHeight = curKindLifeStage.bodyGraphicData.drawSize.y;
            //If animal texture does not fit in a tile, take this into account
            float extraOffset = textureHeight > 1f ? (textureHeight - 1f) / 2f : 0;
            //Small extra offset, you don't want to draw pawn exactly on back
            extraOffset += 0.5f; 
            drawOffset = (textureHeight * backHeightRelative - extraOffset);
        }

        private static Texture2D GetReadableTexture(Texture2D texture)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);
            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;
            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;
            // Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);
            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            // Reset the active RenderTexture
            RenderTexture.active = previous;
            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);
            return myTexture2D;
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
