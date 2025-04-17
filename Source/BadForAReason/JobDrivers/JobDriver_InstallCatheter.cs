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
    public class JobDriver_InstallCatheter : JobDriver_TendPatient
    {
	    private bool usesMedicine;
		
	    //private PathEndMode pathEndMode;

	    
	    protected override IEnumerable<Toil> MakeNewToils()
	    {
		    this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		    this.FailOn(delegate
		    {
			    if (MedicineUsed != null && pawn.Faction == Faction.OfPlayer && Deliveree.playerSettings != null &&
			        !Deliveree.playerSettings.medCare.AllowsMedicine(MedicineUsed.def))
			    {
				    return true;
			    }

			    return (pawn == Deliveree && pawn.Faction == Faction.OfPlayer && pawn.playerSettings != null &&
			            !pawn.playerSettings.selfTend)
				    ? true
				    : false;
		    });

		    this.FailOn(() =>
		    {
		    var cache = pawn.Map.GetComponent<MapComponent_CatheterCache>();
		    return !cache.Contains(Deliveree);
			});

	    AddEndCondition(delegate
			{
				if (pawn.Faction == Faction.OfPlayer)
				{
					return JobCondition.Ongoing;
				}
				return job.playerForced || pawn.Faction != Faction.OfPlayer ? JobCondition.Ongoing : JobCondition.Succeeded;
			});
			this.FailOnAggroMentalState(TargetIndex.A);
			Toil reserveMedicine = null;
			Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, pathEndMode);
			if (usesMedicine)
			{
				List<Toil> list = CollectMedicineToils(pawn, Deliveree, job, gotoToil, out reserveMedicine);
				foreach (Toil item in list)
				{
					yield return item;
				}
			}
			yield return gotoToil;
			int ticks = (int)(1f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * 600f);
			Toil waitToil;
			if (!job.draftedTend || pawn == base.TargetPawnA)
			{
				waitToil = Toils_General.Wait(ticks);
			}
			else
			{
				waitToil = Toils_General.WaitWith_NewTemp(TargetIndex.A, ticks, useProgressBar: false, maintainPosture: true, maintainSleep: false, TargetIndex.A, pathEndMode);
				waitToil.AddFinishAction(delegate
				{
					if (Deliveree != null && Deliveree != pawn && Deliveree.CurJob != null && (Deliveree.CurJob.def == JobDefOf.Wait || Deliveree.CurJob.def == JobDefOf.Wait_MaintainPosture))
					{
						Deliveree.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				});
			}
			waitToil.WithProgressBarToilDelay(TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
			waitToil.activeSkill = () => SkillDefOf.Medicine;
			waitToil.handlingFacing = true;
			waitToil.tickAction = delegate
			{
				if (pawn == Deliveree && pawn.Faction != Faction.OfPlayer && pawn.IsHashIntervalTick(100) && !pawn.Position.Fogged(pawn.Map))
				{
					FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
				}
				if (pawn != Deliveree)
				{
					pawn.rotationTracker.FaceTarget(Deliveree);
				}
			};
			waitToil.FailOn(() => pawn != Deliveree && !pawn.CanReachImmediate(Deliveree.SpawnedParentOrMe, pathEndMode));
			yield return Toils_Jump.JumpIf(waitToil, () => !usesMedicine || !IsMedicineInDoctorInventory);
			yield return Toils_Tend.PickupMedicine(TargetIndex.B, Deliveree).FailOnDestroyedOrNull(TargetIndex.B);
			yield return waitToil;
			yield return Toils_InstallCatheter.FinalizeCatheter(base.Deliveree);
			if (usesMedicine)
			{
				yield return FindMoreMedicineToil_NewTemp(pawn, Deliveree, TargetIndex.B, job, reserveMedicine);
			}
			yield return Toils_Jump.Jump(gotoToil);
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
                Medicine medicine = (Medicine)actor.CurJob.targetB.Thing;
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