using System.Collections.Generic;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;

//Used for mechanoids only.  

namespace WhatTheHack.Storage;

public class ExtendedPawnData : IExposable
{
    public bool canWorkNow;
    public Building_PortableChargingPlatform caravanPlatform;
    public Building_RogueAI controllingAI;
    public bool isActive;

    //MAKE SURE TO UPDATE SHOULDCLEAN WHEN ADDING FIELDS!!!!

    public bool isHacked; //obsolete. 
    public Faction originalFaction;

    public Pawn remoteControlLink;
    public bool shouldExplodeNow; //obsolete. Still here for compatibility with old saves.
    public CompMountable turretMount = null;
    public List<WorkTypeDef> workTypes;

    public void ExposeData()
    {
        Scribe_Values.Look(ref isHacked, "isHacked"); //Obsolete. 
        Scribe_Values.Look(ref canWorkNow, "canWorkNow");
        Scribe_Values.Look(ref shouldExplodeNow, "shouldExplodeNow");
        Scribe_Values.Look(ref isActive, "isActive");
        Scribe_References.Look(ref remoteControlLink, "remoteControlLink");
        Scribe_References.Look(ref controllingAI, "controllingAI");
        Scribe_References.Look(ref caravanPlatform, "caravanPlatform");
        Scribe_References.Look(ref originalFaction, "originalFaction");
        //Scribe_References.Look(ref turretMount, "turretMount");
        Scribe_Collections.Look(ref workTypes, "workTypes");
    }


    public bool ShouldSave(Pawn pawn)
    {
        if ((pawn.IsColonist || pawn.IsMechanoid()) && !(pawn.Dead || pawn.Destroyed))
        {
            return true;
        }

        return false;
    }

    public bool ShouldClean()
    {
        var foundValue = false;
        foreach (var fi in GetType().GetFields())
        {
            var fival = fi.GetValue(this);

            if (fival is bool and true)
            {
                foundValue = true;
            }
            else if (fival != null && fival is not int && fival is not float && fival is not bool)
            {
                foundValue = true;
            }
        }

        if (!foundValue)
        {
            return true;
        }

        return false;
    }
}