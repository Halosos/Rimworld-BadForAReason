using System;
using System.Collections.Generic;
using System.IO;
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
        
        public const int BaseTendDuration = 600;
        private const int TicksBetweenSelfTendMotes = 100;
        
        private static List<Toil> tmpCollectToils = new List<Toil>();

        protected Thing MedicineUsed => job.targetB.Thing;

        protected Pawn Patient => job.targetA.Pawn;

        protected bool IsMedicineInDoctorInventory => MedicineUsed != null && pawn.inventory.Contains(MedicineUsed);
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Patient != pawn)
            {
                if (!pawn.Reserve(Patient, job, 1, -1, null, errorOnFailed))
                {
                    return false;
                }
            }
            
            if (MedicineUsed != null)
            {
                int num = pawn.Map.reservationManager.CanReserveStack(pawn, MedicineUsed, 10);
                if (num <= 0 || !pawn.Reserve(MedicineUsed, job, 10,
                        10))
                {
                    return false;
                }
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() =>
            {
                if (MedicineUsed != null 
                    && pawn.Faction == Faction.OfPlayer 
                    && Patient.playerSettings != null 
                    && !Patient.playerSettings.medCare.AllowsMedicine(MedicineUsed.def))
                {
                    return true;
                }

                return (pawn == Patient 
                        && pawn.Faction == Faction.OfPlayer 
                        && pawn.playerSettings != null 
                        && !pawn.playerSettings.selfTend);


            });
            
            this.FailOnAggroMentalState(TargetIndex.A);
            
            usesMedicine = MedicineUsed != null;
            
            Toil reserveMedicine = null;
            Toil gotoPatient = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            if (usesMedicine)
            {
                var toils = CollectMedicineToils(pawn, Patient, job, gotoPatient, out reserveMedicine);
                foreach (var toil in toils)
                {
                    yield return toil;
                }
                
            }
            
            yield return gotoPatient;

            int ticks = (int)(1f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * 600f);
            Toil wait = Toils_General.Wait(ticks);
            wait.WithProgressBarToilDelay(TargetIndex.A);
            wait.activeSkill = () => SkillDefOf.Medicine;
            wait.handlingFacing = true;
            
            yield return wait;

            yield return Toils_InstallCatheter.FinalizeCatheter(Patient);

            if (usesMedicine)
            {
                yield return FindMoreMedicineToil_NewTemp(pawn, Patient, MedicineIndex, job, reserveMedicine);
            }

            yield return Toils_Jump.Jump(gotoPatient);
        }


        public static List<Toil> CollectMedicineToils(Pawn doctor, Pawn patient, Job job, Toil gotoPatient,
            out Toil reserveMedicine)
        {
            tmpCollectToils.Clear();

            Thing medicineUsed = job.targetB.Thing;
            Pawn_InventoryTracker holderInv = medicineUsed?.ParentHolder as Pawn_InventoryTracker;
            Pawn otherHolder = job.targetC.Pawn;
            
            reserveMedicine = Toils_Tend.ReserveMedicine(MedicineIndex, patient).FailOnDespawnedNullOrForbidden(MedicineIndex);
            
            tmpCollectToils.Add(Toils_Jump.JumpIf(gotoPatient, () => medicineUsed != null && doctor.inventory.Contains(medicineUsed)));
            
            Toil approach = Toils_Goto.GotoThing(MedicineHolderIndex, PathEndMode.Touch).FailOn(() => otherHolder != holderInv?.pawn || otherHolder.IsForbidden(doctor));
            
            tmpCollectToils.Add(Toils_Haul.CheckItemCarriedByOtherPawn(medicineUsed, MedicineHolderIndex, approach));
            tmpCollectToils.Add(reserveMedicine);
            tmpCollectToils.Add(Toils_Goto.GotoThing(MedicineIndex, PathEndMode.ClosestTouch));
            tmpCollectToils.Add(Toils_Tend.PickupMedicine(MedicineIndex, patient));
            tmpCollectToils.Add(Toils_Haul.CheckForGetOpportunityDuplicate(reserveMedicine, MedicineIndex, TargetIndex.None, takeFromValidStorage: true));
            tmpCollectToils.Add(Toils_Jump.Jump(gotoPatient));
            tmpCollectToils.Add(approach);
            tmpCollectToils.Add(Toils_General.Wait(25).WithProgressBarToilDelay(MedicineHolderIndex));
            tmpCollectToils.Add(Toils_Haul.TakeFromOtherInventory(medicineUsed, doctor.inventory.innerContainer, holderInv?.innerContainer, 1, MedicineIndex));
            
            return tmpCollectToils;
        }
        
        public static Toil FindMoreMedicineToil_NewTemp(Pawn doctor, Pawn patient, TargetIndex medIndex, Job job, Toil reserveToil)
        {
            Toil toil = ToilMaker.MakeToil("FindMoreMedicineToil_NewTemp");
            toil.initAction = delegate
            {
                if (job.GetTarget(medIndex).Thing.DestroyedOrNull())
                {
                    Thing newMed = HealthAIUtility.FindBestMedicine(doctor, patient);
                    if (newMed != null)
                    {
                        job.SetTarget(medIndex, newMed);
                        doctor.jobs.curDriver.JumpToToil(reserveToil);
                    }
                }
            };
            return toil;
        }
    }
    
    

    public class Toils_InstallCatheter : Toils_Tend
    {
        public static Toil FinalizeCatheter(Pawn patient)
        {
            Toil toil = ToilMaker.MakeToil("FinalizeCatheter");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Medicine medicine = actor.CurJob.targetB.Thing as Medicine;
                if (actor.skills != null)
                {
                    float num = (patient.RaceProps.Animal ? 175f : 500f);
                    float num2 = medicine?.def.MedicineTendXpGainFactor ?? 0.1f;
                    actor.skills.Learn(SkillDefOf.Medicine, num * num2);
                }

                if (!patient.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
                {
                    patient.health.AddHediff(BFARDef.BFARInstalledCatheter);
                }
                if (medicine != null && medicine.Destroyed)
                {
                    actor.CurJob.SetTarget(TargetIndex.B, LocalTargetInfo.Invalid);
                }
                if (toil.actor.CurJob.endAfterTendedOnce)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
            }
        }
    }