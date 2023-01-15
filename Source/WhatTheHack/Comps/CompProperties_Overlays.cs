using UnityEngine;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps;

public class CompProperties_Overlays : CompProperties
{
    public GraphicOverlay background;
    public GraphicOverlay overlayEyeAnnoyed;
    public GraphicOverlay overlayEyeHappy;
    public GraphicOverlay overlayEyeMad;

    public CompProperties_Overlays()
    {
        compClass = typeof(CompOverlay);
    }

    public GraphicOverlay GetEyeOverlay(Building_RogueAI.Mood mood)
    {
        if (mood == Building_RogueAI.Mood.Happy)
        {
            return overlayEyeHappy;
        }

        if (mood == Building_RogueAI.Mood.Annoyed)
        {
            return overlayEyeAnnoyed;
        }

        return overlayEyeMad;
    }


    public class GraphicOverlay
    {
        public GraphicData graphicData;
        public Vector3 offsetDefault = Vector3.zero;
    }
}