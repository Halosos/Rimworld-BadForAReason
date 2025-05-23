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
    public static class HelperMethods
    {
        
        public static void DrawOverlay(Thing thing, BFAR_OverlayTypes type)
        {
           if (thing == null || !thing.Spawned) return;
           
           Material mat = GetOverlayType(type);
           
           if (mat == null) return;
           
           float size = 1f;
           Vector3 drawPos = thing.DrawPos + Altitudes.AltIncVect;
           Vector3 scale = new Vector3(size, 1f, size);
           Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.identity, scale);
           
           Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
        }

        private static Material GetOverlayType(BFAR_OverlayTypes type) // pipe warnings
        {
            switch (type)
            {
                case BFAR_OverlayTypes.Blocked:
                    return GraphicsCache.plumbingBreakdownMat;
                case BFAR_OverlayTypes.NoPipe:
                    return GraphicsCache.MissingPipes;
                default:
                    if (DebugSettings.godMode)
                    {
                        return ContentFinder<Material>.Get("UI/Misc/BadTexture");
                    }
                    else
                    {
                        return ContentFinder<Material>.Get("Things/Mote/Transparent");
                    }
                    
            }
        }

        public static bool IsBedWithCatheter(Building_Bed bed, Map map)
        {
            foreach (var machine in map.listerBuildings.AllBuildingsColonistOfClass<Building_CatheterMachine>())
            {
                var fac = machine.TryGetComp<CompFacility>();
                if (fac != null && fac.LinkedBuildings.Contains(bed))
                {
                    return true;
                }
            }
            return false;
        }
        
        
        
    }
}