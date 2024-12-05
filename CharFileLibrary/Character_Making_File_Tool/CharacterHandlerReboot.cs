using AquaModelLibrary.Helpers;
using System;
using static Character_Making_File_Tool.CharacterDataStructs;
using static Character_Making_File_Tool.CharacterDataStructsReboot;
using static Character_Making_File_Tool.CharacterMainStructs;
using static Character_Making_File_Tool.CharacterStructConstants;
using static Character_Making_File_Tool.Vector3Int;

namespace Character_Making_File_Tool
{
    public unsafe class CharacterHandlerReboot
    {
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
            public Vec3Int hornVerts;
            public int eyeSize;
            public int eyeHorizontalPosition;
            public int neckAngle;

            public FaceFIGR classicFace;

            //COL2 - These are just standard RGBA in NGS as opposed to the original COLR slider positions
            public COL2 ngsCOL2;

            //SLCT
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;
            public uint leftEyePart;

            //SLCT continued
            public BaseSLCTNGS baseSLCTNGS;
            public SLCTNGSExtended slctNGSExtended;

            public AccessorySlidersRebootExtended accessorySlidersRebootExtended;

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

            public AccessoryMiscExtended accessoryMiscDataExtended;

            public CastColorIdSet castColorIds;

            public int celShadingIsEnabled;

            public xxpGeneralReboot()
            {
            }

            public xxpGeneralReboot(XXPV2 tempXXP)
            {
                xxpVersion = 12;

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

                FaceFIGR face = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetClassicFaceData(face);
                }
            }

            public xxpGeneralReboot(XXPV5 tempXXP)
            {
                xxpVersion = 12;

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

                accessorySlidersRebootExtended = tempXXP.oldPosSliders.GetAccessorySlidersRebootExtended();

                paintPriority = PaintPriority.GetDefault();

                FaceFIGR face = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetClassicFaceData(face);
                }
            }

            public xxpGeneralReboot(XXPV6 tempXXP)
            {
                xxpVersion = 12;

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
                if (baseDOC.race == 3)
                {
                    byte[] eyes = BitConverter.GetBytes(baseSLCT.eyePart);
                    baseSLCT.eyePart = eyes[0];
                    leftEyePart = eyes[1];
                }
                else
                {
                    leftEyePart = baseSLCT.eyePart;
                }

                //Conditionally assign premade expressions based on gender
                SetDefaultExpressions();

                accessorySlidersRebootExtended = tempXXP.oldAccessorySliders.GetAccessorySlidersRebootExtended();

                paintPriority = PaintPriority.GetDefault();

                FaceFIGR face = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetClassicFaceData(face);
                }
            }

            public xxpGeneralReboot(XXPV7 tempXXP)
            {
                xxpVersion = 12;

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
                if (baseDOC.race == 3)
                {
                    byte[] eyes = BitConverter.GetBytes(baseSLCT.eyePart);
                    baseSLCT.eyePart = eyes[0];
                    leftEyePart = eyes[1];
                }
                else
                {
                    leftEyePart = baseSLCT.eyePart;
                }

                //Conditionally assign premade expressions based on gender
                SetDefaultExpressions();

                accessorySlidersRebootExtended = tempXXP.accessorySliders.GetAccessorySlidersRebootExtended();

                paintPriority = PaintPriority.GetDefault();

                FaceFIGR face = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetClassicFaceData(face);
                }
            }

            public xxpGeneralReboot(XXPV9 tempXXP)
            {
                xxpVersion = 12;

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

                accessorySlidersRebootExtended = tempXXP.accessorySliders.GetAccessorySlidersRebootExtended();

                paintPriority = PaintPriority.GetDefault();

                FaceFIGR face = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetClassicFaceData(face);
                }
            }

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
                hornVerts = tempXXP.horns;
                eyeSize = tempXXP.eyeSize;
                eyeHorizontalPosition = tempXXP.eyeHorizontalPosition;
                neckAngle = tempXXP.neckAngle;

                ngsCOL2 = tempXXP.ngsCOL2;

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;
                leftEyePart = tempXXP.leftEyePart;

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySlidersRebootExtended = tempXXP.accessorySlidersReboot.GetRebootExtendedSliders();

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

                accessoryMiscDataExtended = tempXXP.accessoryMiscData.GetAccessoryMiscExtended();

                FaceFIGR face = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetClassicFaceData(face);
                }
            }

            public xxpGeneralReboot(XXPV11 tempXXP)
            {
                xxpVersion = 11;

                baseDOC = tempXXP.baseDOC;
                skinVariant = tempXXP.skinVariant;
                eyebrowDensity = tempXXP.eyebrowDensity;
                cmlVariant = tempXXP.cmlVariant;

                baseFIGR = tempXXP.baseFIGR;
                neckVerts = tempXXP.neckVerts;
                waistVerts = tempXXP.waistVerts;

                hands = tempXXP.hands;
                hornVerts = tempXXP.horns;
                eyeSize = tempXXP.eyeSize;
                eyeHorizontalPosition = tempXXP.eyeHorizontalPosition;
                neckAngle = tempXXP.neckAngle;

                ngsCOL2 = tempXXP.ngsCOL2;

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;
                leftEyePart = tempXXP.leftEyePart;

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySlidersRebootExtended = tempXXP.accessorySlidersReboot.GetRebootExtendedSliders();

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

                accessoryMiscDataExtended = tempXXP.accessoryMiscData.GetAccessoryMiscExtended();

                FaceFIGR face = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetClassicFaceData(face);
                }
            }

            public xxpGeneralReboot(XXPV12 tempXXP)
            {
                xxpVersion = 12;

                baseDOC = tempXXP.baseDOC;
                skinVariant = tempXXP.skinVariant;
                eyebrowDensity = tempXXP.eyebrowDensity;
                cmlVariant = tempXXP.cmlVariant;

                baseFIGR = tempXXP.baseFIGR;
                neckVerts = tempXXP.neckVerts;
                waistVerts = tempXXP.waistVerts;

                hands = tempXXP.hands;
                hornVerts = tempXXP.horns;
                eyeSize = tempXXP.eyeSize;
                eyeHorizontalPosition = tempXXP.eyeHorizontalPosition;
                neckAngle = tempXXP.neckAngle;

                classicFace = tempXXP.classicFace;

                ngsCOL2 = tempXXP.ngsCOL2;

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;
                leftEyePart = tempXXP.leftEyePart;

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySlidersRebootExtended = tempXXP.accessorySlidersReboot.GetRebootExtendedSliders();

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

                accessoryMiscDataExtended = tempXXP.accessoryMiscData.GetAccessoryMiscExtended();
            }

            public xxpGeneralReboot(XXPV13 tempXXP)
            {
                xxpVersion = 13;

                baseDOC = tempXXP.baseDOC;
                skinVariant = tempXXP.skinVariant;
                eyebrowDensity = tempXXP.eyebrowDensity;
                cmlVariant = tempXXP.cmlVariant;

                baseFIGR = tempXXP.baseFIGR;
                neckVerts = tempXXP.neckVerts;
                waistVerts = tempXXP.waistVerts;

                hands = tempXXP.hands;
                hornVerts = tempXXP.horns;
                eyeSize = tempXXP.eyeSize;
                eyeHorizontalPosition = tempXXP.eyeHorizontalPosition;
                neckAngle = tempXXP.neckAngle;

                classicFace = tempXXP.classicFace;

                ngsCOL2 = tempXXP.ngsCOL2;

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;
                leftEyePart = tempXXP.leftEyePart;

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySlidersRebootExtended = tempXXP.accessorySlidersReboot.GetRebootExtendedSliders();

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

                accessoryMiscDataExtended = tempXXP.accessoryMiscData.GetAccessoryMiscExtended();

                castColorIds = tempXXP.castColorIds;
            }

            public xxpGeneralReboot(XXPV14 tempXXP)
            {
                xxpVersion = 14;

                baseDOC = tempXXP.baseDOC;
                skinVariant = tempXXP.skinVariant;
                eyebrowDensity = tempXXP.eyebrowDensity;
                cmlVariant = tempXXP.cmlVariant;

                baseFIGR = tempXXP.baseFIGR;
                neckVerts = tempXXP.neckVerts;
                waistVerts = tempXXP.waistVerts;

                hands = tempXXP.hands;
                hornVerts = tempXXP.horns;
                eyeSize = tempXXP.eyeSize;
                eyeHorizontalPosition = tempXXP.eyeHorizontalPosition;
                neckAngle = tempXXP.neckAngle;

                classicFace = tempXXP.classicFace;

                ngsCOL2 = tempXXP.ngsCOL2;

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;
                leftEyePart = tempXXP.leftEyePart;

                baseSLCTNGS = tempXXP.baseSLCTNGS;

                accessorySlidersRebootExtended = tempXXP.accessorySlidersReboot.GetRebootExtendedSliders();

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

                accessoryMiscDataExtended = tempXXP.accessoryMiscData.GetAccessoryMiscExtended();

                castColorIds = tempXXP.castColorIds;

                celShadingIsEnabled = tempXXP.celShadingIsEnabled;
            }

            public xxpGeneralReboot(XXPV15 tempXXP)
            {
                xxpVersion = 15;

                baseDOC = tempXXP.baseDOC;
                skinVariant = tempXXP.skinVariant;
                eyebrowDensity = tempXXP.eyebrowDensity;
                cmlVariant = tempXXP.cmlVariant;

                baseFIGR = tempXXP.baseFIGR;
                neckVerts = tempXXP.neckVerts;
                waistVerts = tempXXP.waistVerts;

                hands = tempXXP.hands;
                hornVerts = tempXXP.horns;
                eyeSize = tempXXP.eyeSize;
                eyeHorizontalPosition = tempXXP.eyeHorizontalPosition;
                neckAngle = tempXXP.neckAngle;

                classicFace = tempXXP.classicFace;

                ngsCOL2 = tempXXP.ngsCOL2;

                baseSLCT = tempXXP.baseSLCT;
                baseSLCT2 = tempXXP.baseSLCT2;
                leftEyePart = tempXXP.leftEyePart;

                baseSLCTNGS = tempXXP.baseSLCTNGS;
                
                slctNGSExtended = tempXXP.slctNGSExtended;

                accessorySlidersRebootExtended = tempXXP.accessorySlidersRebootExtended;

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

                accessoryMiscDataExtended = tempXXP.accessoryMiscDataExtended;

                castColorIds = tempXXP.castColorIds;

                celShadingIsEnabled = tempXXP.celShadingIsEnabled;
            }

            public XXPV2 GetXXPV2()
            {
                FaceFIGR classicFace = GetClassicFaceData();
                FaceFIGR ngsFace = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(classicFace);
                }
                XXPV2 xxpv2 = new();

                xxpv2.baseDOC = baseDOC;
                xxpv2.baseFIGR = baseFIGR;
                xxpv2.baseFIGR.ToOld();
                xxpv2.baseCOLR = ColorConversion.COL2ToCOLR(ngsCOL2, baseDOC.race);
                xxpv2.baseSLCT = baseSLCT;

                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(ngsFace);
                }
                return xxpv2;
            }

            public XXPV5 GetXXPV5()
            {
                FaceFIGR classicFace = GetClassicFaceData();
                FaceFIGR ngsFace = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(classicFace);
                }
                XXPV5 xxpv5 = new();

                xxpv5.baseDOC = baseDOC;
                xxpv5.baseFIGR = baseFIGR;
                xxpv5.baseFIGR.ToOld();
                xxpv5.baseCOLR = ColorConversion.COL2ToCOLR(ngsCOL2, baseDOC.race);
                xxpv5.baseSLCT = baseSLCT;
                xxpv5.baseSLCT2 = baseSLCT2;
                xxpv5.oldPosSliders = accessorySlidersRebootExtended.GetOldAccessoryPositionSliders();

                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(ngsFace);
                }
                return xxpv5;
            }

            public XXPV6 GetXXPV6()
            {
                FaceFIGR classicFace = GetClassicFaceData();
                FaceFIGR ngsFace = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(classicFace);
                }
                XXPV6 xxpv6 = new();

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
                xxpv6.oldAccessorySliders = accessorySlidersRebootExtended.GetOldAccessorySliders();
                xxpv6.paintPriority = paintPriority;

                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(ngsFace);
                }
                return xxpv6;
            }

            public XXPV9 GetXXPV9()
            {
                FaceFIGR classicFace = GetClassicFaceData();
                FaceFIGR ngsFace = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(classicFace);
                }
                XXPV9 xxpv9 = new();

                xxpv9.baseDOC = baseDOC;
                xxpv9.skinVariant = 2; //Hack to deal with limitations of backwards conversion
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

                xxpv9.baseCOLR = ColorConversion.COL2ToCOLR(ngsCOL2, 2);
                xxpv9.baseSLCT = baseSLCT;
                xxpv9.baseSLCT2 = baseSLCT2;
                xxpv9.leftEyePart = leftEyePart;
                xxpv9.accessorySliders = accessorySlidersRebootExtended.GetClassicAccessorySliders();
                xxpv9.paintPriority = paintPriority;

                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(ngsFace);
                }
                return xxpv9;
            }

            public XXPV10 GetXXPV10()
            {
                FaceFIGR classicFace = GetClassicFaceData();
                FaceFIGR ngsFace = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(classicFace);
                }
                XXPV10 tempXXP = new();

                tempXXP.baseDOC = baseDOC;
                tempXXP.skinVariant = skinVariant;
                tempXXP.eyebrowDensity = eyebrowDensity;
                tempXXP.cmlVariant = cmlVariant;
                tempXXP.baseFIGR = baseFIGR;
                tempXXP.neckVerts = neckVerts;
                tempXXP.waistVerts = waistVerts;
                tempXXP.hands = hands;
                tempXXP.horns = hornVerts;
                tempXXP.eyeSize = eyeSize;
                tempXXP.eyeHorizontalPosition = eyeHorizontalPosition;
                tempXXP.neckAngle = neckAngle;
                tempXXP.ngsCOL2 = ngsCOL2;
                tempXXP.baseSLCT = baseSLCT;
                tempXXP.baseSLCT2 = baseSLCT2;
                tempXXP.leftEyePart = leftEyePart;
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySlidersReboot = accessorySlidersRebootExtended.GetRebootAccessorySliders();
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
                tempXXP.accessoryMiscData = accessoryMiscDataExtended.GetAccessoryMisc();

                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(ngsFace);
                }
                return tempXXP;
            }

            public XXPV11 GetXXPV11()
            {
                FaceFIGR classicFace = GetClassicFaceData();
                FaceFIGR ngsFace = GetNGSFaceData();
                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(classicFace);
                }
                XXPV11 tempXXP = new();

                tempXXP.baseDOC = baseDOC;
                tempXXP.skinVariant = skinVariant;
                tempXXP.eyebrowDensity = eyebrowDensity;
                tempXXP.cmlVariant = cmlVariant;
                tempXXP.baseFIGR = baseFIGR;
                tempXXP.neckVerts = neckVerts;
                tempXXP.waistVerts = waistVerts;
                tempXXP.hands = hands;
                tempXXP.horns = hornVerts;
                tempXXP.eyeSize = eyeSize;
                tempXXP.eyeHorizontalPosition = eyeHorizontalPosition;
                tempXXP.neckAngle = neckAngle;
                tempXXP.ngsCOL2 = ngsCOL2;
                tempXXP.baseSLCT = baseSLCT;
                tempXXP.baseSLCT2 = baseSLCT2;
                tempXXP.leftEyePart = leftEyePart;
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySlidersReboot = accessorySlidersRebootExtended.GetRebootAccessorySliders();
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
                tempXXP.accessoryMiscData = accessoryMiscDataExtended.GetAccessoryMisc();

                if (baseSLCT.faceTypePart < 100000)
                {
                    SetNGSFaceData(ngsFace);
                }
                return tempXXP;
            }

            public XXPV12 GetXXPV12()
            {
                XXPV12 tempXXP = new();

                tempXXP.baseDOC = baseDOC;
                tempXXP.skinVariant = skinVariant;
                tempXXP.eyebrowDensity = eyebrowDensity;
                tempXXP.cmlVariant = cmlVariant;
                tempXXP.baseFIGR = baseFIGR;
                tempXXP.neckVerts = neckVerts;
                tempXXP.waistVerts = waistVerts;
                tempXXP.hands = hands;
                tempXXP.horns = hornVerts;
                tempXXP.eyeSize = eyeSize;
                tempXXP.eyeHorizontalPosition = eyeHorizontalPosition;
                tempXXP.neckAngle = neckAngle;
                tempXXP.classicFace = classicFace;
                tempXXP.ngsCOL2 = ngsCOL2;
                tempXXP.baseSLCT = baseSLCT;
                tempXXP.baseSLCT2 = baseSLCT2;
                tempXXP.leftEyePart = leftEyePart;
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySlidersReboot = accessorySlidersRebootExtended.GetRebootAccessorySliders();
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
                tempXXP.accessoryMiscData = accessoryMiscDataExtended.GetAccessoryMisc();

                return tempXXP;
            }

            public XXPV13 GetXXPV13()
            {
                XXPV13 tempXXP = new();

                tempXXP.baseDOC = baseDOC;
                tempXXP.skinVariant = skinVariant;
                tempXXP.eyebrowDensity = eyebrowDensity;
                tempXXP.cmlVariant = cmlVariant;
                tempXXP.baseFIGR = baseFIGR;
                tempXXP.neckVerts = neckVerts;
                tempXXP.waistVerts = waistVerts;
                tempXXP.hands = hands;
                tempXXP.horns = hornVerts;
                tempXXP.eyeSize = eyeSize;
                tempXXP.eyeHorizontalPosition = eyeHorizontalPosition;
                tempXXP.neckAngle = neckAngle;
                tempXXP.classicFace = classicFace;
                tempXXP.ngsCOL2 = ngsCOL2;
                tempXXP.baseSLCT = baseSLCT;
                tempXXP.baseSLCT2 = baseSLCT2;
                tempXXP.leftEyePart = leftEyePart;
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySlidersReboot = accessorySlidersRebootExtended.GetRebootAccessorySliders();
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
                tempXXP.accessoryMiscData = accessoryMiscDataExtended.GetAccessoryMisc();
                tempXXP.castColorIds = castColorIds;

                return tempXXP;
            }

            public XXPV14 GetXXPV14()
            {
                XXPV14 tempXXP = new();

                tempXXP.baseDOC = baseDOC;
                tempXXP.skinVariant = skinVariant;
                tempXXP.eyebrowDensity = eyebrowDensity;
                tempXXP.cmlVariant = cmlVariant;
                tempXXP.baseFIGR = baseFIGR;
                tempXXP.neckVerts = neckVerts;
                tempXXP.waistVerts = waistVerts;
                tempXXP.hands = hands;
                tempXXP.horns = hornVerts;
                tempXXP.eyeSize = eyeSize;
                tempXXP.eyeHorizontalPosition = eyeHorizontalPosition;
                tempXXP.neckAngle = neckAngle;
                tempXXP.classicFace = classicFace;
                tempXXP.ngsCOL2 = ngsCOL2;
                tempXXP.baseSLCT = baseSLCT;
                tempXXP.baseSLCT2 = baseSLCT2;
                tempXXP.leftEyePart = leftEyePart;
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.accessorySlidersReboot = accessorySlidersRebootExtended.GetRebootAccessorySliders();
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
                tempXXP.accessoryMiscData = accessoryMiscDataExtended.GetAccessoryMisc();
                tempXXP.castColorIds = castColorIds;
                tempXXP.celShadingIsEnabled = celShadingIsEnabled;

                return tempXXP;
            }

            public XXPV15 GetXXPV15()
            {
                XXPV15 tempXXP = new();

                tempXXP.baseDOC = baseDOC;
                tempXXP.skinVariant = skinVariant;
                tempXXP.eyebrowDensity = eyebrowDensity;
                tempXXP.cmlVariant = cmlVariant;
                tempXXP.baseFIGR = baseFIGR;
                tempXXP.neckVerts = neckVerts;
                tempXXP.waistVerts = waistVerts;
                tempXXP.hands = hands;
                tempXXP.horns = hornVerts;
                tempXXP.eyeSize = eyeSize;
                tempXXP.eyeHorizontalPosition = eyeHorizontalPosition;
                tempXXP.neckAngle = neckAngle;
                tempXXP.classicFace = classicFace;
                tempXXP.ngsCOL2 = ngsCOL2;
                tempXXP.baseSLCT = baseSLCT;
                tempXXP.baseSLCT2 = baseSLCT2;
                tempXXP.leftEyePart = leftEyePart;
                tempXXP.baseSLCTNGS = baseSLCTNGS;
                tempXXP.slctNGSExtended = slctNGSExtended;
                tempXXP.accessorySlidersRebootExtended = accessorySlidersRebootExtended;
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
                tempXXP.accessoryMiscDataExtended = accessoryMiscDataExtended;
                tempXXP.castColorIds = castColorIds;
                tempXXP.celShadingIsEnabled = celShadingIsEnabled;

                return tempXXP;
            }

            private void SetDefaultExpressions()
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

            public FaceFIGR GetNGSFaceData()
            {
                FaceFIGR faceFIGR = new();

                faceFIGR.headVerts = baseFIGR.headVerts;
                faceFIGR.faceShapeVerts = baseFIGR.faceShapeVerts;
                faceFIGR.eyeShapeVerts = baseFIGR.eyeShapeVerts;
                faceFIGR.noseHeightVerts = baseFIGR.noseHeightVerts;
                faceFIGR.noseShapeVerts = baseFIGR.noseShapeVerts;
                faceFIGR.mouthVerts = baseFIGR.mouthVerts;
                faceFIGR.ear_hornVerts = baseFIGR.ear_hornVerts;
                faceFIGR.neckVerts = neckVerts;
                faceFIGR.hornVerts = hornVerts;
                faceFIGR.unkFaceVerts = new Vec3Int();

                return faceFIGR;
            }

            public FaceFIGR GetClassicFaceData()
            {
                return classicFace;
            }

            public void SetNGSFaceData(FaceFIGR faceData)
            {
                baseFIGR.headVerts = faceData.headVerts;
                baseFIGR.faceShapeVerts = faceData.faceShapeVerts;
                baseFIGR.eyeShapeVerts = faceData.eyeShapeVerts;
                baseFIGR.noseHeightVerts = faceData.noseHeightVerts;
                baseFIGR.noseShapeVerts = faceData.noseShapeVerts;
                baseFIGR.mouthVerts = faceData.mouthVerts;
                baseFIGR.ear_hornVerts = faceData.ear_hornVerts;
                neckVerts = faceData.neckVerts;
                hornVerts = faceData.hornVerts;
            }

            public void SetClassicFaceData(FaceFIGR faceData)
            {
                classicFace = faceData;
            }

            public void GetXXPWildcards(out string letterOne, out string letterTwo)
            {
                switch (baseDOC.gender)
                {
                    case 0:
                        letterOne = "m";
                        break;
                    case 1:
                        letterOne = "f";
                        break;
                    default:
                        letterOne = "m";
                        break;
                }

                switch (baseDOC.race)
                {
                    case 0:
                        letterTwo = "h";
                        break;
                    case 1:
                        letterTwo = "n";
                        break;
                    case 2:
                        letterTwo = "c";
                        break;
                    case 3:
                        letterTwo = "d";
                        break;
                    default:
                        letterTwo = "h";
                        break;
                }
            }

            public byte[] GetBytes()
            {
                switch (xxpVersion)
                {
                    case 2:
                        return DataHelpers.ConvertStruct(GetXXPV2());
                    case 5:
                        return DataHelpers.ConvertStruct(GetXXPV5());
                    case 6:
                        return DataHelpers.ConvertStruct(GetXXPV6());
                    case 9:
                        return DataHelpers.ConvertStruct(GetXXPV9());
                    case 10:
                        return DataHelpers.ConvertStruct(GetXXPV10());
                    case 11:
                        return DataHelpers.ConvertStruct(GetXXPV11());
                    case 12:
                        return DataHelpers.ConvertStruct(GetXXPV12());
                    case 13:
                        return DataHelpers.ConvertStruct(GetXXPV13());
                    case 14:
                        return DataHelpers.ConvertStruct(GetXXPV14());
                    case 15:
                        return DataHelpers.ConvertStruct(GetXXPV15());
                    default:
                        return DataHelpers.ConvertStruct(GetXXPV15());
                }
                throw new NotImplementedException();
            }
        }

    }
}
