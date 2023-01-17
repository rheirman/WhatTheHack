using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatTheHack.Buildings;

/*
namespace WhatTheHack.Stats
{
    class StatPart_RepairRate : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder sb = new StringBuilder();
            if (req.Thing is Building_MechanoidPlatform platform)
            {
                sb.AppendLine(platform.Label + ": " + platform.def.building.bed_healPerDay);
            }
            return sb.ToString();
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if(req.Thing is Building_MechanoidPlatform platform)
            {
                val += platform.def.building.bed_healPerDay;
            }
            throw new NotImplementedException();
        }
    }
}
*/