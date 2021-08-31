using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Character_Making_File_Tool.CharacterHandler;
using static Character_Making_File_Tool.Vector3Int;
using static Character_Making_File_Tool.CharacterHandlerUtilityMethods;

namespace CharFileLibrary
{
    public unsafe class CharacterHandlerReboot
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

        public struct XXPV2
        {
            public BaseDOC baseDOC;
            public BaseFIGR baseFIGR;

            public fixed byte paddingA[0x24];

            public BaseCOLR baseCOLR;
            public BaseSLCT baseSLCT;

            public fixed byte paddingB[0x8];
        }

        public struct XXPV5
        {
            public BaseDOC baseDOC;
            public BaseFIGR baseFIGR;

            public fixed byte paddingA[0x24];

            public BaseCOLR baseCOLR;
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;

            public int paddingB;

            public OldAccessoryPositionSliders oldPosSliders;

            public short paddingC;
        }

        //Added FIGR2, accessory rotation, and accessory scale sliders, body paint priority
        public struct XXPV6
        {
            public BaseDOC baseDOC;
            public BaseFIGR baseFIGR;
            public BaseFIGR2 baseFIGR2;

            public fixed byte paddingA[0x6C];

            public BaseCOLR baseCOLR;

            public fixed byte paddingB[0x78];

            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;

            public OldAccessorySliders oldAccessorySliders;

            public PaintPriority paintPriority;

            public fixed byte paddingC[0x8];
        }

        /*
        //XXPV6, but adds the newer accessory slider format
        public struct XXPV7
        {
            public BaseDOC baseDOC;
            public BaseFIGR baseFIGR;
            public BaseFIGR2 baseFIGR2;

            public fixed byte paddingA[0x6C];

            public BaseCOLR baseCOLR;

            public fixed byte paddingB[0x78];

            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;

            public AccessorySliders accessorySliders;

            public PaintPriority paintPriority;

            public fixed byte paddingC[0x8];
        }*/

        //XXPV8, XXPV9. V8 as V9, but ignores some fields. Adds dedicated eye part uint
        public struct XXPV9
        {
            public BaseDOC baseDOC;
            public byte skinVariant;
            public sbyte eyebrowDensity;
            public short cmlVariant;

            public BaseFIGR baseFIGR;
            public BaseFIGR2 baseFIGR2;

            public fixed byte paddingA[0x6C];

            public BaseCOLR baseCOLR;

            public fixed byte paddingB[0x78];

            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;
            public uint leftEyePart;

            public fixed byte paddingC[0x2C];

            public AccessorySliders accessorySliders;
            public PaintPriority paintPriority;

            public fixed byte paddingD[0xE];
        }

        //Significant restructure from others.
        //Assume values that ranged from -10000 to 10000 range from -127 to 127 now, despite being full ints still
        public struct XXPV10
        {
            //DOC 0x10
            public BaseDOC baseDOC;
            public byte skinVariant; //0 or above 3 for default, 1 for human, 2 for dewman, 3 for cast. This decides the color map used for the skin. 
            public sbyte eyebrowDensity; //-100 to 100 
            public short cmlVariant;

            //0x20
            public BaseFIGR baseFIGR;
            public Vec3Int neckVerts;
            public Vec3Int waistVerts;

            //0xBC
            public Vec3Int hands;
            public Vec3Int horns;
            public int eyeSize;
            public int eyeHorizontalPosition;
            public int neckAngle;

            //0xE0 COL2 - These are just standard RGBA in NGS as opposed to the original COLR slider positions
            public COL2 ngsCOL2;

            //0x128 SLCT
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;
            public uint leftEyePart;
            public uint skinTextureSet;

            //0x180 SLCT continued
            public BaseSLCTNGS baseSLCTNGS;

            //0x1AC Padding?
            public uint padding0;

            public uint padding1;
            public uint padding2;
            public uint padding3;
            public uint padding4;

            public AccessorySliders accessorySliders;

            //0x22C
            public FaceExpression faceNatural;
            public FaceExpression faceSmile;
            public FaceExpression faceAngry;
            public FaceExpression faceSad;

            public FaceExpression faceSus;
            public FaceExpression faceEyesClosed;
            public FaceExpression faceSmile2;
            public FaceExpression faceWink;

            public FaceExpression faceUnused1;
            public FaceExpression faceUnused2;

            //0x2E0
            public PaintPriority paintPriority;
            public ushort padding14;
            public uint padding15;
            public uint padding16;

            //0x2F0 NGS extra slider data
            public NGSSLID ngsSLID;

            //0x330 - Motion change 
            public NGSMTON ngsMTON;

            //0x350 - Costume ornament hiding leftover?
            public int int_350;
            public int int_354;

            //0x358 Ornament Display - Seemingly all just boolean int true or false. 1 is true. VISI in cml?
            public int hideBasewearOrnament1;
            public int hideBasewearOrnament2;

            public int hideHeadPartOrnament;
            public int hideBodyPartOrnament;
            public int hideArmPartOrnament;
            public int hideLegPartOrnament;

            public int hideOuterwearOrnament;

            //0x374
            public uint padding17;
            public uint padding18;
            public uint padding19;

            public uint padding20;
            public uint padding21;
            public uint padding22;
            public uint padding23;

            public uint padding24;
            public uint padding25;
            public uint padding26;
            public uint padding27;

            //0x398 - Accessory attach points followed by color choices
            public AccessoryMisc accessoryMiscData;
        }

        public class xxpGeneralReboot
        {
            public int xxpVersion; 


            public BaseDOC baseDOC;
            public byte skinVariant; //0 or above 3 for default, 1 for human, 2 for dewman, 3 for cast. This decides the color map used for the skin. 
            public sbyte eyebrowDensity; //-100 to 100 
            public short cmlVariant;

            public BaseFIGR baseFIGR;
            public Vec3Int neckVerts;
            public Vec3Int waistVerts;

            public Vec3Int hands;
            public Vec3Int horns;
            public int eyeSize;
            public int eyeHorizontalPosition;
            public int neckAngle;

            //COL2 - These are just standard RGBA in NGS as opposed to the original COLR slider positions
            public COL2 ngsCOL2;

            //SLCT
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;
            public uint leftEyePart;
            public uint skinTextureSet;

            //SLCT continued
            public BaseSLCTNGS baseSLCTNGS;

            public AccessorySliders accessorySliders;

            public FaceExpression faceNatural;
            public FaceExpression faceSmile;
            public FaceExpression faceAngry;
            public FaceExpression faceSad;

            public FaceExpression faceSus;
            public FaceExpression faceEyesClosed;
            public FaceExpression faceSmile2;
            public FaceExpression faceWink;

            public FaceExpression faceUnused1;
            public FaceExpression faceUnused2;

            public PaintPriority paintPriority;

            //NGS extra slider data
            public NGSSLID ngsSLID;

            //Motion change 
            public NGSMTON ngsMTON;

            //Costume ornament hiding leftover?
            public int int_350;
            public int int_354;

            public int hideBasewearOrnament1;
            public int hideBasewearOrnament2;

            public int hideHeadPartOrnament;
            public int hideBodyPartOrnament;
            public int hideArmPartOrnament;
            public int hideLegPartOrnament;

            public int hideOuterwearOrnament;

            public AccessoryMisc accessoryMiscData;

            public xxpGeneralReboot(XXPV10 tempXXP)
            {
                xxpVersion = 10;

                baseDOC = tempXXP.baseDOC;
                skinVariant = tempXXP.skinVariant;
                eyebrowDensity = tempXXP.eyebrowDensity;
                cmlVariant = tempXXP.cmlVariant;

                baseFIGR = tempXXP.baseFIGR;
                neckVerts = tempXXP.neckVerts;
                waistVerts = tempXXP.waistVerts;

                hands = tempXXP.hands;
                horns = tempXXP.horns;
                eyeSize = tempXXP.eyeSize;
                eyeHorizontalPosition = tempXXP.eyeHorizontalPosition;
                neckAngle = tempXXP.neckAngle;

                ngsCOL2 = tempXXP.ngsCOL2;

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;
                leftEyePart = tempXXP.leftEyePart;
                skinTextureSet = tempXXP.skinTextureSet;

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySliders = tempXXP.accessorySliders;

                faceNatural = tempXXP.faceNatural;
                faceSmile = tempXXP.faceSmile;
                faceAngry = tempXXP.faceAngry;
                faceSad = tempXXP.faceSad;

                faceSus = tempXXP.faceSus;
                faceEyesClosed = tempXXP.faceEyesClosed;
                faceSmile2 = tempXXP.faceSmile2;
                faceWink = tempXXP.faceWink;

                faceUnused1 = tempXXP.faceUnused1;
                faceUnused2 = tempXXP.faceUnused2;

                paintPriority = tempXXP.paintPriority;

                ngsSLID = tempXXP.ngsSLID;
                ngsMTON = tempXXP.ngsMTON;

                int_350 = tempXXP.int_350;
                int_354 = tempXXP.int_354;
                hideBasewearOrnament1 = tempXXP.hideBasewearOrnament1;
                hideBasewearOrnament2 = tempXXP.hideBasewearOrnament2;

                hideHeadPartOrnament = tempXXP.hideHeadPartOrnament;
                hideBodyPartOrnament = tempXXP.hideBodyPartOrnament;
                hideArmPartOrnament = tempXXP.hideArmPartOrnament;
                hideLegPartOrnament = tempXXP.hideLegPartOrnament;

                hideOuterwearOrnament = tempXXP.hideOuterwearOrnament;

                accessoryMiscData = tempXXP.accessoryMiscData;
            }

            public XXPV2 GetXXPV2()
            {
                XXPV2 xxpv2 = new XXPV2();

                xxpv2.baseDOC = baseDOC;
                //xxpv2.baseFIGR =
                //xxpv2.baseCOLR = 
                xxpv2.baseSLCT = baseSLCT;

                return xxpv2;
            }

            public XXPV10 GetXXPV10()
            {
                XXPV10 tempXXP = new XXPV10();

                tempXXP.baseDOC = baseDOC;
                tempXXP.skinVariant = skinVariant;
                tempXXP.eyebrowDensity = eyebrowDensity;
                tempXXP.cmlVariant = cmlVariant;
                tempXXP.baseFIGR = baseFIGR;
                tempXXP.neckVerts = neckVerts;
                tempXXP.waistVerts = waistVerts;
                tempXXP.hands = hands;
                tempXXP.horns = horns;
                tempXXP.eyeSize = eyeSize;
                tempXXP.eyeHorizontalPosition = eyeHorizontalPosition;
                tempXXP.neckAngle = neckAngle;
                tempXXP.ngsCOL2 = ngsCOL2;
                tempXXP.baseSLCT = baseSLCT;
                tempXXP.baseSLCT2 = baseSLCT2;
                tempXXP.leftEyePart = leftEyePart;
                tempXXP.skinTextureSet = skinTextureSet;
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySliders = accessorySliders;
                tempXXP.faceNatural = faceNatural;
                tempXXP.faceSmile = faceSmile;
                tempXXP.faceAngry = faceAngry;
                tempXXP.faceSad = faceSad;
                tempXXP.faceSus = faceSus;
                tempXXP.faceEyesClosed = faceEyesClosed;
                tempXXP.faceSmile2 = faceSmile2;
                tempXXP.faceWink = faceWink;
                tempXXP.faceUnused1 = faceUnused1;
                tempXXP.faceUnused2 = faceUnused2;
                tempXXP.paintPriority = paintPriority;
                tempXXP.ngsSLID = ngsSLID;
                tempXXP.ngsMTON = ngsMTON;
                tempXXP.int_350 = int_350;
                tempXXP.int_354 = int_354;
                tempXXP.hideBasewearOrnament1 = hideBasewearOrnament1;
                tempXXP.hideBasewearOrnament2 = hideBasewearOrnament2;
                tempXXP.hideHeadPartOrnament = hideHeadPartOrnament;
                tempXXP.hideBodyPartOrnament = hideBodyPartOrnament;
                tempXXP.hideArmPartOrnament = hideArmPartOrnament;
                tempXXP.hideLegPartOrnament = hideLegPartOrnament;
                tempXXP.hideOuterwearOrnament = hideOuterwearOrnament;
                tempXXP.accessoryMiscData = accessoryMiscData;

                return tempXXP;
            }

            public byte[] GetBytes()
            {
                switch(xxpVersion)
                {
                    case 2:
                        throw new NotImplementedException();
                        break;
                    case 5:
                        throw new NotImplementedException();
                        break;
                    case 6:
                    case 7:
                        throw new NotImplementedException();
                        break;
                    case 8:
                    case 9:
                        throw new NotImplementedException();
                        break;
                    case 10:
                        return Reloaded.Memory.Struct.GetBytes(GetXXPV10());
                }
                throw new NotImplementedException();
            }
        }

    }
}
