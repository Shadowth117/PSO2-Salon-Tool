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
using static Character_Making_File_Tool.CharacterDataStructs;
using static Character_Making_File_Tool.CharacterDataStructsReboot;
using static Character_Making_File_Tool.CharacterStructConstants;
using System.Diagnostics;

namespace Character_Making_File_Tool
{
    public unsafe class CharacterHandlerReboot
    {
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
        }

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

            //0x180 SLCT continued
            public BaseSLCTNGS baseSLCTNGS;

            //0x1AC Padding?
            public uint padding0;

            public uint padding1;
            public uint padding2;
            public uint padding3;
            public uint padding4;

            //0x1C0
            public AccessorySlidersReboot accessorySlidersReboot;

            //0x22C
            public FaceExpressionV10 faceNatural;
            public FaceExpressionV10 faceSmile;
            public FaceExpressionV10 faceAngry;
            public FaceExpressionV10 faceSad;

            public FaceExpressionV10 faceSus;
            public FaceExpressionV10 faceEyesClosed;
            public FaceExpressionV10 faceSmile2;
            public FaceExpressionV10 faceWink;

            public FaceExpressionV10 faceUnused1;
            public FaceExpressionV10 faceUnused2;

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

            //0x358 Ornament Display - VISI, stored as 0 or 1 in xxp. In CML, these are stored as bits in a single byte.
            public VISI ngsVISI;

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

            //0x398 - Accessory attach points followed by color choices
            public AccessoryMisc accessoryMiscData;
        }

        //V10, but expressions have 0x2 added to them
        public struct XXPV11
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

            //0x180 SLCT continued
            public BaseSLCTNGS baseSLCTNGS;

            //0x1AC Padding?
            public uint padding0;

            public uint padding1;
            public uint padding2;
            public uint padding3;
            public uint padding4;

            //0x1C0
            public AccessorySlidersReboot accessorySlidersReboot;

            //0x22C
            public FaceExpressionV11 faceNatural;
            public FaceExpressionV11 faceSmile;
            public FaceExpressionV11 faceAngry;
            public FaceExpressionV11 faceSad;

            public FaceExpressionV11 faceSus;
            public FaceExpressionV11 faceEyesClosed;
            public FaceExpressionV11 faceSmile2;
            public FaceExpressionV11 faceWink;

            public FaceExpressionV11 faceUnused1;
            public FaceExpressionV11 faceUnused2;

            //0x2F4
            public PaintPriority paintPriority;
            public ushort padding14;
            public uint padding15;
            public uint padding16;

            //0x304 NGS extra slider data
            public NGSSLID ngsSLID;

            //0x344 - Motion change 
            public NGSMTON ngsMTON;

            //0x364 - Costume ornament hiding leftover?
            public int int_350;
            public int int_354;

            //0x36C Ornament Display - VISI, stored as 0 or 1 in xxp. In CML, these are stored as bits in a single byte.
            public VISI ngsVISI;

            //0x388
            public uint padding17;
            public uint padding18;
            public uint padding19;

            public uint padding20;
            public uint padding21;
            public uint padding22;
            public uint padding23;

            public uint padding24;
            public uint padding25;

            //0x3AC - Accessory attach points followed by color choices
            public AccessoryMisc accessoryMiscData;
        }

        //Adds the AltFaceFIGR struct between neckAngle and ngsCOL2
        public struct XXPV12
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

            //0xE0 AltFaceFIGR
            public AltFaceFIGR altFace;

            //0x158 COL2 - These are just standard RGBA in NGS as opposed to the original COLR slider positions
            public COL2 ngsCOL2;

            //0x1A0 SLCT
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;
            public uint leftEyePart;

            //0x1F8 SLCT continued
            public BaseSLCTNGS baseSLCTNGS;

            //0x224 Padding?
            public uint padding0;

            public uint padding1;
            public uint padding2;
            public uint padding3;
            public uint padding4;

            //0x238
            public AccessorySlidersReboot accessorySlidersReboot;

            //0x2A4
            public FaceExpressionV11 faceNatural;
            public FaceExpressionV11 faceSmile;
            public FaceExpressionV11 faceAngry;
            public FaceExpressionV11 faceSad;

            public FaceExpressionV11 faceSus;
            public FaceExpressionV11 faceEyesClosed;
            public FaceExpressionV11 faceSmile2;
            public FaceExpressionV11 faceWink;

            public FaceExpressionV11 faceUnused1;
            public FaceExpressionV11 faceUnused2;

            //0x36C
            public PaintPriority paintPriority;
            public ushort padding14;
            public uint padding15;
            public uint padding16;

            //0x37C NGS extra slider data
            public NGSSLID ngsSLID;

            //0x3BC - Motion change 
            public NGSMTON ngsMTON;

            //0x3DC - Costume ornament hiding leftover?
            public int int_350;
            public int int_354;

            //0x3E4 Ornament Display - VISI, stored as 0 or 1 in xxp. In CML, these are stored as bits in a single byte.
            public VISI ngsVISI;

            //0x400
            public uint padding17;
            public uint padding18;
            public uint padding19;

            public uint padding20;
            public uint padding21;
            public uint padding22;
            public uint padding23;

            public uint padding24;
            public uint padding25;

            //0x424 - Accessory attach points followed by color choices
            public AccessoryMisc accessoryMiscData;
        }

        public class xxpGeneralReboot
        {
            public int xxpVersion;
            public int fileSize;

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

            public AltFaceFIGR altFace;

            //COL2 - These are just standard RGBA in NGS as opposed to the original COLR slider positions
            public COL2 ngsCOL2;

            //SLCT
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;
            public uint leftEyePart;

            //SLCT continued
            public BaseSLCTNGS baseSLCTNGS;

            public AccessorySlidersReboot accessorySlidersReboot;

            public FaceExpressionV11 faceNatural;
            public FaceExpressionV11 faceSmile;
            public FaceExpressionV11 faceAngry;
            public FaceExpressionV11 faceSad;

            public FaceExpressionV11 faceSus;
            public FaceExpressionV11 faceEyesClosed;
            public FaceExpressionV11 faceSmile2;
            public FaceExpressionV11 faceWink;

            public FaceExpressionV11 faceUnused1;
            public FaceExpressionV11 faceUnused2;

            public PaintPriority paintPriority = PaintPriority.GetDefault();

            //NGS extra slider data
            public NGSSLID ngsSLID;

            //Motion change 
            public NGSMTON ngsMTON;

            //Costume ornament hiding leftover?
            public int int_350;
            public int int_354;

            public VISI ngsVISI;

            public AccessoryMisc accessoryMiscData;

            public xxpGeneralReboot()
            {
            }

            protected void SetDefaultExpressions()
            {
                if (baseDOC.gender == 0)
                {
                    faceNatural = defaultMaleExpressions[0];
                    faceSmile = defaultMaleExpressions[1];
                    faceAngry = defaultMaleExpressions[2];
                    faceSad = defaultMaleExpressions[3];

                    faceSus = defaultMaleExpressions[4];
                    faceEyesClosed = defaultMaleExpressions[5];
                    faceSmile2 = defaultMaleExpressions[6];
                    faceWink = defaultMaleExpressions[7];

                    faceUnused1 = defaultMaleExpressions[8];
                    faceUnused2 = defaultMaleExpressions[9];
                }
                else
                {
                    faceNatural = defaultFemaleExpressions[0];
                    faceSmile = defaultFemaleExpressions[1];
                    faceAngry = defaultFemaleExpressions[2];
                    faceSad = defaultFemaleExpressions[3];

                    faceSus = defaultFemaleExpressions[4];
                    faceEyesClosed = defaultFemaleExpressions[5];
                    faceSmile2 = defaultFemaleExpressions[6];
                    faceWink = defaultFemaleExpressions[7];

                    faceUnused1 = defaultFemaleExpressions[8];
                    faceUnused2 = defaultFemaleExpressions[9];
                }
            }

            public virtual byte[] GetBytes()
            {
                throw new NotImplementedException();
            }
        }

        public class xxpGeneralRebootV2 : xxpGeneralReboot
        {
            public xxpGeneralRebootV2(XXPV2 tempXXP)
            {
                xxpVersion = 2;
                fileSize = 0x15C;

                baseDOC = tempXXP.baseDOC;
                skinVariant = 0;
                eyebrowDensity = 0;
                cmlVariant = 0;

                baseFIGR = tempXXP.baseFIGR;
                baseFIGR.ToNGS();

                ngsCOL2 = ColorConversion.COLRToCOL2(tempXXP.baseCOLR, baseDOC.race);

                baseSLCT = tempXXP.baseSLCT;

                //Eye parts 
                byte[] eyes = BitConverter.GetBytes(baseSLCT.eyePart);
                baseSLCT.eyePart = eyes[0];
                if (baseDOC.race == 3)
                {
                    leftEyePart = eyes[1];
                }
                else
                {
                    leftEyePart = eyes[0];
                }

                //Conditionally assign premade expressions based on gender
                SetDefaultExpressions();

                paintPriority = PaintPriority.GetDefault();
            }

            public XXPV2 GetXXP()
            {
                XXPV2 xxpv2 = new XXPV2();

                xxpv2.baseDOC = baseDOC;
                xxpv2.baseFIGR = baseFIGR;
                xxpv2.baseFIGR.ToOld();
                xxpv2.baseCOLR = ColorConversion.COL2ToCOLR(ngsCOL2, baseDOC.race);
                xxpv2.baseSLCT = baseSLCT;

                return xxpv2;
            }

            public override byte[] GetBytes()
            {
                return Reloaded.Memory.Struct.GetBytes(GetXXP());
            }
        }

        public class xxpGeneralRebootV5 : xxpGeneralReboot
        {
            public xxpGeneralRebootV5(XXPV5 tempXXP)
            {
                xxpVersion = 5;
                fileSize = 0x170;

                baseDOC = tempXXP.baseDOC;
                skinVariant = 0;
                eyebrowDensity = 0;
                cmlVariant = 0;

                baseFIGR = tempXXP.baseFIGR;
                baseFIGR.ToNGS();

                ngsCOL2 = ColorConversion.COLRToCOL2(tempXXP.baseCOLR, baseDOC.race);

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;

                //Eye parts 
                byte[] eyes = BitConverter.GetBytes(baseSLCT.eyePart);
                baseSLCT.eyePart = eyes[0];
                if (baseDOC.race == 3)
                {
                    leftEyePart = eyes[1];
                }
                else
                {
                    leftEyePart = eyes[0];
                }

                //Conditionally assign premade expressions based on gender
                SetDefaultExpressions();

                accessorySlidersReboot = tempXXP.oldPosSliders.GetAccessorySlidersReboot();

                paintPriority = PaintPriority.GetDefault();
            }

            public XXPV5 GetXXP()
            {
                XXPV5 xxpv5 = new XXPV5();

                xxpv5.baseDOC = baseDOC;
                xxpv5.baseFIGR = baseFIGR;
                xxpv5.baseFIGR.ToOld();
                xxpv5.baseCOLR = ColorConversion.COL2ToCOLR(ngsCOL2, baseDOC.race);
                xxpv5.baseSLCT = baseSLCT;
                xxpv5.baseSLCT2 = baseSLCT2;
                xxpv5.oldPosSliders = accessorySlidersReboot.GetOldAccessoryPositionSliders();

                return xxpv5;
            }

            public override byte[] GetBytes()
            {
                return Reloaded.Memory.Struct.GetBytes(GetXXP());
            }
        }

        public class xxpGeneralRebootV6 : xxpGeneralReboot
        {
            public xxpGeneralRebootV6(XXPV6 tempXXP)
            {
                xxpVersion = 6;
                fileSize = 0x2DC;

                baseDOC = tempXXP.baseDOC;
                skinVariant = 0;
                eyebrowDensity = 0;
                cmlVariant = 0;

                baseFIGR = tempXXP.baseFIGR;
                baseFIGR.ToNGS();
                neckVerts = tempXXP.baseFIGR2.neckVerts;
                neckVerts.ToNGSSliders();
                waistVerts = tempXXP.baseFIGR2.waistVerts;
                waistVerts.ToNGSSliders();

                ngsCOL2 = ColorConversion.COLRToCOL2(tempXXP.baseCOLR, baseDOC.race);

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;

                //Eye parts 
                byte[] eyes = BitConverter.GetBytes(baseSLCT.eyePart);
                baseSLCT.eyePart = eyes[0];
                if (baseDOC.race == 3)
                {
                    leftEyePart = eyes[1];
                }
                else
                {
                    leftEyePart = eyes[0];
                }

                //Conditionally assign premade expressions based on gender
                SetDefaultExpressions();

                accessorySlidersReboot = tempXXP.oldAccessorySliders.GetAccessorySlidersReboot();

                paintPriority = PaintPriority.GetDefault();
            }

            public XXPV6 GetXXP()
            {
                XXPV6 xxpv6 = new XXPV6();

                xxpv6.baseDOC = baseDOC;
                xxpv6.baseFIGR = baseFIGR;
                xxpv6.baseFIGR.ToOld();

                xxpv6.baseFIGR2 = new BaseFIGR2();
                xxpv6.baseFIGR2.neckVerts = neckVerts;
                xxpv6.baseFIGR2.waistVerts = waistVerts;
                xxpv6.baseFIGR2.neck2Verts = neckVerts;
                xxpv6.baseFIGR2.waist2Verts = waistVerts;
                xxpv6.baseFIGR2.arm2Verts = baseFIGR.armVerts;
                xxpv6.baseFIGR2.body2Verts = baseFIGR.bodyVerts;
                xxpv6.baseFIGR2.bust2Verts = baseFIGR.bustVerts;
                xxpv6.baseFIGR2.leg2Verts = baseFIGR.legVerts;
                xxpv6.baseFIGR2.ToOld();

                xxpv6.baseCOLR = ColorConversion.COL2ToCOLR(ngsCOL2, baseDOC.race);
                xxpv6.baseSLCT = baseSLCT;
                xxpv6.baseSLCT2 = baseSLCT2;
                xxpv6.oldAccessorySliders = accessorySlidersReboot.GetOldAccessorySliders();
                xxpv6.paintPriority = paintPriority;

                return xxpv6;
            }

            public override byte[] GetBytes()
            {
                return Reloaded.Memory.Struct.GetBytes(GetXXP());
            }
        }

        public class xxpGeneralRebootV7 : xxpGeneralReboot
        {
            public xxpGeneralRebootV7(XXPV7 tempXXP)
            {
                xxpVersion = 7;
                fileSize = 0; // TODO: Get filesize for V7

                baseDOC = tempXXP.baseDOC;
                skinVariant = 0;
                eyebrowDensity = 0;
                cmlVariant = 0;

                baseFIGR = tempXXP.baseFIGR;
                baseFIGR.ToNGS();
                neckVerts = tempXXP.baseFIGR2.neckVerts;
                neckVerts.ToNGSSliders();
                waistVerts = tempXXP.baseFIGR2.waistVerts;
                waistVerts.ToNGSSliders();

                ngsCOL2 = ColorConversion.COLRToCOL2(tempXXP.baseCOLR, baseDOC.race);

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;

                //Eye parts 
                byte[] eyes = BitConverter.GetBytes(baseSLCT.eyePart);
                baseSLCT.eyePart = eyes[0];
                if (baseDOC.race == 3)
                {
                    leftEyePart = eyes[1];
                }
                else
                {
                    leftEyePart = eyes[0];
                }

                //Conditionally assign premade expressions based on gender
                SetDefaultExpressions();

                accessorySlidersReboot = tempXXP.accessorySliders.GetAccessorySlidersReboot();

                paintPriority = PaintPriority.GetDefault();
            }

            public XXPV7 GetXXP() // TODO This is copied from V6
            {
                XXPV7 xxpv7 = new XXPV7();

                xxpv7.baseDOC = baseDOC;
                xxpv7.baseFIGR = baseFIGR;
                xxpv7.baseFIGR.ToOld();

                xxpv7.baseFIGR2 = new BaseFIGR2();
                xxpv7.baseFIGR2.neckVerts = neckVerts;
                xxpv7.baseFIGR2.waistVerts = waistVerts;
                xxpv7.baseFIGR2.neck2Verts = neckVerts;
                xxpv7.baseFIGR2.waist2Verts = waistVerts;
                xxpv7.baseFIGR2.arm2Verts = baseFIGR.armVerts;
                xxpv7.baseFIGR2.body2Verts = baseFIGR.bodyVerts;
                xxpv7.baseFIGR2.bust2Verts = baseFIGR.bustVerts;
                xxpv7.baseFIGR2.leg2Verts = baseFIGR.legVerts;
                xxpv7.baseFIGR2.ToOld();

                xxpv7.baseCOLR = ColorConversion.COL2ToCOLR(ngsCOL2, baseDOC.race);
                xxpv7.baseSLCT = baseSLCT;
                xxpv7.baseSLCT2 = baseSLCT2;
                xxpv7.accessorySliders = accessorySlidersReboot.GetClassicAccessorySliders();
                xxpv7.paintPriority = paintPriority;

                return xxpv7;
            }

            public override byte[] GetBytes()
            {
                return Reloaded.Memory.Struct.GetBytes(GetXXP());
            }
        }

        public class xxpGeneralRebootV9 : xxpGeneralReboot
        {
            public xxpGeneralRebootV9(XXPV9 tempXXP)
            {
                xxpVersion = 10;
                fileSize = 0x2F0;

                baseDOC = tempXXP.baseDOC;
                skinVariant = tempXXP.skinVariant;
                eyebrowDensity = tempXXP.eyebrowDensity;
                cmlVariant = tempXXP.cmlVariant;

                baseFIGR = tempXXP.baseFIGR;
                baseFIGR.ToNGS();
                neckVerts = tempXXP.baseFIGR2.neckVerts;
                neckVerts.ToNGSSliders();
                waistVerts = tempXXP.baseFIGR2.waistVerts;
                waistVerts.ToNGSSliders();

                ngsCOL2 = ColorConversion.COLRToCOL2(tempXXP.baseCOLR, baseDOC.race);

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;

                //Eye parts 
                if (baseDOC.race == 3)
                {
                    leftEyePart = tempXXP.leftEyePart;
                }
                else
                {
                    leftEyePart = tempXXP.baseSLCT.eyePart;
                }

                //Conditionally assign premade expressions based on gender
                SetDefaultExpressions();

                accessorySlidersReboot = tempXXP.accessorySliders.GetAccessorySlidersReboot();

                paintPriority = PaintPriority.GetDefault();
            }

            public XXPV9 GetXXP()
            {
                XXPV9 xxpv9 = new XXPV9();

                xxpv9.baseDOC = baseDOC;
                xxpv9.skinVariant = 3; //Hack to deal with limitations of backwards conversion
                xxpv9.eyebrowDensity = eyebrowDensity;
                xxpv9.cmlVariant = cmlVariant;

                xxpv9.baseFIGR = baseFIGR;
                xxpv9.baseFIGR.ToOld();

                xxpv9.baseFIGR2 = new BaseFIGR2();
                xxpv9.baseFIGR2.neckVerts = neckVerts;
                xxpv9.baseFIGR2.waistVerts = waistVerts;
                xxpv9.baseFIGR2.neck2Verts = neckVerts;
                xxpv9.baseFIGR2.waist2Verts = waistVerts;
                xxpv9.baseFIGR2.arm2Verts = baseFIGR.armVerts;
                xxpv9.baseFIGR2.body2Verts = baseFIGR.bodyVerts;
                xxpv9.baseFIGR2.bust2Verts = baseFIGR.bustVerts;
                xxpv9.baseFIGR2.leg2Verts = baseFIGR.legVerts;
                xxpv9.baseFIGR2.ToOld();

                xxpv9.baseCOLR = ColorConversion.COL2ToCOLR(ngsCOL2, baseDOC.race);
                xxpv9.baseSLCT = baseSLCT;
                xxpv9.baseSLCT2 = baseSLCT2;
                xxpv9.leftEyePart = leftEyePart;
                xxpv9.accessorySliders = accessorySlidersReboot.GetClassicAccessorySliders();
                xxpv9.paintPriority = paintPriority;

                return xxpv9;
            }

            public override byte[] GetBytes()
            {
                return Reloaded.Memory.Struct.GetBytes(GetXXP());
            }
        }

        public class xxpGeneralRebootV10 : xxpGeneralReboot
        {
            public xxpGeneralRebootV10(XXPV10 tempXXP)
            {
                xxpVersion = 10;
                fileSize = 0x3AC;

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

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySlidersReboot = tempXXP.accessorySlidersReboot;

                faceNatural = FaceExpressionV11.CreateExpression(tempXXP.faceNatural);
                faceSmile = FaceExpressionV11.CreateExpression(tempXXP.faceSmile);
                faceAngry = FaceExpressionV11.CreateExpression(tempXXP.faceAngry);
                faceSad = FaceExpressionV11.CreateExpression(tempXXP.faceSad);

                faceSus = FaceExpressionV11.CreateExpression(tempXXP.faceSus);
                faceEyesClosed = FaceExpressionV11.CreateExpression(tempXXP.faceEyesClosed);
                faceSmile2 = FaceExpressionV11.CreateExpression(tempXXP.faceSmile2);
                faceWink = FaceExpressionV11.CreateExpression(tempXXP.faceWink);

                faceUnused1 = FaceExpressionV11.CreateExpression(tempXXP.faceUnused1);
                faceUnused2 = FaceExpressionV11.CreateExpression(tempXXP.faceUnused2);

                paintPriority = tempXXP.paintPriority;

                ngsSLID = tempXXP.ngsSLID;
                ngsMTON = tempXXP.ngsMTON;

                int_350 = tempXXP.int_350;
                int_354 = tempXXP.int_354;
                ngsVISI = tempXXP.ngsVISI;

                accessoryMiscData = tempXXP.accessoryMiscData;
            }

            public XXPV10 GetXXP()
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
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySlidersReboot = accessorySlidersReboot;
                tempXXP.faceNatural = faceNatural.expStruct;
                tempXXP.faceSmile = faceSmile.expStruct;
                tempXXP.faceAngry = faceAngry.expStruct;
                tempXXP.faceSad = faceSad.expStruct;
                tempXXP.faceSus = faceSus.expStruct;
                tempXXP.faceEyesClosed = faceEyesClosed.expStruct;
                tempXXP.faceSmile2 = faceSmile2.expStruct;
                tempXXP.faceWink = faceWink.expStruct;
                tempXXP.faceUnused1 = faceUnused1.expStruct;
                tempXXP.faceUnused2 = faceUnused2.expStruct;
                tempXXP.paintPriority = paintPriority;
                tempXXP.ngsSLID = ngsSLID;
                tempXXP.ngsMTON = ngsMTON;
                tempXXP.int_350 = int_350;
                tempXXP.int_354 = int_354;
                tempXXP.ngsVISI = ngsVISI;
                tempXXP.accessoryMiscData = accessoryMiscData;

                return tempXXP;
            }

            public override byte[] GetBytes()
            {
                return Reloaded.Memory.Struct.GetBytes(GetXXP());
            }
        }

        public class xxpGeneralRebootV11 : xxpGeneralReboot
        {
            public xxpGeneralRebootV11(XXPV11 tempXXP)
            {
                xxpVersion = 11;
                fileSize = 0x3C0;

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

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySlidersReboot = tempXXP.accessorySlidersReboot;

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
                ngsVISI = tempXXP.ngsVISI;

                accessoryMiscData = tempXXP.accessoryMiscData;
            }

            public XXPV11 GetXXP()
            {
                XXPV11 tempXXP = new XXPV11();

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
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySlidersReboot = accessorySlidersReboot;
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
                tempXXP.ngsVISI = ngsVISI;
                tempXXP.accessoryMiscData = accessoryMiscData;

                return tempXXP;
            }

            public override byte[] GetBytes()
            {
                return Reloaded.Memory.Struct.GetBytes(GetXXP());
            }
        }

        public class xxpGeneralRebootV12 : xxpGeneralReboot
        {
            public xxpGeneralRebootV12(XXPV12 tempXXP)
            {
                xxpVersion = 12;
                fileSize = 0x438;

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

                altFace = tempXXP.altFace;

                ngsCOL2 = tempXXP.ngsCOL2;

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;
                leftEyePart = tempXXP.leftEyePart;

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySlidersReboot = tempXXP.accessorySlidersReboot;

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
                ngsVISI = tempXXP.ngsVISI;

                accessoryMiscData = tempXXP.accessoryMiscData;
            }

            public XXPV12 GetXXP()
            {
                XXPV12 tempXXP = new XXPV12();

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
                tempXXP.altFace = altFace;
                tempXXP.ngsCOL2 = ngsCOL2;
                tempXXP.baseSLCT = baseSLCT;
                tempXXP.baseSLCT2 = baseSLCT2;
                tempXXP.leftEyePart = leftEyePart;
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySlidersReboot = accessorySlidersReboot;
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
                tempXXP.ngsVISI = ngsVISI;
                tempXXP.accessoryMiscData = accessoryMiscData;

                return tempXXP;
            }

            public override byte[] GetBytes()
            {
                return Reloaded.Memory.Struct.GetBytes(GetXXP());
            }
        }
    }
}
