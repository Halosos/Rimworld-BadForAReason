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
    public class Building_ComaBed : Building_Bed
    {
        CompPipe pipe;
        CompSewageHandler sewageHandler;
        private CompBlockage blockage;

        public float sewage = 0f;
        public float sewageLimit = 100f;
        
        public float Sewage
        {
            get => sewage;
            set => sewage = value;
        }

        public virtual bool DrawBrokenPipe => true;
        public bool brokenPipe;
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.pipe = GetComp<CompPipe>();
            this.sewageHandler = GetComp<CompSewageHandler>();
            this.blockage = GetComp<CompBlockage>();
            if (pipe == null)
            {
                throw new System.MissingFieldException(nameof (CompPipe));
            }
            if (sewageHandler == null)
            {
                throw new System.MissingFieldException(nameof (CompSewageHandler));
            }

            if (blockage == null)
            {
                throw new System.MissingFieldException(nameof (CompBlockage));
            }
        }
        
        public override void TickRare()
        {
            
            
            if (GetCurOccupant(0) != null) 
            {
                Pawn pawn = GetCurOccupant(0);
                Need_Bladder needBladder = pawn.needs.TryGetNeed<Need_Bladder>();

                
                if (this.sewageHandler.Blocked == false && pipe?.pipeNet?.Sewers?.Any(h => h.parent != this) == true)
                {

                    if (needBladder.CurLevel < 0.75f)
                    {

                        PawnStatController.AdjustBladder(0.25f, pawn);
                        
                        sewage += (14f * ModOption.FlushSize.Val);
                        
                        pipe.pipeNet.PushSewage(sewage);
                        
                        sewage = 0f;
                        
                    }
                }
                
                
                
                if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("BFARHaveCatheter")))  // make this cleaner later
                {
                    pawn.health.AddHediff(HediffDef.Named("BFARHaveCatheter"));
                } 
            }
        }
        
        
        
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