using Verse;

namespace WhatTheHack;

public class DefModExtension_Ability : DefModExtension
{
    public float failChance = 0f;
    public float fuelDrain = 0f;
    public HediffDef hediffSelf;
    public HediffDef hediffTarget;
    public float powerDrain = 0f;
    public int warmupTicks = 0;
}