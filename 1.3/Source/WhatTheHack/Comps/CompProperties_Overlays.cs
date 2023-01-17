using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps
{
    public class CompProperties_Overlays : CompProperties
    {
        public GraphicOverlay background;
        public GraphicOverlay overlayEyeHappy;
        public GraphicOverlay overlayEyeAnnoyed;
        public GraphicOverlay overlayEyeMad;


        public class GraphicOverlay
        {
            public GraphicData graphicData;
            public Vector3 offsetDefault = Vector3.zero;
        }
        public GraphicOverlay GetEyeOverlay(Building_RogueAI.Mood mood)
        {
            if(mood == Building_RogueAI.Mood.Happy)
            {
                return overlayEyeHappy;
            }
            else if (mood == Building_RogueAI.Mood.Annoyed)
            {
                return overlayEyeAnnoyed;
            }
            else {
                return overlayEyeMad;
            }
        }
        public CompProperties_Overlays()
        {
            compClass = typeof(CompOverlay);
        }
    }
}
