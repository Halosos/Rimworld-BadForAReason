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
        
        public override bool ShouldSkip(Pawn pawn, bool forced = false) // TODO: THIS IS BORK
        {
            Log.Message("Starting should skip");
            
            if (cache == null || cache.EligiblePawns.Count == 0)
            {
                Log.Message("Should skip returned: true");
                return true;
            }
            Log.Message("[ShouldSkip] Possibly skipped");
            return false;
        }
        
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Log.Message("HasJobOnThing called");
            if (!(t is Pawn eligiblePawn))
            {
                Log.Message("!(t is Pawn eligiblePawn): true");
                return false;
            }
            if (!eligiblePawn.InBed())
            {
                Log.Message("!eligiblePawn.InBed(): true");
                return false;
            }
            if (eligiblePawn.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
            {
                Log.Message("Has catheter already");
                return false;
            }
            Building_Bed bed = eligiblePawn.CurrentBed();

            if (bed == null)
            {
                Log.Message("Bed is null");
                return false;
            }
            
            if (!HelperMethods.IsBedWithCatheter(bed, eligiblePawn.Map))
            {
                Log.Message("No catheter linked to bed");
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
                Log.Message("Target is: " + target);
                
                if (!target.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
                {
                    Log.Message("Target does not have catheter. Call job.");
                    return JobMaker.MakeJob(BFARDef.BFARInstallCatheter, t);
                }
            }
            return null;
        }
    }
}