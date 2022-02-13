using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Character_Making_File_Tool
{
    public class CharacterConstants
    {
        public static int MinSliderNGS = -127;
        public static int MaxSliderNGS = 127;
        public static int MinHeightLegSliderNGS = -71;
        public static int MinHeightBodySliderNGS = -71;
        public static int MinSliderClassic = -10000;
        public static int MaxSliderClassic = 10000;
        public static int MaxSliderClassicHue = 60000; //Range 0-60000
        public static int MaxSliderClassicLightness = 50000; //Range 0-50000 //Note, 0 is up, 50000 is down.
        public static int MaxSliderClassicSaturation = 10000; //Range 0-10000
        public static int LightnessThreshold = 10000; //The threshold after which saturation is 0ed. Classic PSO2 had a black to white area above the colored range's white
        public static int MinHeightBodySlider = -5501;
        public static int MinHeightLegSlider = -5600;
        public static int DeicerCMLType = 0x6C6D63;
        public static byte[] ConstantCMLHeader = { 0x56, 0x54, 0x42, 0x46, 0x10, 0, 0, 0, 0x43, 0x4D, 0x4C, 0x50, 0x1, 0, 0, 0x4C };
        public static byte[] Vtc0 = { 0x76, 0x74, 0x63, 0x30 };
        public static byte[] DocText = { 0x44, 0x4F, 0x43, 0x20 };
        public static byte[] FigrText = { 0x46, 0x49, 0x47, 0x52 };
        public static byte[] OfstText = { 0x4F, 0x46, 0x53, 0x54 };
        public static byte[] ColrText = { 0x43, 0x4F, 0x4C, 0x52 };
        public static byte[] SlctText = { 0x53, 0x4C, 0x43, 0x54 };

        public static Dictionary<int, int> classicSalonToolFileSizes = new Dictionary<int, int>()
        {
            //Numbers correlate to save dialog selection numbers
            { 3, 0x15C}, //v2
            { 2, 0x170}, //v5
            { 1, 0x2DC}, //v6
            { 0, 0x2F0}  //v9
        };

        //Stores palette colors [column, row]
        //Each palette has 7 columns and 6 rows. Overall they mirror the slider backdrop, but with some differences due to it not matching what the game uses
        //All palette data was recreated frrom the .cmx file(s) except for skin. Skin is handled a bit differently than other colors, and so I have recreated it a different way.

        public static byte[,][] skinPalette = new byte[,][]
        {
            {
                new byte[] { 94, 69, 53, 255},
                new byte[] { 201, 160, 99, 255},
                new byte[] { 244, 210, 165, 255},
                new byte[] { 247, 201, 176, 255},
                new byte[] { 208, 143, 117, 255},
                new byte[] { 94, 69, 53, 255}
            },
            {
                new byte[] { 90, 69, 53, 255},
                new byte[] { 195, 160, 96, 255},
                new byte[] { 244, 210, 165, 255},
                new byte[] { 247, 201, 176, 255},
                new byte[] { 207, 144, 117, 255},
                new byte[] { 94, 69, 53, 255}
            },
            {
                new byte[] { 86, 69, 53, 255},
                new byte[] { 192, 157, 102, 255},
                new byte[] { 239, 210, 170, 255},
                new byte[] { 242, 202, 179, 255},
                new byte[] { 202, 145, 123, 255},
                new byte[] { 87, 68, 54, 255}
            },
            {
                new byte[] { 86, 69, 61, 255},
                new byte[] { 185, 156, 111, 255},
                new byte[] { 234, 208, 176, 255},
                new byte[] { 238, 203, 183, 255},
                new byte[] { 196, 148, 130, 255},
                new byte[] { 86, 69, 57, 255}
            },
            {
                new byte[] { 82, 69, 61, 255},
                new byte[] { 176, 156, 118, 255},
                new byte[] { 226, 207, 181, 255},
                new byte[] { 231, 204, 189, 255},
                new byte[] { 190, 150, 134, 255},
                new byte[] { 79, 69, 61, 255}
            },
            {
                new byte[] { 78, 70, 62, 255},
                new byte[] { 172, 153, 123, 255},
                new byte[] { 223, 208, 189, 255},
                new byte[] { 228, 206, 195, 255},
                new byte[] { 184, 153, 140, 255},
                new byte[] { 79, 69, 61, 255}
            },
            {
                new byte[] { 78, 70, 62, 255},
                new byte[] { 170, 152, 123, 255},
                new byte[] { 223, 208, 189, 255},
                new byte[] { 231, 204, 189, 255},
                new byte[] { 183, 153, 140, 255},
                new byte[] { 78, 69, 61, 255}
            }
        };

        public static byte[,][] deumanSkinPalette = new byte[,][]
        {
            {
                new byte[] { 49, 49, 76, 255},
                new byte[] { 95, 106, 127, 255},
                new byte[] { 214, 215, 216, 255},
                new byte[] { 216, 188, 184, 255},
                new byte[] { 173, 126, 123, 255},
                new byte[] { 86, 45, 43, 255}
            },
            {
                new byte[] { 53, 53, 79, 255},
                new byte[] { 101, 110, 130, 255},
                new byte[] { 217, 218, 219, 255},
                new byte[] { 216, 192, 188, 255},
                new byte[] { 173, 132, 130, 255},
                new byte[] { 86, 49, 47, 255}
            },
            {
                new byte[] { 56, 56, 79, 255},
                new byte[] { 106, 114, 130, 255},
                new byte[] { 217, 218, 219, 255},
                new byte[] { 216, 194, 190, 255},
                new byte[] { 173, 137, 135, 255},
                new byte[] { 86, 54, 52, 255}
            },
            {
                new byte[] { 61, 61, 81, 255},
                new byte[] { 112, 119, 132, 255},
                new byte[] { 219, 220, 221, 255},
                new byte[] { 216, 197, 195, 255},
                new byte[] { 173, 144, 142, 255},
                new byte[] { 86, 58, 57, 255}
            },
            {
                new byte[] { 63, 63, 81, 255},
                new byte[] { 118, 124, 135, 255},
                new byte[] { 222, 223, 224, 255},
                new byte[] { 216, 201, 199, 255},
                new byte[] { 173, 150, 149, 255},
                new byte[] { 86, 63, 62, 255}
            },
            {
                new byte[] { 68, 68, 84, 255},
                new byte[] { 125, 129, 137, 255},
                new byte[] { 224, 225, 226, 255},
                new byte[] { 216, 205, 203, 255},
                new byte[] { 173, 157, 156, 255},
                new byte[] { 86, 67, 66, 255}
            },
            {
                new byte[] { 76, 76, 86, 255},
                new byte[] { 137, 138, 140, 255},
                new byte[] { 227, 228, 229, 255},
                new byte[] { 216, 211, 210, 255},
                new byte[] { 173, 168, 168, 255},
                new byte[] { 86, 76, 76, 255}
            }
        };

        public static byte[,][] hairPalette = new byte[,][]
        {
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 178, 121, 121, 255},
                new byte[] { 178, 19, 19, 255},
                new byte[] { 127, 14, 14, 255},
                new byte[] { 25, 2, 2, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 178, 168, 121, 255},
                new byte[] { 204, 177, 61, 255},
                new byte[] { 127, 111, 38, 255},
                new byte[] { 25, 22, 7, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 122, 178, 121, 255},
                new byte[] { 63, 204, 61, 255},
                new byte[] { 39, 127, 38, 255},
                new byte[] { 7, 25, 7, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 121, 178, 178, 255},
                new byte[] { 19, 178, 178, 255},
                new byte[] { 14, 127, 127, 255},
                new byte[] { 2, 25, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 119, 120, 175, 255},
                new byte[] { 19, 22, 178, 255},
                new byte[] { 19, 22, 178, 255},
                new byte[] { 2, 3, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 178, 121, 178, 255},
                new byte[] { 175, 19, 178, 255},
                new byte[] { 127, 14, 127, 255},
                new byte[] { 25, 2, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 178, 121, 121, 255},
                new byte[] { 178, 19, 19, 255},
                new byte[] { 127, 14, 14, 255},
                new byte[] { 25, 10, 10, 255}
            },
        };

        public static byte[,][] eyePalette = new byte[,][]
        {
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 178, 121, 121, 255},
                new byte[] { 255, 0, 0, 255},
                new byte[] { 127, 0, 0, 255},
                new byte[] { 25, 0, 0, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 178, 168, 121, 255},
                new byte[] { 255, 208, 0, 255},
                new byte[] { 127, 104, 0, 255},
                new byte[] { 25, 20, 0, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 122, 178, 121, 255},
                new byte[] { 4, 255, 0, 255},
                new byte[] { 2, 127, 0, 255},
                new byte[] { 0, 25, 0, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 121, 178, 178, 255},
                new byte[] { 0, 255, 255, 255},
                new byte[] { 0, 127, 127, 255},
                new byte[] { 0, 25, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 119, 120, 175, 255},
                new byte[] { 0, 4, 255, 255},
                new byte[] { 0, 2, 127, 255},
                new byte[] { 0, 0, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 178, 121, 178, 255},
                new byte[] { 250, 0, 255, 255},
                new byte[] { 127, 0, 127, 255},
                new byte[] { 25, 0, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 178, 121, 121, 255},
                new byte[] { 255, 0, 0, 255},
                new byte[] { 127, 0, 0, 255},
                new byte[] { 25, 0, 0, 255}
            },
        };

        public static byte[,][] castPalette = new byte[,][]
        {            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 255, 173, 173, 255},
                new byte[] { 255, 0, 0, 255},
                new byte[] { 127, 0, 0, 255},
                new byte[] { 25, 0, 0, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 255, 240, 173, 255},
                new byte[] { 255, 208, 0, 255},
                new byte[] { 127, 104, 0, 255},
                new byte[] { 25, 20, 0, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 174, 255, 173, 255},
                new byte[] { 4, 255, 0, 255},
                new byte[] { 2, 127, 0, 255},
                new byte[] { 0, 25, 0, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 173, 255, 255, 255},
                new byte[] { 0, 255, 255, 255},
                new byte[] { 0, 127, 127, 255},
                new byte[] { 0, 25, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 173, 174, 255, 255},
                new byte[] { 0, 4, 255, 255},
                new byte[] { 0, 2, 127, 255},
                new byte[] { 0, 0, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 255, 173, 255, 255},
                new byte[] { 250, 0, 255, 255},
                new byte[] { 127, 0, 127, 255},
                new byte[] { 25, 0, 25, 255}
            },
            {
                new byte[] { 25, 25, 25, 255},
                new byte[] { 242, 248, 255, 255},
                new byte[] { 255, 173, 173, 255},
                new byte[] { 255, 0, 0, 255},
                new byte[] { 127, 0, 0, 255},
                new byte[] { 25, 0, 0, 255}
            },
        };
    }
}
