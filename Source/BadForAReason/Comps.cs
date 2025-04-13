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
    // comps
    public class CompBFARBlockage : CompBlockage
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "block",
                    defaultDesc = "",
                    action = bog
                };
            }
        }
    
    }

    public class CompProperties_BFARBlockage : CompProperties_Blockage
    {
        public CompProperties_BFARBlockage()
        {
            compClass = typeof(CompBFARBlockage);
        }
    }
    
    public class CompBFARSewageHandler : CompSewageHandler // I'm in your comp, stealin' your fill button
    {
        
        
        public void fill()
        {
            if (this.parent is ISewageContainer container)
            {
                container.Sewage = container.SewageLimit;
            }

        }
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "BFAR fill with copius amounts of shit", action = () => this.fill()
                };
            }
        }

    }
    
    public class CompProperties_BFARSewageHandler : CompProperties_SewageHandler // If you really think about it, sewage handler is a bad name
    {
        public CompProperties_BFARSewageHandler()
        {
            this.compClass = typeof(CompBFARSewageHandler);
        }
    }
}