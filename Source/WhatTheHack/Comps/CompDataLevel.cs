using RimWorld;
using RimWorld.Planet;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Comps;

public class CompDataLevel : ThingComp
{
    private const int MAXLEVEL = 5;
    private float accumulatedData;
    public int curLevel = 1;
    private float extraDataNextLevel = 20;
    private float levelledData;

    private float DataNextLevel => levelledData + extraDataNextLevel;

    private float DataNeededNextLevel => DataNextLevel - accumulatedData;

    //CompRefuelable.Refuel() is prefixed with Harmony to call AccumulateData so data is accumulated in the accumulatedData variable
    public void AccumulateData(float amount)
    {
        accumulatedData += amount;
        MaybeLevelUp();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref accumulatedData, "accumulatedData");
        Scribe_Values.Look(ref levelledData, "levelledData");
        Scribe_Values.Look(ref curLevel, "curLevel");
        Scribe_Values.Look(ref extraDataNextLevel, "extraDataNextLevel");
    }

    public override string CompInspectStringExtra()
    {
        var text = "";
        if (parent is Building_RogueAI { IsConscious: false })
        {
            text += "WTH_CompDataLevel_NotConscious".Translate();
            return text;
        }

        text += "WTH_CompDataLevel_CurLevel".Translate(curLevel);
        if (curLevel != MAXLEVEL)
        {
            text += "\n" +
                    "WTH_CompDataLevel_DataNeededNextLevel".Translate(DataNeededNextLevel.ToStringDecimalIfSmall());
        }
        else
        {
            text += "\n" + "WTH_CompDataLevel_MaxLevel".Translate();
        }

        return text;
    }

    private void MaybeLevelUp()
    {
        if (curLevel >= MAXLEVEL)
        {
            return;
        }

        while (accumulatedData >= DataNextLevel)
        {
            Levelup();
        }
    }

    private void Levelup()
    {
        levelledData += extraDataNextLevel;
        curLevel += 1;
        extraDataNextLevel *= 1.5f;
        if (curLevel < MAXLEVEL)
        {
            Find.LetterStack.ReceiveLetter("WTH_Message_LevelUp_Label".Translate(curLevel),
                "WTH_Message_LevelUp_Description".Translate(curLevel), LetterDefOf.PositiveEvent,
                new GlobalTargetInfo(parent));
        }
        else
        {
            Find.LetterStack.ReceiveLetter("WTH_Message_LevelUpMax_Label".Translate(),
                "WTH_Message_LevelUpMax_Description".Translate(), LetterDefOf.ThreatBig, new GlobalTargetInfo(parent));
            ((Building_RogueAI)parent).GoRogue();
        }
    }
}