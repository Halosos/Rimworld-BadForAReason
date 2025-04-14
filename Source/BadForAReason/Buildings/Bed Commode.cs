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

    public class Building_BedCommode : Building_Bed, ISewageContainer
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
        
        public float SewageLimit => sewageLimit;

        private float _amountToDump;

        
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            pipe = GetComp<CompPipe>();
            sewageHandler = GetComp<CompSewageHandler>();
            blockage = GetComp<CompBlockage>();
            if (pipe == null)
            {
                throw new MissingFieldException(nameof (CompPipe));
            }
            if (sewageHandler == null)
            {
                throw new MissingFieldException(nameof (CompSewageHandler));
            }

            if (blockage == null)
            {
                throw new MissingFieldException(nameof (CompBlockage));
            }
        }
        
        public override void TickRare()
        {
            
            
            if (GetCurOccupant(0) != null) 
            {
                Pawn pawn = GetCurOccupant(0);
                Need_Bladder needBladder = pawn.needs.TryGetNeed<Need_Bladder>();

                
                if (sewageHandler.Blocked == false)
                {

                    if (needBladder.CurLevel < 0.75f)
                    {

                        PawnStatController.AdjustBladder(0.25f, pawn);
                        PawnStatController.AdjustHygiene(-0.01f, pawn);
                        
                        sewage += (14f * ModOption.FlushSize.Val);
                        
                        
                        if (pipe?.pipeNet?.Sewers?.Any(h => h.parent != this) == true) // is there a place to dump sewage?
                        {

                            pipe.pipeNet.PushSewage(sewage);
                            
                            sewage = 0f;
                        }
                        else
                        {

                            
                            if (sewage >= sewageLimit) // Too much shit
                            {
                                _amountToDump = sewage - sewageLimit;

                                pipe.MapComp.SewageGrid.AddAt(GetFootSlotPos(0), _amountToDump, true, true, null);

                                sewage = sewageLimit;

                            }
                        }
                    }
                }
                else
                {
                    if (needBladder.CurLevel < 0.75f)
                    {
                        PawnStatController.AdjustBladder(0.25f, pawn);
                        PawnStatController.AdjustHygiene(-0.05f, pawn);
                        
                        pawn.needs.mood.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed("SoiledSelf"));
                        
                        _amountToDump = 14f * ModOption.FlushSize.Val;

                        pipe.MapComp.SewageGrid.AddAt(GetFootSlotPos(0), _amountToDump, true, true, null);
                    }
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
            if (sewage >= sewageLimit)
            {
                stringBuilder.AppendLine(Translator.Translate("BedpanFull"));
            }
            else
            {
                stringBuilder.AppendLine(TranslatorFormattedStringExtensions.Translate("BedpanCapacity", GenText.ToStringPercent(this.sewage / this.sewageLimit, "0")));
            }
            return GenText.TrimEndNewlines(stringBuilder.ToString());
        }

        public void PostDraw(Vector3 drawPos, bool flip = false)
        {
            base.DrawAt(drawPos, flip);
            if (blockage.blocked || blockage == null)
            {
                HelperMethods.DrawOverlay(this, BFAR_OverlayTypes.Blocked);
            }
        }
        
    }
}