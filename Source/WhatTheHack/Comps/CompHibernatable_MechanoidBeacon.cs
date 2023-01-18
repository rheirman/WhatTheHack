using RimWorld;
using RimWorld.Planet;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps;

internal class CompHibernatable_MechanoidBeacon : CompHibernatable
{
    public int coolDownTicks;
    public int extraStartUpDays;

    public new CompProperties_Hibernatable_MechanoidBeacon Props => (CompProperties_Hibernatable_MechanoidBeacon)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref extraStartUpDays, "extraStartUpDays");
        Scribe_Values.Look(ref coolDownTicks, "coolDownTicks");
    }

    public override void CompTick()
    {
        if (State == HibernatableStateDefOf.Starting && Find.TickManager.TicksGame > endStartupTick)
            // Traverse.Create(this).Field("endStartupTick").GetValue<int>())
        {
            FinishWarmup();
        }

        if (coolDownTicks > 0)
        {
            coolDownTicks--;
        }

        if (State == HibernatableStateDefOf.Running && coolDownTicks == 0)
        {
            State = HibernatableStateDefOf.Hibernating;
        }
    }

    private void FinishWarmup()
    {
        State = HibernatableStateDefOf.Running;
        endStartupTick = 0;
        //Traverse.Create(this).Field("endStartupTick").SetValue(0);
        if (parent.Map.listerBuildings.allBuildingsColonist.FirstOrDefault(b => b is Building_RogueAI) is
            not Building_RogueAI rogueAI)
        {
            return;
        }

        Find.LetterStack.ReceiveLetter("WTH_MechanoidBeaconComplete_Label".Translate(),
            "WTH_MechanoidBeaconComplete_Description".Translate(), LetterDefOf.PositiveEvent,
            new GlobalTargetInfo(parent));
        if (!rogueAI.IsConscious)
        {
            rogueAI.IsConscious = true;
        }

        extraStartUpDays += 2;
        coolDownTicks += Props.coolDownDaysAfterSuccess * GenDate.TicksPerDay;
        var md = ThingMaker.MakeThing(WTH_DefOf.WTH_MechanoidData);
        md.stackCount = Rand.Range(25, 40);
        GenPlace.TryPlaceThing(md, parent.Position, parent.Map, ThingPlaceMode.Near);
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        text += "WTH_CompHibernatable_MechanoidBeacon_StartUpDays".Translate(Props.startupDays + extraStartUpDays);
        if (coolDownTicks > 0)
        {
            text += "\n" +
                    "WTH_CompHibernatable_MechanoidBeacon_Cooldown".Translate(
                        (coolDownTicks / (float)GenDate.TicksPerDay).ToStringDecimalIfSmall());
        }

        return text;
    }
}