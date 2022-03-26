using System.Collections.Generic;
using System.IO;
using zamboni;
using AquaModelLibrary;
using static AquaExtras.FilenameConstants;
using static AquaModelLibrary.CharacterMakingIndex;
using static AquaModelLibrary.CharacterMakingIndexMethods;
using static Character_Making_File_Tool.CharacterHandlerReboot;

namespace CharFileLibrary.Character_Making_File_Tool.Utilities
{
    public class ModelUtility
    {
        private xxpGeneralReboot xxp = null;
        private CharacterMakingIndex cmx = null;
        private string pso2_binDir = null;
        public const string basewearStr = "basewear";
        public const string costumeStr = "costume";
        public const string castStr = "cast";
        public string basewearFilename = null;
        public string basewearRPFilename = null;
        public string basewearHNFilename = null;
        public string basewearLinkedInnerFilename = null;

        public AquaObject basewearModel = null;
        public AquaNode basewearNode = null;
        public ModelUtility(string pso2_binDirectory)
        {
            pso2_binDir = pso2_binDirectory;
        }

        public void GetCharacterModel(xxpGeneralReboot xxpIn, CharacterMakingIndex cmxIn)
        {
            xxp = xxpIn;
            cmx = cmxIn;
            //Load Proportions ICE
            string proportionsPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicSystemData));
            string bodyTypeString = GetBodyTypeString(xxp, out string bodyCategory);
            AquaMotion bodyProps = GetMotion(xxp, proportionsPath, bodyTypeString);

            //Get filenames
            GetCharacterFilenames();

            //Load body models
            switch (bodyCategory)
            {
                case basewearStr: //Basewear is base, Outer wear is attached
                    basewearModel = GetModel(xxp, Path.Combine(pso2_binDir, dataDir, GetFileHash(basewearFilename)), new List<string>() { ".aqp", ".aqn" }, out basewearNode);
                    break;
                case costumeStr: //On its own
                    break;
                case castStr: //Legs are base, Body and Arms are to be attached
                    break;
            }

            //Load head models 

            //Load NGS mouth/teeth

            //Load NGS ears

            //Load NGS horns

            //Load accessories - Accessories should scale with their associated parts

            //Assemble model data and bones

            //Blend body textures
            switch (bodyCategory)
            {
                case basewearStr: //Basewear is base, Outer wear is attached. Body paint and inner should be applied 
                    break;
                case costumeStr: //On its own. Inner should not be applied
                    break;
                case castStr: //Legs are base, Body and Arms are to be attached. Body paints and inner should NOT be applied outside of NGS linked inner
                    break;
            }

            //Blend face textures - cast and special head/full coverage hairs should NOT have blending

            //Color accessory textures
        }

        public void GetCharacterFilenames()
        {
            GetBasewearFilenames();
            
        }

        public void GetBasewearFilenames()
        {
            int id = (int)xxp.baseSLCT2.basewearPart;
            //Get SoundID
            int soundId = -1;
            if (cmx.baseWearDict.ContainsKey(id))
            {
                soundId = cmx.baseWearDict[id].body2.costumeSoundId;
            }

            //Double check these ids and use an adjustedId if needed
            int adjustedId = id;
            if (cmx.baseWearIdLink.ContainsKey(id))
            {
                adjustedId = cmx.baseWearIdLink[id].bcln.fileId;
            }

            //Decide if it needs to be handled as a reboot file or not
            if (id >= 100000)
            {
                string reb = $"{rebootStart}bw_{adjustedId}.ice";
                string rebEx = $"{rebootExStart}bw_{adjustedId}_ex.ice";
                string rebHash = GetFileHash(reb);
                string rebExHash = GetFileHash(rebEx);
                string rebLinkedInner = $"{rebootStart}b1_{adjustedId + 50000}.ice";
                string rebLinkedInnerEx = $"{rebootExStart}b1_{adjustedId + 50000}_ex.ice";
                string rebLinkedInnerHash = GetFileHash(rebLinkedInner);
                string rebLinkedInnerExHash = GetFileHash(rebLinkedInnerEx);

                GetBasewearExtraFileStrings(rebEx, out string rp, out string hn);

                basewearFilename = rebExHash;
                basewearLinkedInnerFilename = rebLinkedInnerExHash;

                basewearRPFilename = rp;
                basewearHNFilename = hn;
            }
            else
            {
                string finalId = ToFive(adjustedId);
                string classic = $"{classicStart}bw_{finalId}.ice";

                var classicHash = GetFileHash(classic);
                GetBasewearExtraFileStrings(classic, out string rp, out string hn);

                basewearFilename = classicHash;
                basewearLinkedInnerFilename = null;

                basewearRPFilename = rp;
                basewearHNFilename = hn;
            }
        }

        //String 0 should be the aqp, string 1 should be the aqn
        private static AquaObject GetModel(xxpGeneralReboot xxp, string icePath, List<string> modelStrings, out AquaNode aqn)
        {
            AquaUtil aqua = new AquaUtil();
            var files = GetFilesFromIceLooseMatch(icePath, modelStrings);
            aqua.BeginReadModel(files[modelStrings[0]]);
            AquaObject model = aqua.aquaModels[0].models[0];
            aqua.ReadBones(files[modelStrings[1]]);
            aqn = aqua.aquaBones[0];

            return model;
        }

        private static AquaMotion GetMotion(xxpGeneralReboot xxp, string icePath, string motionString)
        {
            AquaUtil aqua = new AquaUtil();
            byte[] foundFile = GetFilesFromIce(icePath, new List<string>(){ motionString })[motionString];
            aqua.ReadMotion(foundFile);
            foundFile = null;
            AquaMotion motion = aqua.aquaMotions[0].anims[0];

            return motion;
        }

        private static Dictionary<string, byte[]> GetFilesFromIce(string icePath, List<string> fileNames)
        {
            Dictionary<string, byte[]> foundFiles = new Dictionary<string, byte[]>();
            if (File.Exists(icePath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(icePath));
                var ice = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(ice.groupOneFiles);
                files.AddRange(ice.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    var name = IceFile.getFileName(file).ToLower();
                    if (fileNames.Contains(name))
                    {
                        foundFiles.Add(name, file);
                    }
                }
                files = null;
                ice = null;
            }

            return foundFiles;
        }

        private static Dictionary<string, byte[]> GetFilesFromIceLooseMatch(string icePath, List<string> fileNames)
        {
            Dictionary<string, byte[]> foundFiles = new Dictionary<string, byte[]>();
            if (File.Exists(icePath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(icePath));
                var ice = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(ice.groupOneFiles);
                files.AddRange(ice.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    var name = IceFile.getFileName(file).ToLower();
                    foreach(var str in fileNames)
                    {
                        if(name.Contains(str))
                        {
                            foundFiles.Add(name, file);
                            break;
                        }
                    }
                }
                files = null;
                ice = null;
            }

            return foundFiles;
        }

        //Does NOT account for hand variants on old type characters. May not be needed??
        private static string GetBodyTypeString(xxpGeneralReboot xxp, out string type)
        {
            type = basewearStr;
            uint costumePart = xxp.baseSLCT.costumePart;
            uint basewearPart = xxp.baseSLCT2.basewearPart;
            uint face = xxp.baseSLCT.faceTypePart;
            if (costumePart >= 300000 && costumePart < 500000)      //NGS Cast
            {
                if (costumePart < 400000)
                {
                    return "pl_cmakemot_b_mh_rb.aqm";
                }
                else
                {
                    return "pl_cmakemot_b_fh_rb.aqm";
                }
            } else if (costumePart >= 40000 && costumePart < 60000)  //Cast
            {
                type = castStr;
                if (costumePart < 50000)
                {
                    return "pl_cmakemot_b_mc.aqm";
                }
                else
                {
                    return "pl_cmakemot_b_fc.aqm";
                }
            }
            else if (costumePart > 0 && costumePart < 20000) //Costume
            {
                type = costumeStr;
                if (costumePart < 10000)
                {
                    return "pl_cmakemot_b_mh.aqm";
                }
                else
                {
                    return "pl_cmakemot_b_fh.aqm";
                }
            }
            else                                             //Basewear
            {
                if (basewearPart >= 100000) //Reboot
                {
                    if(basewearPart >= 500000 || (basewearPart < 200000)) //Male. Currently 200000-300000 is unused. 400000-500000 is cast reserved and so not used for basewears. 500000 is 'genderless' which uses male reboot
                    {
                        if(face >= 100000)
                        {
                            return "pl_cmakemot_b_mh_rb.aqm";
                        } else
                        {
                            return "pl_cmakemot_b_mh_rb_oldface.aqm";
                        }
                    } else
                    {
                        if (face >= 100000)
                        {
                            return "pl_cmakemot_b_fh_rb.aqm";
                        }
                        else
                        {
                            return "pl_cmakemot_b_fh_rb_oldface.aqm";
                        }
                    }
                }
                else                     //Classic
                {
                    if (costumePart < 30000)
                    {
                        return "pl_cmakemot_b_mh.aqm";
                    }
                    else
                    {
                        return "pl_cmakemot_b_fh.aqm";
                    }
                }
            }

        }
    }
}
