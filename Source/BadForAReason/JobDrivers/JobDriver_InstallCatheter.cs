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
    public class JobDriver_InstallCatheter : JobDriver
    {

        private bool usesMedicine;
        private const TargetIndex MedicineIndex = TargetIndex.B;
        private const TargetIndex MedicineHolderIndex = TargetIndex.C;
        private static List<Toil> tmpCollectToils = new List<Toil>();
        protected Thing MedicineUsed => job.targetA.Thing;
        protected Pawn Deliveree => job.targetA.Pawn;

        protected bool IsMedicineInDoctorInventory
        {
            get
            {
                if (MedicineUsed != null)
                {
                    return pawn.inventory.Contains(MedicineUsed);
                }
                return false;
            }
        }

        protected Pawn_InventoryTracker MedicineHolderInventory => MedicineUsed?.ParentHolder as Pawn_InventoryTracker;
        
        protected Pawn OtherPawnMedicineHolder => job.targetB.Pawn;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return new Toil().FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            
            Toil toil = new Toil();
            toil.defaultDuration = 300;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.AddFinishAction(delegate
            {
                Pawn target = (Pawn)job.targetA.Thing;

                if (!target.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
                {
                    target.health.AddHediff(BFARDef.BFARInstalledCatheter);
                }
                
            });
            yield return toil;
        }
    }
}