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
    public class JobDriver_emptyBedCommode : JobDriver
    {
        
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job);
        }

        

        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Building_BedCommode building_BedCommode = pawn.CurJob.GetTarget(TargetIndex.A).Thing as Building_BedCommode;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => pawn.Drafted);
            this.FailOn(() => building_BedCommode == null || building_BedCommode.sewage == 0f);
            yield return new Toil().FailOnDestroyedNullOrForbidden(TargetIndex.A);
            
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil toil = new Toil();
            toil.defaultDuration = 60;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.AddFinishAction(delegate
            {
                if (building_BedCommode == null || building_BedCommode.Sewage == 0)
                {
                    return;
                }
                Thing thing = ThingMaker.MakeThing(DubDef.FecalSludge);
                thing.stackCount = Mathf.CeilToInt(building_BedCommode.Sewage);
                GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                building_BedCommode.sewage = 0f;
                
            });
            yield return toil;
        }
    }
}