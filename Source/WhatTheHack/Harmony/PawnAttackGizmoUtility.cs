using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    // all patches here are identical. They detour pawn.get_IsColonistPlayerControlled to pawn.CanTakeOrder (an extension method added here). This extension method returns true 
    // Patching get_IsColonistPlayerControlled directly would be too aggressive since that method is also used for purposes unrelated to combat, and is probably used for all kinds of purposes by other mods. 
}
