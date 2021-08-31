using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static Character_Making_File_Tool.Vector3Int;
using System.Windows;
using System.Drawing;

namespace Character_Making_File_Tool
{
    public class ColorConversion
    {
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
            double horizontalSlider = (double)vec3.X / CharacterHandler.MaxSliderClassicHue * 6;
            double verticalSlider = (double)vec3.Y / CharacterHandler.MaxSliderClassicLightness * 5;
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
            double tSat = (double)vec3.Z / CharacterHandler.MaxSliderClassicSaturation;

            return LerpRGBA(baseColor, new byte[] { average, average, average, 255 }, tSat);
        }

        //Idea for HSL color space interpretation of PSO2 COLR values. Only reasonable for cast colors, as it turns out
        public static byte[] RGBAFromCOLRHSL(Vec3Int vec3)
        {
            int trueHue = (int)((double)vec3.X / CharacterHandler.MaxSliderClassicHue * 240);
            int trueLightness = (int)((CharacterHandler.MaxSliderClassicLightness - (double)vec3.Y) / CharacterHandler.MaxSliderClassicLightness * 240);
            int trueSaturation = 0;
            if(vec3.Y > CharacterHandler.LightnessThreshold)
            {
                trueSaturation = (int)(((double)vec3.Z) / CharacterHandler.MaxSliderClassicSaturation * 240);
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
            vec3.X = (int)(color.GetHue() / 360 * CharacterHandler.MaxSliderClassicHue);
            vec3.Y = CharacterHandler.MaxSliderClassicLightness - (int)(color.GetBrightness() * (CharacterHandler.MaxSliderClassicLightness - CharacterHandler.LightnessThreshold));
            vec3.Z = (int)(color.GetSaturation() * CharacterHandler.MaxSliderClassicSaturation);
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
