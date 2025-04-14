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
    public class HediffCatheter : Hediff
    {
        ThoughtDef memoryDef = DefDatabase<ThoughtDef>.GetNamed("BFARHadCatheter");
        
        public void PostAdd()
        {
            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(memoryDef);
        }
        
        public override void Tick()
        {
            base.Tick();
            
            if (pawn.IsHashIntervalTick(250))
            {
                if (!(pawn.CurrentBed() is Building_BedCommode))
                {
                    pawn.health.RemoveHediff(this);
                }
            }
        }

        public override void PostRemoved()
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(memoryDef);
        }
    }
}