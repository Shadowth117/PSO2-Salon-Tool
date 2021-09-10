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
        public struct AccessorySlidersReboot
        {
            public fixed sbyte scaleSliders[0x24];
            public fixed sbyte rotSliders[0x24];
            public fixed sbyte posSliders[0x24];
            
            public AccessorySliders GetClassicAccessorySliders()
            {
                AccessorySliders acceSl = new();
                for(int i = 0; i < 0xC; i++)
                {
                    acceSl.sliders[i] = posSliders[i];
                    acceSl.sliders[i + 0xC] = scaleSliders[i];
                    acceSl.sliders[i + 0x18] = rotSliders[i];
                }

                return acceSl;
            }

            public OldAccessorySliders GetOldAccessorySliders()
            {
                OldAccessorySliders oldSliders = new();

                //Position
                //Accessory 1 Y+Z
                oldSliders.oldSliders[0] = SignednibblePack(posSliders[1], posSliders[2]);
                //Accessory 4 X + Accessory 1 X
                oldSliders.oldSliders[1] = SignednibblePack(posSliders[9], posSliders[0]);

                //Accessory 2 Y+Z
                oldSliders.oldSliders[2] = SignednibblePack(posSliders[4], posSliders[5]);
                //Accessory 4 Y + Accessory 2 X
                oldSliders.oldSliders[3] = SignednibblePack(posSliders[10], posSliders[3]);

                //Accessory 3 Y+Z
                oldSliders.oldSliders[4] = SignednibblePack(posSliders[7], posSliders[8]);
                //Accessory 4 Y + Accessory 3 X
                oldSliders.oldSliders[5] = SignednibblePack(posSliders[11], posSliders[6]);

                //Scale
                //Accessory 1 Y+Z
                oldSliders.oldSliders[6] = SignednibblePack(scaleSliders[1], scaleSliders[2]);
                //Accessory 4 X + Accessory 1 X
                oldSliders.oldSliders[7] = SignednibblePack(scaleSliders[9], scaleSliders[0]);

                //Accessory 2 Y+Z
                oldSliders.oldSliders[8] = SignednibblePack(scaleSliders[4], scaleSliders[5]);
                //Accessory 4 Y + Accessory 2 X
                oldSliders.oldSliders[9] = SignednibblePack(scaleSliders[10], scaleSliders[3]);

                //Accessory 3 Y+Z
                oldSliders.oldSliders[10] = SignednibblePack(scaleSliders[7], scaleSliders[8]);
                //Accessory 4 Y + Accessory 3 X
                oldSliders.oldSliders[11] = SignednibblePack(scaleSliders[11], scaleSliders[6]);

                //Rotation
                //Accessory 1 Y+Z
                oldSliders.oldSliders[12] = SignednibblePack(rotSliders[1], rotSliders[2]);
                //Accessory 4 X + Accessory 1 X
                oldSliders.oldSliders[13] = SignednibblePack(rotSliders[9], rotSliders[0]);

                //Accessory 2 Y+Z
                oldSliders.oldSliders[14] = SignednibblePack(rotSliders[4], rotSliders[5]);
                //Accessory 4 Y + Accessory 2 X
                oldSliders.oldSliders[15] = SignednibblePack(rotSliders[10], rotSliders[3]);

                //Accessory 3 Y+Z
                oldSliders.oldSliders[16] = SignednibblePack(rotSliders[7], rotSliders[8]);
                //Accessory 4 Y + Accessory 3 X
                oldSliders.oldSliders[17] = SignednibblePack(rotSliders[11], rotSliders[6]);

                return oldSliders;
            }

            public OldAccessoryPositionSliders GetOldAccessoryPositionSliders()
            {
                OldAccessoryPositionSliders oldSliders = new();

                //Accessory 1 Y+Z
                oldSliders.oldSliders[0] = SignednibblePack(posSliders[1], posSliders[2]);
                //Accessory 4 X + Accessory 1 X
                oldSliders.oldSliders[1] = SignednibblePack(posSliders[9], posSliders[0]);

                //Accessory 2 Y+Z
                oldSliders.oldSliders[2] = SignednibblePack(posSliders[4], posSliders[5]);
                //Accessory 4 Y + Accessory 2 X
                oldSliders.oldSliders[3] = SignednibblePack(posSliders[10], posSliders[3]);

                //Accessory 3 Y+Z
                oldSliders.oldSliders[4] = SignednibblePack(posSliders[7], posSliders[8]);
                //Accessory 4 Y + Accessory 3 X
                oldSliders.oldSliders[5] = SignednibblePack(posSliders[11], posSliders[6]);


                return oldSliders;
            }
        }

        public struct AccessorySliders
        {
            //Position, Scale, and Rotation arrays 0xC bytes each, in that order.
            public fixed sbyte sliders[0x24];

            public AccessorySlidersReboot GetAccessorySlidersReboot()
            {
                AccessorySlidersReboot acceSl = new AccessorySlidersReboot();
                for(int i = 0; i < 0xC; i++)
                {
                    acceSl.posSliders[i] = sliders[i];
                    acceSl.scaleSliders[i] = sliders[i + 0xC];
                    acceSl.rotSliders[i] = sliders[i + 0x18];
                }

                return acceSl;
            }

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
            public OldAccessoryPositionSliders GetOldAccessoryPositionSliders()
            {
                OldAccessoryPositionSliders oldSliders = new OldAccessoryPositionSliders();
                //Accessory 1 Y+Z
                oldSliders.oldSliders[0] = SignednibblePack(sliders[1], sliders[2]);
                //Accessory 4 X + Accessory 1 X
                oldSliders.oldSliders[1] = SignednibblePack(sliders[9], sliders[0]);

                //Accessory 2 Y+Z
                oldSliders.oldSliders[2] = SignednibblePack(sliders[4], sliders[5]);
                //Accessory 4 Y + Accessory 2 X
                oldSliders.oldSliders[3] = SignednibblePack(sliders[10], sliders[3]);

                //Accessory 3 Y+Z
                oldSliders.oldSliders[4] = SignednibblePack(sliders[7], sliders[8]);
                //Accessory 4 Y + Accessory 3 X
                oldSliders.oldSliders[5] = SignednibblePack(sliders[11], sliders[6]);

                return oldSliders;
            }
        }

        public struct OldAccessorySliders
        {
            public fixed byte oldSliders[0x12];

            public AccessorySlidersReboot GetAccessorySlidersReboot()
            {
                AccessorySlidersReboot acceSL = new();

                //Position
                //Accessory 1 Y+Z
                SignednibbleUnpack(oldSliders[0], out acceSL.posSliders[1], out acceSL.posSliders[2]);
                //Accessory 4 X + Accessory 1 X
                SignednibbleUnpack(oldSliders[1], out acceSL.posSliders[9], out acceSL.posSliders[0]);
                //Accessory 2 Y+Z
                SignednibbleUnpack(oldSliders[2], out acceSL.posSliders[4], out acceSL.posSliders[5]);
                //Accessory 4 Y + Accessory 2 X
                SignednibbleUnpack(oldSliders[3], out acceSL.posSliders[10], out acceSL.posSliders[3]);
                //Accessory 3 Y+Z
                SignednibbleUnpack(oldSliders[4], out acceSL.posSliders[7], out acceSL.posSliders[8]);
                //Accessory 4 Y + Accessory 3 X
                SignednibbleUnpack(oldSliders[5], out acceSL.posSliders[11], out acceSL.posSliders[6]);

                //Scale
                //Accessory 1 Y+Z
                SignednibbleUnpack(oldSliders[6], out acceSL.scaleSliders[1], out acceSL.scaleSliders[2]);
                //Accessory 4 X + Accessory 1 X
                SignednibbleUnpack(oldSliders[7], out acceSL.scaleSliders[9], out acceSL.scaleSliders[0]);
                //Accessory 2 Y+Z
                SignednibbleUnpack(oldSliders[8], out acceSL.scaleSliders[4], out acceSL.scaleSliders[5]);
                //Accessory 4 Y + Accessory 2 X
                SignednibbleUnpack(oldSliders[9], out acceSL.scaleSliders[10], out acceSL.scaleSliders[3]);
                //Accessory 3 Y+Z
                SignednibbleUnpack(oldSliders[10], out acceSL.scaleSliders[7], out acceSL.scaleSliders[8]);
                //Accessory 4 Y + Accessory 3 X
                SignednibbleUnpack(oldSliders[11], out acceSL.scaleSliders[11], out acceSL.scaleSliders[6]);

                //Rotation
                //Accessory 1 Y+Z
                SignednibbleUnpack(oldSliders[12], out acceSL.rotSliders[1], out acceSL.rotSliders[2]);
                //Accessory 4 X + Accessory 1 X
                SignednibbleUnpack(oldSliders[13], out acceSL.rotSliders[9], out acceSL.rotSliders[0]);
                //Accessory 2 Y+Z
                SignednibbleUnpack(oldSliders[14], out acceSL.rotSliders[4], out acceSL.rotSliders[5]);
                //Accessory 4 Y + Accessory 2 X
                SignednibbleUnpack(oldSliders[15], out acceSL.rotSliders[10], out acceSL.rotSliders[3]);
                //Accessory 3 Y+Z
                SignednibbleUnpack(oldSliders[16], out acceSL.rotSliders[7], out acceSL.rotSliders[8]);
                //Accessory 4 Y + Accessory 3 X
                SignednibbleUnpack(oldSliders[17], out acceSL.rotSliders[11], out acceSL.rotSliders[6]);

                return acceSL;
            }


            public AccessorySliders GetAccessorySliders()
            {
                AccessorySliders accessorySliders = new();

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

            public AccessorySlidersReboot GetAccessorySlidersReboot()
            {
                AccessorySlidersReboot acceSL = new();

                //Accessory 1 Y+Z
                SignednibbleUnpack(oldSliders[0], out acceSL.posSliders[1], out acceSL.posSliders[2]);
                //Accessory 4 X + Accessory 1 X
                SignednibbleUnpack(oldSliders[1], out acceSL.posSliders[9], out acceSL.posSliders[0]);
                //Accessory 2 Y+Z
                SignednibbleUnpack(oldSliders[2], out acceSL.posSliders[4], out acceSL.posSliders[5]);
                //Accessory 4 Y + Accessory 2 X
                SignednibbleUnpack(oldSliders[3], out acceSL.posSliders[10], out acceSL.posSliders[3]);
                //Accessory 3 Y+Z
                SignednibbleUnpack(oldSliders[4], out acceSL.posSliders[7], out acceSL.posSliders[8]);
                //Accessory 4 Y + Accessory 3 X
                SignednibbleUnpack(oldSliders[5], out acceSL.posSliders[11], out acceSL.posSliders[6]);

                return acceSL;
            }

            public AccessorySliders GetAccessorySliders()
            {
                AccessorySliders accessorySliders = new();

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
            public uint skinTextureSet;

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
            public fixed byte outerColor1[4];
            public fixed byte baseColor1[4]; //Also used for costume colors
            public fixed byte mainColor[4];
            public fixed byte subColor1[4];

            public fixed byte subColor2[4];
            public fixed byte subColor3[4];
            public fixed byte rightEyeColor[4];
            public fixed byte hairColor1[4];

            public fixed byte eyebrowColor[4];
            public fixed byte eyelashColor[4];
            public fixed byte skinColor1[4];
            public fixed byte skinColor2[4];

            public fixed byte baseColor2[4];
            public fixed byte outerColor2[4];
            public fixed byte innerColor1[4];
            public fixed byte innerColor2[4];

            public fixed byte leftEyeColor[4];
            public fixed byte hairColor2[4];
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

        public struct VISI //VISI, stored as 0 or 1 in xxp. In CML, these are stored as bits in a single byte.
        {
            public int hideBasewearOrnament1;
            public int hideBasewearOrnament2;

            public int hideHeadPartOrnament;
            public int hideBodyPartOrnament;
            public int hideArmPartOrnament;
            public int hideLegPartOrnament;

            public int hideOuterwearOrnament;
        }
    }
}
