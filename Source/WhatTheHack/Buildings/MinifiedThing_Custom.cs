using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Buildings
{
    //class for minified things with a custom graphic. 
    class MinifiedThing_Custom : MinifiedThing
    {
        public override Graphic Graphic
        {
            get
            {
                return DefaultGraphic;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            this.Graphic.Draw(drawLoc, Rot4.North, this, 0f);
        }
    }
}
