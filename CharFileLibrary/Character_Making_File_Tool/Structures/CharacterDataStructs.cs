using static Character_Making_File_Tool.Vector3Int;

namespace Character_Making_File_Tool
{
    public class CharacterDataStructs
    {
        public struct XXPHeader
        {
            public uint xxpVersion;  //Version of the xxp
            public uint bodySize; //Size of the data after the header
            public uint fileHash; //Crc32 hash of the encrypted file contents after the header
            public uint headerEnd; //0 in all known instances
        }

        public struct BaseDOC //0xC bytes
        {
            //Contains important info regarding the type of character.Also has random info that doesn't use int or uint data types. 
            public uint race;
            public uint gender;
            public float muscleMass; //float from 0 to 60000 
        }

        public struct BaseFIGR //0x132 bytes
        {
            //Slider vertex positions. 10000 to -10000. Center Vert Y, Side Vert X, Side Vert Y
            public Vec3Int bodyVerts;
            public Vec3Int armVerts;
            public Vec3Int legVerts;
            public Vec3Int bustVerts;

            public Vec3Int headVerts;
            public Vec3Int faceShapeVerts;
            public Vec3Int eyeShapeVerts;
            public Vec3Int noseHeightVerts;

            public Vec3Int noseShapeVerts;
            public Vec3Int mouthVerts;
            public Vec3Int ear_hornVerts;

            public void ToNGS()
            {
                bodyVerts.ToNGSSliders();
                armVerts.ToNGSSliders();
                legVerts.ToNGSSliders();
                bustVerts.ToNGSSliders();

                headVerts.ToNGSSliders();
                faceShapeVerts.ToNGSSliders();
                eyeShapeVerts.ToNGSSliders();
                noseHeightVerts.ToNGSSliders();

                noseShapeVerts.ToNGSSliders();
                mouthVerts.ToNGSSliders();
                ear_hornVerts.ToNGSSliders();
            }

            public void ToOld()
            {
                bodyVerts.ToOldSliders();
                armVerts.ToOldSliders();
                legVerts.ToOldSliders();
                bustVerts.ToOldSliders();

                headVerts.ToOldSliders();
                faceShapeVerts.ToOldSliders();
                eyeShapeVerts.ToOldSliders();
                noseHeightVerts.ToOldSliders();

                noseShapeVerts.ToOldSliders();
                mouthVerts.ToOldSliders();
                ear_hornVerts.ToOldSliders();
            }
        }

        public struct BaseFIGR2 //0x60 bytes
        {
            //FIGR values added in ep4 to increase customization and add separate fleshy and cast sliders
            public Vec3Int neckVerts;
            public Vec3Int waistVerts;
            public Vec3Int body2Verts;
            public Vec3Int arm2Verts;

            public Vec3Int leg2Verts;
            public Vec3Int bust2Verts;
            public Vec3Int neck2Verts;
            public Vec3Int waist2Verts;
            public void ToNGS()
            {
                neckVerts.ToNGSSliders();
                waistVerts.ToNGSSliders();
                body2Verts.ToNGSSliders();
                arm2Verts.ToNGSSliders();

                leg2Verts.ToNGSSliders();
                bust2Verts.ToNGSSliders();
                neck2Verts.ToNGSSliders();
                waist2Verts.ToNGSSliders();
            }

            public void ToOld()
            {
                neckVerts.ToOldSliders();
                waistVerts.ToOldSliders();
                body2Verts.ToOldSliders();
                arm2Verts.ToOldSliders();

                leg2Verts.ToOldSliders();
                bust2Verts.ToOldSliders();
                neck2Verts.ToOldSliders();
                waist2Verts.ToOldSliders();
            }
        }

        public struct BaseCOLR //0x60
        {
            //Color vertex positions. 0 to 60000 or 10000 to -10000. X, Y, and Saturation
            public Vec3Int outer_MainColorVerts; //Changed to copy of Main Color on changing body part allegedly as well as Ou color
            public Vec3Int costumeColorVerts;
            public Vec3Int mainColor_hair2Verts;
            public Vec3Int subColor1Verts;

            public Vec3Int skinSubColor2Verts;
            public Vec3Int subColor3_leftEye_castHair2Verts;
            public Vec3Int rightEye_EyesVerts;
            public Vec3Int hairVerts;
        }

        public struct BaseSLCT //0x40
        {
            //.cmx (Character Making Index) piece references.
            public uint costumePart;
            public uint bodyPaintPart;
            public uint stickerPart;
            public uint eyePart;     //In files prior to v9, assume byte 0 and byte 1 are right and left eye respectively for dumans 

            public uint eyebrowPart;
            public uint eyelashPart;
            public uint faceTypePart;
            public uint faceTexPart;

            public uint makeup1Part;
            public uint hairPart;
            public uint acc1Part;
            public uint acc2Part;

            public uint acc3Part;
            public uint makeup2Part;
            public uint legPart;
            public uint armPart;
        }

        public struct BaseSLCT2 //0x10
        {
            // .cmx bits added for EP4
            public uint acc4Part;
            public uint basewearPart;
            public uint innerwearPart;
            public uint bodyPaint2Part;
        }

        public struct PaintPriority
        {
            //Body Paint Priority order
            //Innerwear is 0x0, bodypaint1 is 0x1, bodypaint2 is 0x2
            public ushort priority1;
            public ushort priority2;
            public ushort priority3;

            public static PaintPriority GetDefault()
            {
                PaintPriority paintPriority = new PaintPriority();
                paintPriority.priority1 = 0;
                paintPriority.priority2 = 1;
                paintPriority.priority3 = 2;

                return paintPriority;
            }
        }

        public struct FaceExpressionV10
        {
            public sbyte irisSize;
            public sbyte leftEyebrowVertical;
            public sbyte leftMouthVertical;
            public sbyte rightEyebrowVertical;

            public sbyte rightMouthVertical;
            public sbyte eyeCorner;
            public sbyte leftEyelidVertical;
            public sbyte leftEyebrowExpression;

            public sbyte rightEyelidVertical;
            public sbyte rightEyebrowExpression;
            public sbyte mouthA;
            public sbyte mouthI;

            public sbyte mouthU;
            public sbyte mouthE;
            public sbyte mouthO;
            public sbyte leftEyebrowVerticalUnused;

            public sbyte rightEyebrowVerticalUnused;
            public sbyte tongue;

            public static FaceExpressionV10 CreateExpression(sbyte[] raw)
            {
                FaceExpressionV10 exp = new FaceExpressionV10();

                exp.irisSize = raw[0];
                exp.leftEyebrowVertical = raw[1];
                exp.leftMouthVertical = raw[2];
                exp.rightEyebrowVertical = raw[3];

                exp.rightMouthVertical = raw[4];
                exp.eyeCorner = raw[5];
                exp.leftEyelidVertical = raw[6];
                exp.leftEyebrowExpression = raw[7];

                exp.rightEyelidVertical = raw[8];
                exp.rightEyebrowExpression = raw[9];
                exp.mouthA = raw[0xA];
                exp.mouthI = raw[0xB];

                exp.mouthU = raw[0xC];
                exp.mouthE = raw[0xD];
                exp.mouthO = raw[0xE];
                exp.leftEyebrowVerticalUnused = raw[0xF];

                exp.rightEyebrowVerticalUnused = raw[0x10];
                exp.tongue = raw[0x11];

                return exp;
            }
        }

        public struct FaceExpressionV11
        {
            public FaceExpressionV10 expStruct;
            public sbyte tongueVertical;
            public sbyte tongueHorizontal;

            public static FaceExpressionV11 CreateExpression(sbyte[] raw)
            {
                FaceExpressionV11 obj = new();
                FaceExpressionV10 exp = new();

                exp.irisSize = raw[0];
                exp.leftEyebrowVertical = raw[1];
                exp.leftMouthVertical = raw[2];
                exp.rightEyebrowVertical = raw[3];

                exp.rightMouthVertical = raw[4];
                exp.eyeCorner = raw[5];
                exp.leftEyelidVertical = raw[6];
                exp.leftEyebrowExpression = raw[7];

                exp.rightEyelidVertical = raw[8];
                exp.rightEyebrowExpression = raw[9];
                exp.mouthA = raw[0xA];
                exp.mouthI = raw[0xB];

                if (raw.Length > 0xC)
                {
                    exp.mouthU = raw[0xC];
                    exp.mouthE = raw[0xD];
                    exp.mouthO = raw[0xE];
                    exp.leftEyebrowVerticalUnused = raw[0xF];

                    exp.rightEyebrowVerticalUnused = raw[0x10];
                    exp.tongue = raw[0x11];
                }

                if (raw.Length > 0x12)
                {
                    obj.tongueVertical = raw[0x12];
                    obj.tongueHorizontal = raw[0x13];
                }

                obj.expStruct = exp;

                return obj;
            }
            public static FaceExpressionV11 CreateExpression(FaceExpressionV10 exp)
            {
                FaceExpressionV11 obj = new();

                obj.expStruct = exp;
                obj.tongueVertical = 0;
                obj.tongueHorizontal = 0;

                return obj;
            }
        }

        public struct FaceExpressionV12
        {
            public FaceExpressionV11 expStruct;
            public sbyte mouthOpenClose;
            public sbyte mouthCornerPosition;
            public sbyte mouthCornerShape;
            public sbyte upperLipShape;

            public sbyte lowerLipShape;
            public sbyte upperLowerLipRatio;
            public sbyte teethOpenClose;
            public sbyte teethVerticalPosition;

            public sbyte lowerJawOpenClose;
            public sbyte eyelashAngle;
            public sbyte eyelashLength;
            public sbyte eyeShadows;

            public sbyte pupilShape;
            public sbyte lipSyncStrength;

            public static FaceExpressionV12 CreateExpression(sbyte[] raw)
            {
                FaceExpressionV12 exp12 = new();
                FaceExpressionV11 exp11 = new();
                FaceExpressionV10 exp10 = new();

                exp10.irisSize = raw[0];
                exp10.leftEyebrowVertical = raw[1];
                exp10.leftMouthVertical = raw[2];
                exp10.rightEyebrowVertical = raw[3];

                exp10.rightMouthVertical = raw[4];
                exp10.eyeCorner = raw[5];
                exp10.leftEyelidVertical = raw[6];
                exp10.leftEyebrowExpression = raw[7];

                exp10.rightEyelidVertical = raw[8];
                exp10.rightEyebrowExpression = raw[9];
                exp10.mouthA = raw[0xA];
                exp10.mouthI = raw[0xB];

                if (raw.Length > 0xC)
                {
                    exp10.mouthU = raw[0xC];
                    exp10.mouthE = raw[0xD];
                    exp10.mouthO = raw[0xE];
                    exp10.leftEyebrowVerticalUnused = raw[0xF];

                    exp10.rightEyebrowVerticalUnused = raw[0x10];
                    exp10.tongue = raw[0x11];
                }

                if (raw.Length > 0x12)
                {
                    exp11.tongueVertical = raw[0x12];
                    exp11.tongueHorizontal = raw[0x13];
                }

                if (raw.Length > 0x14)
                {
                    exp12.mouthOpenClose = raw[0x14];
                    exp12.mouthCornerPosition = raw[0x15];
                    exp12.mouthCornerShape = raw[0x16];
                    exp12.upperLipShape = raw[0x17];

                    exp12.lowerLipShape = raw[0x18];
                    exp12.upperLowerLipRatio = raw[0x19];
                    exp12.teethOpenClose = raw[0x1A];
                    exp12.teethVerticalPosition = raw[0x1B];
                    
                    exp12.lowerJawOpenClose = raw[0x1C];
                    exp12.eyelashAngle = raw[0x1D];
                    exp12.eyelashLength = raw[0x1E];
                    exp12.eyeShadows = raw[0x1F];

                    exp12.pupilShape = raw[0x20];
                    exp12.lipSyncStrength = raw[0x21];
                }

                exp11.expStruct = exp10;
                exp12.expStruct = exp11;

                return exp12;
            }
            public static FaceExpressionV12 CreateExpression(FaceExpressionV10 exp)
            {
                FaceExpressionV12 obj = new();
                FaceExpressionV11 exp11 = new();

                exp11.expStruct = exp;
                obj.expStruct = exp11;

                return obj;
            }
            public static FaceExpressionV12 CreateExpression(FaceExpressionV11 exp)
            {
                FaceExpressionV12 obj = new();

                obj.expStruct = exp;

                return obj;
            }
        }
    }
}
