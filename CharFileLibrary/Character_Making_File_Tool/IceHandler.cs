﻿using AquaModelLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;
using zamboni;
using static AquaModelLibrary.Extra.ReferenceConstructor;

namespace Character_Making_File_Tool
{
    public static class IceHandler
    {
        public unsafe static BitmapSource GetIconFromIce(string pso2_bin, int fileNum, string type, int gender = -1)
        {
            string typeString;
            string finalId = GetFinalId(fileNum);
            switch (type)
            {
                case "arm_":
                    typeString = GetCastArmIconString(finalId);
                    break;
                case "basewear01_":
                    typeString = GetBasewearIconString(finalId);
                    break;
                case "bodypaint01_":
                    typeString = GetBodyPaintIconString(finalId);
                    break;
                case "bodypaint02_":
                    typeString = GetStickerIconString(finalId);
                    break;
                //case CharacterMakingIndex.costumeIcon: //Preview feature only. Maybe in .NET 6...
                case "costume01_":
                    typeString = GetCostumeOuterIconString(pso2_bin, finalId);
                    break;
                case "decoy01_":
                    typeString = GetAccessoryIconString(finalId);
                    break;
                case "dental01_":
                    typeString = GetTeethIconString(finalId);
                    break;
                case "ears01_":
                    typeString = GetEarIconString(finalId);
                    break;
                case "eye01_":
                    typeString = GetEyeIconString(finalId);
                    break;
                case "eyebrows01_":
                    typeString = GetEyebrowsIconString(finalId);
                    break;
                case "eyelashes01_":
                    typeString = GetEyelashesIconString(finalId);
                    break;
                case "face01_":
                    typeString = GetFaceIconString(finalId);
                    break;
                case "facepaint02_":
                    typeString = GetFacePaintIconString(finalId);
                    break;
                case "hair01_":
                    //Hair is a bit odd to handle as intended
                    if (fileNum >= 40000 && fileNum < 100000)
                    {
                        typeString = GetHairCastIconString(finalId);
                    }
                    else
                    {
                        string hairMaleFileName = Path.Combine(pso2_bin, CharacterMakingIndex.dataDir, GetHairManIconString(finalId));
                        string hairFemaleFileName = Path.Combine(pso2_bin, CharacterMakingIndex.dataDir, GetHairWomanIconString(finalId));
                        hairMaleFileName = DefaultIfNonexistant(pso2_bin, hairMaleFileName, out bool foundMale);
                        hairFemaleFileName = DefaultIfNonexistant(pso2_bin, hairFemaleFileName, out bool foundFemale);
                        if ((gender == 0 || foundFemale == false) && foundMale == true)
                        {
                            return GetFirstImageFromIce(hairMaleFileName);
                        }
                        else
                        {
                            return GetFirstImageFromIce(hairFemaleFileName);
                        }
                    }
                    break;
                case "horn01_":
                    typeString = GetHornIconString(finalId);
                    break;
                case "innerwear01_":
                    typeString = GetInnerwearIconString(finalId);
                    break;
                case "leg_":
                    typeString = GetCastLegIconString(finalId);
                    break;
                default:
                    throw new Exception("Unexpected icon type!");
            }
            string fileName = Path.Combine(pso2_bin, CharacterMakingIndex.dataDir, typeString);
            fileName = DefaultIfNonexistant(pso2_bin, fileName, out bool found);
            return GetFirstImageFromIce(fileName);
        }

        private static unsafe string GetFinalId(int fileNum)
        {
            string finalId;
            if (fileNum < 100000)
            {
                finalId = $"{fileNum:D5}";
            }
            else
            {
                finalId = fileNum.ToString();
            }

            return finalId;
        }

        private static unsafe string DefaultIfNonexistant(string pso2_bin, string fileName, out bool found)
        {
            found = false;
            if (!File.Exists(fileName))
            {
                fileName = Path.Combine(pso2_bin, CharacterMakingIndex.dataDir, GetCostumeOuterIconString(pso2_bin, "00001"));
            }
            else
            {
                found = true;
            }

            return fileName;
        }

        public unsafe static IceFile GetIceFile(string fileName)
        {
            IceFile ice;
            using (var strm = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                try
                {
                    ice = IceFile.LoadIceFile(strm);
                }
                catch
                {
                    return null;
                }
            }

            return ice;
        }
        public unsafe static IceFile GetIceFile(byte[] file)
        {
            IceFile ice;
            using (var strm = new MemoryStream(file))
            {
                try
                {
                    ice = IceFile.LoadIceFile(strm);
                }
                catch
                {
                    return null;
                }
            }
            return ice;
        }

        public unsafe static BitmapSource GetFirstImageFromIce(string fileName)
        {
            IceFile ice;
            using (var strm = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                try
                {
                    ice = IceFile.LoadIceFile(strm);
                }
                catch
                {
                    return null;
                }
            }
            List<byte[]> files = (new List<byte[]>(ice.groupOneFiles));
            files.AddRange(ice.groupTwoFiles);

            foreach (byte[] file in files)
            {
                if (IceFile.getFileName(file).ToLower().Contains(".dds"))
                {
                    int int32 = BitConverter.ToInt32(file, 16);
                    string str = Encoding.ASCII.GetString(file, 64, int32).TrimEnd(new char[1]);

                    int iceHeaderSize = BitConverter.ToInt32(file, 0xC);
                    int newLength = file.Length - iceHeaderSize;
                    byte[] trueFile = new byte[newLength];
                    Array.ConstrainedCopy(file, iceHeaderSize, trueFile, 0, newLength);

                    return GetDDSBitMapSource(trueFile);

                }
            }

            return null;
        }

        private unsafe static BitmapSource GetDDSBitMapSource(byte[] trueFile)
        {
            using (var image = Pfim.Pfim.FromStream(new MemoryStream(trueFile)))
            {
                PixelFormat format;

                // Convert from Pfim's backend agnostic image format into GDI+'s image format
                switch (image.Format)
                {
                    case Pfim.ImageFormat.Rgba32:
                        format = PixelFormat.Format32bppArgb;
                        break;
                    default:
                        // see the sample for more details
                        throw new NotImplementedException();
                }

                // Pin pfim's data array so that it doesn't get reaped by GC, unnecessary
                // in this snippet but useful technique if the data was going to be used in
                // control like a picture box
                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                try
                {
                    var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, data);
                    return BitmapToImageSource(bitmap);
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
