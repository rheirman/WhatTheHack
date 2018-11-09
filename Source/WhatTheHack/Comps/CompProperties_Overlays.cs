using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Comps
{
    public class CompProperties_Overlays : CompProperties
    {
        public List<GraphicOverlay> overlayFront;
        public GraphicOverlay overlaySide;
        public GraphicOverlay overlayBack;


        public class GraphicOverlay
        {
            public GraphicData graphicDataDefault;
            public GraphicData graphicDataFemale;
            public GraphicData graphicDataMale;

            public Vector3 offsetDefault = Vector3.zero;
            public Vector3 offsetFemale = Vector3.zero;
            public Vector3 offsetMale = Vector3.zero;


        }
        public List<GraphicOverlay> GetOverlay(Rot4 dir)
        {
            return overlayFront;
        }

        public CompProperties_Overlays()
        {
            compClass = typeof(CompOverlay);
        }
    }
}
