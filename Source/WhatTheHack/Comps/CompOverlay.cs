using UnityEngine;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps;

internal class CompOverlay : ThingComp
{
    private bool eyeMoving;
    private bool lookAround;
    private float maxX = 0.25f;
    private float minX = -0.25f;
    private int timer;
    private float xOffset;
    private bool xUp;
    public CompProperties_Overlays Props => props as CompProperties_Overlays;

    public bool ShowEye
    {
        get
        {
            var rogueAI = (Building_RogueAI)parent;
            return rogueAI.IsConscious && !rogueAI.WarmingUpAbility;
        }
    }

    public override void Initialize(CompProperties props)
    {
        Log.Message("CompOverlay Initialize");
        base.Initialize(props);
        //SetLookAround();
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
        var buildingRogueAi = parent as Building_RogueAI;
        DrawBackground(buildingRogueAi);
        if (ShowEye)
        {
            DrawEye(buildingRogueAi);
        }
    }

    private void DrawBackground(Building_RogueAI parent)
    {
        var background = Props.background;
        var gdRogueAI = background.graphicData;
        gdRogueAI.Graphic.Draw(parent.DrawPos + new Vector3(0, -1, 0), parent.Rotation, parent);
    }

    private void DrawEye(Building_RogueAI parent)
    {
        var overlay = Props.GetEyeOverlay(parent.goingRogue ? Building_RogueAI.Mood.Mad : parent.CurMoodCategory);
        var gdEye = overlay.graphicData;
        var drawPosEye = parent.DrawPos;
        drawPosEye.y += 0.046875f;
        drawPosEye += overlay.offsetDefault;
        SetAnimationOffset(ref drawPosEye);
        gdEye.Graphic.Draw(drawPosEye, parent.Rotation, parent);
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
            timer = eyeMoving ? Rand.Range(20, 50) : Rand.Range(50, 200);
        }

        timer--;
    }
}