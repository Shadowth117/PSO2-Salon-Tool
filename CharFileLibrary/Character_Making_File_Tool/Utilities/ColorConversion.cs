using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static Character_Making_File_Tool.Vector3Int;
using static Character_Making_File_Tool.CharacterConstants;
using static Character_Making_File_Tool.CharacterDataStructs;
using static Character_Making_File_Tool.CharacterDataStructsReboot;
using System.Windows;
using System.Drawing;

namespace Character_Making_File_Tool
{
    public class ColorConversion
    {
        public static BaseCOLR COL2ToCOLR(COL2 colr, int race)
        {

        }

        public static COL2 COLRToCOL2(BaseCOLR colr, int race)
        {
            COL2 col = new COL2();
            col.outerColor1 = BitConverter.ToInt32(GetPSO2CharColor(colr.outer_MainColorVerts, castPalette));
            col.baseColor1 = BitConverter.ToInt32(GetPSO2CharColor(colr.costumeColorVerts, castPalette));
            col.mainColor = BitConverter.ToInt32(GetPSO2CharColor(colr.mainColor_hair2Verts, castPalette));
            col.subColor1 = BitConverter.ToInt32(GetPSO2CharColor(colr.subColor1Verts, castPalette));

            col.subColor2 = BitConverter.ToInt32(GetPSO2CharColor(colr.skinSubColor2Verts, castPalette));
            col.subColor3 = BitConverter.ToInt32(GetPSO2CharColor(colr.subColor3_leftEye_castHair2Verts, castPalette));
            col.rightEyeColor = BitConverter.ToInt32(GetPSO2CharColor(colr.rightEye_EyesVerts, eyePalette));
            col.hairColor1 = BitConverter.ToInt32(GetPSO2CharColor(colr.hairVerts, hairPalette));

            col.eyebrowColor = BitConverter.ToInt32(new byte[] { 0, 0, 0, 0xFF });
            col.eyelashColor = BitConverter.ToInt32(new byte[] { 0, 0, 0, 0xFF });
            
            //Handle skin conditionally
            switch(race)
            {
                case 0:
                case 1:
                    col.skinColor1 = BitConverter.ToInt32(GetPSO2CharColor(colr.skinSubColor2Verts, skinPalette));
                    break;
                case 2:
                    col.skinColor1 = BitConverter.ToInt32(GetPSO2CharColor(colr.skinSubColor2Verts, castPalette));
                    break;
                case 3:
                    col.skinColor1 = BitConverter.ToInt32(GetPSO2CharColor(colr.skinSubColor2Verts, deumanSkinPalette));
                    break;
            }
            col.skinColor2 = BitConverter.ToInt32(new byte[] { 0xFF, 0, 0, 0xFF });

            col.baseColor2 = BitConverter.ToInt32(new byte[] { 0, 0, 0, 0xFF });
            col.outerColor2 = BitConverter.ToInt32(new byte[] { 0, 0, 0, 0xFF });
            col.innerColor1 = BitConverter.ToInt32(new byte[] { 0, 0, 0, 0xFF });
            col.innerColor2 = BitConverter.ToInt32(new byte[] { 0, 0, 0, 0xFF });

            //If deuman, use eye 2 color
            if (race == 3)
            {
                col.leftEyeColor = BitConverter.ToInt32(GetPSO2CharColor(colr.subColor3_leftEye_castHair2Verts, eyePalette));
            } else
            {
                col.leftEyeColor = BitConverter.ToInt32(GetPSO2CharColor(colr.rightEye_EyesVerts, eyePalette));
            }

            //If cast, get hair 2 differently
            if(race == 2)
            {
                col.hairColor2 = BitConverter.ToInt32(GetPSO2CharColor(colr.subColor3_leftEye_castHair2Verts, castPalette));
            } else
            {
                col.hairColor2 = BitConverter.ToInt32(GetPSO2CharColor(colr.mainColor_hair2Verts, hairPalette));
            }

            return col;
        }

        //Linear interpolation between two colors
        public static byte[] LerpRGBA(byte[] a, byte[] b, double t, bool roundDown = false)
        {
            var rounding = MidpointRounding.ToEven;
            if(roundDown)
            {
                rounding = MidpointRounding.ToZero;
            }

            return new byte[]
            {
                (byte)Math.Round(a[0] + (b[0] - a[0]) * t, rounding),
                (byte)Math.Round(a[1] + (b[1] - a[1]) * t, rounding),
                (byte)Math.Round(a[2] + (b[2] - a[2]) * t, rounding),
                (byte)Math.Round(a[3] + (b[3] - a[3]) * t, rounding)
            };
        }

        public static byte[] GetPSO2CharColor(Vec3Int vec3, byte[,][] palette)
        {
            double horizontalSlider = (double)vec3.X / MaxSliderClassicHue * 6;
            double verticalSlider = (double)vec3.Y / MaxSliderClassicLightness * 5;
            int leftColumn = (int)Math.Floor(horizontalSlider);
            int rightColumn = (int)Math.Ceiling(horizontalSlider);
            int topRow = (int)Math.Floor(verticalSlider);
            int bottomRow = (int)Math.Ceiling(verticalSlider);

            //Get step between points as a remainder using mod of floored values
            double tHoriz = 0;
            if(horizontalSlider >= 1)
            {
                tHoriz = horizontalSlider % leftColumn;
            } else
            {
                tHoriz = horizontalSlider;
            }

            double tVert = 0;
            if (verticalSlider >= 1)
            {
                tVert = verticalSlider % topRow;
            }
            else
            {
                tVert = verticalSlider;
            }

            //Get base color from the interpolation of the 4 colors we get from these values
            byte[] baseColor =  LerpRGBA(
                                   LerpRGBA(palette[leftColumn, topRow], palette[rightColumn, topRow], tHoriz),
                                   LerpRGBA(palette[leftColumn, bottomRow], palette[rightColumn, bottomRow], tHoriz),
                                   tVert,
                                   true
                                   );

            //The grayscale of a pso2 color is the average of r, g, and b applied to r, g, and b on a color.
            //Saturation is handled by interpolating between the color and this grayscale color
            byte average = (byte)Math.Round(((double)baseColor[0] + baseColor[1] + baseColor[2]) / 3);
            double tSat = (double)vec3.Z / MaxSliderClassicSaturation;

            return LerpRGBA(baseColor, new byte[] { average, average, average, 255 }, tSat);
        }

        //Idea for HSL color space interpretation of PSO2 COLR values. Only reasonable for cast colors due to variations between other palettes.
        public static byte[] RGBAFromCOLRHSL(Vec3Int vec3)
        {
            int trueHue = (int)((double)vec3.X / MaxSliderClassicHue * 240);
            int trueLightness = (int)((MaxSliderClassicLightness - (double)vec3.Y) / MaxSliderClassicLightness * 240);
            int trueSaturation = 0;
            if(vec3.Y > LightnessThreshold)
            {
                trueSaturation = (int)(((double)vec3.Z) / MaxSliderClassicSaturation * 240);
            }
            var newColor = (System.Drawing.Color)new HSLColor(trueHue, trueLightness, trueSaturation);
            

            return new byte[] { newColor.R, newColor.G, newColor.B, newColor.A };
        }

        public static byte[] RGBAFromCOLR(Vec3Int vec3)
        {
            return GetPSO2CharColor(vec3, castPalette);
        }

        public static byte[] RGBAFromCOLREyes(Vec3Int vec3)
        {
            return GetPSO2CharColor(vec3, eyePalette);
        }

        public static byte[] RGBAFromCOLRHair(Vec3Int vec3)
        {
            return GetPSO2CharColor(vec3, hairPalette);
        }

        public static byte[] RGBAFromCOLRSkin(Vec3Int vec3)
        {
            return GetPSO2CharColor(vec3, skinPalette);
        }

        public static byte[] RGBAFromCOLRDeumanSkin(Vec3Int vec3)
        {
            return GetPSO2CharColor(vec3, deumanSkinPalette);
        }

        //PSO2 COLR values are stored as ingame slider values like proportion sliders
        //In order to convert this to something usable, we need to throw it through Hue Saturation Luminance
        public static Vec3Int COLRFromRGBA(byte[] colorRGBA)
        {
            Vec3Int vec3 = new Vec3Int();
            System.Drawing.Color color = System.Drawing.Color.FromArgb(colorRGBA[3], colorRGBA[0], colorRGBA[1], colorRGBA[2]);
            vec3.X = (int)(color.GetHue() / 360 * MaxSliderClassicHue);
            vec3.Y = MaxSliderClassicLightness - (int)(color.GetBrightness() * (MaxSliderClassicLightness - LightnessThreshold));
            vec3.Z = (int)(color.GetSaturation() * MaxSliderClassicSaturation);
#if DEBUG
            HSLColor hsl = new HSLColor(colorRGBA[0], colorRGBA[1], colorRGBA[2]);
            MessageBox.Show($"{hsl.Hue / 240.0 * 100} {hsl.Saturation / 240.0 * 100} {hsl.Luminosity / 240.0 * 100} {hsl.ToRGBString()}");
            MessageBox.Show($"{color.GetHue() / 360 * 100} {color.GetSaturation() * 100} {color.GetBrightness() * 100}");
#endif
            return vec3;
        }

        //The old PSO2 skin colors are HSL colors with a much more limited range. Essentially, they have a split halfway up the "lightness" slider into 2 hues.
        //The lightness has upper and lower limits within its regions and saturation as well has upper and lower limits.
        public static Vec3Int COLRSkinFromRGBA()
        {
            return new Vec3Int();
        }

        public static Vec3Int COLRDeumanSkinFromRGBA()
        {
            return new Vec3Int();
        }

    }

    //HSLColor sourced from https://richnewman.wordpress.com/about/code-listings-and-diagrams/hslcolor-class/
    public class HSLColor
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double hue = 1.0;
        private double saturation = 1.0;
        private double luminosity = 1.0;

        private const double scale = 240.0;

        public double Hue
        {
            get { return hue * scale; }
            set { hue = CheckRange(value / scale); }
        }
        public double Saturation
        {
            get { return saturation * scale; }
            set { saturation = CheckRange(value / scale); }
        }
        public double Luminosity
        {
            get { return luminosity * scale; }
            set { luminosity = CheckRange(value / scale); }
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        public string ToRGBString()
        {
            Color color = (Color)this;
            return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
        }

        #region Casts to/from System.Drawing.Color
        public static implicit operator Color(HSLColor hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor.luminosity != 0)
            {
                if (hslColor.saturation == 0)
                    r = g = b = hslColor.luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor.luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor.hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor.hue);
                    b = GetColorComponent(temp1, temp2, hslColor.hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb(
            Convert.ToInt32(255 * r),
            Convert.ToInt32(255 * g),
            Convert.ToInt32(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }
        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;
            if (hslColor.luminosity < 0.5)  //<=??
                temp2 = hslColor.luminosity * (1.0 + hslColor.saturation);
            else
                temp2 = hslColor.luminosity + hslColor.saturation - (hslColor.luminosity * hslColor.saturation);
            return temp2;
        }

        public static implicit operator HSLColor(Color color)
        {
            HSLColor hslColor = new HSLColor();
            hslColor.hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor.luminosity = color.GetBrightness();
            hslColor.saturation = color.GetSaturation();
            return hslColor;
        }
        #endregion

        public void SetRGB(int red, int green, int blue)
        {
            HSLColor hslColor = (HSLColor)Color.FromArgb(red, green, blue);
            this.hue = hslColor.hue;
            this.saturation = hslColor.saturation;
            this.luminosity = hslColor.luminosity;
        }

        public HSLColor() { }
        public HSLColor(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }
        public HSLColor(int red, int green, int blue)
        {
            SetRGB(red, green, blue);
        }
        public HSLColor(double hue, double saturation, double luminosity)
        {
            this.Hue = hue;
            this.Saturation = saturation;
            this.Luminosity = luminosity;
        }

    }
}
