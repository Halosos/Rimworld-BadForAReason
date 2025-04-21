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
    // Code that makes other code work. 


    public enum BFAR_OverlayTypes
    {
        Blocked,
        NoPipe,
        Default
    }
    
    
    public interface ISewageContainer // Home of sewage variables
    {
        float Sewage { get; set; }
        float SewageLimit { get; }
    }
    
    [DefOf]
    public static class ExternalDefOf // Kidnapping dubs pawn stats against their will
    {
        public static NeedDef Bladder;
        public static NeedDef Hygiene;
        static ExternalDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ExternalDefOf));
        }
    }
    
    public class PawnStatController // Lets you control pawns hygiene and bladder stats. 
    {
        public static bool AdjustBladder(float amount, Pawn target) // have a shower
        {
            Need bladder = target?.needs.TryGetNeed(ExternalDefOf.Bladder);
            

            
            
            if(bladder != null)
            {

                bladder.CurLevel += amount;

                return true;
            }

            return false;
        }

        public static bool AdjustHygiene(float amount, Pawn target) // shit yourself
        {
            Need hygiene = target?.needs.TryGetNeed(ExternalDefOf.Hygiene);
            

            
            if (hygiene != null)
            {

                hygiene.CurLevel += amount;

                return true;
            }
            
            return false;
        }

        

    }
    
    public class MapComponent_CatheterCache : MapComponent // tracks pawns who need shit stuck up their hole
    {
        public MapComponent_CatheterCache(Map map) : base(map)
        {
                
        }

        private List<Pawn> eligiblePawns = new List<Pawn>();
            
        public List<Pawn> EligiblePawns => eligiblePawns;

        public void Clear() => eligiblePawns.Clear();

        public void Add(Pawn pawn)
        {
            if (pawn != null && !eligiblePawns.Contains(pawn))
            {
                Log.Message("Adding to eligible list: " + pawn);
                eligiblePawns.Add(pawn);
            }
        }

        public void Remove(Pawn pawn)
        {
            eligiblePawns.Remove(pawn);
        }
            
        public bool Contains(Pawn pawn) => eligiblePawns.Contains(pawn);
    }

}