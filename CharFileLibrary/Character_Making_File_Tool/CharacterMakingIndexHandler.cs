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
using AquaModelLibrary;
using System.Windows;
using System.Threading;
using System.Windows.Threading;

namespace Character_Making_File_Tool
{
    public class CharacterMakingIndexHandler
    {
        public bool NAInstall = false;
        public bool invalid = false;
        public WIPBox messageBox = null;
        public CharacterMakingIndex cmx = null;
        public PSO2Text partsText = null;
        public PSO2Text acceText = null;
        public PSO2Text commonText = null;
        public Dictionary<int, string> faceIds = null;

        public string namePath = null;
        public string costumeNamePath = null;
        public string basewearNamePath = null;
        public string innerwearNamePath = null;
        public string castArmNamePath = null;
        public string castLegNamePath = null;
        public Dictionary<string, int> costumeOuterDict = new Dictionary<string, int>();
        public Dictionary<string, int> basewearDict = new Dictionary<string, int>();
        public Dictionary<string, int> innerwearDict = new Dictionary<string, int>();
        public Dictionary<string, int> castArmDict = new Dictionary<string, int>();
        public Dictionary<string, int> castLegDict = new Dictionary<string, int>();

        public CharacterMakingIndexHandler(string pso2_binPath)
        {
            try
            {
                GenerateDictionaries(pso2_binPath);
            }
            catch
            {
                invalid = true;
                System.Windows.MessageBox.Show("Unable to read .cmx files; parts will not be editable. Please check permissions and set an appropriate pso2_bin Path.");
            }
        }

        public CharacterMakingIndexHandler(string pso2_binPath, WIPBox box)
        {
            messageBox = box;
            try
            {
                GenerateDictionaries(pso2_binPath);
            }
            catch
            {
                invalid = true;
                System.Windows.MessageBox.Show("Unable to read .cmx files; parts will not be editable. Please check permissions and set an appropriate pso2_bin Path.");
            }
        }

        //Generate dictionaries for storing part names and for storing part ids
        public void GenerateDictionaries(string pso2_binPath)
        {
            //Generate path strings
            namePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "names\\");
            costumeNamePath = Path.Combine(namePath, "costumeOuterNames.txt");
            basewearNamePath = Path.Combine(namePath, "basewearNames.txt");
            innerwearNamePath = Path.Combine(namePath, "innerwearNames.txt");
            castArmNamePath = Path.Combine(namePath, "castArmNames.txt");
            castLegNamePath = Path.Combine(namePath, "castLegNames.txt");

            //Check CMX data
            string cmxPath = Path.Combine(pso2_binPath, CharacterMakingIndex.dataDir, CharacterMakingIndexMethods.GetFileHash(CharacterMakingIndex.classicCMX));
            string cmxMD5Path = Path.Combine(namePath, "cmxMD5.txt");
            string currentHash = CharacterMakingIndexMethods.GetFileDataHash(cmxPath);
            string storedHash = null;
            if (File.Exists(cmxMD5Path))
            {
                storedHash = File.ReadAllText(cmxMD5Path);
            }

            //Conditionally generate caches if cmx doesn't match
            if (storedHash == null || storedHash != currentHash)
            {
                if(messageBox != null)
                {
                    messageBox.CenterWindowOnScreen();
                    messageBox.Show();
                }

                //Read the game data
                cmx = CharacterMakingIndexMethods.ExtractCMX(pso2_binPath);
                CharacterMakingIndexMethods.ReadCMXText(pso2_binPath, out partsText, out acceText, out commonText);
                faceIds = CharacterMakingIndexMethods.GetFaceVariationLuaNameDict(pso2_binPath, faceIds);
                if (Directory.Exists(Path.Combine(pso2_binPath, CharacterMakingIndex.dataNADir)))
                {
                    NAInstall = true;
                }

                GenerateNameCache();

                //Write the md5 data if we successfully cached.
                File.WriteAllText(cmxMD5Path, currentHash);
            }

            //Read cached names
            ReadCache(costumeNamePath, costumeOuterDict);
            ReadCache(basewearNamePath, basewearDict);
            ReadCache(innerwearNamePath, innerwearDict);
            ReadCache(castArmNamePath, castArmDict);
            ReadCache(castLegNamePath, castLegDict);
            GC.Collect();

        }

        private void ReadCache(string cachePath, Dictionary<string, int> dict)
        {
            string[] cache = File.ReadAllLines(cachePath);
            for (int i = 0; i < cache.Length - 1; i += 2)
            {
                if(!dict.ContainsKey(cache[i]))
                {
                    dict[cache[i]] = Int32.Parse(cache[i + 1]);
                }
            }
            cache = null;
        }

        private void GenerateNameCache()
        {
            //Since we have an idea of what should be there and what we're interested in parsing out, throw these into a dictionary and go
            Dictionary<string, List<List<PSO2Text.textPair>>> textByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> commByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            for (int i = 0; i < partsText.text.Count; i++)
            {
                textByCat.Add(partsText.categoryNames[i], partsText.text[i]);
            }
            for (int i = 0; i < acceText.text.Count; i++)
            {
                //Handle dummy decoy entry in old versions
                if (textByCat.ContainsKey(acceText.categoryNames[i]) && textByCat[acceText.categoryNames[i]][0].Count == 0)
                {
                    textByCat[acceText.categoryNames[i]] = acceText.text[i];
                }
                else
                {
                    textByCat.Add(acceText.categoryNames[i], acceText.text[i]);
                }
            }
            for (int i = 0; i < commonText.text.Count; i++)
            {
                commByCat.Add(commonText.categoryNames[i], commonText.text[i]);
            }

            //***Costumes/Outers
            //Build text Dict
            List<int> masterIdList = new List<int>();
            List<Dictionary<int, string>> nameDicts = new List<Dictionary<int, string>>();
            StringBuilder nameCache = new StringBuilder();
            CharacterMakingIndexMethods.GatherTextIds(textByCat, masterIdList, nameDicts, "costume", true);
            CharacterMakingIndexMethods.GatherTextIds(textByCat, masterIdList, nameDicts, "body", false);
            Dictionary<int, string> dict = nameDicts[0];

            //Add potential cmx ids that wouldn't be stored with 
            CharacterMakingIndexMethods.GatherDictKeys(masterIdList, cmx.costumeDict.Keys);
            CharacterMakingIndexMethods.GatherDictKeys(masterIdList, cmx.outerDict.Keys);

            BuildNameCache(masterIdList, nameCache, dict);
            File.WriteAllText(costumeNamePath, nameCache.ToString());

            //***Basewear
            dict = WriteNames(textByCat, masterIdList, nameDicts, nameCache, "basewear", basewearNamePath, cmx.baseWearDict);

            //***Innerwear
            dict = WriteNames(textByCat, masterIdList, nameDicts, nameCache, "innerwear", innerwearNamePath, cmx.innerWearDict);

            //***Cast Arms
            dict = WriteNames(textByCat, masterIdList, nameDicts, nameCache, "arm", castArmNamePath, cmx.carmDict);

            //***Cast Legs
            dict = WriteNames(textByCat, masterIdList, nameDicts, nameCache, "Leg", castLegNamePath, cmx.clegDict);

            if(messageBox != null)
            {
                messageBox.Hide();
            }
        }

        private Dictionary<int, string> WriteNames<T>(Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, List<int> masterIdList, 
            List<Dictionary<int, string>> nameDicts, StringBuilder nameCache, string category, string outPath, Dictionary<int, T> cmxDict)
        {
            Dictionary<int, string> dict;
            masterIdList.Clear();
            nameDicts.Clear();
            nameCache.Clear();
            CharacterMakingIndexMethods.GatherTextIds(textByCat, masterIdList, nameDicts, category, true);
            dict = nameDicts[0];

            //Add potential cmx ids that wouldn't be stored with 
            CharacterMakingIndexMethods.GatherDictKeys(masterIdList, cmxDict.Keys);

            BuildNameCache(masterIdList, nameCache, dict);
            File.WriteAllText(outPath, nameCache.ToString());
            return dict;
        }

        private static void BuildNameCache(List<int> masterIdList, StringBuilder nameCache, Dictionary<int, string> dict)
        {
            masterIdList.Sort();
            for (int i = 0; i < masterIdList.Count; i++)
            {
                int id = masterIdList[i];
                string name;
                if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                {
                    name = str;
                }
                else
                {
                    name = $"[Unnamed {id}]";
                }
                nameCache.AppendLine(name);
                nameCache.AppendLine(id.ToString());
            }
        }
    }
}
