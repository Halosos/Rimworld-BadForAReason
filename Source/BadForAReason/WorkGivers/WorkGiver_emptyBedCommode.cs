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
    public class WorkGiver_emptyBedCommode : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (def.workType == WorkTypeDefOf.Warden)
            {
                foreach (Thing item in pawn.Map.listerThings.ThingsOfDef(BFARDef.BFARBedCommode))
                {
                    if (item is Building_BedCommode wardenBuilding_BedCommode)
                    {
                        yield return wardenBuilding_BedCommode;
                    }
                }

                yield break;
            }

            IEnumerable<CompSewageHandler> enumerable = pawn.Map.PipeNet().PipeNets.SelectMany((PlumbingNet x) => x.Sewers); // may cause issues later
            foreach (Thing item2 in pawn.Map.listerThings.ThingsOfDef(BFARDef.BFARBedCommode))
            {
                if (item2 is Building_BedCommode nonWardenBuilding_BedCommode)
                {
                    yield return nonWardenBuilding_BedCommode;
                }
            }
        }
    
    
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return base.ShouldSkip(pawn, forced);
        }
        
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is Building_BedCommode building_BedCommode)
            {
                if (forced && building_BedCommode.sewage < 10f)
                {
                    return false;
                }

                if (!forced && building_BedCommode.sewage < building_BedCommode.sewageLimit * 0.3f)
                {
                    return false;
                }
            }

            if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger()))
            {
                return false;
            }

            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            return !t.IsBurning();
        }
        
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //if (t is Building_BedCommode)
           //{
                return JobMaker.MakeJob(BFARDef.BFAREmptyBedCommode, t);
            //}
        }
    }

}