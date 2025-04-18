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
        
        
        public override void PostAdd(DamageInfo? dinfo)
        {
            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(memoryDef);
            MapComponent_CatheterCache cache = pawn.Map.GetComponent<MapComponent_CatheterCache>();
            cache.Remove(pawn);
            
        }
        
        public override void Tick()
        {
            base.Tick();
            
            if (pawn.IsHashIntervalTick(150))
            {
                if (!(pawn.CurrentBed() is Building_ComaBed)) 
                {
                    pawn.health.RemoveHediff(this);
                }

                if (IsCatheterMachineGone())
                {
                    pawn.health.RemoveHediff(this);
                }
            }
        }

        public override void PostRemoved()
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(memoryDef);
        }

        private bool IsCatheterMachineGone()
        {
            var bed = pawn.CurrentBed() as Building_Bed;
            if (bed == null || pawn.Map == null)
            {
                return true;
            }
            int bedSlot = bed.CurOccupants.ToList().IndexOf(pawn);

            var compFacility = bed.GetComp<CompFacility>();
            if (compFacility == null || compFacility.LinkedBuildings.NullOrEmpty())
            {
                return true;
            }
            
            IntVec3 pawnPos = pawn.Position;
            foreach (Thing thing in compFacility.LinkedBuildings)
            {
                if (thing is Building_CatheterMachine building)
                {
                    if (building.Spawned && !building.DestroyedOrNull())
                    {
                        var adjacentcells = GenAdj.CellsAdjacent8Way(building).ToList();
                        if (adjacentcells.Contains(pawn.Position) || adjacentcells.Contains(bed.GetFootSlotPos(bedSlot)))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}