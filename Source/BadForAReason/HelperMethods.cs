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
    public class Helper_Methods
    {

        
        

        public static bool JobEligibility(Thing target, out string outMessage, bool forced = false)
        {
            
            outMessage = "";
            
            if (!(Find.Selector.SingleSelectedThing is Pawn selPawn))
            {
                return false;
            }

            if (target == null)
            {

                return false;
            }

            if (target.IsForbidden(selPawn))
            {
                outMessage = "CannotPrioritizeForbidden";
                
                return false;
            }

            if (!selPawn.CanReserve(target, 1, -1, null, forced))
            {
                outMessage = "CannotUseReserved";
                
                return false;
            }

            if (!selPawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
            {
                outMessage = "CannotReach";

                return false;
            }
            
            
            outMessage = "";
            return true;

        }
        
    }
}