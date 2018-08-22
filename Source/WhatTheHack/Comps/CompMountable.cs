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
                if (turret.Map != null && turret.Map.reservationManager.IsReservedByAnyoneOf(turret, Faction.OfPlayer))
                {
                    if(mountedTo.CurJobDef != JobDefOf.Wait_Combat)
                    {
                        mountedTo.jobs.StartJob(new Job(JobDefOf.Wait_Combat) { expiryInterval = 20 }, JobCondition.InterruptForced);
                    }
                }
            }
            if (Active && parent.IsHashIntervalTick(120))
            {
                CompPowerTrader compPower = parent.GetComp<CompPowerTrader>();
                CompFlickable compFlickable = parent.GetComp<CompFlickable>();
                if (compPower != null && compFlickable != null && compFlickable.SwitchIsOn && parent.Spawned)
                {
                    if (mountedTo.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Power) is Need_Power powerNeed && !powerNeed.DirectlyPowered(mountedTo))
                    {
                        powerNeed.CurLevel -= compPower.Props.basePowerConsumption * 0.008f;
                    }
                }
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
