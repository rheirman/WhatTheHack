using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Comps
{
    class CompOverlay : ThingComp
    {
        public CompProperties_Overlays Props => props as CompProperties_Overlays;
        private float maxX = 0.5f;
        private float minX = -0.5f;
        private float xOffset = 0.0f;
        private bool xUp = true; 
        public override void PostDraw()
        {
            base.PostDraw();

            List<CompProperties_Overlays.GraphicOverlay> overlays = Props.GetOverlay(parent.Rotation);
            int i = 0;
            foreach(CompProperties_Overlays.GraphicOverlay overlay in overlays)
            {
                Vector3 drawPos = parent.DrawPos;
                GraphicData gd;
                gd = overlay.graphicDataDefault;
                //g.data.
                drawPos.y += 0.046875f;
                drawPos += overlay.offsetDefault;
                if(i == 1)
                {
                    drawPos.x += Base.tempOffsetX;
                    drawPos.z += Base.tempOffsetY;
                    SetAnimationOffset(ref drawPos);
                }
                gd.Graphic.Draw(drawPos, parent.Rotation, parent, 0f);
                i++;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            ConfigureAnimation();
        }
        private void SetAnimationOffset(ref Vector3 drawPos)
        {

            drawPos.x += xOffset;
            drawPos.z += 0.5f * xOffset * xOffset;

        }
        private void ConfigureAnimation()
        {
            if (xUp)
            {
                xOffset += 0.01f;
            }
            else
            {
                xOffset -= 0.01f;
            }
            if (xOffset >= maxX)
            {
                xUp = false;
            }
            if (xOffset <= minX)
            {
                xUp = true;
            }
        }
    }

}
