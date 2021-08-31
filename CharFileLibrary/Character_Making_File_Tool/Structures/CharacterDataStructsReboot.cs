using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Character_Making_File_Tool.NibbleUtility;

namespace Character_Making_File_Tool
{
    public unsafe class CharacterDataStructsReboot
    {
        public struct AccessorySliders
        {
            //Position, Scale, and Rotation arrays 0xC bytes each, in that order.
            public fixed sbyte sliders[0x24];
            public OldAccessorySliders GetOldAccessorySliders()
            {
                OldAccessorySliders oldSliders = new OldAccessorySliders();
                for (int i = 0; i < 3; i++)
                {
                    //Accessory 1 Y+Z
                    oldSliders.oldSliders[i * 6] = SignednibblePack(sliders[i * 12 + 1], sliders[i * 12 + 2]);
                    //Accessory 4 X + Accessory 1 X
                    oldSliders.oldSliders[i * 6 + 1] = SignednibblePack(sliders[i * 12 + 9], sliders[i * 12]);

                    //Accessory 2 Y+Z
                    oldSliders.oldSliders[i * 6 + 2] = SignednibblePack(sliders[i * 12 + 4], sliders[i * 12 + 5]);
                    //Accessory 4 Y + Accessory 2 X
                    oldSliders.oldSliders[i * 6 + 3] = SignednibblePack(sliders[i * 12 + 10], sliders[i * 12 + 3]);

                    //Accessory 3 Y+Z
                    oldSliders.oldSliders[i * 6 + 4] = SignednibblePack(sliders[i * 12 + 7], sliders[i * 12 + 8]);
                    //Accessory 4 Y + Accessory 3 X
                    oldSliders.oldSliders[i * 6 + 5] = SignednibblePack(sliders[i * 12 + 11], sliders[i * 12 + 6]);
                }

                return oldSliders;
            }
        }

        public struct OldAccessorySliders
        {
            public fixed byte oldSliders[0x12];

            public AccessorySliders GetAccessorySliders()
            {
                AccessorySliders accessorySliders = new AccessorySliders();

                for (int i = 0; i < 3; i++)
                {
                    //Accessory 1 Y+Z
                    SignednibbleUnpack(oldSliders[i * 6], out accessorySliders.sliders[i * 12 + 1], out accessorySliders.sliders[i * 12 + 2]);
                    //Accessory 4 X + Accessory 1 X
                    SignednibbleUnpack(oldSliders[i * 6 + 1], out accessorySliders.sliders[i * 12 + 9], out accessorySliders.sliders[i * 12]);
                    //Accessory 2 Y+Z
                    SignednibbleUnpack(oldSliders[i * 6 + 2], out accessorySliders.sliders[i * 12 + 4], out accessorySliders.sliders[i * 12 + 5]);
                    //Accessory 4 Y + Accessory 2 X
                    SignednibbleUnpack(oldSliders[i * 6 + 3], out accessorySliders.sliders[i * 12 + 10], out accessorySliders.sliders[i * 12 + 3]);
                    //Accessory 3 Y+Z
                    SignednibbleUnpack(oldSliders[i * 6 + 4], out accessorySliders.sliders[i * 12 + 7], out accessorySliders.sliders[i * 12 + 8]);
                    //Accessory 4 Y + Accessory 3 X
                    SignednibbleUnpack(oldSliders[i * 6 + 5], out accessorySliders.sliders[i * 12 + 11], out accessorySliders.sliders[i * 12 + 6]);
                }

                return accessorySliders;
            }
        }

        public struct OldAccessoryPositionSliders
        {
            public fixed byte oldSliders[0x6];
            public AccessorySliders GetAccessorySliders()
            {
                AccessorySliders accessorySliders = new AccessorySliders();

                //Accessory 1 Y+Z
                SignednibbleUnpack(oldSliders[0], out accessorySliders.sliders[1], out accessorySliders.sliders[2]);
                //Accessory 4 X + Accessory 1 X
                SignednibbleUnpack(oldSliders[1], out accessorySliders.sliders[9], out accessorySliders.sliders[0]);
                //Accessory 2 Y+Z
                SignednibbleUnpack(oldSliders[2], out accessorySliders.sliders[4], out accessorySliders.sliders[5]);
                //Accessory 4 Y + Accessory 2 X
                SignednibbleUnpack(oldSliders[3], out accessorySliders.sliders[10], out accessorySliders.sliders[3]);
                //Accessory 3 Y+Z
                SignednibbleUnpack(oldSliders[4], out accessorySliders.sliders[7], out accessorySliders.sliders[8]);
                //Accessory 4 Y + Accessory 3 X
                SignednibbleUnpack(oldSliders[5], out accessorySliders.sliders[11], out accessorySliders.sliders[6]);

                return accessorySliders;
            }
        }

        //Assumedly the ACWK stuctures in cml? In ACWK, they would be 3 ints, maybe.
        public struct AccessoryMisc
        {
            public fixed byte accessoryAttach[0xC];
            public fixed byte accessoryColorChoices[0x18]; //Each accessory has choice 1 and choice 2 next to each other
        }

        public struct BaseSLCTNGS
        {
            public uint earsPart;
            public uint teethPart;
            public uint hornPart;
            public uint acc5Part;

            public uint acc6Part;
            public uint acc7Part;
            public uint acc8Part;
            public uint acc9Part;

            public uint acc10Part;
            public uint acc11Part;
            public uint acc12Part;
        }
        public struct COL2
        {
            public int outerColor1;
            public int baseColor1; //Also used for costume colors?
            public int mainColor;
            public int subColor1;

            public int subColor2;
            public int subColor3;
            public int rightEyeColor;
            public int hairColor1;

            public int eyebrowColor;
            public int eyelashColor;
            public int skinColor1;
            public int skinColor2;

            public int baseColor2;
            public int outerColor2;
            public int innerColor1;
            public int innerColor2;

            public int leftEyeColor;
            public int hairColor2;
        }

        //1 dimensional extra sliders added by NGS
        public struct NGSSLID
        {
            public int int_2F0; //Unknown, possibly unused
            public int shoulderSize;
            public int hairAdjust;
            public int skinGloss;

            public int mouthVertical;
            public int eyebrowHoriz;
            public int irisVertical;
            public int facePaint1Opacity;

            public int facePaint2Opacity;
            public int shoulderVertical;
            public int thighsAdjust;
            public int calvesAdjust;

            public int forearmsAdjust;
            public int handThickness;
            public int footSize;
            public int int_32C;
        }

        public struct NGSMTON
        {
            public int int_330; //Unused motion change property
            public int walkRunMotion;
            public int swimMotion;
            public int dashMotion;

            public int glideMotion;
            public int landingMotion;
            public int idleMotion;
            public int jumpMotion;
        }
    }
}
