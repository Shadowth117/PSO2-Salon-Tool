using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Character_Making_File_Tool.Vector3Int;
using static Character_Making_File_Tool.CharacterConstants;

namespace Character_Making_File_Tool
{
    public static class CharacterHandlerUtilityMethods
    {
        public static void ToNGSSliders(this Vec3Int vec3)
        {
            vec3.X = (int)((double)vec3.X / MaxSliderClassic * MaxSliderNGS);
            vec3.Y = (int)((double)vec3.Y / MaxSliderClassic * MaxSliderNGS);
            vec3.Z = (int)((double)vec3.Z / MaxSliderClassic * MaxSliderNGS);
        }

        public static void ToOldSliders(this Vec3Int vec3)
        {
            vec3.X = (int)((double)vec3.X / MaxSliderNGS * MaxSliderClassic);
            vec3.Y = (int)((double)vec3.Y / MaxSliderNGS * MaxSliderClassic);
            vec3.Z = (int)((double)vec3.Z / MaxSliderNGS * MaxSliderClassic);
        }

        public static void Vec3IntSwap(ref Vec3Int vec3A, ref Vec3Int vec3B)
        {
            var temp = Vec3Int.CreateVec3Int(vec3A);
            vec3A.SetVec3(vec3B);
            vec3B.SetVec3(temp);
        }
    }
}
