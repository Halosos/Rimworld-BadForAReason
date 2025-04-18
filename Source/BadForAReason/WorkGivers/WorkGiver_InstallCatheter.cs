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
    public class WorkGiver_InstallCatheter : WorkGiver_Scanner
    {
        private MapComponent_CatheterCache cache;
        
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            
            
            cache = pawn.Map.GetComponent<MapComponent_CatheterCache>();
            

            if (cache != null)
            {
                foreach (Pawn eligiblePawn in cache.EligiblePawns)
                {
                    if (eligiblePawn != null && !eligiblePawn.Dead && !eligiblePawn.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
                    {
                        yield return eligiblePawn;
                    }
                }
            }
        }
        
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (cache == null || cache.EligiblePawns.Count == 0)
            {
                return true;
            }
            
            return base.ShouldSkip(pawn, forced);
        }
        
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn eligiblePawn))
            {
                return false;
            }
            if (!eligiblePawn.InBed())
            {
                return false;
            }
            if (!eligiblePawn.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
            {
                return false;
            }
            Thing bed = eligiblePawn.CurrentBed();

            if (bed == null)
            {
                return false;
            }
            
            var facility = bed.TryGetComp<CompFacility>();

            if (facility == null || !facility.LinkedBuildings.OfType<Building_CatheterMachine>().Any())
            {
                return false;
            }

            return true;
        }
        
        
    }
}