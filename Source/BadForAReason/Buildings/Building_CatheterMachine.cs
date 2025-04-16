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
using UnityEngine.Diagnostics;

namespace BadForAReason
{
    public class Building_CatheterMachine : Building
    {
        public CompPowerTrader power;
        public CompPipe pipe;
        public CompBFARSewageHandler compFARSewageHandler;
        public CompBlockage blockage;
        public CompFacility facility;

        public CatheterState useMode = CatheterState.ManualOnly;
        public MapComponent_CatheterCache cache;
        
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            power = GetComp<CompPowerTrader>();
            pipe = GetComp<CompPipe>();
            compFARSewageHandler = GetComp<CompBFARSewageHandler>();
            blockage = GetComp<CompBlockage>();
            facility = GetComp<CompFacility>();
            cache = Map.GetComponent<MapComponent_CatheterCache>();
            

            if (power == null)
            {
                throw new MissingFieldException(nameof (CompPower));
            }

            if (pipe == null)
            {
                throw new MissingFieldException(nameof (CompPipe));
            }

            if (compFARSewageHandler == null)
            {
                throw new MissingFieldException(nameof (CompBFARSewageHandler));
            }

            if (blockage == null)
            {
                throw new MissingFieldException(nameof (CompBlockage));
            }

            if (facility == null)
            {
                throw new MissingFieldException(nameof (CompFacility));
            }
            
        }
        
        
        
        public override void TickRare()
        {
            
            Thing linkedThing = facility.LinkedBuildings.FirstOrDefault();

            if (!power.PowerOn || !pipe.pipeNet.Sewers.Any(h => h.parent != this) || blockage.blocked)
            {
                return;
            }
            
            if (!(linkedThing is Building_Bed bed && bed.CurOccupants.Count() != 0))
            {
                return;
            }
            
            {
                
                Pawn targetPawn = null;
                float shortDistance = float.MaxValue;
                
                foreach (Pawn pawn in bed.CurOccupants)
                {
                    if (!pawn.health.hediffSet.HasHediff(BFARDef.BFARInstalledCatheter))
                    {
                        float distance = (pawn.Position - this.Position).LengthHorizontalSquared;
                        if (distance < shortDistance)
                        {
                            shortDistance = distance;
                            targetPawn = pawn;
                        }
                    }
                }

                if (targetPawn == null)
                {
                    return;
                }
                if (returnEligibility(targetPawn))
                {
                    cache.Add(targetPawn);
                }
            }
        }
        
        // Gizmo logic below

        public enum CatheterState
        {
            ManualOnly,
            AllowCannotWalk,
            AllowIll,
            AllowUnconscious,
            AllowAll
        }

        private string GetModeLabel(CatheterState mode)
        {
            switch (mode)
            {
                case CatheterState.ManualOnly:
                    return "BFARManual".Translate();
                case CatheterState.AllowCannotWalk:
                    return "BFARAllowCanWalk".Translate();
                case CatheterState.AllowIll:
                    return "BFARAllowIll".Translate();
                case CatheterState.AllowUnconscious:
                    return "BFARAllowUnconcious".Translate();
                case CatheterState.AllowAll:
                    return "BFARAllowAll".Translate();
                default:
                    return "ALL HAIL THE DARK LORD! BRINGER OF THE END TIMES!";
            };
        }

        private void CycleUseMode()
        {
            useMode = (CatheterState)(((int)useMode + 1) % Enum.GetValues(typeof(CatheterState)).Length);
        }

        
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            yield return new Command_Action
            {
                defaultLabel = "BFARCatheterActionLabel".Translate() + GetModeLabel(useMode),
                defaultDesc = "BFARCatheterCycleDesc".Translate(),
                action = () => CycleUseMode()
            };
        }
        
        // Set pawn eligibility

        private bool returnEligibility(Pawn pawn) 
        {
            switch (useMode)
            {
                case CatheterState.ManualOnly:
                    return false;
                case CatheterState.AllowCannotWalk:
                    if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving)) 
                        return true;
                    break;
                case CatheterState.AllowIll:
                    if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
                        return true;
                    break;
                case CatheterState.AllowUnconscious:
                    if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness) || !pawn.Awake())
                        return true;
                    break;
                case CatheterState.AllowAll:
                    return true;
            }
            return false;
        }
        
        // Additional functions and features
        
        public override string GetInspectString() // this makes stats on the item visible in game. Don't forget to add changes here to the translation files.
        {
            if (base.ParentHolder is MinifiedThing)
            {
                return base.GetInspectString();
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            if (blockage != null && blockage.blocked)
            {
                stringBuilder.AppendLine(Translator.Translate("BlockedDrain"));
            }
            if (pipe.pipeNet.Sewers.Any(h => h.parent != this))
            {
                stringBuilder.AppendLine(Translator.Translate("MustBePlumbing"));
            }
            return GenText.TrimEndNewlines(stringBuilder.ToString());
        }

        public void PostDraw(Vector3 drawPos, bool flip = false)
        {
            base.DrawAt(drawPos, flip);
            if (blockage.blocked)
            {
                HelperMethods.DrawOverlay(this, BFAR_OverlayTypes.Blocked);
            }
            if (pipe.pipeNet.Sewers.Any(h => h.parent != this))
            {
                HelperMethods.DrawOverlay(this, BFAR_OverlayTypes.NoPipe);
            }
        }
    }
}