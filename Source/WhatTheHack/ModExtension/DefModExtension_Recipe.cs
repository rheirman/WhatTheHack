using Verse;

namespace WhatTheHack;

public class DefModExtension_Recipe : DefModExtension
{
    public BodyPartDef additionalHediffBodyPart;
    public HediffDef addsAdditionalHediff;
    public float deathOnFailedSurgeryChance = -1f;
    public float minBodySize = -1f;
    public bool needsFixedBodyPart = false;
    public bool requireBed;
    public HediffDef requiredHediff;
    public float surgerySuccessCap = -1f;
    public float surgerySuccessChanceFactor = -1f;
}