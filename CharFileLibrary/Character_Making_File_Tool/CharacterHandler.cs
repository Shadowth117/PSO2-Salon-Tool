using Microsoft.WindowsAPICodePack.Dialogs;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using zamboni;
using static Character_Making_File_Tool.CharacterHandlerUtilityMethods;
using static Character_Making_File_Tool.NibbleUtility;
using static Character_Making_File_Tool.Vector3Int;
using static Character_Making_File_Tool.CharacterConstants;
using static Character_Making_File_Tool.CharacterDataStructs;

namespace Character_Making_File_Tool
{
    //Thanks to Agrajag for CharacterCrypt.
    //Thanks to Chikinface/Raujinn for .xxp and .cml struct information as well as for code from CMLParser for reference. 
    //Thanks to Rosenblade for helping with UI design.
    public unsafe class CharacterHandler
    {
        private static uint CharacterBlowfishKey = 2588334024;
        public int version;
        public string pso2_binPathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "pso2_binPath.txt");

        public XXPGeneral xxpGeneral;
        public FileHandler fh = null;
        public int fileLoadRace;
        public bool newLoad; //flag to avoid redoing fix changes if they're already done.
        public string pso2_binPath;

        public int getVersion()
        {
            return version;
        }

        public struct XXPV2
        {
            //Naming is based upon equivalent .cml file tag naming

            //DOC 0x10 
            public BaseDOC baseDOC;

            //FIGR 0x1C
            public BaseFIGR baseFIGR;

            //Padding 1 0xA0
            public fixed long padding1[4]; //4 sets of 8 bytes
            public uint padding1End;

            //COLR 0xC4
            public BaseCOLR baseCOLR;

            //SLCT 0x124 
            public BaseSLCT baseSLCT;

            //End Padding 0x15E - 8 bytes. Ends on 0x16C; Does not align to 0x10.
            public long endPadding;
        }

        public struct XXPV5
        {
            //Naming is based upon equivalent .cml file tag naming

            //DOC 0x10 
            public BaseDOC baseDOC;

            //FIGR 0x1C
            public BaseFIGR baseFIGR;

            //Padding 1 0xA0
            public fixed long padding1[4]; //4 sets of 8 bytes
            public uint padding1End;

            //COLR 0xD0 
            public BaseCOLR baseCOLR;

            //SLCT 0x130 
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;

            //Padding 2 0x172
            public short padding2Short;
            public uint padding2Int;

            //Accessory Positions. Same format as V6, but ONLY position is present. 
            //Format is 1Y_1Z 4X_1X 2Y_2Z 4Y_2X 3Y_3Z 4Z_3X and is repeated for scale and rotation.
            public fixed byte accessorySlidersv6[6];

            //Padding 3 0x17E
            public short padding3Short;
        }

        public struct XXPV6
        {
            //Naming is based upon equivalent .cml file tag naming

            //DOC 0x10
            public BaseDOC baseDOC;

            //FIGR 0x20
            public BaseFIGR baseFIGR;
            public BaseFIGR2 baseFIGR2;

            //Padding 1 0x100 - 60 bytes + Padding to a multiple of 0x10 
            public fixed long padding1[13]; //13 sets of 8 bytes
            public uint padding1Start;

            //COLR 0x16C
            public BaseCOLR baseCOLR;

            //Padding 2 0x1CC - padding of 0x4 + 0x70 bytes + padding of 0x4
            public fixed long padding2[15]; //15 sets of 8 bytes

            //SLCT 0x244 - .cmx piece references.
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;

            //Padding 3 0x294 - 0x30 bytes padding
            public fixed long padding3[6]; //6 sets of 8 bytes

            //Accessory 0x2C4 - Position Scale Rotation, stored as signed nibbles. Values go to 7, go down from 0 starting at 8 until E for a min of -7.
            //Format is 1Y_1Z 4X_1X 2Y_2Z 4Y_2X 3Y_3Z 4Z_3X and is repeated for scale and rotation.
            public fixed byte accessorySliders[18]; //18 bytes.

            //Body paint order 0x2D6 - followed by padding to end of row and 0xC bytes. Innerwear is 0x0, bodypaint1 is 0x1, bodypaint2 is 0x2
            public PaintPriority paintPriority;

            //Final padding
            public fixed uint finalPadding[2];

        }

        public struct XXPV9
        {
            //Naming is based upon equivalent .cml file tag naming

            //DOC 0x10
            public BaseDOC baseDOC;
            public byte skinVariant; //0 or above 3 for default, 1 for human, 2 for dewman, 3 for cast. This decides the color map used for the skin. 
            public sbyte eyebrowDensity; //-100 to 100 
            public short cmlVariant;

            //FIGR 0x20
            public BaseFIGR baseFIGR;
            public BaseFIGR2 baseFIGR2;

            //Padding 1 0x104 - Padding to a multiple of 0x10 + 60 bytes
            public uint padding1Start;
            public fixed long padding1[13]; //13 sets of 8 bytes

            //COLR 0x170
            public BaseCOLR baseCOLR;

            //Padding 2 0x1D0 - 0x70 bytes + padding of 0x8
            public fixed long padding2[15]; //15 sets of 8 bytes

            //SLCT 0x248 - .cmx piece references.
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;
            public uint leftEyePart;

            //Padding 3 0x29C - Padding to a multiple of 0x10 + 0x20 bytes + padding of 0x8?
            public uint padding3Start;
            public fixed long padding3[5]; //5 sets of 8 bytes

            //Accessory 0x2C8 - Position Scale Rotation. Each transformation type has values for each laid out 1234 
            //before repeating in the next set. In v9, range is -126 to 126 
            public fixed sbyte accessorySliders[36]; //36 bytes. Each slider in v9 is one byte.

            //Body paint order 0x2EC - followed by padding to end. Innerwear is 0x0, bodypaint1 is 0x1, bodypaint2 is 0x2
            public PaintPriority paintPriority;

            //Final Padding
            public ushort shortPadding;
            public uint semifinalPadding;
            public long finalPadding;
        }

        public struct XXPGeneral
        {
            //Naming is based upon equivalent .cml file tag naming

            //DOC
            public BaseDOC baseDOC;
            public byte skinVariant; //0 or above 3 for default, 1 for human, 2 for dewman, 3 for cast. This decides the color map used for the skin. 
            public sbyte eyebrowDensity; //-100 to 100 
            public short cmlVariant;

            //FIGR 
            public BaseFIGR baseFIGR;
            public BaseFIGR2 baseFIGR2;

            //COLR 
            public BaseCOLR baseCOLR;

            //SLCT  .cmx piece references.
            public BaseSLCT baseSLCT;
            public BaseSLCT2 baseSLCT2;
            public uint leftEyePart;

            //Accessory - Position Scale Rotation, stored as nibbles in v6. In v9, range is -126 to 126
            public fixed byte accessorySlidersv6[18]; //18 bytes.
            public fixed sbyte accessorySliders[36]; //36 bytes. Each slider in v9 is one byte.

            //Body paint order 0x2EC - followed by padding to end. Innerwear is 0x0, bodypaint1 is 0x1, bodypaint2 is 0x2
            public PaintPriority paintPriority;

        }

        public CharacterHandler()
        {
            LoadCMXData();
        }

        private void LoadCMXData()
        {
            //Load the pso2_bin path if it's stored. 
            if (File.Exists(pso2_binPathFile))
            {
                pso2_binPath = File.ReadAllLines(pso2_binPathFile)[0];
                if (pso2_binPath != null)
                {
                    fh = new FileHandler(pso2_binPath);
                    //Don't keep throwing errors every startup if it's not done right.
                    if (fh.cmxReader.invalid)
                    {
                        fh = null;
                        File.Delete(pso2_binPathFile);
                    }
                }
            }
        }

        public void ArrayGetValue(Dictionary<int, int[]> dict, int key, out int[] value)
        {
            dict.TryGetValue(key, out value);
            if (value == null)
            {
                value = new int[] { 0 };
            }
        }

        //Expects two fixed int arrays of same length
        public unsafe void ArrayOfIntsSwap(int* array1, int* array2, int length)
        {
            int temp;
            for (int i = 0; i < length; i++)
            {
                temp = array1[i];
                array1[i] = array2[i];
                array2[i] = temp;
            }
        }

        public bool CheckForV6AccSliders(Dictionary<int, int[]> dict)
        {
            if (dict.ContainsKey(0x80) || dict.ContainsKey(0x81) || dict.ContainsKey(0x82) || dict.ContainsKey(0x83)
                || dict.ContainsKey(0x84) || dict.ContainsKey(0x85) || dict.ContainsKey(0x86) || dict.ContainsKey(0x87)
                || dict.ContainsKey(0x88))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckForV9AccSliders(Dictionary<int, int[]> dict)
        {
            if (dict.ContainsKey(0x90) || dict.ContainsKey(0x91) || dict.ContainsKey(0x92) || dict.ContainsKey(0x93)
                || dict.ContainsKey(0x95) || dict.ContainsKey(0x96) || dict.ContainsKey(0x97) || dict.ContainsKey(0x98)
                || dict.ContainsKey(0x9B) || dict.ContainsKey(0x9C) || dict.ContainsKey(0x9D) || dict.ContainsKey(0x9E))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public byte[] DecryptFile(string inFilename)
        {
            byte[] fileVersion = new byte[4];
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            {
                stream.Read(fileVersion, 0, 4);
                this.version = BitConverter.ToInt32(fileVersion, 0);
            }
            return DecryptXXP(inFilename);
        }

        public static byte[] DecryptXXP(string inFilename)
        {
            byte[] header;
            byte[] sizeBuffer = new byte[4];
            byte[] encryptedData;
            byte[] decryptedData;
            byte[] fileData;
            uint key;
            int dataSize;

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            {
                stream.Seek(4, SeekOrigin.Begin);

                stream.Read(sizeBuffer, 0, 4);
                key = BitConverter.ToUInt32(((IEnumerable<byte>)sizeBuffer).Reverse<byte>().ToArray<byte>(), 0);
                dataSize = BitConverter.ToInt32(sizeBuffer, 0);

                encryptedData = new byte[dataSize];
                header = new byte[16];

                stream.Seek(0L, SeekOrigin.Begin);
                stream.Read(header, 0, 16);
                stream.Seek(16L, SeekOrigin.Begin);
                stream.Read(encryptedData, 0, dataSize);
            }
            if (inFilename.LastOrDefault() == 'u')
            {
                decryptedData = encryptedData;
            }
            else
            {
                decryptedData = new BlewFish(key ^ CharacterBlowfishKey).decryptBlock(encryptedData);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(header, 0, 16);
                memoryStream.Write(decryptedData, 0, dataSize);
                fileData = memoryStream.ToArray();
            }

            return fileData;
        }

        public void DumpData(string filename, byte[] fileData)
        {
            var fileDumpPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + "_decrypted.txt");

            using (StreamWriter fileDump = new StreamWriter(fileDumpPath))
            {
                for (int i = 0; i < fileData.Length; i++)
                {
                    if (fileData[i] < 16)
                    {
                        fileDump.Write("0");
                    }
                    fileDump.Write(fileData[i].ToString("X"));
                    if ((i + 1) % 16 == 0)
                    {
                        fileDump.WriteLine();
                    }
                }
            }
        }

        public void EncryptAndSaveFile(string filename, int saveVersion, bool fixNAHeight, bool leaveUnencrypted, out string windowVersion)
        {
            byte[] body;
            int hash = 0;
            int ingameVersion;
            bool cmlSave = false;

            if (saveVersion == 4 || saveVersion == 5)
            {
                cmlSave = true;
            }

            //Fix skin color 
            if(fileLoadRace != xxpGeneral.baseDOC.race)
            {
                switch(fileLoadRace)
                {
                    case 0:
                    case 1:
                        xxpGeneral.skinVariant = 1;
                        break;
                    case 2:
                        xxpGeneral.skinVariant = 3;
                        break;
                    case 3:
                        xxpGeneral.skinVariant = 2;
                        break;
                    default:
                        break;
                }
            }

            //Fix cast hair color 2 nonsense
            if ((fileLoadRace == 2 && xxpGeneral.baseDOC.race != 2) || (fileLoadRace != 2 && xxpGeneral.baseDOC.race == 2))
            {
                Vec3IntSwap(ref xxpGeneral.baseCOLR.mainColor_hair2Verts, ref xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts);
            }

            //Fix deuman eye color
            if ((fileLoadRace != 3 && xxpGeneral.baseDOC.race == 3))
            {
                Vec3IntSwap(ref xxpGeneral.baseCOLR.rightEye_EyesVerts, ref xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts);
            }

            fileLoadRace = (int)xxpGeneral.baseDOC.race;

            //Set heights that are too low to NA's minimum
            if (fixNAHeight == true)
            {
                if (xxpGeneral.baseFIGR.bodyVerts.X < MinHeightBodySlider)
                {
                    xxpGeneral.baseFIGR.bodyVerts.X = MinHeightBodySlider;
                }
                if (xxpGeneral.baseFIGR2.body2Verts.X < MinHeightBodySlider)
                {
                    xxpGeneral.baseFIGR2.body2Verts.X = MinHeightBodySlider;
                }
                if (xxpGeneral.baseFIGR.legVerts.X < MinHeightLegSlider)
                {
                    xxpGeneral.baseFIGR.legVerts.X = MinHeightLegSlider;
                }
                if (xxpGeneral.baseFIGR2.leg2Verts.X < MinHeightLegSlider)
                {
                    xxpGeneral.baseFIGR2.leg2Verts.X = MinHeightLegSlider;
                }
            }

            //Swap eye colors for cml from xxp and vice versa since it's needed.
            if (version != -1 && cmlSave && (xxpGeneral.baseDOC.race == 3))
            {
                version = -1;
                Vec3IntSwap(ref xxpGeneral.baseCOLR.rightEye_EyesVerts, ref xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts);
            } else if (version == -1 && !cmlSave && (xxpGeneral.baseDOC.race == 3))
            {
                version = 9;
                Vec3IntSwap(ref xxpGeneral.baseCOLR.rightEye_EyesVerts, ref xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts);
            }

            using (MemoryStream xxpMem = new MemoryStream())
            {
                switch (saveVersion)
                {
                    case 0:
                        ingameVersion = 9;
                        body = SetupV9();
                        break;
                    case 1:
                        ingameVersion = 6;
                        body = SetupV6();
                        break;
                    case 2:
                        ingameVersion = 5;
                        body = SetupV5();
                        break;
                    case 3:
                        ingameVersion = 2;
                        body = SetupV2();
                        break;
                    case 4:
                        windowVersion = "cml";
                        SaveCML(filename);
                        return;
                    case 5:
                        windowVersion = "cml";
                        OutputToText(filename);
                        return;
                    default:
                        windowVersion = "";
                        MessageBox.Show("Huh... that's not supposed to happen. Sorry about that!");
                        return;
                }
                int fileSize = classicSalonToolFileSizes[saveVersion];

                windowVersion = ingameVersion.ToString();
                xxpMem.Write(Reloaded.Memory.Struct.GetBytes(ingameVersion), 0, 4);
                xxpMem.Write(Reloaded.Memory.Struct.GetBytes(fileSize), 0, 4);

                if (leaveUnencrypted == false)
                {
                    body = EncryptData(body, fileSize, out hash);
                }

                xxpMem.Write(Reloaded.Memory.Struct.GetBytes(hash), 0, 4);
                xxpMem.Write(Reloaded.Memory.Struct.GetBytes((int)0), 0, 4);
                xxpMem.Write(body, 0, body.Count());

                File.WriteAllBytes(filename, xxpMem.ToArray());
            }
        }

        public static void EncryptAndWrite(string fileName)
        {
            var file = File.ReadAllBytes(fileName);
            byte[] body = new byte[file.Length - 0x10];
            Array.Copy(file, 0x10, body, 0, file.Length - 0x10);

            Array.Copy(EncryptData(body, BitConverter.ToInt32(file, 4), out int hash), 0, file, 0x10, body.Length);
            Array.Copy(BitConverter.GetBytes(hash), 0, file, 0x8, 0x4);

            File.WriteAllBytes(fileName + "_e", file);
        }

        public static byte[] EncryptData(byte[] body, int size, out int hashInt)
        {
            byte[] encryptedData;
            byte[] hash;
            uint key = BitConverter.ToUInt32(((IEnumerable<byte>)BitConverter.GetBytes(size)).Reverse<byte>().ToArray<byte>(), 0);

            encryptedData = new BlewFish(key ^ CharacterBlowfishKey).encryptBlock(body);
            hash = new Crc32().ComputeHash(encryptedData);

            hash = ((IEnumerable<byte>)hash).Reverse<byte>().ToArray<byte>();
            hashInt = BitConverter.ToInt32(hash, 0);

            return encryptedData;
        }

        /*Deprecated parsing method. In old PSO2, these ids wouldn't overlap. However due to NGS's implementation, now they can.
        
            TODO: Reimplment with AquaModelLibrary VTBF Parser
        
         */
        public void ParseCML(string inFilename)
        {
            this.version = -1;
            Dictionary<int, int[]> tagStructs = new Dictionary<int, int[]>();

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                //Handle deicer cmls too
                int fileVariant = streamReader.Read<int>();

                if (fileVariant == DeicerCMLType)
                {
                    streamReader.Seek(0x5C, SeekOrigin.Current);
                } else
                {
                    streamReader.Seek(0xC, SeekOrigin.Current);
                }
                
                while(streamReader.CanRead(0x10))
                {
                    streamReader.Seek(0x8, SeekOrigin.Current); //We don't really need the vtc0 information as long as everything else is written correctly.
                    int structType = streamReader.Read<int>();
                    streamReader.Seek(0x2, SeekOrigin.Current);
                    short structCount = streamReader.Read<short>();
                    
                    for(int i = 0; i < structCount; i++)
                    {
                        int id = streamReader.Read<byte>();
                        int subStructDataType = streamReader.Read<byte>();
                        switch(subStructDataType)
                        {
                            case 0x8: //DOC or SLCT data - Two shorts possibly? Seemingly fine to read as an int32 since short 2 seems blank in character files
                                tagStructs[id] = new int[] { streamReader.Read<int>() };
                                break;
                            case 0x48: //FIGR or COLR data - Array of 3 int32s
                                streamReader.Seek(0x1, SeekOrigin.Current); //Skip byte 0x1 that follows 0x48
                                tagStructs[id] = new int[] { streamReader.Read<int>(), streamReader.Read<int>(), streamReader.Read<int>() };
                                break;
                            case 0x83: //OFST data - Array of 4 signed bytes, first byte always observed as 0x2. 
                                streamReader.Seek(0x1, SeekOrigin.Current); //Skip byte 0x8 that follows 0x83
                                tagStructs[id] = new int[] { streamReader.Read<sbyte>(), streamReader.Read<sbyte>(), streamReader.Read<sbyte>(), streamReader.Read<sbyte>() };
                                break;
                            default:
                                MessageBox.Show(inFilename + " " + subStructDataType + " Position: " + streamReader.Position());
                                break;
                        }
                    }
                }
            }

            //Throw together an xxpGeneral based on the dictionary. Main thing to watch for is the accessory 
            xxpGeneral = new XXPGeneral();
            int[] value = { 0, 0, 0};

            //DOC
            ArrayGetValue(tagStructs, 0x70, out value); xxpGeneral.baseDOC.race = (uint)value[0];
            fileLoadRace = (int)xxpGeneral.baseDOC.race;
            ArrayGetValue(tagStructs, 0x71, out value); xxpGeneral.baseDOC.gender = (uint)value[0];
            ArrayGetValue(tagStructs, 0x72, out value); xxpGeneral.baseDOC.muscleMass = value[0];
            ArrayGetValue(tagStructs, 0x73, out value); xxpGeneral.cmlVariant = (byte)value[0];
            ArrayGetValue(tagStructs, 0x74, out value); xxpGeneral.skinVariant = (byte)value[0];
            ArrayGetValue(tagStructs, 0x75, out value); xxpGeneral.eyebrowDensity = (sbyte)value[0];

            //FIGR
            ArrayGetValue(tagStructs, 0x0, out value); xxpGeneral.baseFIGR.bodyVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x1, out value); xxpGeneral.baseFIGR.armVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x2, out value); xxpGeneral.baseFIGR.legVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x3, out value); xxpGeneral.baseFIGR.bustVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x4, out value); xxpGeneral.baseFIGR.headVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x5, out value); xxpGeneral.baseFIGR.faceShapeVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x6, out value); xxpGeneral.baseFIGR.eyeShapeVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x7, out value); xxpGeneral.baseFIGR.noseHeightVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x8, out value); xxpGeneral.baseFIGR.noseShapeVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x9, out value); xxpGeneral.baseFIGR.mouthVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0xA, out value); xxpGeneral.baseFIGR.ear_hornVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0xC, out value); xxpGeneral.baseFIGR2.neckVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0xD, out value); xxpGeneral.baseFIGR2.waistVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x10, out value); xxpGeneral.baseFIGR2.body2Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x11, out value); xxpGeneral.baseFIGR2.arm2Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x12, out value); xxpGeneral.baseFIGR2.leg2Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x13, out value); xxpGeneral.baseFIGR2.bust2Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x14, out value); xxpGeneral.baseFIGR2.neck2Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x15, out value); xxpGeneral.baseFIGR2.waist2Verts = Vec3Int.CreateVec3Int(value);

            //Fill in accessory values
            for (int i = 0; i < 36; i++)
            {
                xxpGeneral.accessorySliders[i] = 0;
            }
            for (int i = 0; i < 18; i++)
            {
                xxpGeneral.accessorySlidersv6[i] = 0;
            }

            //Check if we're using the newer format or not
            if (xxpGeneral.cmlVariant >= 0x5)
            {
                //Set up v9 accessories
                int typeIterator = 0;
                for (int i = 0; i < 12; i++)
                {
                    switch (typeIterator)
                    {
                        case 4:
                            typeIterator = 1;
                            break;
                        case 8:
                            typeIterator = 2;
                            break;
                        default:
                            break;
                    }

                    ArrayGetValue(tagStructs, 0x90 + i, out value); List<byte> byteList = (BitConverter.GetBytes(value[0])).ToList();
                    xxpGeneral.accessorySlidersv6[i * 3 + typeIterator] = byteList.ElementAtOrDefault(0);
                    xxpGeneral.accessorySlidersv6[i * 3 + typeIterator + 1] = byteList.ElementAtOrDefault(1);
                    xxpGeneral.accessorySlidersv6[i * 3 + typeIterator + 2] = byteList.ElementAtOrDefault(2);
                }

                ConvertV9toV6Sliders();
            }
            else
            {
                //Set up v6 accessories
                for (int i = 0; i < 9; i++)
                {
                    ArrayGetValue(tagStructs, 0x80 + i, out value); List<byte> byteList = (BitConverter.GetBytes(value[0])).ToList();
                    xxpGeneral.accessorySlidersv6[i * 2] = byteList.ElementAtOrDefault(0);
                    xxpGeneral.accessorySlidersv6[i * 2 + 1] = byteList.ElementAtOrDefault(1);
                }

                SwapV6RotationScale();

                ConvertV6toV9Sliders();
            }

            //COLR
            ArrayGetValue(tagStructs, 0x27, out value); xxpGeneral.baseCOLR.outer_MainColorVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x20, out value); xxpGeneral.baseCOLR.costumeColorVerts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x21, out value); xxpGeneral.baseCOLR.mainColor_hair2Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x22, out value); xxpGeneral.baseCOLR.subColor1Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x23, out value); xxpGeneral.baseCOLR.skinSubColor2Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x24, out value); xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts = Vec3Int.CreateVec3Int(value);
            ArrayGetValue(tagStructs, 0x25, out value); xxpGeneral.baseCOLR.rightEye_EyesVerts = Vec3Int.CreateVec3Int(value);

            //Technically in the data twice, but we only read it once. It's copy pasted in all observed instances
            ArrayGetValue(tagStructs, 0x26, out value); xxpGeneral.baseCOLR.hairVerts = Vec3Int.CreateVec3Int(value);

            //SLCT
            ArrayGetValue(tagStructs, 0x40, out value); xxpGeneral.baseSLCT.costumePart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x41, out value); xxpGeneral.baseSLCT.bodyPaintPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x42, out value); xxpGeneral.baseSLCT.stickerPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x44, out value); xxpGeneral.baseSLCT.eyebrowPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x45, out value); xxpGeneral.baseSLCT.eyelashPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x46, out value); xxpGeneral.baseSLCT.faceTypePart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x47, out value); xxpGeneral.baseSLCT.faceTexPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x48, out value); xxpGeneral.baseSLCT.makeup1Part = (uint)value[0];
            ArrayGetValue(tagStructs, 0x49, out value); xxpGeneral.baseSLCT.hairPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x50, out value); xxpGeneral.baseSLCT.acc1Part = (uint)value[0];
            ArrayGetValue(tagStructs, 0x51, out value); xxpGeneral.baseSLCT.acc2Part = (uint)value[0];
            ArrayGetValue(tagStructs, 0x52, out value); xxpGeneral.baseSLCT.acc3Part = (uint)value[0];
            ArrayGetValue(tagStructs, 0x53, out value); xxpGeneral.baseSLCT.makeup2Part = (uint)value[0];
            ArrayGetValue(tagStructs, 0x54, out value); xxpGeneral.baseSLCT.legPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x55, out value); xxpGeneral.baseSLCT.armPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x56, out value); xxpGeneral.baseSLCT2.acc4Part = (uint)value[0];
            ArrayGetValue(tagStructs, 0x57, out value); xxpGeneral.baseSLCT2.basewearPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x58, out value); xxpGeneral.baseSLCT2.innerwearPart = (uint)value[0];
            ArrayGetValue(tagStructs, 0x59, out value); xxpGeneral.baseSLCT2.bodypaint2Part = (uint)value[0];

            //Handle eye part
            ArrayGetValue(tagStructs, 0x43, out value);
            byte[] eyes = BitConverter.GetBytes(value[0]);

            if (tagStructs.ContainsKey(0x63))
            {
                ArrayGetValue(tagStructs, 0x43, out value); xxpGeneral.baseSLCT.eyePart = BitConverter.ToUInt32(eyes, 0);
                ArrayGetValue(tagStructs, 0x63, out value); xxpGeneral.leftEyePart = (uint)BitConverter.ToInt16(eyes, 0);
            }
            else
            {
                if(xxpGeneral.cmlVariant >= 0x9)
                {
                    ArrayGetValue(tagStructs, 0x43, out value); xxpGeneral.baseSLCT.eyePart = BitConverter.ToUInt32(eyes, 0);
                } else
                {
                    xxpGeneral.baseSLCT.eyePart = eyes[0];
                    xxpGeneral.leftEyePart = eyes[1];
                }
            }


            //SLCT 2 - Body Paint Priority
            ArrayGetValue(tagStructs, 0x60, out value); xxpGeneral.paintPriority.priority1 = (ushort)value[0];
            ArrayGetValue(tagStructs, 0x61, out value); xxpGeneral.paintPriority.priority2 = (ushort)value[0];
            ArrayGetValue(tagStructs, 0x62, out value); xxpGeneral.paintPriority.priority3 = (ushort)value[0];

            if (fh != null)
            {
                fh.GenerateFilenames(xxpGeneral);
            }
        }

        private void SwapV6RotationScale()
        {
            //Swap Rotation and Scale to get consistency with other formats
            byte[] temp = {xxpGeneral.accessorySlidersv6[6], xxpGeneral.accessorySlidersv6[7], xxpGeneral.accessorySlidersv6[8],
                    xxpGeneral.accessorySlidersv6[9], xxpGeneral.accessorySlidersv6[10], xxpGeneral.accessorySlidersv6[11] };

            for (int i = 6; i < 12; i++)
            {
                xxpGeneral.accessorySlidersv6[i] = xxpGeneral.accessorySlidersv6[i + 6];
            }
            for (int i = 12; i < 18; i++)
            {
                xxpGeneral.accessorySlidersv6[i] = temp[i - 12];
            }
        }

        private void ConvertV6toV9Sliders()
        {
            //Set up v9 accessories
            for (int i = 0; i < 3; i++)
            {
                //Accessory 1 Y+Z
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6], out xxpGeneral.accessorySliders[i * 12 + 1], out xxpGeneral.accessorySliders[i * 12 + 2]);
                //Accessory 4 X + Accessory 1 X
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 1], out xxpGeneral.accessorySliders[i * 12 + 9], out xxpGeneral.accessorySliders[i * 12]);
                //Accessory 2 Y+Z
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 2], out xxpGeneral.accessorySliders[i * 12 + 4], out xxpGeneral.accessorySliders[i * 12 + 5]);
                //Accessory 4 Y + Accessory 2 X
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 3], out xxpGeneral.accessorySliders[i * 12 + 10], out xxpGeneral.accessorySliders[i * 12 + 3]);
                //Accessory 3 Y+Z
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 4], out xxpGeneral.accessorySliders[i * 12 + 7], out xxpGeneral.accessorySliders[i * 12 + 8]);
                //Accessory 4 Y + Accessory 3 X
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 5], out xxpGeneral.accessorySliders[i * 12 + 11], out xxpGeneral.accessorySliders[i * 12 + 6]);
            }
        }

        private void ConvertV9toV6Sliders()
        {
            //Set up v6 accessories
            for (int i = 0; i < 3; i++)
            {
                //Accessory 1 Y+Z
                xxpGeneral.accessorySlidersv6[i * 6] = SignednibblePack(xxpGeneral.accessorySliders[i * 12 + 1], xxpGeneral.accessorySliders[i * 12 + 2]);
                //Accessory 4 X + Accessory 1 X
                xxpGeneral.accessorySlidersv6[i * 6 + 1] = SignednibblePack(xxpGeneral.accessorySliders[i * 12 + 9], xxpGeneral.accessorySliders[i * 12]);

                //Accessory 2 Y+Z
                xxpGeneral.accessorySlidersv6[i * 6 + 2] = SignednibblePack(xxpGeneral.accessorySliders[i * 12 + 4], xxpGeneral.accessorySliders[i * 12 + 5]);
                //Accessory 4 Y + Accessory 2 X
                xxpGeneral.accessorySlidersv6[i * 6 + 3] = SignednibblePack(xxpGeneral.accessorySliders[i * 12 + 10], xxpGeneral.accessorySliders[i * 12 + 3]);

                //Accessory 3 Y+Z
                xxpGeneral.accessorySlidersv6[i * 6 + 4] = SignednibblePack(xxpGeneral.accessorySliders[i * 12 + 7], xxpGeneral.accessorySliders[i * 12 + 8]);
                //Accessory 4 Y + Accessory 3 X
                xxpGeneral.accessorySlidersv6[i * 6 + 5] = SignednibblePack(xxpGeneral.accessorySliders[i * 12 + 11], xxpGeneral.accessorySliders[i * 12 + 6]);
            }
        }

        public void ParseToStruct(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                this.version = streamReader.Read<int>();
                streamReader.Seek(0xC

                    , SeekOrigin.Current);
                fileLoadRace = streamReader.Read<int>();
                streamReader.Seek(0x10, SeekOrigin.Begin);

                xxpGeneral = new XXPGeneral();
                switch (this.version)
                {
                    case 2:
                        ReadV2(streamReader);
                        break;
                    case 5:
                        ReadV5(streamReader);
                        break;
                    case 6:
                    case 7:
                        ReadV6_V7(streamReader);
                        break;
                    case 8:
                    case 9:
                        ReadV8_V9(streamReader);
                        break;
                    default:
                        MessageBox.Show("Error: File version unknown. If this is a proper salon file, please report this!",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

            }
            if (fh != null)
            {
                fh.GenerateFilenames(xxpGeneral);
            }
        }

        public void OutputToText(string filename)
        {
            using (StreamWriter file = new StreamWriter(filename))
            {
                file.WriteLine("***.xxp/.cml Character Data Dump***");

                file.WriteLine("DOC");
                file.WriteLine($"Race: {xxpGeneral.baseDOC.race} (0-3, Human, Newman, Cast, Deuman)");
                file.WriteLine($"Gender: {xxpGeneral.baseDOC.gender} (0 = male, 1 = female)");
                file.WriteLine($"Muscle mass: {xxpGeneral.baseDOC.muscleMass}");
                file.WriteLine($"Cml variant: {xxpGeneral.cmlVariant}");
                file.WriteLine($"Eyebrow density: {xxpGeneral.eyebrowDensity} ");
                file.WriteLine($"Skin variant: {xxpGeneral.skinVariant} (Determines which race's skin slider to use; Same values as race)");
                file.WriteLine("");

                file.WriteLine("FIGR (Output in slider vertex positions. Vertex 1 Y, vertex 2 X, vertex 2 Y)");
                file.Write($"Body verts: "); foreach (int i in xxpGeneral.baseFIGR.bodyVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Arm verts: "); foreach (int i in xxpGeneral.baseFIGR.armVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Leg verts: "); foreach (int i in xxpGeneral.baseFIGR.legVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Bust verts: "); foreach (int i in xxpGeneral.baseFIGR.bustVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Head verts: "); foreach (int i in xxpGeneral.baseFIGR.headVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Face shape verts: "); foreach (int i in xxpGeneral.baseFIGR.faceShapeVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Eye shape verts: "); foreach (int i in xxpGeneral.baseFIGR.eyeShapeVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Nose height verts: "); foreach (int i in xxpGeneral.baseFIGR.noseHeightVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Nose shape verts: "); foreach (int i in xxpGeneral.baseFIGR.noseShapeVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Mouth verts: "); foreach (int i in xxpGeneral.baseFIGR.mouthVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Ear/horn verts: "); foreach (int i in xxpGeneral.baseFIGR.ear_hornVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");

                file.Write($"Neck verts: "); foreach (int i in xxpGeneral.baseFIGR2.neckVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Waist verts: "); foreach (int i in xxpGeneral.baseFIGR2.waistVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Body2 verts: "); foreach (int i in xxpGeneral.baseFIGR2.body2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine(" (These are cast body proportions)");
                file.Write($"Arm2 verts: "); foreach (int i in xxpGeneral.baseFIGR2.arm2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Leg2 verts: "); foreach (int i in xxpGeneral.baseFIGR2.leg2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Bust2 verts: "); foreach (int i in xxpGeneral.baseFIGR2.bust2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Neck2 verts: "); foreach (int i in xxpGeneral.baseFIGR2.neck2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Waist2 verts: "); foreach (int i in xxpGeneral.baseFIGR2.waist2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.WriteLine("");

                file.WriteLine("COLR (Output in slider vertex positions. Vertex 1 Y, vertex 2 X, vertex 2 Y)");
                file.Write($"Outer/main color verts: "); foreach (int i in xxpGeneral.baseCOLR.outer_MainColorVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Costume color verts: "); foreach (int i in xxpGeneral.baseCOLR.costumeColorVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Main/hair2 color verts: "); foreach (int i in xxpGeneral.baseCOLR.mainColor_hair2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Sub color 1 verts: "); foreach (int i in xxpGeneral.baseCOLR.subColor1Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Skin color/sub color 2 verts: "); foreach (int i in xxpGeneral.baseCOLR.skinSubColor2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Left eye color/ cast hair color/sub color 3 verts: "); foreach (int i in xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Eyes/right eye color verts: "); foreach (int i in xxpGeneral.baseCOLR.rightEye_EyesVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.Write($"Hair color verts: "); foreach (int i in xxpGeneral.baseCOLR.hairVerts.GetAsArray()) { file.Write(i + " "); } file.WriteLine("");
                file.WriteLine("");

                file.WriteLine("SLCT (Id per tag type in .cmx)");
                file.WriteLine($"Costume/Outerwear/Cast Body Part: {xxpGeneral.baseSLCT.costumePart} (first digit of the 5 possible determines category)");
                file.WriteLine($"Body Paint 1: {xxpGeneral.baseSLCT.bodyPaintPart}");
                file.WriteLine($"Sticker: {xxpGeneral.baseSLCT.stickerPart}");
                file.WriteLine($"Eyes/Deuman Right Eye: {xxpGeneral.baseSLCT.eyePart}");
                file.WriteLine($"Deuman Left Eye: {xxpGeneral.leftEyePart}");
                file.WriteLine($"Eyebrow: {xxpGeneral.baseSLCT.eyebrowPart}");
                file.WriteLine($"Eyelash: {xxpGeneral.baseSLCT.eyelashPart}");
                file.WriteLine($"Face Type: {xxpGeneral.baseSLCT.faceTypePart}");
                file.WriteLine($"Face Tex: {xxpGeneral.baseSLCT.faceTexPart}");
                file.WriteLine($"Makeup 1: {xxpGeneral.baseSLCT.makeup1Part}");
                file.WriteLine($"Hair: {xxpGeneral.baseSLCT.hairPart}");
                file.WriteLine($"Accessory 1: {xxpGeneral.baseSLCT.acc1Part}");
                file.WriteLine($"Accessory 2: {xxpGeneral.baseSLCT.acc2Part}");
                file.WriteLine($"Accessory 3: {xxpGeneral.baseSLCT.acc3Part}");
                file.WriteLine($"Makeup 2: {xxpGeneral.baseSLCT.makeup2Part}");
                file.WriteLine($"Cast Leg Part: {xxpGeneral.baseSLCT.legPart}");
                file.WriteLine($"Cast Arm Part: {xxpGeneral.baseSLCT.armPart}");

                file.WriteLine($"Accessory 4: {xxpGeneral.baseSLCT2.acc4Part}");
                file.WriteLine($"Basewear: {xxpGeneral.baseSLCT2.basewearPart}");
                file.WriteLine($"Innerwear: {xxpGeneral.baseSLCT2.innerwearPart}");
                file.WriteLine($"Body Paint 2: {xxpGeneral.baseSLCT2.bodypaint2Part}");
                file.WriteLine("");

                file.WriteLine("OFST (Accessory sliders)");
                for(int i = 0; i < 36; i++)
                {
                    switch(i)
                    {
                        case 0:
                            file.Write($"Accessory 1 Position: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 3:
                            file.Write($"\nAccessory 2 Position: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 6:
                            file.Write($"\nAccessory 3 Position: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 9:
                            file.Write($"\nAccessory 4 Position: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 12:
                            file.Write($"\nAccessory 1 Rotation: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 15:
                            file.Write($"\nAccessory 2 Rotation: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 18:
                            file.Write($"\nAccessory 3 Rotation: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 21:
                            file.Write($"\nAccessory 4 Rotation: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 24:
                            file.Write($"\nAccessory 1 Scale: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 27:
                            file.Write($"\nAccessory 2 Scale: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 30:
                            file.Write($"\nAccessory 3 Scale: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        case 33:
                            file.Write($"\nAccessory 4 Scale: {xxpGeneral.accessorySliders[i]} ");
                            break;
                        default:
                            file.Write($"{xxpGeneral.accessorySliders[i]} ");
                            break;
                    }
                }
                file.WriteLine("");
                file.WriteLine("");

                file.WriteLine("Body Paint Priority (0 = inner, 1 = body paint 1, 2 = body paint 2)");
                file.WriteLine($"Priority 1: {xxpGeneral.paintPriority.priority1}");
                file.WriteLine($"Priority 2: {xxpGeneral.paintPriority.priority2}");
                file.WriteLine($"Priority 3: {xxpGeneral.paintPriority.priority3}");

                if(fh != null)
                {
                    file.WriteLine("");
                    file.WriteLine("");
                    file.WriteLine("Character Files (Indices adjusted to file indices):");
                    file.WriteLine($"Costume/Outerwear/Cast Body Part: {fh.costumePart} {fh.GetFileHash(fh.costumePart)}");
                    file.WriteLine($"Body Paint 1: {fh.bodyPaintPart} {fh.GetFileHash(fh.bodyPaintPart)}");
                    file.WriteLine($"Sticker: {fh.stickerPart} {fh.GetFileHash(fh.stickerPart)}");
                    file.WriteLine($"Eyes/Deuman Right Eye: {fh.eyePart} {fh.GetFileHash(fh.eyePart)}");
                    file.WriteLine($"Deuman Left Eye: {fh.leftEyePart} {fh.GetFileHash(fh.leftEyePart)}");
                    file.WriteLine($"Eyebrow: {fh.eyebrowPart} {fh.GetFileHash(fh.eyebrowPart)}");
                    file.WriteLine($"Eyelash: {fh.eyelashPart} {fh.GetFileHash(fh.eyelashPart)}");
                    file.WriteLine($"Face Type: {fh.faceTypePart} {fh.GetFileHash(fh.faceTypePart)}");
                    file.WriteLine($"Face Tex: {fh.faceTexPart} {fh.GetFileHash(fh.faceTexPart)}");
                    file.WriteLine($"Makeup 1: {fh.makeup1Part} {fh.GetFileHash(fh.makeup1Part)}");
                    file.WriteLine($"Hair: {fh.hairPart} {fh.GetFileHash(fh.hairPart)}");
                    file.WriteLine($"Accessory 1: {fh.acc1Part} {fh.GetFileHash(fh.acc1Part)}");
                    file.WriteLine($"Accessory 2: {fh.acc2Part} {fh.GetFileHash(fh.acc2Part)}");
                    file.WriteLine($"Accessory 3: {fh.acc3Part} {fh.GetFileHash(fh.acc3Part)}");
                    file.WriteLine($"Makeup 2: {fh.makeup2Part} {fh.GetFileHash(fh.makeup2Part)}");
                    file.WriteLine($"Cast Leg Part: {fh.legPart} {fh.GetFileHash(fh.legPart)}");
                    file.WriteLine($"Cast Arm Part: {fh.armPart} {fh.GetFileHash(fh.armPart)}");
                    file.WriteLine($"Accessory 4: {fh.acc4Part} {fh.GetFileHash(fh.acc4Part)}");
                    file.WriteLine($"Basewear: {fh.basewearPart} {fh.GetFileHash(fh.basewearPart)}");
                    file.WriteLine($"Innerwear: {fh.innerwearPart} {fh.GetFileHash(fh.innerwearPart)}");
                    file.WriteLine($"Body Paint 2: {fh.bodypaint2Part} {fh.GetFileHash(fh.bodypaint2Part)}");
                }
            }
        }

        public void ParsePacket(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                this.version = streamReader.Read<short>();
                xxpGeneral.baseDOC = new BaseDOC();
                xxpGeneral.baseDOC.race = streamReader.Read<ushort>();
                xxpGeneral.baseDOC.gender = streamReader.Read<ushort>();
                xxpGeneral.baseDOC.muscleMass = streamReader.Read<ushort>();
                //xxpGeneral.eyebrowDensity =
                //xxpGeneral.skinVariant =

                xxpGeneral.baseFIGR = new BaseFIGR();
                xxpGeneral.baseFIGR.bodyVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.armVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.legVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.bustVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.headVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.faceShapeVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.eyeShapeVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.noseHeightVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.noseShapeVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.mouthVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR.ear_hornVerts = Vec3IntFromStreamShorts(streamReader);

                xxpGeneral.baseFIGR2 = new BaseFIGR2();
                xxpGeneral.baseFIGR2.neckVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR2.waistVerts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR2.body2Verts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR2.arm2Verts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR2.leg2Verts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR2.bust2Verts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR2.neck2Verts = Vec3IntFromStreamShorts(streamReader);
                xxpGeneral.baseFIGR2.waist2Verts = Vec3IntFromStreamShorts(streamReader);

                streamReader.Seek(0x2A, SeekOrigin.Current);

                for (int i = 0; i < 12; i++)
                {
                    xxpGeneral.accessorySliders[i] = streamReader.Read<sbyte>();
                }

                xxpGeneral.baseCOLR = new BaseCOLR();
                xxpGeneral.baseCOLR.outer_MainColorVerts = Vec3IntFromStreamUshorts(streamReader);
                xxpGeneral.baseCOLR.costumeColorVerts = Vec3IntFromStreamUshorts(streamReader);
                xxpGeneral.baseCOLR.mainColor_hair2Verts = Vec3IntFromStreamUshorts(streamReader);
                xxpGeneral.baseCOLR.subColor1Verts = Vec3IntFromStreamUshorts(streamReader);
                xxpGeneral.baseCOLR.skinSubColor2Verts = Vec3IntFromStreamUshorts(streamReader);
                xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts = Vec3IntFromStreamUshorts(streamReader);
                xxpGeneral.baseCOLR.rightEye_EyesVerts = Vec3IntFromStreamUshorts(streamReader);
                xxpGeneral.baseCOLR.hairVerts = Vec3IntFromStreamUshorts(streamReader);

                streamReader.Seek(0x30, SeekOrigin.Current);

                xxpGeneral.baseSLCT = new BaseSLCT();
                xxpGeneral.baseSLCT.costumePart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.bodyPaintPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.stickerPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.eyePart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.eyebrowPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.eyelashPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.faceTypePart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.faceTexPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.makeup1Part = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.hairPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.acc1Part = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.acc2Part = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.acc3Part = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.makeup2Part = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.legPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT.armPart = streamReader.Read<ushort>();

                xxpGeneral.baseSLCT2 = new BaseSLCT2();
                xxpGeneral.baseSLCT2.acc4Part = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT2.basewearPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT2.innerwearPart = streamReader.Read<ushort>();
                xxpGeneral.baseSLCT2.bodypaint2Part = streamReader.Read<ushort>();
                xxpGeneral.leftEyePart = streamReader.Read<ushort>();

                streamReader.Seek(0x14, SeekOrigin.Current);

                for (int i = 12; i < 36; i++)
                {
                    xxpGeneral.accessorySliders[i] = streamReader.Read<sbyte>();
                }

                xxpGeneral.paintPriority = new PaintPriority();
                xxpGeneral.paintPriority.priority1 = streamReader.Read<byte>();
                xxpGeneral.paintPriority.priority2 = streamReader.Read<byte>();
                xxpGeneral.paintPriority.priority3 = streamReader.Read<byte>();

                if(xxpGeneral.paintPriority.priority1 == xxpGeneral.paintPriority.priority2 || xxpGeneral.paintPriority.priority1 == xxpGeneral.paintPriority.priority3
                    || xxpGeneral.paintPriority.priority2 == xxpGeneral.paintPriority.priority3)
                {
                    //Fix priorities as needed
                    xxpGeneral.paintPriority.priority1 = 0;
                    xxpGeneral.paintPriority.priority2 = 1;
                    xxpGeneral.paintPriority.priority3 = 2;
                }

                ConvertV9toV6Sliders();
            }
            if (fh != null)
            {
                fh.GenerateFilenames(xxpGeneral);
            }
        }

        private Vec3Int Vec3IntFromStreamShorts(BufferedStreamReader streamReader)
        {
            var vec3 = new Vec3Int();
            vec3.X = streamReader.Read<short>();
            vec3.Y = streamReader.Read<short>();
            vec3.Z = streamReader.Read<short>();

            return vec3;
        }

        private Vec3Int Vec3IntFromStreamUshorts(BufferedStreamReader streamReader)
        {
            var vec3 = new Vec3Int();
            vec3.X = streamReader.Read<ushort>();
            vec3.Y = streamReader.Read<ushort>();
            vec3.Z = streamReader.Read<ushort>();

            return vec3;
        }

        public void ReadV2(BufferedStreamReader streamReader)
        {
            xxpGeneral.baseDOC = streamReader.Read<BaseDOC>();
            xxpGeneral.baseFIGR = streamReader.Read<BaseFIGR>();
            xxpGeneral.baseFIGR2 = new BaseFIGR2();
            xxpGeneral.baseFIGR2.arm2Verts = xxpGeneral.baseFIGR.armVerts;
            xxpGeneral.baseFIGR2.body2Verts = xxpGeneral.baseFIGR.bodyVerts;
            xxpGeneral.baseFIGR2.leg2Verts = xxpGeneral.baseFIGR.legVerts;
            xxpGeneral.baseFIGR2.bust2Verts = xxpGeneral.baseFIGR.headVerts;
            xxpGeneral.baseFIGR2.neckVerts = new Vec3Int();
            xxpGeneral.baseFIGR2.neck2Verts = new Vec3Int();
            xxpGeneral.baseFIGR2.waistVerts = new Vec3Int();
            xxpGeneral.baseFIGR2.waist2Verts = new Vec3Int();
            streamReader.Seek(0x24, SeekOrigin.Current);

            xxpGeneral.baseCOLR = streamReader.Read<BaseCOLR>();
            xxpGeneral.baseSLCT = streamReader.Read<BaseSLCT>();

            SeparateEyes();

            xxpGeneral.baseSLCT2 = new BaseSLCT2();

            xxpGeneral.paintPriority = new PaintPriority();

            for (int i = 0; i < 36; i++)
            {
                xxpGeneral.accessorySliders[i] = 0;
            }
            for (int i = 0; i < 18; i++)
            {
                xxpGeneral.accessorySlidersv6[i] = 0;
            }
        }


        public void ReadV5(BufferedStreamReader streamReader)
        {
            xxpGeneral.baseDOC = streamReader.Read<BaseDOC>();
            xxpGeneral.baseFIGR = streamReader.Read<BaseFIGR>();
            xxpGeneral.baseFIGR2 = new BaseFIGR2();
            xxpGeneral.baseFIGR2.arm2Verts = xxpGeneral.baseFIGR.armVerts;
            xxpGeneral.baseFIGR2.body2Verts = xxpGeneral.baseFIGR.bodyVerts;
            xxpGeneral.baseFIGR2.leg2Verts = xxpGeneral.baseFIGR.legVerts;
            xxpGeneral.baseFIGR2.bust2Verts = xxpGeneral.baseFIGR.headVerts;
            xxpGeneral.baseFIGR2.neckVerts = new Vec3Int();
            xxpGeneral.baseFIGR2.neck2Verts = new Vec3Int();
            xxpGeneral.baseFIGR2.waistVerts = new Vec3Int();
            xxpGeneral.baseFIGR2.waist2Verts = new Vec3Int();

            streamReader.Seek(0x24, SeekOrigin.Current);

            xxpGeneral.baseCOLR = streamReader.Read<BaseCOLR>();
            xxpGeneral.baseSLCT = streamReader.Read<BaseSLCT>();

            SeparateEyes();

            xxpGeneral.baseSLCT2 = streamReader.Read<BaseSLCT2>();

            xxpGeneral.paintPriority = new PaintPriority();

            for (int i = 0; i < 36; i++)
            {
                xxpGeneral.accessorySliders[i] = 0;
            }
            for (int i = 0; i < 18; i++)
            {
                xxpGeneral.accessorySlidersv6[i] = 0;
            }

            streamReader.Seek(0x4, SeekOrigin.Current);
            for (int i = 0; i < 6; i++)
            {
                xxpGeneral.accessorySlidersv6[i] = streamReader.Read<byte>();
            }
            for (int i = 0; i < 1; i++)
            {
                //Accessory 1 Y+Z
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6], out xxpGeneral.accessorySliders[i * 12 + 1], out xxpGeneral.accessorySliders[i * 12 + 2]);
                //Accessory 4 X + Accessory 1 X
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 1], out xxpGeneral.accessorySliders[i * 12 + 9], out xxpGeneral.accessorySliders[i * 12]);
                //Accessory 2 Y+Z
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 2], out xxpGeneral.accessorySliders[i * 12 + 4], out xxpGeneral.accessorySliders[i * 12 + 5]);
                //Accessory 4 Y + Accessory 2 X
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 3], out xxpGeneral.accessorySliders[i * 12 + 10], out xxpGeneral.accessorySliders[i * 12 + 3]);
                //Accessory 3 Y+Z
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 4], out xxpGeneral.accessorySliders[i * 12 + 7], out xxpGeneral.accessorySliders[i * 12 + 8]);
                //Accessory 4 Y + Accessory 3 X
                SignednibbleUnpack(xxpGeneral.accessorySlidersv6[i * 6 + 5], out xxpGeneral.accessorySliders[i * 12 + 11], out xxpGeneral.accessorySliders[i * 12 + 6]);
            }
        }

        public void ReadV6_V7(BufferedStreamReader streamReader)
        {
            xxpGeneral.baseDOC = streamReader.Read<BaseDOC>();
            xxpGeneral.baseFIGR = streamReader.Read<BaseFIGR>();
            xxpGeneral.baseFIGR2 = streamReader.Read<BaseFIGR2>();

            streamReader.Seek(0x6C, SeekOrigin.Current);

            xxpGeneral.baseCOLR = streamReader.Read<BaseCOLR>();

            streamReader.Seek(0x78, SeekOrigin.Current);

            xxpGeneral.baseSLCT = streamReader.Read<BaseSLCT>();

            SeparateEyes();

            xxpGeneral.baseSLCT2 = streamReader.Read<BaseSLCT2>();

            streamReader.Seek(0x30, SeekOrigin.Current);

            if (this.version > 6)
            {
                ReadNewAccSliders(streamReader);
            } else
            {
                for (int i = 0; i < 18; i++)
                {
                    xxpGeneral.accessorySlidersv6[i] = streamReader.Read<byte>();
                }
                ConvertV6toV9Sliders();
            }
            
            xxpGeneral.paintPriority = streamReader.Read<PaintPriority>();
        }


        public void ReadV8_V9(BufferedStreamReader streamReader)
        {
            xxpGeneral.baseDOC = streamReader.Read<BaseDOC>();
            xxpGeneral.skinVariant = streamReader.Read<byte>();
            xxpGeneral.eyebrowDensity = streamReader.Read<sbyte>();
            xxpGeneral.cmlVariant = streamReader.Read<short>();

            xxpGeneral.baseFIGR = streamReader.Read<BaseFIGR>();
            xxpGeneral.baseFIGR2 = streamReader.Read<BaseFIGR2>();

            streamReader.Seek(0x6C, SeekOrigin.Current);

            xxpGeneral.baseCOLR = streamReader.Read<BaseCOLR>();

            streamReader.Seek(0x78, SeekOrigin.Current);
            
            xxpGeneral.baseSLCT = streamReader.Read<BaseSLCT>();
            xxpGeneral.baseSLCT2 = streamReader.Read<BaseSLCT2>();
            xxpGeneral.leftEyePart = streamReader.Read<uint>();

            streamReader.Seek(0x2C, SeekOrigin.Current);

            ReadNewAccSliders(streamReader);

            xxpGeneral.paintPriority = streamReader.Read<PaintPriority>();
        }

        private void ReadNewAccSliders(BufferedStreamReader streamReader)
        {
            for (int i = 0; i < 36; i++)
            {
                xxpGeneral.accessorySliders[i] = streamReader.Read<sbyte>();
            }
            ConvertV9toV6Sliders();
        }

        public void SaveCML(string filename)
        {
            using (MemoryStream cml = new MemoryStream())
            {
                //Header
                cml.Write(ConstantCMLHeader, 0, 16); //Always the same in all observed files

                //DOC tag
                cml.Write(Vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x2C), 0, 4);
                cml.Write(DocText, 0, 4);
                cml.Write(BitConverter.GetBytes(0x7),0,2); cml.Write(BitConverter.GetBytes(0x6), 0, 2); //First short is unknown, second is number of structs inside tag

                cml.WriteByte(0x70); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseDOC.race), 0, 4);
                cml.WriteByte(0x71); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseDOC.gender), 0, 4);
                cml.WriteByte(0x72); cml.WriteByte(0x8);

                uint muscles = Convert.ToUInt16(xxpGeneral.baseDOC.muscleMass); //Muscle Mass is a float ranging 0 - 60000 in xxp, but in cml is an integer
                cml.Write(BitConverter.GetBytes(muscles), 0, 4);
                cml.WriteByte(0x73); cml.WriteByte(0x8);
                cml.WriteByte(0x9); cml.WriteByte(0); cml.WriteByte(0); cml.WriteByte(0); //Equivalent Salon File version? Determines mostly how accessory sliders will be interpreted seemingly. Same with Deuman eyes. Use 9
                cml.WriteByte(0x74); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.skinVariant), 0, 1); cml.WriteByte(0); cml.WriteByte(0); cml.WriteByte(0);
                cml.WriteByte(0x75); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.eyebrowDensity), 0, 1); cml.WriteByte(0); cml.WriteByte(0); cml.WriteByte(0);

                //FIGR tag
                cml.Write(Vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x125), 0, 4);
                cml.Write(FigrText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x13), 0, 2);
                cml.WriteByte(0); cml.WriteByte(0x48); cml.WriteByte(0x1); //Second byte here in these is data type
                foreach (int i in xxpGeneral.baseFIGR.bodyVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x1); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.armVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x2); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.legVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x3); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.bustVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x4); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.headVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x5); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.faceShapeVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x6); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.eyeShapeVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x7); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.noseHeightVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x8); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.noseShapeVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x9); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.mouthVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0xA); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR.ear_hornVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0xC); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR2.neckVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0xD); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR2.waistVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x10); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR2.body2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x11); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR2.arm2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x12); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR2.leg2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x13); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR2.bust2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x14); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR2.neck2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x15); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseFIGR2.waist2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }

                //Either OFST or SLCT format can go here for accessory sliders. SLCT offers less range.
                //OFST - Accessory Position 
                cml.Write(Vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x24), 0, 4);
                cml.Write(OfstText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x4), 0, 2);
                cml.WriteByte(0x90); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x91); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 3; i < 6; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x92); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 6; i < 9; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x93); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 9; i < 12; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }

                //OFST - Accessory Scale
                cml.Write(Vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x24), 0, 4);
                cml.Write(OfstText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x4), 0, 2);
                cml.WriteByte(0x95); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 24; i < 27; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }  //In XXP, rotation is stored after scale, unlike here. So we start later in the array
                cml.WriteByte(0x96); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 27; i < 30; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x97); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 30; i < 33; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x98); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 33; i < 36; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }

                //OFST - Accessory Rotation
                cml.Write(Vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x24), 0, 4);
                cml.Write(OfstText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x4), 0, 2);
                cml.WriteByte(0x9B); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 12; i < 15; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x9C); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 15; i < 18; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x9D); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 18; i < 21; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x9E); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 21; i < 24; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }

                //COLR
                cml.Write(Vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x8F), 0, 4);
                cml.Write(ColrText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x9), 0, 2);

                cml.WriteByte(0x27); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseCOLR.outer_MainColorVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x20); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseCOLR.costumeColorVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x21); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseCOLR.mainColor_hair2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x22); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseCOLR.subColor1Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x23); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseCOLR.skinSubColor2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x24); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x25); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseCOLR.rightEye_EyesVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x26); cml.WriteByte(0x48); cml.WriteByte(0x1);
                foreach (int i in xxpGeneral.baseCOLR.hairVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }
                cml.WriteByte(0x26); cml.WriteByte(0x48); cml.WriteByte(0x1);                                               //Yes, they actually write it twice _(:3/
                foreach (int i in xxpGeneral.baseCOLR.hairVerts.GetAsArray()) { cml.Write(BitConverter.GetBytes(i), 0, 4); }

                //SLCT
                cml.Write(Vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x86), 0, 4);
                cml.Write(SlctText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x15), 0, 2);
                cml.WriteByte(0x40); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.costumePart), 0, 4);
                cml.WriteByte(0x41); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.bodyPaintPart), 0, 4);
                cml.WriteByte(0x42); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.stickerPart), 0, 4);
                cml.WriteByte(0x43); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.eyePart), 0, 4);
                cml.WriteByte(0x44); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.eyebrowPart), 0, 4);
                cml.WriteByte(0x45); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.eyelashPart), 0, 4);
                cml.WriteByte(0x46); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.faceTypePart), 0, 4);
                cml.WriteByte(0x47); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.faceTexPart), 0, 4);
                cml.WriteByte(0x48); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.makeup1Part), 0, 4);
                cml.WriteByte(0x49); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.hairPart), 0, 4);
                cml.WriteByte(0x50); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.acc1Part), 0, 4);
                cml.WriteByte(0x51); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.acc2Part), 0, 4);
                cml.WriteByte(0x52); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.acc3Part), 0, 4);
                cml.WriteByte(0x53); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.makeup2Part), 0, 4);
                cml.WriteByte(0x54); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.legPart), 0, 4);
                cml.WriteByte(0x55); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.armPart), 0, 4);
                cml.WriteByte(0x56); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT2.acc4Part), 0, 4);      //Sometimes omitted
                cml.WriteByte(0x57); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT2.basewearPart), 0, 4);
                cml.WriteByte(0x58); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT2.innerwearPart), 0, 4);
                cml.WriteByte(0x59); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT2.bodypaint2Part), 0, 4);
                cml.WriteByte(0x63); cml.WriteByte(0x08);                                    //Absent in non deumans normally
                cml.Write(BitConverter.GetBytes(xxpGeneral.leftEyePart), 0, 4);

                //SLCT 2 - Body Paint Priority
                cml.Write(Vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x1A), 0, 4);
                cml.Write(SlctText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x3), 0, 2);
                cml.WriteByte(0x60); cml.WriteByte(0x08);                                    
                cml.Write(BitConverter.GetBytes(xxpGeneral.paintPriority.priority1), 0, 2); cml.Write(BitConverter.GetBytes(0), 0, 2);
                cml.WriteByte(0x61); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.paintPriority.priority2), 0, 2); cml.Write(BitConverter.GetBytes(0), 0, 2);
                cml.WriteByte(0x62); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.paintPriority.priority3), 0, 2); cml.Write(BitConverter.GetBytes(0), 0, 2);

                File.WriteAllBytes(filename, cml.ToArray());
            }
        }

        private void SeparateEyes()
        {
            byte[] eye = BitConverter.GetBytes(xxpGeneral.baseSLCT.eyePart);
            xxpGeneral.leftEyePart = eye[1];
            eye[1] = 0;
            xxpGeneral.baseSLCT.eyePart = BitConverter.ToUInt32(eye, 0);
        }
        public void SetPso2BinPath()
        {
            CommonOpenFileDialog goodOpenFileDialog = new CommonOpenFileDialog();
            goodOpenFileDialog.IsFolderPicker = true;
            goodOpenFileDialog.Title = "Select your PSO2 install's 'pso2_bin' folder.";
            if (goodOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pso2_binPath = goodOpenFileDialog.FileName;
                File.WriteAllText(pso2_binPathFile, goodOpenFileDialog.FileName);
                LoadCMXData();
            }
        }

        public byte[] SetupV2()
        {
            MemoryStream xxp = new MemoryStream();
            byte[] zeroInt = { 0, 0, 0, 0 };
            byte[] zeroLong = { 0, 0, 0, 0, 0, 0, 0, 0 };

            //DOC
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseDOC), 0, 0xC);

            //FIGR
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseFIGR), 0, sizeof(BaseFIGR));

            //Padding 1
            xxp.Write(zeroInt, 0, 4);
            for (int i = 0; i < 4; i++)
            {
                xxp.Write(zeroLong, 0, 8);
            }

            //COLR
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseCOLR), 0, sizeof(BaseCOLR));

            //SLCT
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT), 0, sizeof(BaseSLCT));

            //Final Padding
            xxp.Write(zeroLong, 0, 8);

            return xxp.ToArray();
        }

        public byte[] SetupV5()
        {
            MemoryStream xxp = new MemoryStream();
            byte[] zeroInt = { 0, 0, 0, 0 };
            byte[] zeroLong = { 0, 0, 0, 0, 0, 0, 0, 0 };

            //DOC
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseDOC), 0, 0xC);

            //FIGR
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseFIGR), 0, sizeof(BaseFIGR));

            //Padding 1
            xxp.Write(zeroInt, 0, 4);
            for (int i = 0; i < 4; i++)
            {
                xxp.Write(zeroLong, 0, 8);
            }

            //COLR
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseCOLR), 0, sizeof(BaseCOLR));

            //SLCT
                //Handle eye combination
            byte[] eye = BitConverter.GetBytes(xxpGeneral.baseSLCT.eyePart);
            eye[1] = BitConverter.GetBytes(xxpGeneral.leftEyePart)[0];
            xxpGeneral.baseSLCT.eyePart = BitConverter.ToUInt32(eye, 0);

            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT), 0, sizeof(BaseSLCT));

                //Convert eye combination back to normal
            eye[1] = 0;
            xxpGeneral.baseSLCT.eyePart = BitConverter.ToUInt32(eye, 0);

            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT2), 0, sizeof(BaseSLCT2));

            //Padding 2
            xxp.Write(zeroInt, 0, 4);

            //Accessory Sliders
            for (int i = 0; i < 6; i++)
            {
                xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.accessorySlidersv6[i]), 0, 1);
            }

            //End Padding
            xxp.WriteByte(0); xxp.WriteByte(0);

            return xxp.ToArray();
        }

        public byte[] SetupV6()
        {
            MemoryStream xxp = new MemoryStream();
            byte[] zeroInt = { 0, 0, 0, 0 };
            byte[] zeroLong = { 0, 0, 0, 0, 0, 0, 0, 0 };

            //DOC
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseDOC), 0, 0xC);

            //FIGR
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseFIGR), 0, sizeof(BaseFIGR));
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseFIGR2), 0, sizeof(BaseFIGR2));

            //Padding 1
            xxp.Write(zeroInt, 0, 4);
            for (int i = 0; i < 13; i++)
            {
                xxp.Write(zeroLong, 0, 8);
            }

            //COLR
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseCOLR), 0, sizeof(BaseCOLR));

            //Padding 2
            for (int i = 0; i < 15; i++)
            {
                xxp.Write(zeroLong, 0, 8);
            }

            //SLCT
                //Handle eye combination
            byte[] eye = BitConverter.GetBytes(xxpGeneral.baseSLCT.eyePart);
            eye[1] = BitConverter.GetBytes(xxpGeneral.leftEyePart)[0];
            xxpGeneral.baseSLCT.eyePart = BitConverter.ToUInt32(eye, 0);

            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT), 0, sizeof(BaseSLCT));

                //Convert eye combination back to normal
            eye[1] = 0;
            xxpGeneral.baseSLCT.eyePart = BitConverter.ToUInt32(eye, 0);

            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT2), 0, sizeof(BaseSLCT2));

            //Padding 3
            for (int i = 0; i < 6; i++)
            {
                xxp.Write(zeroLong, 0, 8);
            }

            //Accessory Sliders
            for (int i = 0; i < 18; i++)
            {
                xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.accessorySlidersv6[i]), 0, 1);
            }

            //Body Paint Priority
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.paintPriority), 0, sizeof(PaintPriority));
            xxp.Write(zeroLong, 0, 8);
            xxp.Write(zeroLong, 0, 8);

            return xxp.ToArray();
        }

        public byte[] SetupV9()
        {
            MemoryStream xxp = new MemoryStream();
            byte[] zeroInt = { 0, 0, 0, 0};
            byte[] zeroLong = { 0, 0, 0, 0, 0, 0, 0, 0};

            //DOC
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseDOC), 0, 0xC);
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.skinVariant), 0, 1);
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.eyebrowDensity), 0, 1);
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.cmlVariant),0, 2);

            //FIGR
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseFIGR), 0, sizeof(BaseFIGR));
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseFIGR2), 0, sizeof(BaseFIGR2));

            //Padding 1
            xxp.Write(zeroInt, 0, 4);
            for(int i = 0; i < 13; i++)
            {
                xxp.Write(zeroLong, 0, 8);
            }

            //COLR
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseCOLR), 0, sizeof(BaseCOLR));

            //Padding 2
            for (int i = 0; i < 15; i++)
            {
                xxp.Write(zeroLong, 0, 8);
            }

            //SLCT
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT), 0, sizeof(BaseSLCT));
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT2), 0, sizeof(BaseSLCT2));
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.leftEyePart), 0, 4);

            //Padding 3
            xxp.Write(zeroInt, 0, 4);
            for (int i = 0; i < 5; i++)
            {
                xxp.Write(zeroLong, 0, 8);
            }

            //Accessory Sliders
            for(int i = 0; i < 36; i++)
            {
                xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1);
            }

            //Body Paint Priority
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.paintPriority), 0, sizeof(PaintPriority));
            xxp.WriteByte(0); xxp.WriteByte(0);
            xxp.Write(zeroInt, 0, 4);
            xxp.Write(zeroLong, 0, 8);

            return xxp.ToArray();
        }

        public int TryGetIndex(int[] arr, int index)
        {
            if( arr.Length - 1 < index)
            {
                return 0;
            } else
            {
                return arr[index];
            }
        }
    }
}
