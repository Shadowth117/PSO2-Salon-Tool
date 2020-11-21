using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Character_Making_File_Tool.CharacterHandler;

namespace Character_Making_File_Tool
{
    public class FileHandler
    {
        private string costumeDir;
        
        public bool useWin32_na = false;
        public string pso2_binDirectory;
        public CharacterMakingIndexHandler cmxReader;
        public XXPGeneral xxpGeneral;

        //Stores each SLCT part as a string. Order is based on stored data order in files.
        public string costumePart;
        public string bodyPaintPart;
        public string stickerPart;
        public string eyePart;
        public string eyebrowPart;
        public string eyelashPart;
        public string faceTypePart;
        public string faceTexPart;
        public string makeup1Part;
        public string hairPart;
        public string acc1Part;
        public string acc2Part;
        public string acc3Part;
        public string makeup2Part;
        public string legPart;
        public string armPart;
        public string acc4Part;
        public string basewearPart;
        public string innerwearPart;
        public string bodypaint2Part;
        public string leftEyePart;

        public FileHandler(string pso2BinDir)
        {
            pso2_binDirectory = pso2BinDir;
            cmxReader = new CharacterMakingIndexHandler(pso2BinDir);
        }

        public void GenerateFilenames(XXPGeneral xxpg)
        {
            xxpGeneral = xxpg;
            GetTrueFileNumbers();
            //Male Ou are 20000ish, Female Ou are 30000ish. In the event of no Ou, default Ous of 20000 and 30000 respectively are used
            //Cast parts are 40000 and 50000 for male and female respectively, but are bd anyways.
            if (xxpGeneral.baseSLCT.costumePart >= 20000 && xxpGeneral.baseSLCT.costumePart < 40000)
            {
                costumeDir = "character/making/pl_ow_";
            }
            else
            {
                costumeDir = "character/making/pl_bd_";
            }
            costumePart = costumeDir + ToFive(xxpGeneral.baseSLCT.costumePart) + ".ice";
            bodyPaintPart = "character/making/pl_b1_" + ToFive(xxpGeneral.baseSLCT.bodyPaintPart) + ".ice";
            stickerPart = "character/making/pl_b2_" + ToFive(xxpGeneral.baseSLCT.stickerPart) + ".ice";
            eyePart = "character/making/pl_ey_" + ToFive(xxpGeneral.baseSLCT.eyePart) + ".ice";
            eyebrowPart = "character/making/pl_eb_" + ToFive(xxpGeneral.baseSLCT.eyebrowPart) + ".ice";
            eyelashPart = "character/making/pl_el_" + ToFive(xxpGeneral.baseSLCT.eyelashPart) + ".ice";
            faceTypePart = "character/making/pl_fc_" + ToFive(xxpGeneral.baseSLCT.faceTypePart) + ".ice"; //pl_fm is for facial motions for the model
            faceTexPart = "character/making/pl_f1_" + ToFive(xxpGeneral.baseSLCT.faceTexPart) + ".ice"; //In practice, should always be the same as above for players
            makeup1Part = "character/making/pl_f2_" + ToFive(xxpGeneral.baseSLCT.makeup1Part) + ".ice";
            hairPart = "character/making/pl_hr_" + ToFive(xxpGeneral.baseSLCT.hairPart) + ".ice";
            acc1Part = "character/making/pl_ac_" + ToFive(xxpGeneral.baseSLCT.acc1Part) + ".ice";
            acc2Part = "character/making/pl_ac_" + ToFive(xxpGeneral.baseSLCT.acc2Part) + ".ice";
            acc3Part = "character/making/pl_ac_" + ToFive(xxpGeneral.baseSLCT.acc3Part) + ".ice";
            makeup2Part = "character/making/pl_f2_" + ToFive(xxpGeneral.baseSLCT.makeup2Part) + ".ice";
            legPart = "character/making/pl_lg_" + ToFive(xxpGeneral.baseSLCT.legPart) + ".ice";
            armPart = "character/making/pl_am_" + ToFive(xxpGeneral.baseSLCT.armPart) + ".ice";
            acc4Part = "character/making/pl_ac_" + ToFive(xxpGeneral.baseSLCT2.acc4Part) + ".ice";
            basewearPart = "character/making/pl_bw_" + ToFive(xxpGeneral.baseSLCT2.basewearPart) + ".ice";
            innerwearPart = "character/making/pl_iw_" + ToFive(xxpGeneral.baseSLCT2.innerwearPart) + ".ice";
            bodypaint2Part = "character/making/pl_b1_" + ToFive(xxpGeneral.baseSLCT2.bodypaint2Part) + ".ice";
            leftEyePart = "character/making/pl_ey_" + ToFive(xxpGeneral.leftEyePart) + ".ice";
        }

        public Vector3 GetColor()
        {
            //Load color map from cmx ice color map files from dc3f4e27c1cfc4f902a169294818be69;

            //var tempColor = System.Drawing.Color.FromArgb()
            Vector3 color = new Vector3();


            return color;
        }

        public string GetFilePath(string str)
        {
            string path = pso2_binDirectory;

            if(useWin32_na)
            {
                string win32NAStr = Path.Combine(path, @"\data\win32_na\" + str);
                if (File.Exists(win32NAStr))
                {
                    return win32NAStr;
                }
            }

            path = Path.Combine(path, @"\data\win32\" + str);

            if(File.Exists(path))
            {
                return path;
            }

            Console.WriteLine("Invalid filepath");
            return null;
        }

        public string GetFileHash(string str)
        {
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(new UTF8Encoding().GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        //Get costume/basewear, cast arm/leg, and innerwear true numbers. For later things that are just recolors, Sega has structs to link the real indices with the model or "true" indices.
        //The color is stored independently elsewhere. 
        public void GetTrueFileNumbers()
        {
            if(cmxReader.parts.bclnTags.TryGetValue((int)xxpGeneral.baseSLCT.costumePart, out var linkedCostume))
            {
                xxpGeneral.baseSLCT.costumePart =  (uint)((int)linkedCostume[0][0x1]);
            }
            if (cmxReader.parts.bclnTags.TryGetValue((int)xxpGeneral.baseSLCT2.basewearPart, out var linkedBase))
            {
                xxpGeneral.baseSLCT2.basewearPart = (uint)((int)linkedBase[0][0x1]);
            }
            if (cmxReader.parts.lclnTags.TryGetValue((int)xxpGeneral.baseSLCT.legPart, out var linkedLeg))
            {
                xxpGeneral.baseSLCT.legPart = (uint)((int)linkedLeg[0][0x1]);
            }
            if (cmxReader.parts.aclnTags.TryGetValue((int)xxpGeneral.baseSLCT.armPart, out var linkedArm))
            {
                xxpGeneral.baseSLCT.armPart = (uint)((int)linkedArm[0][0x1]);
            }
            if (cmxReader.parts.iclnTags.TryGetValue((int)xxpGeneral.baseSLCT2.innerwearPart, out var linkedInner))
            {
                xxpGeneral.baseSLCT2.innerwearPart = (uint)((int)linkedInner[0][0x1]);
            }
        }

        public string ToFive(uint num)
        {
            string numString = num.ToString();
            while(numString.Length < 5)
            {
                numString = numString.Insert(0, "0");
            }

            return numString;
        }
    }
}
