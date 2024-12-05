using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.PSO2;
using AquaModelLibrary.Helpers.Readers;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static Character_Making_File_Tool.CharacterConstants;
using static Character_Making_File_Tool.CharacterDataStructs;
using static Character_Making_File_Tool.CharacterDataStructsReboot;

namespace Character_Making_File_Tool
{
    public class CMLHandler
    {
        public static CharacterHandlerReboot.xxpGeneralReboot ParseCML(byte[] bytes)
        {
            using (MemoryStream strm = new MemoryStream(bytes))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(strm))
            {
                return ParseCML(sr);
            }
        }
        public static CharacterHandlerReboot.xxpGeneralReboot ParseCML(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            CharacterHandlerReboot.xxpGeneralReboot xxp = new CharacterHandlerReboot.xxpGeneralReboot();

            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("cml\0") || type.Equals("eml\0"))
            {
                streamReader.Seek(0xC, SeekOrigin.Begin);
                //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                int headJunkSize = streamReader.Read<int>();

                streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += headJunkSize;
            }

            //Proceed based on file variant
            if (type.Equals("NIFL"))
            {
                MessageBox.Show("NIFL cml variant detected! Please report!");
            }
            else if (type.Equals("VTBF"))
            {
                return ParseVTBFCML(streamReader);
            }
            else
            {
                MessageBox.Show("Improper File Format!");
            }

            return null;
        }

        public static unsafe CharacterHandlerReboot.xxpGeneralReboot ParseVTBFCML(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            CharacterHandlerReboot.xxpGeneralReboot xxp = new();

            int dataEnd = (int)streamReader.BaseStream.Length;

            //Seek past vtbf tag
            streamReader.Seek(0x10, SeekOrigin.Current);          //VTBF + CMLP tags
            bool hasClassicFaceData = false; //Track if classic face data is stored in an NGS file. If not, this is a classic file and we copy the face values into the old slots.
            //Previous CML iterations had unique ids for each variable in the file. However now they simply have unique ids per tag type.
            //There often are multiple of the same tag type placed about the file, though these shouldn't ever overlap.
            while (streamReader.Position < dataEnd)
            {
                var data = VTBFMethods.ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "DOC ":
                        var doc = data[0];
                        TryGet(ref xxp.baseDOC.race, doc, 0x70);
                        TryGet(ref xxp.baseDOC.gender, doc, 0x71);
                        TryGet(ref xxp.baseDOC.muscleMass, doc, 0x72);
                        TryGet(ref xxp.cmlVariant, doc, 0x73);
                        TryGet(ref xxp.skinVariant, doc, 0x74);
                        TryGet(ref xxp.eyebrowDensity, doc, 0x75);
                        TryGet(ref xxp.xxpVersion, doc, 0xFF);
                        break;
                    case "FIGR":
                        var figr = data[0];
                        TryGetFromArray(ref xxp.baseFIGR.bodyVerts, figr, 0x0);
                        TryGetFromArray(ref xxp.baseFIGR.armVerts, figr, 0x1);
                        TryGetFromArray(ref xxp.baseFIGR.legVerts, figr, 0x2);
                        TryGetFromArray(ref xxp.baseFIGR.bustVerts, figr, 0x3);
                        TryGetFromArray(ref xxp.baseFIGR.headVerts, figr, 0x4);
                        TryGetFromArray(ref xxp.baseFIGR.faceShapeVerts, figr, 0x5);
                        TryGetFromArray(ref xxp.baseFIGR.eyeShapeVerts, figr, 0x6);
                        TryGetFromArray(ref xxp.baseFIGR.noseHeightVerts, figr, 0x7);
                        TryGetFromArray(ref xxp.baseFIGR.noseShapeVerts, figr, 0x8);
                        TryGetFromArray(ref xxp.baseFIGR.mouthVerts, figr, 0x9);
                        TryGetFromArray(ref xxp.baseFIGR.ear_hornVerts, figr, 0xA);
                        TryGetFromArray(ref xxp.neckVerts, figr, 0xC);
                        TryGetFromArray(ref xxp.waistVerts, figr, 0xD);
                        TryGetFromArray(ref xxp.hands, figr, 0x16);
                        TryGetFromArray(ref xxp.hornVerts, figr, 0x20);

                        //Technically this is the same struct as the above, but its values are used ingame much differently
                        if (figr.TryGetValue(0x21, out object extra))
                        {
                            int[] extraArr = ((int[][])extra)[0];
                            xxp.eyeSize = extraArr[0];
                            xxp.eyeHorizontalPosition = extraArr[1];
                            xxp.neckAngle = extraArr[2];
                        }

                        //Classic Face
                        if (figr.ContainsKey(0x30))
                        {
                            hasClassicFaceData = true;
                        }

                        TryGetFromArray(ref xxp.classicFace.headVerts, figr, 0x30);
                        TryGetFromArray(ref xxp.classicFace.faceShapeVerts, figr, 0x31);
                        TryGetFromArray(ref xxp.classicFace.eyeShapeVerts, figr, 0x32);
                        TryGetFromArray(ref xxp.classicFace.noseHeightVerts, figr, 0x33);
                        TryGetFromArray(ref xxp.classicFace.noseShapeVerts, figr, 0x34);
                        TryGetFromArray(ref xxp.classicFace.mouthVerts, figr, 0x35);
                        TryGetFromArray(ref xxp.classicFace.ear_hornVerts, figr, 0x36);
                        TryGetFromArray(ref xxp.classicFace.neckVerts, figr, 0x37);
                        TryGetFromArray(ref xxp.classicFace.hornVerts, figr, 0x38);
                        TryGetFromArray(ref xxp.classicFace.unkFaceVerts, figr, 0x39); //Might just be padding

                        if (xxp.xxpVersion == 0)
                        {
                            xxp.baseFIGR.ToNGS();
                        }
                        break;
                    case "OFST":
                        var ofst = data[0];
                        foreach (int key in ofst.Keys)
                        {
                            TryGetFromOFST(ref xxp.accessorySlidersRebootExtended, ofst, key);
                        }
                        break;
                    case "ACWK":
                        var acwk = data[0];
                        foreach (int key in acwk.Keys)
                        {
                            TryGetFromACWK(ref xxp.accessoryMiscDataExtended, acwk, key);
                        }
                        break;
                    case "COLR":
                        var colr = data[0];
                        BaseCOLR baseCOLR = new();
                        TryGetFromArray(ref baseCOLR.outer_MainColorVerts, colr, 0x27);
                        TryGetFromArray(ref baseCOLR.costumeColorVerts, colr, 0x20);
                        TryGetFromArray(ref baseCOLR.mainColor_hair2Verts, colr, 0x21);
                        TryGetFromArray(ref baseCOLR.subColor1Verts, colr, 0x22);
                        TryGetFromArray(ref baseCOLR.skinSubColor2Verts, colr, 0x23);
                        TryGetFromArray(ref baseCOLR.subColor3_leftEye_castHair2Verts, colr, 0x24);
                        TryGetFromArray(ref baseCOLR.rightEye_EyesVerts, colr, 0x25);
                        TryGetFromArray(ref baseCOLR.hairVerts, colr, 0x26);

                        xxp.ngsCOL2 = ColorConversion.COLRToCOL2(baseCOLR, xxp.baseDOC.race);
                        break;
                    case "COL2":
                        var col2 = data[0];
                        COL2 ngsCol2 = new COL2();
                        TryGetCOL2(ngsCol2.outerColor1, col2, 0x27);
                        TryGetCOL2(ngsCol2.baseColor1, col2, 0x20);
                        TryGetCOL2(ngsCol2.mainColor, col2, 0x21);
                        TryGetCOL2(ngsCol2.subColor1, col2, 0x22);

                        TryGetCOL2(ngsCol2.subColor2, col2, 0x23);
                        TryGetCOL2(ngsCol2.subColor3, col2, 0x24);
                        TryGetCOL2(ngsCol2.rightEyeColor, col2, 0x25);
                        TryGetCOL2(ngsCol2.hairColor1, col2, 0x26);

                        TryGetCOL2(ngsCol2.eyebrowColor, col2, 0x38);
                        TryGetCOL2(ngsCol2.eyelashColor, col2, 0x39);
                        TryGetCOL2(ngsCol2.skinColor1, col2, 0x35);
                        TryGetCOL2(ngsCol2.skinColor2, col2, 0x30);

                        TryGetCOL2(ngsCol2.baseColor2, col2, 0x31);
                        TryGetCOL2(ngsCol2.outerColor2, col2, 0x32);
                        TryGetCOL2(ngsCol2.innerColor1, col2, 0x33);
                        TryGetCOL2(ngsCol2.innerColor2, col2, 0x34);

                        TryGetCOL2(ngsCol2.leftEyeColor, col2, 0x36);
                        TryGetCOL2(ngsCol2.hairColor2, col2, 0x37);

                        xxp.ngsCOL2 = ngsCol2;
                        break;
                    case "SLCT":
                        var slct = data[0];

                        //BaseSLCT
                        TryGet(ref xxp.baseSLCT.costumePart, slct, 0x40);
                        TryGet(ref xxp.baseSLCT.bodyPaintPart, slct, 0x41);
                        TryGet(ref xxp.baseSLCT.stickerPart, slct, 0x42);
                        TryGet(ref xxp.baseSLCT.eyebrowPart, slct, 0x44);
                        TryGet(ref xxp.baseSLCT.eyelashPart, slct, 0x45);
                        TryGet(ref xxp.baseSLCT.faceTypePart, slct, 0x46);
                        TryGet(ref xxp.baseSLCT.faceTexPart, slct, 0x47);
                        TryGet(ref xxp.baseSLCT.makeup1Part, slct, 0x48);
                        TryGet(ref xxp.baseSLCT.hairPart, slct, 0x49);
                        TryGet(ref xxp.baseSLCT.acc1Part, slct, 0x50);
                        TryGet(ref xxp.baseSLCT.acc2Part, slct, 0x51);
                        TryGet(ref xxp.baseSLCT.acc3Part, slct, 0x52);
                        TryGet(ref xxp.baseSLCT.makeup2Part, slct, 0x53);
                        TryGet(ref xxp.baseSLCT.legPart, slct, 0x54);
                        TryGet(ref xxp.baseSLCT.armPart, slct, 0x55);

                        //BaseSLCT2
                        TryGet(ref xxp.baseSLCT2.acc4Part, slct, 0x56);
                        TryGet(ref xxp.baseSLCT2.basewearPart, slct, 0x57);
                        TryGet(ref xxp.baseSLCT2.innerwearPart, slct, 0x58);
                        TryGet(ref xxp.baseSLCT2.bodyPaint2Part, slct, 0x59);

                        //BaseSLCTNGS
                        TryGet(ref xxp.baseSLCTNGS.skinTextureSet, slct, 0x0);

                        TryGet(ref xxp.baseSLCTNGS.earsPart, slct, 0x1);
                        TryGet(ref xxp.baseSLCTNGS.teethPart, slct, 0x2);
                        TryGet(ref xxp.baseSLCTNGS.hornPart, slct, 0x3);
                        TryGet(ref xxp.baseSLCTNGS.acc5Part, slct, 0x4);

                        TryGet(ref xxp.baseSLCTNGS.acc6Part, slct, 0x5);
                        TryGet(ref xxp.baseSLCTNGS.acc7Part, slct, 0x6);
                        TryGet(ref xxp.baseSLCTNGS.acc8Part, slct, 0x7);
                        TryGet(ref xxp.baseSLCTNGS.acc9Part, slct, 0x8);

                        TryGet(ref xxp.baseSLCTNGS.acc10Part, slct, 0x9);
                        TryGet(ref xxp.baseSLCTNGS.acc11Part, slct, 0xA);
                        TryGet(ref xxp.baseSLCTNGS.acc12Part, slct, 0xB);

                        //Guesses on these
                        TryGet(ref xxp.slctNGSExtended.acc13Part, slct, 0xC);
                        TryGet(ref xxp.slctNGSExtended.acc14Part, slct, 0xD);
                        TryGet(ref xxp.slctNGSExtended.acc15Part, slct, 0xE);
                        TryGet(ref xxp.slctNGSExtended.acc16Part, slct, 0xF);
                        TryGet(ref xxp.slctNGSExtended.acc17Part, slct, 0x10);
                        TryGet(ref xxp.slctNGSExtended.acc18Part, slct, 0x11);

                        //Paint priority
                        TryGet(ref xxp.paintPriority.priority1, slct, 0x60);
                        TryGet(ref xxp.paintPriority.priority2, slct, 0x61);
                        TryGet(ref xxp.paintPriority.priority3, slct, 0x62);

                        //Split eye if needed
                        if (slct.ContainsKey(0x63))
                        {
                            TryGet(ref xxp.baseSLCT.eyePart, slct, 0x43);
                            TryGet(ref xxp.leftEyePart, slct, 0x63);
                        }
                        else if (slct.ContainsKey(0x43))
                        {
                            if (xxp.cmlVariant >= 0x9 || xxp.baseDOC.race != 0x3)
                            {
                                TryGet(ref xxp.baseSLCT.eyePart, slct, 0x43);
                                TryGet(ref xxp.leftEyePart, slct, 0x43);
                            }
                            else
                            {
                                var eyeBytes = BitConverter.GetBytes((int)slct[0x43]);
                                xxp.baseSLCT.eyePart = eyeBytes[0];
                                xxp.leftEyePart = eyeBytes[1];
                            }
                        }
                        break;
                    case "SLID":
                        var slid = data[0];
                        TryGet(ref xxp.ngsSLID.shoulderSize, slid, 0x0);
                        TryGet(ref xxp.ngsSLID.hairAdjust, slid, 0x1);
                        TryGet(ref xxp.ngsSLID.skinGloss, slid, 0x2);
                        TryGet(ref xxp.ngsSLID.mouthVertical, slid, 0x3);

                        TryGet(ref xxp.ngsSLID.eyebrowHoriz, slid, 0x4);
                        TryGet(ref xxp.ngsSLID.irisVertical, slid, 0x5);
                        TryGet(ref xxp.ngsSLID.facePaint1Opacity, slid, 0x6);
                        TryGet(ref xxp.ngsSLID.facePaint2Opacity, slid, 0x7);

                        TryGet(ref xxp.ngsSLID.shoulderVertical, slid, 0x8);
                        TryGet(ref xxp.ngsSLID.thighsAdjust, slid, 0x9);
                        TryGet(ref xxp.ngsSLID.calvesAdjust, slid, 0xA);
                        TryGet(ref xxp.ngsSLID.forearmsAdjust, slid, 0xB);

                        TryGet(ref xxp.ngsSLID.handThickness, slid, 0xC);
                        TryGet(ref xxp.ngsSLID.footSize, slid, 0xD);
                        TryGet(ref xxp.ngsSLID.int_32C, slid, 0xE); //May not actually exist
                        break;
                    case "MTON":
                        var mton = data[0];
                        TryGet(ref xxp.ngsMTON.int_330, mton, 0x0);
                        TryGet(ref xxp.ngsMTON.walkRunMotion, mton, 0x1);
                        TryGet(ref xxp.ngsMTON.swimMotion, mton, 0x2);
                        TryGet(ref xxp.ngsMTON.dashMotion, mton, 0x3);

                        TryGet(ref xxp.ngsMTON.glideMotion, mton, 0x4);
                        TryGet(ref xxp.ngsMTON.landingMotion, mton, 0x5);
                        TryGet(ref xxp.ngsMTON.idleMotion, mton, 0x6);
                        TryGet(ref xxp.ngsMTON.jumpMotion, mton, 0x7);
                        break;
                    case "VISI":
                        //VISI values are stored in the first byte of the int as bitflags. Why? I don't know!
                        var visi = data[0];
                        //Usually has 0xC0 and 0xC1. Unsure if 0xC1 is used.
                        if (visi.ContainsKey(0xC0))
                        {
                            byte[] ornaments = BitConverter.GetBytes((int)visi[0xC0]);
                            xxp.ngsVISI.hideBasewearOrnament1 = (ornaments[0] & 0b00000001) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideBasewearOrnament2 = (ornaments[0] & 0b00000010) > 0 ? 1 : 0;

                            xxp.ngsVISI.hideHeadPartOrnament = (ornaments[0] & 0b00000100) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideBodyPartOrnament = (ornaments[0] & 0b00001000) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideArmPartOrnament = (ornaments[0] & 0b00010000) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideLegPartOrnament = (ornaments[0] & 0b00100000) > 0 ? 1 : 0;

                            xxp.ngsVISI.hideOuterwearOrnament = (ornaments[0] & 0b01000000) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideInnerwear = (ornaments[0] & 0b10000000) > 0 ? 1 : 0;
                        }
                        break;
                    case "EXPR":
                        FaceExpressionV11[] expressions;
                        if (xxp.baseDOC.gender == 0)
                        {
                            expressions = CharacterStructConstants.defaultMaleExpressions;
                        }
                        else
                        {
                            expressions = CharacterStructConstants.defaultFemaleExpressions;
                        }

                        var expr = data[0];
                        xxp.faceNatural = TryGetEXPR(expr, 0x0, expressions[0]);
                        xxp.faceSmile = TryGetEXPR(expr, 0x1, expressions[1]);
                        xxp.faceAngry = TryGetEXPR(expr, 0x2, expressions[2]);
                        xxp.faceSad = TryGetEXPR(expr, 0x3, expressions[3]);

                        xxp.faceSus = TryGetEXPR(expr, 0x4, expressions[4]);
                        xxp.faceEyesClosed = TryGetEXPR(expr, 0x5, expressions[5]);
                        xxp.faceSmile2 = TryGetEXPR(expr, 0x6, expressions[6]);
                        xxp.faceWink = TryGetEXPR(expr, 0x7, expressions[7]);

                        xxp.faceUnused1 = TryGetEXPR(expr, 0x1, expressions[8]);
                        xxp.faceUnused2 = TryGetEXPR(expr, 0x9, expressions[9]);
                        break;
                    default:
                        //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                        if (data == null)
                        {
                            CopyToClassicFace(xxp, hasClassicFaceData);
                            return xxp;
                        }
                        throw new System.Exception($"Unexpected tag at {streamReader.Position.ToString("X")}! {tagType} Please report!");
                }
            }
            CopyToClassicFace(xxp, hasClassicFaceData);

            return xxp;
        }

        private static unsafe void CopyToClassicFace(CharacterHandlerReboot.xxpGeneralReboot xxp, bool hasClassicFaceData)
        {
            //At least up to xxp v13 era, cmls seem to still use the main face as the primary face, even though they have alt face slots. This will fix NPCs that use old type faces for v13+ xxp
            if (!hasClassicFaceData)
            {
                xxp.classicFace = xxp.GetNGSFaceData();
            }
        }

        public static void TryGet<T>(ref T value, Dictionary<int, object> dict, int key)
        {
            if (dict.TryGetValue(key, out object dictValue))
            {
                value = (T)Convert.ChangeType(dictValue, typeof(T));
            }
        }

        public static void TryGetFromArray(ref Vector3Int.Vec3Int value, Dictionary<int, object> dict, int key)
        {
            if (dict.TryGetValue(key, out object dictValue))
            {
                value = Vector3Int.Vec3Int.CreateVec3Int(((int[][])dictValue)[0]);
            }
        }

        public static unsafe void TryGetFromACWK(ref CharacterDataStructsReboot.AccessoryMiscExtended misc, Dictionary<int, object> dict, int key)
        {
            if (dict.TryGetValue(key, out object dictValue))
            {
                misc.accessoryAttach[key] = (byte)((int[][])dictValue)[0][0];
                misc.accessoryColorChoices[key * 2] = (byte)((int[][])dictValue)[0][1];
                misc.accessoryColorChoices[key * 2 + 1] = (byte)((int[][])dictValue)[0][2];
            }
        }

        public static unsafe void TryGetFromOFST(ref CharacterDataStructsReboot.AccessorySlidersRebootExtended sliders, Dictionary<int, object> dict, int key)
        {
            int set = key / 0x10;
            int idMult = key % 0x10;
            if (dict.TryGetValue(key, out object dictValue))
            {
                sbyte[] sbytes = (sbyte[])dictValue;
                switch (set)
                {
                    //Pos
                    case 0xB:
                        sliders.scaleSliders[idMult * 3] = sbytes[0];
                        sliders.scaleSliders[idMult * 3 + 1] = sbytes[1];
                        sliders.scaleSliders[idMult * 3 + 2] = sbytes[2];
                        break;
                    //Rot
                    case 0xA:
                        sliders.rotSliders[idMult * 3] = sbytes[0];
                        sliders.rotSliders[idMult * 3 + 1] = sbytes[1];
                        sliders.rotSliders[idMult * 3 + 2] = sbytes[2];
                        break;
                    //Scale
                    case 0x9:
                        sliders.posSliders[idMult * 3] = sbytes[0];
                        sliders.posSliders[idMult * 3 + 1] = sbytes[1];
                        sliders.posSliders[idMult * 3 + 2] = sbytes[2];
                        break;
                    default:
                        MessageBox.Show($"Unexpected accessory transform type: {set:X}");
                        break;

                }
            }
        }

        public static unsafe void TryGetCOL2(byte* value, Dictionary<int, object> dict, int key)
        {
            if (dict.TryGetValue(key, out object dictValue))
            {
                var bytes = BitConverter.GetBytes((int)dictValue);
                for (int i = 0; i < 4; i++)
                {
                    value[i] = bytes[i];
                }
            }
        }

        public static unsafe FaceExpressionV11 TryGetEXPR(Dictionary<int, object> dict, int key, FaceExpressionV11 defaultExpression)
        {
            if (dict.TryGetValue(key, out object dictValue))
            {
                List<sbyte> values = new List<sbyte>();
                if (dictValue is byte[][])
                {
                    foreach (var arr in (sbyte[][])dictValue)
                    {
                        values.AddRange(arr);
                    }
                }
                else if (dictValue is byte[] || dictValue is sbyte[])
                {
                    values.AddRange((sbyte[])dictValue);
                }
                return FaceExpressionV11.CreateExpression(values.ToArray());
            }

            return defaultExpression;
        }

        public unsafe static byte[] GetNGSCML(CharacterHandlerReboot.xxpGeneralReboot xxp)
        {
            List<byte> cml = new List<byte>();
            int version = 0xA;

            //Header
            cml.AddRange(ConstantCMLHeader); //Always the same in all observed files

            //DOC
            List<byte> doc = new List<byte>();
            VTBFMethods.AddBytes(doc, 0x70, 0x8, BitConverter.GetBytes(xxp.baseDOC.race));
            VTBFMethods.AddBytes(doc, 0x71, 0x8, BitConverter.GetBytes(xxp.baseDOC.gender));
            VTBFMethods.AddBytes(doc, 0x72, 0x8, BitConverter.GetBytes((int)xxp.baseDOC.muscleMass));
            VTBFMethods.AddBytes(doc, 0x73, 0x8, BitConverter.GetBytes((int)0x9));
            VTBFMethods.AddBytes(doc, 0xFF, 0x8, BitConverter.GetBytes(version));
            VTBFMethods.AddBytes(doc, 0x74, 0x8, BitConverter.GetBytes((int)xxp.skinVariant));
            VTBFMethods.AddBytes(doc, 0x75, 0x8, BitConverter.GetBytes((int)xxp.eyebrowDensity));
            VTBFMethods.WriteTagHeader(doc, "DOC ", 0xC, 0x7);
            cml.AddRange(doc);

            //FIGR
            List<byte> figr = new List<byte>();
            VTBFMethods.AddBytes(figr, 0x0, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.baseFIGR.bodyVerts));
            VTBFMethods.AddBytes(figr, 0x1, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.baseFIGR.armVerts));
            VTBFMethods.AddBytes(figr, 0x2, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.baseFIGR.legVerts));
            VTBFMethods.AddBytes(figr, 0x3, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.baseFIGR.bustVerts));

            FaceFIGR faceValues;
            if (xxp.baseSLCT.faceTypePart >= 100000)
            {
                faceValues = xxp.GetNGSFaceData();
            }
            else
            {
                //For CMLs, the game seems to still use the original face values, even though for character data it switches pending faceTypePart value.
                faceValues = xxp.GetClassicFaceData();
            }
            VTBFMethods.AddBytes(figr, 0x4, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.headVerts));
            VTBFMethods.AddBytes(figr, 0x5, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.faceShapeVerts));
            VTBFMethods.AddBytes(figr, 0x6, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.eyeShapeVerts));
            VTBFMethods.AddBytes(figr, 0x7, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.noseHeightVerts));
                                                       
            VTBFMethods.AddBytes(figr, 0x8, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.noseShapeVerts));
            VTBFMethods.AddBytes(figr, 0x9, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.mouthVerts));
            VTBFMethods.AddBytes(figr, 0xA, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.ear_hornVerts));
            VTBFMethods.AddBytes(figr, 0xC, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.neckVerts));
            VTBFMethods.AddBytes(figr, 0xD, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.waistVerts));
            VTBFMethods.AddBytes(figr, 0x16, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.hands));
            VTBFMethods.AddBytes(figr, 0x20, 0x48, 0x1, DataHelpers.ConvertStruct(faceValues.hornVerts));
            Vector3Int.Vec3Int vec3_21 = Vector3Int.Vec3Int.CreateVec3Int(xxp.eyeSize, xxp.eyeHorizontalPosition, xxp.neckAngle);
            VTBFMethods.AddBytes(figr, 0x21, 0x48, 0x1, DataHelpers.ConvertStruct(vec3_21));

            //FIGR Alt Face
            VTBFMethods.AddBytes(figr, 0x30, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.headVerts));
            VTBFMethods.AddBytes(figr, 0x31, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.faceShapeVerts));
            VTBFMethods.AddBytes(figr, 0x32, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.eyeShapeVerts));
            VTBFMethods.AddBytes(figr, 0x33, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.noseHeightVerts));
            VTBFMethods.AddBytes(figr, 0x34, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.noseShapeVerts));
            VTBFMethods.AddBytes(figr, 0x35, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.mouthVerts));
            VTBFMethods.AddBytes(figr, 0x36, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.ear_hornVerts));
            VTBFMethods.AddBytes(figr, 0x37, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.neckVerts));
            VTBFMethods.AddBytes(figr, 0x38, 0x48, 0x1, DataHelpers.ConvertStruct(xxp.classicFace.hornVerts));

            VTBFMethods.WriteTagHeader(figr, "FIGR", 0, 0x19);
            cml.AddRange(figr);

            //OFST - Scale
            List<byte> ofstScale = new List<byte>();
            for (int i = 0; i < 0x24; i += 3)
            {
                VTBFMethods.AddBytes(ofstScale, (byte)(0x90 + (i / 3)), 0x83, 0x8, 0x2,
                    new byte[] { (byte)xxp.accessorySlidersRebootExtended.posSliders[i], (byte)xxp.accessorySlidersRebootExtended.posSliders[i + 1], (byte)xxp.accessorySlidersRebootExtended.posSliders[i + 2] });
            }
            VTBFMethods.WriteTagHeader(ofstScale, "OFST", 0, 0xC);
            cml.AddRange(ofstScale);

            //OFST - Pos
            List<byte> ofstPos = new List<byte>();
            for (int i = 0; i < 0x24; i += 3)
            {
                VTBFMethods.AddBytes(ofstPos, (byte)(0xA0 + (i / 3)), 0x83, 0x8, 0x2,
                    new byte[] { (byte)xxp.accessorySlidersRebootExtended.rotSliders[i], (byte)xxp.accessorySlidersRebootExtended.rotSliders[i + 1], (byte)xxp.accessorySlidersRebootExtended.rotSliders[i + 2] });
            }
            VTBFMethods.WriteTagHeader(ofstPos, "OFST", 0, 0xC);
            cml.AddRange(ofstPos);

            //OFST - Rot
            List<byte> ofstRot = new List<byte>();
            for (int i = 0; i < 0x24; i += 3)
            {
                VTBFMethods.AddBytes(ofstRot, (byte)(0xB0 + (i / 3)), 0x83, 0x8, 0x2,
                    new byte[] { (byte)xxp.accessorySlidersRebootExtended.scaleSliders[i], (byte)xxp.accessorySlidersRebootExtended.scaleSliders[i + 1], (byte)xxp.accessorySlidersRebootExtended.scaleSliders[i + 2] });
            }
            VTBFMethods.WriteTagHeader(ofstRot, "OFST", 0, 0xC);
            cml.AddRange(ofstRot);

            //ACWK
            List<byte> acwk = new List<byte>();
            for (int i = 0; i < 0x12; i++)
            {
                VTBFMethods.AddBytes(acwk, (byte)(0x0 + i), 0x48, 0x1, BitConverter.GetBytes((int)xxp.accessoryMiscDataExtended.accessoryAttach[i]));
                acwk.AddRange(BitConverter.GetBytes((int)xxp.accessoryMiscDataExtended.accessoryColorChoices[i * 2]));
                acwk.AddRange(BitConverter.GetBytes((int)xxp.accessoryMiscDataExtended.accessoryColorChoices[i * 2 + 1]));
            }
            VTBFMethods.WriteTagHeader(acwk, "ACWK", 0, 0x12);
            cml.AddRange(acwk);

            //COL2
            List<byte> col2Bytes = new List<byte>();
            var col2 = xxp.ngsCOL2;
            VTBFMethods.AddBytes(col2Bytes, 0x27, 0x09, ColorConversion.BytesFromFixed(col2.outerColor1));
            VTBFMethods.AddBytes(col2Bytes, 0x20, 0x09, ColorConversion.BytesFromFixed(col2.baseColor1));
            VTBFMethods.AddBytes(col2Bytes, 0x21, 0x09, ColorConversion.BytesFromFixed(col2.mainColor));
            VTBFMethods.AddBytes(col2Bytes, 0x22, 0x09, ColorConversion.BytesFromFixed(col2.subColor1));

            VTBFMethods.AddBytes(col2Bytes, 0x23, 0x09, ColorConversion.BytesFromFixed(col2.subColor2));
            VTBFMethods.AddBytes(col2Bytes, 0x24, 0x09, ColorConversion.BytesFromFixed(col2.subColor3));
            VTBFMethods.AddBytes(col2Bytes, 0x25, 0x09, ColorConversion.BytesFromFixed(col2.rightEyeColor));
            VTBFMethods.AddBytes(col2Bytes, 0x26, 0x09, ColorConversion.BytesFromFixed(col2.hairColor1));

            VTBFMethods.AddBytes(col2Bytes, 0x38, 0x09, ColorConversion.BytesFromFixed(col2.eyebrowColor));
            VTBFMethods.AddBytes(col2Bytes, 0x39, 0x09, ColorConversion.BytesFromFixed(col2.eyelashColor));
            VTBFMethods.AddBytes(col2Bytes, 0x35, 0x09, ColorConversion.BytesFromFixed(col2.skinColor1));
            VTBFMethods.AddBytes(col2Bytes, 0x30, 0x09, ColorConversion.BytesFromFixed(col2.skinColor2));

            VTBFMethods.AddBytes(col2Bytes, 0x31, 0x09, ColorConversion.BytesFromFixed(col2.baseColor2));
            VTBFMethods.AddBytes(col2Bytes, 0x32, 0x09, ColorConversion.BytesFromFixed(col2.outerColor2));
            VTBFMethods.AddBytes(col2Bytes, 0x33, 0x09, ColorConversion.BytesFromFixed(col2.innerColor1));
            VTBFMethods.AddBytes(col2Bytes, 0x34, 0x09, ColorConversion.BytesFromFixed(col2.innerColor2));

            VTBFMethods.AddBytes(col2Bytes, 0x36, 0x09, ColorConversion.BytesFromFixed(col2.leftEyeColor));
            VTBFMethods.AddBytes(col2Bytes, 0x37, 0x09, ColorConversion.BytesFromFixed(col2.hairColor2));
            VTBFMethods.WriteTagHeader(col2Bytes, "COL2", 0, 0x12);
            cml.AddRange(col2Bytes);

            //SLCT
            List<byte> slct = new List<byte>();
            VTBFMethods.AddBytes(slct, 0x40, 0x8, BitConverter.GetBytes(xxp.baseSLCT.costumePart));
            VTBFMethods.AddBytes(slct, 0x41, 0x8, BitConverter.GetBytes(xxp.baseSLCT.bodyPaintPart));
            VTBFMethods.AddBytes(slct, 0x42, 0x8, BitConverter.GetBytes(xxp.baseSLCT.stickerPart));
            VTBFMethods.AddBytes(slct, 0x43, 0x8, BitConverter.GetBytes(xxp.baseSLCT.eyePart));
            VTBFMethods.AddBytes(slct, 0x44, 0x8, BitConverter.GetBytes(xxp.baseSLCT.eyebrowPart));
            VTBFMethods.AddBytes(slct, 0x45, 0x8, BitConverter.GetBytes(xxp.baseSLCT.eyelashPart));
            VTBFMethods.AddBytes(slct, 0x46, 0x8, BitConverter.GetBytes(xxp.baseSLCT.faceTypePart));
            VTBFMethods.AddBytes(slct, 0x47, 0x8, BitConverter.GetBytes(xxp.baseSLCT.faceTexPart));
            VTBFMethods.AddBytes(slct, 0x48, 0x8, BitConverter.GetBytes(xxp.baseSLCT.makeup1Part));
            VTBFMethods.AddBytes(slct, 0x49, 0x8, BitConverter.GetBytes(xxp.baseSLCT.hairPart));
            VTBFMethods.AddBytes(slct, 0x50, 0x8, BitConverter.GetBytes(xxp.baseSLCT.acc1Part));
            VTBFMethods.AddBytes(slct, 0x51, 0x8, BitConverter.GetBytes(xxp.baseSLCT.acc2Part));
            VTBFMethods.AddBytes(slct, 0x52, 0x8, BitConverter.GetBytes(xxp.baseSLCT.acc3Part));
            VTBFMethods.AddBytes(slct, 0x53, 0x8, BitConverter.GetBytes(xxp.baseSLCT.makeup2Part));
            VTBFMethods.AddBytes(slct, 0x54, 0x8, BitConverter.GetBytes(xxp.baseSLCT.legPart));
            VTBFMethods.AddBytes(slct, 0x55, 0x8, BitConverter.GetBytes(xxp.baseSLCT.armPart));

            //Ep4 
            VTBFMethods.AddBytes(slct, 0x56, 0x8, BitConverter.GetBytes(xxp.baseSLCT2.acc4Part));
            VTBFMethods.AddBytes(slct, 0x57, 0x8, BitConverter.GetBytes(xxp.baseSLCT2.basewearPart));
            VTBFMethods.AddBytes(slct, 0x58, 0x8, BitConverter.GetBytes(xxp.baseSLCT2.innerwearPart));
            VTBFMethods.AddBytes(slct, 0x59, 0x8, BitConverter.GetBytes(xxp.baseSLCT2.bodyPaint2Part));

            VTBFMethods.AddBytes(slct, 0x63, 0x8, BitConverter.GetBytes(xxp.leftEyePart));

            //Pre NGS
            VTBFMethods.AddBytes(slct, 0x0, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.skinTextureSet));

            //NGS launch
            VTBFMethods.AddBytes(slct, 0x1, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.earsPart));
            VTBFMethods.AddBytes(slct, 0x2, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.teethPart));
            VTBFMethods.AddBytes(slct, 0x3, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.hornPart));
            VTBFMethods.AddBytes(slct, 0x4, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.acc5Part));

            VTBFMethods.AddBytes(slct, 0x5, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.acc6Part));
            VTBFMethods.AddBytes(slct, 0x6, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.acc7Part));
            VTBFMethods.AddBytes(slct, 0x7, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.acc8Part));
            VTBFMethods.AddBytes(slct, 0x8, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.acc9Part));

            VTBFMethods.AddBytes(slct, 0x9, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.acc10Part));
            VTBFMethods.AddBytes(slct, 0xA, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.acc11Part));
            VTBFMethods.AddBytes(slct, 0xB, 0x8, BitConverter.GetBytes(xxp.baseSLCTNGS.acc12Part));

            //NGS Extended Accessories
            VTBFMethods.AddBytes(slct, 0xC, 0x8, BitConverter.GetBytes(xxp.slctNGSExtended.acc13Part));
            VTBFMethods.AddBytes(slct, 0xD, 0x8, BitConverter.GetBytes(xxp.slctNGSExtended.acc14Part));
            VTBFMethods.AddBytes(slct, 0xD, 0x8, BitConverter.GetBytes(xxp.slctNGSExtended.acc15Part));
            VTBFMethods.AddBytes(slct, 0xD, 0x8, BitConverter.GetBytes(xxp.slctNGSExtended.acc16Part));
            VTBFMethods.AddBytes(slct, 0xD, 0x8, BitConverter.GetBytes(xxp.slctNGSExtended.acc17Part));
            VTBFMethods.AddBytes(slct, 0xD, 0x8, BitConverter.GetBytes(xxp.slctNGSExtended.acc18Part));

            VTBFMethods.WriteTagHeader(slct, "SLCT", 0, 0x27);
            cml.AddRange(slct);

            //SLCT - Paint Priority
            List<byte> slctBP = new List<byte>();
            VTBFMethods.AddBytes(slctBP, 0x60, 0x8, BitConverter.GetBytes((int)xxp.paintPriority.priority1));
            VTBFMethods.AddBytes(slctBP, 0x61, 0x8, BitConverter.GetBytes((int)xxp.paintPriority.priority2));
            VTBFMethods.AddBytes(slctBP, 0x62, 0x8, BitConverter.GetBytes((int)xxp.paintPriority.priority3));
            VTBFMethods.WriteTagHeader(slctBP, "SLCT", 0, 0x3);
            cml.AddRange(slctBP);

            //SLID - Extra NGS Sliders
            List<byte> slid = new List<byte>();
            VTBFMethods.AddBytes(slid, 0x0, 0x8, BitConverter.GetBytes(xxp.ngsSLID.shoulderSize));
            VTBFMethods.AddBytes(slid, 0x1, 0x8, BitConverter.GetBytes(xxp.ngsSLID.hairAdjust));
            VTBFMethods.AddBytes(slid, 0x2, 0x8, BitConverter.GetBytes(xxp.ngsSLID.skinGloss));
            VTBFMethods.AddBytes(slid, 0x3, 0x8, BitConverter.GetBytes(xxp.ngsSLID.mouthVertical));

            VTBFMethods.AddBytes(slid, 0x4, 0x8, BitConverter.GetBytes(xxp.ngsSLID.eyebrowHoriz));
            VTBFMethods.AddBytes(slid, 0x5, 0x8, BitConverter.GetBytes(xxp.ngsSLID.irisVertical));
            VTBFMethods.AddBytes(slid, 0x6, 0x8, BitConverter.GetBytes(xxp.ngsSLID.facePaint1Opacity));
            VTBFMethods.AddBytes(slid, 0x7, 0x8, BitConverter.GetBytes(xxp.ngsSLID.facePaint2Opacity));

            VTBFMethods.AddBytes(slid, 0x8, 0x8, BitConverter.GetBytes(xxp.ngsSLID.shoulderVertical));
            VTBFMethods.AddBytes(slid, 0x9, 0x8, BitConverter.GetBytes(xxp.ngsSLID.thighsAdjust));
            VTBFMethods.AddBytes(slid, 0xA, 0x8, BitConverter.GetBytes(xxp.ngsSLID.calvesAdjust));
            VTBFMethods.AddBytes(slid, 0xB, 0x8, BitConverter.GetBytes(xxp.ngsSLID.forearmsAdjust));

            VTBFMethods.AddBytes(slid, 0xC, 0x8, BitConverter.GetBytes(xxp.ngsSLID.handThickness));
            VTBFMethods.AddBytes(slid, 0xD, 0x8, BitConverter.GetBytes(xxp.ngsSLID.footSize));
            VTBFMethods.AddBytes(slid, 0xE, 0x8, BitConverter.GetBytes(xxp.ngsSLID.int_32C));
            VTBFMethods.WriteTagHeader(slid, "SLID", 0, 0xF);
            cml.AddRange(slid);

            //MTON
            List<byte> mton = new List<byte>();
            VTBFMethods.AddBytes(mton, 0x0, 0x8, BitConverter.GetBytes(xxp.ngsMTON.int_330));
            VTBFMethods.AddBytes(mton, 0x1, 0x8, BitConverter.GetBytes(xxp.ngsMTON.walkRunMotion));
            VTBFMethods.AddBytes(mton, 0x2, 0x8, BitConverter.GetBytes(xxp.ngsMTON.swimMotion));
            VTBFMethods.AddBytes(mton, 0x3, 0x8, BitConverter.GetBytes(xxp.ngsMTON.dashMotion));

            VTBFMethods.AddBytes(mton, 0x4, 0x8, BitConverter.GetBytes(xxp.ngsMTON.glideMotion));
            VTBFMethods.AddBytes(mton, 0x5, 0x8, BitConverter.GetBytes(xxp.ngsMTON.landingMotion));
            VTBFMethods.AddBytes(mton, 0x6, 0x8, BitConverter.GetBytes(xxp.ngsMTON.idleMotion));
            VTBFMethods.AddBytes(mton, 0x7, 0x8, BitConverter.GetBytes(xxp.ngsMTON.jumpMotion));
            VTBFMethods.WriteTagHeader(mton, "MTON", 0, 0x8);
            cml.AddRange(mton);

            //VISI
            List<byte> visi = new List<byte>();
            //Build bitflag
            byte base1 = (byte)(xxp.ngsVISI.hideBasewearOrnament1 > 0 ? 0b00000001 : 0b00000000);
            byte base2 = (byte)(xxp.ngsVISI.hideBasewearOrnament2 > 0 ? 0b00000010 : 0b00000000);
            byte head = (byte)(xxp.ngsVISI.hideHeadPartOrnament > 0 ? 0b00000100 : 0b00000000);
            byte body = (byte)(xxp.ngsVISI.hideHeadPartOrnament > 0 ? 0b00001000 : 0b00000000);
            byte arm = (byte)(xxp.ngsVISI.hideHeadPartOrnament > 0 ? 0b00010000 : 0b00000000);
            byte leg = (byte)(xxp.ngsVISI.hideHeadPartOrnament > 0 ? 0b00100000 : 0b00000000);
            byte outer = (byte)(xxp.ngsVISI.hideOuterwearOrnament > 0 ? 0b01000000 : 0b00000000);
            byte inner = (byte)(xxp.ngsVISI.hideInnerwear > 0 ? 0b10000000 : 0b00000000);

            byte bitflags = (byte)(0 | base1 | base2 | head | body | arm | leg | outer | inner);

            VTBFMethods.AddBytes(visi, 0xC0, 0x8, new byte[] { bitflags, 0, 0, 0 });
            VTBFMethods.AddBytes(visi, 0xC1, 0x8, new byte[] { 0, 0, 0, 0 });
            VTBFMethods.WriteTagHeader(visi, "VISI", 0, 0x2);
            cml.AddRange(visi);

            //EXPR
            List<byte> expr = new List<byte>();

            switch (version)
            {
                case 0xA:
                    VTBFMethods.AddBytes(expr, 0x0, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceNatural.expStruct));
                    VTBFMethods.AddBytes(expr, 0x1, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceSmile.expStruct));
                    VTBFMethods.AddBytes(expr, 0x2, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceAngry.expStruct));
                    VTBFMethods.AddBytes(expr, 0x3, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceSad.expStruct));
                                                                     
                    VTBFMethods.AddBytes(expr, 0x4, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceSus.expStruct));
                    VTBFMethods.AddBytes(expr, 0x5, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceEyesClosed.expStruct));
                    VTBFMethods.AddBytes(expr, 0x6, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceSmile2.expStruct));
                    VTBFMethods.AddBytes(expr, 0x7, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceWink.expStruct));
                                                                     
                    VTBFMethods.AddBytes(expr, 0x8, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceUnused1.expStruct));
                    VTBFMethods.AddBytes(expr, 0x9, 0x83, 0x8, 0x11, DataHelpers.ConvertStruct(xxp.faceUnused2.expStruct));
                    VTBFMethods.WriteTagHeader(expr, "EXPR", 0, 0xA);
                    break;
                case 0xB:
                case 0xC:
                case 0xD:
                    VTBFMethods.AddBytes(expr, 0x0, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceNatural));
                    VTBFMethods.AddBytes(expr, 0x1, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceSmile));
                    VTBFMethods.AddBytes(expr, 0x2, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceAngry));
                    VTBFMethods.AddBytes(expr, 0x3, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceSad));
                                                                    
                    VTBFMethods.AddBytes(expr, 0x4, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceSus));
                    VTBFMethods.AddBytes(expr, 0x5, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceEyesClosed));
                    VTBFMethods.AddBytes(expr, 0x6, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceSmile2));
                    VTBFMethods.AddBytes(expr, 0x7, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceWink));
                                                                   
                    VTBFMethods.AddBytes(expr, 0x8, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceUnused1));
                    VTBFMethods.AddBytes(expr, 0x9, 0x83, 0x8, 0x13, DataHelpers.ConvertStruct(xxp.faceUnused2));
                    VTBFMethods.WriteTagHeader(expr, "EXPR", 0, 0xA);
                    break;
                default:
                    throw new Exception();
                    break;
            }
            cml.AddRange(expr);
            cml.AlignWriter(0x10);

            return cml.ToArray();
        }
    }
}
