using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using WhatTheHack.Comps;

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
            bool rogueAIAvailable = false;

            if (GetComp<CompHibernatable_MechanoidBeacon>().coolDownTicks > 0)
            {
                isDisabled = true;
                disabledReason = "WTH_CompHibernatable_MechanoidBeacon_Cooldown".Translate(((GetComp<CompHibernatable_MechanoidBeacon>().coolDownTicks / (float)GenDate.TicksPerDay)).ToStringDecimalIfSmall());
            }
            foreach (ThingWithComps thing in this.Map.listerThings.AllThings.OfType<ThingWithComps>())
            {
                if (thing.def == WTH_DefOf.WTH_RogueAI)
                {
                    rogueAIAvailable = true;
                }
                if(thing.TryGetComp<CompHibernatable_MechanoidBeacon>() is CompHibernatable_MechanoidBeacon comp && comp.State == HibernatableStateDefOf.Starting)
                {
                    if(thing.def == ThingDefOf.Ship_Reactor)
                    {
                        isDisabled = true;
                        disabledReason = "WTH_Reason_ReactorWarmingUp".Translate();
                    }
                    
                    if(thing.def == WTH_DefOf.WTH_MechanoidBeacon)
                    {
                        isDisabled = true;
                        disabledReason = "WTH_Reason_BeaconActive".Translate();
                    }
                   
                }
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
                    CompHibernatable_MechanoidBeacon comp = GetComp<CompHibernatable_MechanoidBeacon>();
                    float numDays = comp.Props.startupDays + comp.extraStartUpDays;
                    DiaNode diaNode = new DiaNode("WTH_BeaconWarmupWarning".Translate(numDays.ToStringDecimalIfSmall()));
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
                defaultLabel = "WTH_MechanoidBeaconStartup_Label".Translate(),
                defaultDesc = "WTH_MechanoidBeaconStartup_Description".Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true)
            };
        }
        public void StartupHibernatingParts()
        {
            CompHibernatable_MechanoidBeacon compHibernatable = this.TryGetComp<CompHibernatable_MechanoidBeacon>();
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
            Messages.Message("WTH_Message_MechanoidBeacon_Cancelled".Translate(), new RimWorld.Planet.GlobalTargetInfo(this.Position, this.Map), MessageTypeDefOf.NegativeEvent);
            CompHibernatable_MechanoidBeacon compHibernatable = this.TryGetComp<CompHibernatable_MechanoidBeacon>();
            compHibernatable.State = HibernatableStateDefOf.Hibernating;
        }
    }
}
