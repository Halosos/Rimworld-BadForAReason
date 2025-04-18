using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using DubsBadHygiene;

namespace BadForAReason 
{
    public class ThoughtWorker_HaveCatheter : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter);
        }
    }
}