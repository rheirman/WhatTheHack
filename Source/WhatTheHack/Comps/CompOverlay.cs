using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps
{
    class CompOverlay : ThingComp
    {
        public CompProperties_Overlays Props => props as CompProperties_Overlays;
        private float maxX = 0.5f;
        private float minX = -0.5f;
        private float xOffset = 0.0f;
        private bool xUp = true;
        private bool eyeMoving = true;
        private bool lookAround = true;
        private int timer = 0;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            SetLookAround();
        }

        public override void PostDraw()
        {
            base.PostDraw();
            Building_RogueAI parent = this.parent as Building_RogueAI;
            CompProperties_Overlays.GraphicOverlay overlayRogueAI = Props.overlayFront;
            CompProperties_Overlays.GraphicOverlay overlay = Props.GetEyeOverlay(parent.CurMoodCategory);

            GraphicData gdRogueAI = overlayRogueAI.graphicDataDefault;
            GraphicData gdEye = overlay.graphicDataDefault;
            //g.data.

            Vector3 drawPosEye = parent.DrawPos;
            drawPosEye.y += 0.046875f;
            drawPosEye += overlay.offsetDefault;
            drawPosEye.x += Base.tempOffsetX;
            drawPosEye.y += Base.tempOffsetY;
            drawPosEye.z += Base.tempOffsetZ;
            SetAnimationOffset(ref drawPosEye);

            gdRogueAI.Graphic.Draw(parent.DrawPos + new Vector3(0, -1, 0), parent.Rotation, parent, 0f);
            gdEye.Graphic.Draw(drawPosEye, parent.Rotation, parent, 0f);

        }
        public override void CompTick()
        {
            base.CompTick();
            if (lookAround)
            {
                ConfigureLookAround();
            }
            if (eyeMoving)
            {
                ConfigureAnimation();
            }
        }
        private void SetAnimationOffset(ref Vector3 drawPos)
        {

            drawPos.x += xOffset;
            drawPos.z += 0.5f * xOffset * xOffset;

        }
        
        private void SetLookAround()
        {
            lookAround = true;
            maxX = 0.25f;
            minX = -0.25f;
        }
        private void UnsetLookAround()
        {
            eyeMoving = true;
            lookAround = true;
            maxX = 0.5f;
            minX = -0.5f;
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

        private void ConfigureLookAround()
        {
            if (timer <= 0)
            {
                eyeMoving = !eyeMoving;
                xUp = Rand.Chance(0.5f);
                if (eyeMoving)
                {
                    timer = Rand.Range(20, 50);
                }
                else
                {
                    timer = Rand.Range(50, 200);
                }
            }
            timer--;
        }
    }

}
