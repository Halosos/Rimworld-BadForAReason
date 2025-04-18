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
    [DefOf]
    public class BFARDef
    {
        public static ThingDef BFARBedCommode;
        
        public static JobDef BFAREmptyBedCommode;

        public static JobDef BFARInstallCatheter;
        
        public static HediffDef BFARInstalledCatheter;
        
        public static ThoughtDef BFARHadCatheter;
        
        public static ThoughtDef BFARHaveCatheter;
        
        static BFARDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BFARDef));
        }
    }
    
}