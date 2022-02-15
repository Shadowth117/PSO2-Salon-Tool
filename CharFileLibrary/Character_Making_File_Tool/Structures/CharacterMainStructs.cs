using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Character_Making_File_Tool.CharacterDataStructs;
using static Character_Making_File_Tool.CharacterDataStructsReboot;
using static Character_Making_File_Tool.Vector3Int;

namespace Character_Making_File_Tool
{
    public unsafe class CharacterMainStructs
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
    }
}
