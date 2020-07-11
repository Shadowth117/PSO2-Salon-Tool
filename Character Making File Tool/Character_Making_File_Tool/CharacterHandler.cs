using Reloaded.Memory.Streams;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using zamboni;

namespace Character_Making_File_Tool
{
    //Thanks to Agrajag for CharacterCrypt.
    //Thanks to Chikinface/Raujinn for .xxp and .cml struct information as well as for code from CMLParser for reference. 
    //Thanks to Rosenblade for helping with UI design.
    unsafe class CharacterHandler
    {
        private static uint CharacterBlowfishKey = 2588334024;
        private static int MinHeightSlider = -5501;
        private static byte[] ConstantCMLHeader = { 0x56,0x54,0x42,0x46,0x10,0,0,0,0x43,0x4D,0x4C,0x50,0x1,0,0,0x4C};
        private int version;
        private Dictionary<int, int> fileSizes = new Dictionary<int, int>()
        {
            //Numbers correlate to save dialog selection numbers
            { 3, 0x15C}, //v2
            { 2, 0x170}, //v5
            { 1, 0x2DC}, //v6
            { 0, 0x2F0}  //v9
        };

        public XXPV2 xxpv2;
        public XXPV5 xxpv5;
        public XXPV6 xxpv6;
        public XXPV9 xxpv9;
        public XXPGeneral xxpGeneral;

        public int fileLoadRace;
        public bool newLoad; //flag to avoid redoing fix changes if they're already done.

        public int getVersion()
        {
            return version;
        }

        public struct CMLHeader
        {
            public int vtbf;  //FileType
            public uint headerSize; //Size of the header data
            public int vtbfType; //Type of VTBF
            public short fileCount; //Number of these files stored in this particular file. AQP Vtbfs sometimes have multiple models inside
            public short unkHeader0; //Always 00 4C for cml
        }

        public struct XXPHeader
        {
            public uint xxpVersion;  //Version of the xxp
            public uint bodySize; //Size of the data after the header
            public uint fileHash; //Crc32 hash of the encrypted file contents after the header
            public uint headerEnd; //0 in all known instances
        }

        public struct BaseDOC
        {
            //Contains important info regarding the type of character.Also has random info that doesn't use int or uint data types. 
            public uint race;
            public uint gender;
            public float muscleMass; //float from 0 to 60000 
        }

        public struct BaseFIGR
        {
            //Slider vertex positions. 10000 to -10000. Center Vert Y, Side Vert X, Side Vert Y
            public fixed int bodyVerts[3];
            public fixed int armVerts[3];
            public fixed int legVerts[3];
            public fixed int bustVerts[3];
            public fixed int unkFIGR0Verts[3]; //?
            public fixed int faceShapeVerts[3];
            public fixed int eyeShapeVerts[3];
            public fixed int noseHeightVerts[3];
            public fixed int noseShapeVerts[3];
            public fixed int mouthVerts[3];
            public fixed int ear_hornVerts[3];
        }

        public struct BaseFIGR2
        {
            //FIGR values added in ep4 to increase customization and add separate fleshy and cast sliders
            public fixed int neckVerts[3]; //?
            public fixed int waistVerts[3];
            public fixed int body2Verts[3];
            public fixed int arm2Verts[3];
            public fixed int leg2Verts[3];
            public fixed int unkFIGR1Verts[3];
            public fixed int neck2Verts[3];
            public fixed int waist2Verts[3];
        }

        public struct BaseCOLR
        {
            //Color vertex positions. 0 to 60000 or 10000 to -10000. X, Y, and Saturation
            public fixed int unkCOLR0Verts[3]; //Changed to copy of Main Color on changing body part allegedly
            public fixed int costumeColorVerts[3];
            public fixed int mainColor_hair2Verts[3];
            public fixed int subColor1Verts[3];
            public fixed int skinSubColor2Verts[3];
            public fixed int subColor3_leftEye_castHair2Verts[3];
            public fixed int rightEye_EyesVerts[3];
            public fixed int hairVerts[3];
        }

        public struct BaseSLCT
        {
            //.cmx (Character Making Index) piece references.
            public uint costumePart;
            public uint bodypaintPart;
            public uint stickerPart;
            public uint rightEyePart; //used for left eye as well prior to v9
            public uint eyebrowPart;
            public uint eyelashPart;
            public uint faceTypePart;
            public uint unknownPart;
            public uint makeup1Part;
            public uint hairPart;
            public uint acc1Part;
            public uint acc2Part;
            public uint acc3Part;
            public uint makeup2Part;
            public uint legPart;
            public uint armPart;
        }

        public struct BaseSLCT2
        {
            // .cmx bits added for EP4
            public uint acc4Part;
            public uint basewearPart;
            public uint innerwearPart;
            public uint bodypaint2Part;
        }

        public struct PaintPriority
        {
            //Body Paint Priority order
            //Innerwear is 0x0, bodypaint1 is 0x1, bodypaint2 is 0x2
            public ushort priority1;
            public ushort priority2;
            public ushort priority3;
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
            public short DOCUnk0;

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
            public short DOCUnk0;

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


        //Expects two fixed int arrays of same length
        public unsafe void ArrayOfIntsSwap(int* array1, int* array2, int length)
        {
            int temp;
            for(int i = 0; i < length; i++)
            {
                temp = array1[i];
                array1[i] = array2[i];
                array2[i] = temp;
            }
        }

        public byte[] DecryptFile(string inFilename)
        {
            byte[] fileVersion = new byte[4];
            byte[] header;
            byte[] sizeBuffer = new byte[4];
            byte[] encryptedData;
            byte[] decryptedData;
            byte[] fileData;
            uint key;
            int dataSize;

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            {
                stream.Read(fileVersion, 0, 4);
                this.version = BitConverter.ToInt32(fileVersion, 0);

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

            #if DEBUG
            DumpData(inFilename, fileData);
            #endif
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

            //Fix cast hair color 2 nonsense
            if ((fileLoadRace == 2 && xxpGeneral.baseDOC.race != fileLoadRace) || (fileLoadRace != 2 && xxpGeneral.baseDOC.race == 2))
            {
                fixed (int* pointer = xxpGeneral.baseFIGR.bodyVerts)
                {
                    fixed (int* pointer2 = xxpGeneral.baseFIGR2.body2Verts)
                    {
                        ArrayOfIntsSwap(pointer, pointer2, 3);
                    }
                }
            }

            //Fix deuman eye color
            if ((fileLoadRace != 3 && xxpGeneral.baseDOC.race == 3))
            {
                fixed (int* pointer = xxpGeneral.baseCOLR.rightEye_EyesVerts)
                {
                    fixed (int* pointer2 = xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts)
                    {
                        ArrayOfIntsSwap(pointer, pointer2, 3);
                    }
                }
            }

            fileLoadRace = (int)xxpGeneral.baseDOC.race;

            //Set heights that are too low to NA's minimum
            if (fixNAHeight == true)
            {
                if (xxpGeneral.baseFIGR.bodyVerts[0] < MinHeightSlider)
                {
                    xxpGeneral.baseFIGR.bodyVerts[0] = MinHeightSlider;
                }
                if (xxpGeneral.baseFIGR2.body2Verts[0] < MinHeightSlider)
                {
                    xxpGeneral.baseFIGR2.body2Verts[0] = MinHeightSlider;
                }
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
                    default:
                        windowVersion = "";
                        MessageBox.Show("Huh... that's not supposed to happen. Sorry about that!");
                        return;
                }
                int fileSize = fileSizes[saveVersion];

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

        public byte[] EncryptData(byte[] body, int size, out int hashInt)
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

        public void ParseCML(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                streamReader.Seek(0x10, SeekOrigin.Begin);
                while(streamReader.CanRead(0x10))
                {
                    streamReader.Seek(0x8, SeekOrigin.Current); //We don't really need the vtc0 information as long as everything else is written correctly.
                    int structType = streamReader.Read<int>();
                    streamReader.Seek(0x2, SeekOrigin.Current);
                    short structCount = streamReader.Read<short>();

                    switch(structType)
                    {
                        case 0x52474946: //FIGR
                            //ReadFIGR(streamReader, structCount);
                            break;
                        default:
                            MessageBox.Show("" + structType + " " + inFilename);
                            break;
                    }
                    /*
                    for(int i = 0; i < structCount; i++)
                    {

                    }*/
                }
            }
        }

        public void ParseToStruct(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                this.version = streamReader.Read<int>();
                streamReader.Seek(0x10, SeekOrigin.Current);
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
                        ReadV6(streamReader);
                        break;
                    case 8:
                    case 9:
                        ReadV9(streamReader);
                        break;
                    default:
                        MessageBox.Show("Error: File version unknown. If this is a proper salon file, please report this!",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

            }
        }

        public void ReadV2(BufferedStreamReader streamReader)
        {
            xxpGeneral.baseDOC = streamReader.Read<BaseDOC>();
            xxpGeneral.baseFIGR = streamReader.Read<BaseFIGR>();
            xxpGeneral.baseFIGR2 = new BaseFIGR2();
            for(int i = 0; i < 3; i++)
            {
                xxpGeneral.baseFIGR2.arm2Verts[i] = xxpGeneral.baseFIGR.armVerts[i];
                xxpGeneral.baseFIGR2.body2Verts[i] = xxpGeneral.baseFIGR.bodyVerts[i];
                xxpGeneral.baseFIGR2.leg2Verts[i] = xxpGeneral.baseFIGR.legVerts[i];
                xxpGeneral.baseFIGR2.unkFIGR1Verts[i] = xxpGeneral.baseFIGR.unkFIGR0Verts[i];
                xxpGeneral.baseFIGR2.neckVerts[i] = 0;
                xxpGeneral.baseFIGR2.neck2Verts[i] = 0;
                xxpGeneral.baseFIGR2.waistVerts[i] = 0;
                xxpGeneral.baseFIGR2.waist2Verts[i] = 0;
            }
            streamReader.Seek(0x24, SeekOrigin.Current);

            xxpGeneral.baseCOLR = streamReader.Read<BaseCOLR>();
            xxpGeneral.baseSLCT = streamReader.Read<BaseSLCT>();
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
            for (int i = 0; i < 3; i++)
            {
                xxpGeneral.baseFIGR2.arm2Verts[i] = xxpGeneral.baseFIGR.armVerts[i];
                xxpGeneral.baseFIGR2.body2Verts[i] = xxpGeneral.baseFIGR.bodyVerts[i];
                xxpGeneral.baseFIGR2.leg2Verts[i] = xxpGeneral.baseFIGR.legVerts[i];
                xxpGeneral.baseFIGR2.unkFIGR1Verts[i] = xxpGeneral.baseFIGR.unkFIGR0Verts[i];
                xxpGeneral.baseFIGR2.neckVerts[i] = 0;
                xxpGeneral.baseFIGR2.neck2Verts[i] = 0;
                xxpGeneral.baseFIGR2.waistVerts[i] = 0;
                xxpGeneral.baseFIGR2.waist2Verts[i] = 0;
            }

            streamReader.Seek(0x24, SeekOrigin.Current);

            xxpGeneral.baseCOLR = streamReader.Read<BaseCOLR>();
            xxpGeneral.baseSLCT = streamReader.Read<BaseSLCT>();
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

        public void ReadV6(BufferedStreamReader streamReader)
        {
            xxpGeneral.baseDOC = streamReader.Read<BaseDOC>();
            xxpGeneral.baseFIGR = streamReader.Read<BaseFIGR>();
            xxpGeneral.baseFIGR2 = streamReader.Read<BaseFIGR2>();

            streamReader.Seek(0x6C, SeekOrigin.Current);

            xxpGeneral.baseCOLR = streamReader.Read<BaseCOLR>();

            streamReader.Seek(0x78, SeekOrigin.Current);

            xxpGeneral.baseSLCT = streamReader.Read<BaseSLCT>();
            xxpGeneral.baseSLCT2 = streamReader.Read<BaseSLCT2>();

            streamReader.Seek(0x30, SeekOrigin.Current);

            for (int i = 0; i < 18; i++)
            {
                xxpGeneral.accessorySlidersv6[i] = streamReader.Read<byte>();
            }
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
            xxpGeneral.paintPriority = streamReader.Read<PaintPriority>();
        }


        public void ReadV9(BufferedStreamReader streamReader)
        {
            xxpGeneral.baseDOC = streamReader.Read<BaseDOC>();
            xxpGeneral.skinVariant = streamReader.Read<byte>();
            xxpGeneral.eyebrowDensity = streamReader.Read<sbyte>();
            xxpGeneral.DOCUnk0 = streamReader.Read<short>();

            xxpGeneral.baseFIGR = streamReader.Read<BaseFIGR>();
            xxpGeneral.baseFIGR2 = streamReader.Read<BaseFIGR2>();

            streamReader.Seek(0x6C, SeekOrigin.Current);

            xxpGeneral.baseCOLR = streamReader.Read<BaseCOLR>();

            streamReader.Seek(0x78, SeekOrigin.Current);
            
            xxpGeneral.baseSLCT = streamReader.Read<BaseSLCT>();
            xxpGeneral.baseSLCT2 = streamReader.Read<BaseSLCT2>();
            xxpGeneral.leftEyePart = streamReader.Read<uint>();

            streamReader.Seek(0x2C, SeekOrigin.Current);

            for(int i = 0; i < 36; i++)
            {
                xxpGeneral.accessorySliders[i] = streamReader.Read<sbyte>();
            }
            for(int i = 0; i < 3; i++)
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

            xxpGeneral.paintPriority = streamReader.Read<PaintPriority>();
        }

        public void SaveCML(string filename)
        {
            byte[] vtc0 = { 0x76, 0x74, 0x63, 0x30 };
            byte[] docText = { 0x44, 0x4F, 0x43, 0x20 };
            byte[] figrText = { 0x46, 0x49, 0x47, 0x52 };
            byte[] fstText = { 0x4F, 0x46, 0x53, 0x54};
            byte[] colrText = { 0x43, 0x4F, 0x4C, 0x52 };
            byte[] slctText = { 0x53, 0x4C, 0x43, 0x54 };

            using (MemoryStream cml = new MemoryStream())
            {
                //Header
                cml.Write(ConstantCMLHeader, 0, 16); //Always the same in all observed files

                //DOC tag
                cml.Write(vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x2C), 0, 4);
                cml.Write(docText, 0, 4);
                cml.Write(BitConverter.GetBytes(0x7),0,2); cml.Write(BitConverter.GetBytes(0x6), 0, 2); //First short is unknown, second is number of structs inside tag

                cml.WriteByte(0x70); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseDOC.race), 0, 4);
                cml.WriteByte(0x71); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseDOC.gender), 0, 4);
                cml.WriteByte(0x72); cml.WriteByte(0x8);

                uint muscles = Convert.ToUInt16(xxpGeneral.baseDOC.muscleMass); //Muscle Mass is a float ranging 0 - 60000 in xxp, but in cml is an integer
                cml.Write(BitConverter.GetBytes(muscles), 0, 4);
                cml.WriteByte(0x73); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.skinVariant), 0, 1); cml.WriteByte(0); cml.WriteByte(0); cml.WriteByte(0);
                cml.WriteByte(0x74); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.eyebrowDensity), 0, 1); cml.WriteByte(0); cml.WriteByte(0); cml.WriteByte(0);
                cml.WriteByte(0x75); cml.WriteByte(0x8);
                cml.Write(BitConverter.GetBytes(xxpGeneral.DOCUnk0), 0, 2); cml.WriteByte(0); cml.WriteByte(0);

                //FIGR tag
                cml.Write(vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x125), 0, 4);
                cml.Write(figrText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x13), 0, 2);
                cml.WriteByte(0); cml.WriteByte(0x48); cml.WriteByte(0x1); //First byte here in these is data type probably? DOC technically probably uses this too
                for (int i = 0; i < 3; i++){cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.bodyVerts[i]), 0, 4);}
                cml.WriteByte(0x1); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.armVerts[i]), 0, 4); }
                cml.WriteByte(0x2); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.legVerts[i]), 0, 4); }
                cml.WriteByte(0x3); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.bustVerts[i]), 0, 4); }
                cml.WriteByte(0x4); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.unkFIGR0Verts[i]), 0, 4); }
                cml.WriteByte(0x5); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.faceShapeVerts[i]), 0, 4); }
                cml.WriteByte(0x6); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.eyeShapeVerts[i]), 0, 4); }
                cml.WriteByte(0x7); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.noseHeightVerts[i]), 0, 4); }
                cml.WriteByte(0x8); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.noseShapeVerts[i]), 0, 4); }
                cml.WriteByte(0x9); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.mouthVerts[i]), 0, 4); }
                cml.WriteByte(0xA); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR.ear_hornVerts[i]), 0, 4); }
                cml.WriteByte(0xC); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR2.neckVerts[i]), 0, 4); }
                cml.WriteByte(0xD); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR2.waistVerts[i]), 0, 4); }
                cml.WriteByte(0x10); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR2.body2Verts[i]), 0, 4); }
                cml.WriteByte(0x11); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR2.arm2Verts[i]), 0, 4); }
                cml.WriteByte(0x12); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR2.leg2Verts[i]), 0, 4); }
                cml.WriteByte(0x13); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR2.unkFIGR1Verts[i]), 0, 4); }
                cml.WriteByte(0x14); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR2.neck2Verts[i]), 0, 4); }
                cml.WriteByte(0x15); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseFIGR2.waist2Verts[i]), 0, 4); }

                //OFST - Accessory Position
                cml.Write(vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x24), 0, 4);
                cml.Write(fstText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x4), 0, 2);
                cml.WriteByte(0x90); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x91); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 3; i < 6; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x92); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 6; i < 9; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x93); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 9; i < 12; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }

                //OFST - Accessory Rotation
                cml.Write(vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x24), 0, 4);
                cml.Write(fstText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x4), 0, 2);
                cml.WriteByte(0x95); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 12; i < 15; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x96); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 15; i < 18; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x97); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 18; i < 21; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x98); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 21; i < 24; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }

                //OFST - Accessory Scale
                cml.Write(vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x24), 0, 4);
                cml.Write(fstText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x4), 0, 2);
                cml.WriteByte(0x9B); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 24; i < 27; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x9C); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 27; i < 30; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x9D); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 30; i < 33; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }
                cml.WriteByte(0x9E); cml.WriteByte(0x83); cml.WriteByte(0x8); cml.WriteByte(0x2);
                for (int i = 33; i < 36; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.accessorySliders[i]), 0, 1); }

                //COLR
                cml.Write(vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x8F), 0, 4);
                cml.Write(colrText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x9), 0, 2);

                cml.WriteByte(0x27); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.unkCOLR0Verts[i]), 0, 4); }
                cml.WriteByte(0x20); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.costumeColorVerts[i]), 0, 4); }
                cml.WriteByte(0x21); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.mainColor_hair2Verts[i]), 0, 4); }
                cml.WriteByte(0x22); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.subColor1Verts[i]), 0, 4); }
                cml.WriteByte(0x23); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.skinSubColor2Verts[i]), 0, 4); }
                cml.WriteByte(0x24); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.subColor3_leftEye_castHair2Verts[i]), 0, 4); }
                cml.WriteByte(0x25); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.rightEye_EyesVerts[i]), 0, 4); }
                cml.WriteByte(0x26); cml.WriteByte(0x48); cml.WriteByte(0x1);
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.hairVerts[i]), 0, 4); }
                cml.WriteByte(0x26); cml.WriteByte(0x48); cml.WriteByte(0x1);                                               //Yes, they actually write it twice _(:3/
                for (int i = 0; i < 3; i++) { cml.Write(BitConverter.GetBytes(xxpGeneral.baseCOLR.hairVerts[i]), 0, 4); }

                //SLCT
                cml.Write(vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x86), 0, 4);
                cml.Write(slctText, 0, 4);
                cml.Write(BitConverter.GetBytes(0), 0, 2); cml.Write(BitConverter.GetBytes(0x15), 0, 2);
                cml.WriteByte(0x40); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.costumePart), 0, 4);
                cml.WriteByte(0x41); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.bodypaintPart), 0, 4);
                cml.WriteByte(0x42); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.stickerPart), 0, 4);
                cml.WriteByte(0x43); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.rightEyePart), 0, 4);
                cml.WriteByte(0x44); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.eyebrowPart), 0, 4);
                cml.WriteByte(0x45); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.eyelashPart), 0, 4);
                cml.WriteByte(0x46); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.faceTypePart), 0, 4);
                cml.WriteByte(0x47); cml.WriteByte(0x08);
                cml.Write(BitConverter.GetBytes(xxpGeneral.baseSLCT.unknownPart), 0, 4);
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
                cml.Write(vtc0.ToArray(), 0, 4);
                cml.Write(BitConverter.GetBytes(0x1A), 0, 4);
                cml.Write(slctText, 0, 4);
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
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT), 0, sizeof(BaseSLCT));
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
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.baseSLCT), 0, sizeof(BaseSLCT));
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
            xxp.Write(Reloaded.Memory.Struct.GetBytes(xxpGeneral.DOCUnk0),0, 2);

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

        public byte SignednibblePack(sbyte left, sbyte right)
        {
            return (byte)(SetupXXPnibble(left) * 0x10 + SetupXXPnibble(right));
        }

        //XXP V5 and V6 store accessory sliders in nibbles. 1-7 is postive while 8-E is negative, but 8-E's magnitude goes up going from 8, -1, to E, -7
        //Therefore, we must convert from a normal signed value to suit this format.
        public int SetupXXPnibble(int nyb)
        {
            if (nyb < 0)
            {
                nyb = Math.Max(nyb, -126);
                nyb = Math.Abs(nyb) + 126;
                //Correct potential underflow to max positive on division and round appropriately
                if (nyb < 135)
                {
                    nyb = 0;
                } else if(nyb < 144)
                {
                    nyb = 144;
                }
            }
            nyb /= 18;

            return nyb;
        }

        public int SetupIntFromXXPnibble(int nyb)
        {
            if (nyb > 7)
            {
                nyb = (nyb - 7) * -1;
            }
            nyb *= 18;

            return nyb;
        }

        public void SignednibbleUnpack(byte signednibbles, out sbyte left, out sbyte right)
        {
            int tempLeft = signednibbles / 0x10;
            int tempRight = signednibbles % 0x10;

            left = Convert.ToSByte(SetupIntFromXXPnibble(tempLeft));
            right = Convert.ToSByte(SetupIntFromXXPnibble(tempRight));
        }
    }
}
