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
        private float maxX = 0.25f;
        private float minX = -0.25f;
        private float xOffset = 0.0f;
        private bool xUp = false;
        private bool eyeMoving = false;
        private bool lookAround = false;
        private int timer = 0;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            //SetLookAround();
        }
        public bool ShowEye
        {
            get
            {
                Building_RogueAI rogueAI = (Building_RogueAI)parent;
                return rogueAI.IsConscious && !rogueAI.WarmingUpAbility;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref maxX, "maxX");
            Scribe_Values.Look(ref minX, "minX");
            Scribe_Values.Look(ref xOffset, "xOffset");
            Scribe_Values.Look(ref xUp, "xUp");
            Scribe_Values.Look(ref eyeMoving, "eyeMoving");
            Scribe_Values.Look(ref lookAround, "lookAround");
            Scribe_Values.Look(ref timer, "timer");
        }

        public override void PostDraw()
        {
            base.PostDraw();
            Building_RogueAI parent = this.parent as Building_RogueAI;
            DrawBackground(parent);
            if (ShowEye)
            {
                DrawEye(parent);
            }
            
        }

        private void DrawBackground(Building_RogueAI parent)
        {
            CompProperties_Overlays.GraphicOverlay background = Props.background;
            GraphicData gdRogueAI = background.graphicDataDefault;
            gdRogueAI.Graphic.Draw(parent.DrawPos + new Vector3(0, -1, 0), parent.Rotation, parent, 0f);
        }

        private void DrawEye(Building_RogueAI parent)
        {
            CompProperties_Overlays.GraphicOverlay overlay = Props.GetEyeOverlay(parent.goingRogue ? Building_RogueAI.Mood.Mad : parent.CurMoodCategory);
            GraphicData gdEye = overlay.graphicDataDefault;
            Vector3 drawPosEye = parent.DrawPos;
            drawPosEye.y += 0.046875f;
            drawPosEye += overlay.offsetDefault;
            SetAnimationOffset(ref drawPosEye);
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
        
        public void SetLookAround()
        {
            lookAround = true;
            maxX = 0.25f;
            minX = -0.25f;
        }
        public void UnsetLookAround()
        {
            eyeMoving = true;
            lookAround = false;
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
