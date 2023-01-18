using RimWorld;
using UnityEngine;
using Verse;

namespace WhatTheHack.Buildings;

//class for minified things with a custom graphic. 
internal class MinifiedThing_Custom : MinifiedThing
{
    public override Graphic Graphic => DefaultGraphic;

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        Graphic.Draw(drawLoc, Rot4.North, this);
    }
}