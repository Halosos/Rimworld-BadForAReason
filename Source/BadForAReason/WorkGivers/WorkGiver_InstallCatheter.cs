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
            Log.Message("PotentialWorkThingsGlobal called: " + pawn);
            
            cache = pawn.Map.GetComponent<MapComponent_CatheterCache>();
            

            if (cache != null)
            {
                foreach (Pawn eligiblePawn in cache.EligiblePawns)
                {
                    if (eligiblePawn != null && !eligiblePawn.Dead && !eligiblePawn.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
                    {
                        Log.Message("Eligible pawn set: " + eligiblePawn);
                        yield return eligiblePawn;
                    }
                }
                
            }
        }
        
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        
        public override bool ShouldSkip(Pawn pawn, bool forced = false) // TODO: THIS IS BORK
        {
            if (cache == null || cache.EligiblePawns.Count == 0)
            {
                Log.Message("Should skip returned: true");
                return true;
            }
            Log.Message("Should skip returned: " + pawn + forced);
            return base.ShouldSkip(pawn, forced);
        }
        
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Log.Message("HasJobOnThing called");
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
                Log.Message("HasJobOnThing: false");
                return false;
            }
            Log.Message("HasJobOnThing: true");
            return true;
        }
        
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Log.Message("JobOnThing called. Thing is: " + t.ToString());
            if (t is Pawn target)
            {
                if (!target.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
                {
                    Log.Message("Source: " + pawn);
                    Log.Message("Target: " + target);
                    return JobMaker.MakeJob(BFARDef.BFARInstallCatheter, t);
                }
            }
            return null;
        }
    }
}