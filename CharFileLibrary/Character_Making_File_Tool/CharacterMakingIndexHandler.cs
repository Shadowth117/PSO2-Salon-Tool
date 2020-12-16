using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AquaModelLibrary.VTBFMethods;

namespace Character_Making_File_Tool
{
    public class CharacterMakingIndexHandler
    {
        private string cmxFilename = "1c5f7a7fbcdd873336048eaf6e26cd87";
        private string iceExeName = "ice_ext.exe";
        public bool invalid = false;
        public PartsCMX parts;
        public Dictionary<int, List<Dictionary<int, object>>> acceTags;

        public CharacterMakingIndexHandler(string pso2_binPath)
        {
            try
            {
                ExtractCMXIce(pso2_binPath);
                ReadPartsCMX();
                ReadAccessoryCMX();
            }
            catch
            {
                invalid = true;
                MessageBox.Show("Unable to read .cmx files. Please check permissions and set an appropriate pso2_bin Path.");
            }
        }

        public class PartsCMX
        {
            //Bit of an ugly look, but it's a bit of a complex thing to parse.
            public Dictionary<int, List<Dictionary<int, object>>> bodyTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Costume/Outerwear/Cast Body
            public Dictionary<int, List<Dictionary<int, object>>> carmTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Cast Arm
            public Dictionary<int, List<Dictionary<int, object>>> clegTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Cast Leg
            public Dictionary<int, List<Dictionary<int, object>>> bdp1Tags = new Dictionary<int, List<Dictionary<int, object>>>(); //Body paint
            public Dictionary<int, List<Dictionary<int, object>>> bdp2Tags = new Dictionary<int, List<Dictionary<int, object>>>(); //Stickers
            public Dictionary<int, List<Dictionary<int, object>>> faceTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Face models
            public Dictionary<int, List<Dictionary<int, object>>> fcmnTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Face motions
            public Dictionary<int, List<Dictionary<int, object>>> fcp1Tags = new Dictionary<int, List<Dictionary<int, object>>>(); //Face Textures
            public Dictionary<int, List<Dictionary<int, object>>> fcp2Tags = new Dictionary<int, List<Dictionary<int, object>>>(); //Makeup
            public Dictionary<int, List<Dictionary<int, object>>> eyeTags = new Dictionary<int, List<Dictionary<int, object>>>();  //Eye
            public Dictionary<int, List<Dictionary<int, object>>> eyeBTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Eyebrow
            public Dictionary<int, List<Dictionary<int, object>>> eyeLTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Eyelash
            public Dictionary<int, List<Dictionary<int, object>>> hairTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Hair
            public Dictionary<int, List<Dictionary<int, object>>> colTags = new Dictionary<int, List<Dictionary<int, object>>>();  //Color slider info
            public Dictionary<int, List<Dictionary<int, object>>> bblyTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Inner Wear 
            public Dictionary<int, List<Dictionary<int, object>>> bclnTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Costume/Basewear(/Cast Body?) file index lookup
            public Dictionary<int, List<Dictionary<int, object>>> lclnTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Cast Leg file index lookup
            public Dictionary<int, List<Dictionary<int, object>>> aclnTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Cast Arm file index lookup
            public Dictionary<int, List<Dictionary<int, object>>> iclnTags = new Dictionary<int, List<Dictionary<int, object>>>(); //Inner wear file index lookup
        }

        private void ReadPartsCMX()
        {
            parts = new PartsCMX();
            string filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\cmx\" + cmxFilename + @"_ext\parts.cmx";

            using (Stream stream = (Stream)new FileStream(filePath, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                int type = streamReader.Peek<int>();
                //Deal with deicer's extra header nonsense
                if (type.Equals(0x786D63))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = streamReader.Peek<int>();
                }

                //Seek to and read the DOC tag's 
                streamReader.Seek(0x1C, SeekOrigin.Current);
                int cmxCount = streamReader.Read<ushort>();
                streamReader.Seek(0x2, SeekOrigin.Current);
                //Read tags, get id from subtag 0xFF, and assign to dictionary with that. 
                for (int i = 0; i < cmxCount; i++)
                {
                    List<Dictionary<int, object>> data = ReadVTBFTag(streamReader, out string tagType, out int entryCount);
                    switch (tagType)
                    {
                        case "BODY":
                            parts.bodyTags.Add((int)data[0][0xFF], data);
                            break;
                        case "CARM":
                            parts.carmTags.Add((int)data[0][0xFF], data);
                            break;
                        case "CLEG":
                            parts.clegTags.Add((int)data[0][0xFF], data);
                            break;
                        case "BDP1":
                            parts.bdp1Tags.Add((int)data[0][0xFF], data);
                            break;
                        case "BDP2":
                            parts.bdp2Tags.Add((int)data[0][0xFF], data);
                            break;
                        case "FACE":
                            parts.faceTags.Add((int)data[0][0xFF], data);
                            break;
                        case "FCMN":
                            parts.fcmnTags.Add((int)data[0][0xFF], data);
                            break;
                        case "FCP1":
                            parts.fcp1Tags.Add((int)data[0][0xFF], data);
                            break;
                        case "FCP2":
                            parts.fcp2Tags.Add((int)data[0][0xFF], data);
                            break;
                        case "EYE ":
                            parts.eyeTags.Add((int)data[0][0xFF], data);
                            break;
                        case "EYEB":
                            parts.eyeBTags.Add((int)data[0][0xFF], data);
                            break;
                        case "EYEL":
                            parts.eyeLTags.Add((int)data[0][0xFF], data);
                            break;
                        case "HAIR":
                            parts.hairTags.Add((int)data[0][0xFF], data);
                            break;
                        case "COL ":
                            parts.colTags.Add((int)data[0][0xFF], data);
                            break;
                        case "BBLY":
                            parts.bblyTags.Add((int)data[0][0xFF], data);
                            break;
                        case "BCLN":
                            parts.bclnTags.Add((int)data[0][0xFF], data);
                            break;
                        case "LCLN":
                            parts.lclnTags.Add((int)data[0][0xFF], data);
                            break;
                        case "ACLN":
                            parts.aclnTags.Add((int)data[0][0xFF], data);
                            break;
                        case "ICLN":
                            parts.iclnTags.Add((int)data[0][0xFF], data);
                            break;
                        default:
                            throw new Exception($"Unexpected tag type {tagType}");
                            break;
                    }
                }
            }

        }

        private void ReadAccessoryCMX()
        {
            //Bit of an ugly look, but it's a bit of a complex thing to parse. Thankfully, accessories ONLY have accessories included in them. 
            acceTags = new Dictionary<int, List<Dictionary<int, object>>>();

            //Read tags, get id from subtag 0xFF, and assign to dictionary with that. 
            //ReadVTBFTag()
        }

        private string ExtractCMXIce(string pso2_binPath)
        {
            //Create the path
            string cmxPath = pso2_binPath + "\\data\\win32\\" + cmxFilename;

            //Copy and extract
            string cmxFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cmx\\";

            Directory.CreateDirectory(cmxFolder);

            //Get rid of the cmx if it exists. We want to use the latest one.
            if (File.Exists(cmxFolder + cmxFilename))
            {
                File.Delete(cmxFolder + cmxFilename);
            }
            File.Copy(cmxPath, cmxFolder + cmxFilename);

            //Since we can't redirect output folders seemingly, copy the repacker to the subfolder
            if (!File.Exists(cmxFolder + iceExeName))
            {
                File.Copy(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + iceExeName, cmxFolder + "\\" + iceExeName);
            }

            //Extract the files
            ProcessStartInfo ice = new ProcessStartInfo();
            ice.FileName = "\"" + Path.Combine(cmxFolder, iceExeName) + "\"";
            ice.Arguments = "\"" + Path.Combine(cmxFolder, cmxFilename) + "\"";
            ice.WindowStyle = ProcessWindowStyle.Hidden;
            ice.UseShellExecute = false;
            ice.CreateNoWindow = true;
            ice.RedirectStandardOutput = true;
            int exitCode;

            using (Process proc = Process.Start(ice))
            {
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }

            return cmxFolder + cmxFilename;
        }

    }
}
