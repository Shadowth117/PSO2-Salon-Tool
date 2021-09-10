using AquaModelLibrary;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Character_Making_File_Tool.CharacterDataStructs;
using static Character_Making_File_Tool.CharacterDataStructsReboot;

namespace Character_Making_File_Tool
{
    public class CMLHandler
    {
        public static CharacterHandlerReboot.xxpGeneralReboot ParseCML(BufferedStreamReader streamReader)
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

        public static unsafe CharacterHandlerReboot.xxpGeneralReboot ParseVTBFCML(BufferedStreamReader streamReader)
        {
            CharacterHandlerReboot.xxpGeneralReboot xxp = new();

            int dataEnd = (int)streamReader.BaseStream().Length;

            //Seek past vtbf tag
            streamReader.Seek(0x10, SeekOrigin.Current);          //VTBF + CMLP tags

            //Previous CML iterations had unique ids for each variable in the file. However now they simply have unique ids per tag type.
            //There often are multiple of the same tag type placed about the file, though these shouldn't ever overlap.
            while (streamReader.Position() < dataEnd)
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
                        TryGetFromArray(ref xxp.neckVerts, figr, 0xB);
                        TryGetFromArray(ref xxp.waistVerts, figr, 0xC);
                        break;
                    case "OFST":
                        var ofst = data[0];
                        foreach(int key in ofst.Keys)
                        {
                            TryGetFromOFST(ref xxp.accessorySlidersReboot, ofst, key);
                        }
                        break;
                    case "ACWK":
                        var acwk = data[0];
                        foreach (int key in acwk.Keys)
                        {
                            TryGetFromACWK(ref xxp.accessoryMiscData, acwk, key);
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
                        TryGet(ref xxp.baseSLCT2.bodypaint2Part, slct, 0x59);

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
                        else if(slct.ContainsKey(0x43))
                        {
                            if (xxp.cmlVariant >= 0x9)
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
                        if(visi.ContainsKey(0xC0))
                        {
                            byte[] ornaments = BitConverter.GetBytes((int)visi[0xC0]);
                            xxp.ngsVISI.hideBasewearOrnament1 = (ornaments[0] & 0b00000001) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideBasewearOrnament2 = (ornaments[0] & 0b00000010) > 0 ? 1 : 0;

                            xxp.ngsVISI.hideHeadPartOrnament = (ornaments[0] & 0b00000100) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideBodyPartOrnament = (ornaments[0] & 0b00001000) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideArmPartOrnament = (ornaments[0] & 0b00010000) > 0 ? 1 : 0;
                            xxp.ngsVISI.hideLegPartOrnament = (ornaments[0] & 0b00100000) > 0 ? 1 : 0;

                            xxp.ngsVISI.hideOuterwearOrnament = (ornaments[0] & 0b01000000) > 0 ? 1 : 0;
                        }
                        break;
                    case "EXPR":
                        var expr = data[0];
                        xxp.faceNatural = TryGetEXPR(xxp.faceNatural, expr, 0x0);
                        xxp.faceSmile = TryGetEXPR(xxp.faceSmile, expr, 0x1);
                        xxp.faceAngry = TryGetEXPR(xxp.faceAngry, expr, 0x2);
                        xxp.faceSad = TryGetEXPR(xxp.faceSad, expr, 0x3);

                        xxp.faceSus = TryGetEXPR(xxp.faceSus, expr, 0x4);
                        xxp.faceEyesClosed = TryGetEXPR(xxp.faceEyesClosed, expr, 0x5);
                        xxp.faceSmile2 = TryGetEXPR(xxp.faceSmile2, expr, 0x6);
                        xxp.faceWink = TryGetEXPR(xxp.faceWink, expr, 0x7);

                        xxp.faceUnused1 = TryGetEXPR(xxp.faceUnused1, expr, 0x8);
                        xxp.faceUnused2 = TryGetEXPR(xxp.faceUnused2, expr, 0x9);
                        break;
                    default:
                        //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                        if (data == null)
                        {
                            return xxp;
                        }
                        throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                }
            }

            return xxp;
        }

        public static void TryGet<T>(ref T value, Dictionary<int, object> dict, int key)
        {
            if(dict.TryGetValue(key, out object dictValue))
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

        public static unsafe void TryGetFromACWK(ref CharacterDataStructsReboot.AccessoryMisc misc, Dictionary<int, object> dict, int key)
        {
            if (dict.TryGetValue(key, out object dictValue))
            {
                misc.accessoryAttach[key] = (byte)((int[][])dictValue)[0][0];
                misc.accessoryColorChoices[key * 2] = (byte)((int[][])dictValue)[0][1];
                misc.accessoryColorChoices[key * 2 + 1] = (byte)((int[][])dictValue)[0][2];
            }
        }

        public static unsafe void TryGetFromOFST(ref CharacterDataStructsReboot.AccessorySlidersReboot sliders, Dictionary<int, object> dict, int key)
        {
            int set = key / 0x10;
            int idMult = key % 0x10;
            if (dict.TryGetValue(key, out object dictValue))
            {
                sbyte[] sbytes = (sbyte[])dictValue;
                switch(set)
                {
                    //Pos
                    case 0xB:
                        sliders.posSliders[idMult * 3] = sbytes[0];
                        sliders.posSliders[idMult * 3 + 1] = sbytes[1];
                        sliders.posSliders[idMult * 3 + 2] = sbytes[2];
                        break;
                    //Rot
                    case 0xA:
                        sliders.rotSliders[idMult * 3] = sbytes[0];
                        sliders.rotSliders[idMult * 3 + 1] = sbytes[1];
                        sliders.rotSliders[idMult * 3 + 2] = sbytes[2];
                        break;
                    //Scale
                    case 0x9:
                        sliders.scaleSliders[idMult * 3] = sbytes[0];
                        sliders.scaleSliders[idMult * 3 + 1] = sbytes[1];
                        sliders.scaleSliders[idMult * 3 + 2] = sbytes[2];
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
                for(int i = 0; i < 4; i++)
                {
                    value[i] = bytes[i];
                }
            }
        }

        public static unsafe FaceExpression TryGetEXPR(FaceExpression expr, Dictionary<int, object> dict, int key)
        {
            if(dict.TryGetValue(key, out object dictValue))
            {
                FaceExpression outExpr = new FaceExpression();
                IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(expr));
                Marshal.StructureToPtr(outExpr, intPtr, true);
                Marshal.Copy((byte[])dictValue, 0, intPtr, 4);
                return Marshal.PtrToStructure<FaceExpression>(intPtr);
            }

            return expr;
        }

        public static byte[] WriteNGSCML(CharacterHandlerReboot.xxpGeneralReboot xxp)
        {
            return null;
        }
    }
}
