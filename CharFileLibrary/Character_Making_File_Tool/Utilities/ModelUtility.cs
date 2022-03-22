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
        private string pso2_binDir;
        public static string basewearStr = "basewear";
        public static string costumeStr = "costume";
        public static string castStr = "cast";
        public ModelUtility(string pso2_binDirectory)
        {
            pso2_binDir = pso2_binDirectory;
        }

        public void GetCharacterModel(xxpGeneralReboot xxp)
        {
            //Load Proportions ICE
            string icePath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicSystemData));
            string bodyTypeString = GetBodyTypeString(xxp, out string bodyCategory);
            AquaMotion bodyProps = GetMotion(xxp, icePath, bodyTypeString);


        }

        private static AquaObject GetModel(xxpGeneralReboot xxp, string icePath, string bodyTypeString)
        {
            AquaUtil aqua = new AquaUtil();
            byte[] foundFile = GetFileFromIce(icePath, bodyTypeString);
            aqua.BeginReadModel(foundFile);
            foundFile = null;
            AquaObject model = aqua.aquaModels[0].models[0];

            return model;
        }

        private static AquaMotion GetMotion(xxpGeneralReboot xxp, string icePath, string bodyTypeString)
        {
            AquaUtil aqua = new AquaUtil();
            byte[] foundFile = GetFileFromIce(icePath, bodyTypeString);
            aqua.ReadMotion(foundFile);
            foundFile = null;
            AquaMotion motion = aqua.aquaMotions[0].anims[0];

            return motion;
        }

        private static byte[] GetFileFromIce(string icePath, string fileName)
        {
            byte[] foundFile = null;
            if (File.Exists(icePath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(icePath));
                var ice = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = (new List<byte[]>(ice.groupOneFiles));
                files.AddRange(ice.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    if (IceFile.getFileName(file).ToLower() == fileName)
                    {
                        foundFile = file;
                    }
                }
                files = null;
                ice = null;
            }

            return foundFile;
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
