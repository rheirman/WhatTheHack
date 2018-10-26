using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Buildings
{
    
    class Building_MechanoidBeacon : Building
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            foreach (Gizmo c2 in StartupGizmos())
            {
                yield return c2;
            }
        }
        public IEnumerable<Gizmo> StartupGizmos()
        {
            bool isDisabled = false;
            string disabledReason = "";
            foreach (ThingWithComps current in this.Map.listerThings.AllThings.OfType<ThingWithComps>())
            {
                if(current.TryGetComp<CompHibernatable>() is CompHibernatable comp && comp.State == HibernatableStateDefOf.Starting)
                {
                    if(current.def == ThingDefOf.Ship_Reactor)
                    {
                        isDisabled = true;
                        disabledReason = "WTH_Reason_ReactorWarmingUp";
                    }
                    
                    if(current.def == WTH_DefOf.WTH_MechanoidBeacon)
                    {
                        isDisabled = true;
                        disabledReason = "WTH_Reason_BeaconActive";
                    }
                   
                }
            }

            yield return new Command_Action
            {


                action = delegate
                {
                    string text = "WTH_BeaconWarmupWarning";
                    /*
                    if (!Find.Storyteller.difficulty.allowBigThreats)
                    {
                        text += "Pacifist";
                    }
                    */
                    DiaNode diaNode = new DiaNode(text.Translate());
                    DiaOption diaOption = new DiaOption("Confirm".Translate());
                    diaOption.action = delegate
                    {
                        StartupHibernatingParts();
                    };
                    diaOption.resolveTree = true;
                    diaNode.options.Add(diaOption);
                    DiaOption diaOption2 = new DiaOption("GoBack".Translate());
                    diaOption2.resolveTree = true;
                    diaNode.options.Add(diaOption2);
                    Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, null));
                },
                disabled = isDisabled,
                disabledReason = disabledReason,
                defaultLabel = "CommandShipStartup".Translate(),
                defaultDesc = "CommandShipStartupDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true)
            };
        }
        public void StartupHibernatingParts()
        {
            CompHibernatable compHibernatable = this.TryGetComp<CompHibernatable>();
            if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
            {
                compHibernatable.Startup();
            }
        }
    }
}
