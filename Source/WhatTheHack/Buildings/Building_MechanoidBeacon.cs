using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Buildings;

internal class Building_MechanoidBeacon : Building
{
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var c in base.GetGizmos())
        {
            yield return c;
        }

        foreach (var c2 in StartupGizmos())
        {
            yield return c2;
        }
    }

    public IEnumerable<Gizmo> StartupGizmos()
    {
        var isDisabled = false;
        var disabledReason = "";
        var rogueAIAvailable = false;

        if (GetComp<CompHibernatable_MechanoidBeacon>().coolDownTicks > 0)
        {
            isDisabled = true;
            disabledReason = "WTH_CompHibernatable_MechanoidBeacon_Cooldown".Translate(
                (GetComp<CompHibernatable_MechanoidBeacon>().coolDownTicks / (float)GenDate.TicksPerDay)
                .ToStringDecimalIfSmall());
        }

        foreach (var thing in Map.listerThings.AllThings.OfType<ThingWithComps>())
        {
            if (thing.def == WTH_DefOf.WTH_RogueAI)
            {
                rogueAIAvailable = true;
            }

            if (thing.TryGetComp<CompHibernatable_MechanoidBeacon>() is not { } comp ||
                comp.State != HibernatableStateDefOf.Starting)
            {
                continue;
            }

            if (thing.def == ThingDefOf.Ship_Reactor)
            {
                isDisabled = true;
                disabledReason = "WTH_Reason_ReactorWarmingUp".Translate();
            }

            if (thing.def != WTH_DefOf.WTH_MechanoidBeacon)
            {
                continue;
            }

            isDisabled = true;
            disabledReason = "WTH_Reason_BeaconActive".Translate();
        }

        if (!rogueAIAvailable)
        {
            isDisabled = true;
            disabledReason = "WTH_Reason_NoRogueAI".Translate();
        }

        if (!GetComp<CompPowerTrader>().PowerOn)
        {
            isDisabled = true;
            disabledReason = "WTH_Reason_NoPower".Translate();
        }


        yield return new Command_Action
        {
            action = delegate
            {
                var comp = GetComp<CompHibernatable_MechanoidBeacon>();
                var numDays = comp.Props.startupDays + comp.extraStartUpDays;
                var diaNode = new DiaNode("WTH_BeaconWarmupWarning".Translate(numDays.ToStringDecimalIfSmall()));
                var diaOption = new DiaOption("Confirm".Translate())
                {
                    action = StartupHibernatingParts,
                    resolveTree = true
                };
                diaNode.options.Add(diaOption);
                var diaOption2 = new DiaOption("GoBack".Translate())
                {
                    resolveTree = true
                };
                diaNode.options.Add(diaOption2);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true));
            },
            disabled = isDisabled,
            disabledReason = disabledReason,
            defaultLabel = "WTH_MechanoidBeaconStartup_Label".Translate(),
            defaultDesc = "WTH_MechanoidBeaconStartup_Description".Translate(),
            hotKey = KeyBindingDefOf.Misc1,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower")
        };
    }

    public void StartupHibernatingParts()
    {
        var compHibernatable = this.TryGetComp<CompHibernatable_MechanoidBeacon>();
        if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
        {
            compHibernatable.Startup();
        }
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        CancelStartup();
        base.Destroy(mode);
    }

    public void CancelStartup()
    {
        Messages.Message("WTH_Message_MechanoidBeacon_Cancelled".Translate(), new GlobalTargetInfo(Position, Map),
            MessageTypeDefOf.NegativeEvent);
        var compHibernatable = this.TryGetComp<CompHibernatable_MechanoidBeacon>();
        compHibernatable.State = HibernatableStateDefOf.Hibernating;
    }
}