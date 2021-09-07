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
        public int language = 0;
        public bool invalid = false;
        public WIPBox messageBox = null;
        public CharacterMakingIndex cmx = null;
        public PSO2Text partsText = null;
        public PSO2Text acceText = null;
        public PSO2Text commonText = null;
        public PSO2Text commonTextReboot = null;
        public PSO2Text actorNameTextReboot = null;
        public Dictionary<int, string> faceIds = null;

        public string namePath = null;
        public string costumeNamePath = null;
        public string basewearNamePath = null;
        public string innerwearNamePath = null;
        public string castArmNamePath = null;
        public string castLegNamePath = null;

        public Dictionary<string, int> costumeOuterDict = new();
        public Dictionary<string, int> basewearDict = new();
        public Dictionary<string, int> innerwearDict = new();
        public Dictionary<string, int> castArmDict = new();
        public Dictionary<string, int> castLegDict = new();
        public Dictionary<string, int> bodyPaintDict = new();
        public Dictionary<string, int> stickerDict = new();
        public Dictionary<string, int> hairDict = new();
        public Dictionary<string, int> eyeDict = new();
        public Dictionary<string, int> eyebrowDict = new();
        public Dictionary<string, int> eyelashDict = new();
        public Dictionary<string, int> accessoryDict = new();
        public Dictionary<string, int> faceDict = new();
        public Dictionary<string, int> faceTexDict = new();
        public Dictionary<string, int> facePaintDict = new();
        public Dictionary<string, int> earDict = new();
        public Dictionary<string, int> hornDict = new();
        public Dictionary<string, int> teethDict = new();

        public Dictionary<string, int> swimDict = new();
        public Dictionary<string, int> glideDict = new();
        public Dictionary<string, int> jumpDict = new();
        public Dictionary<string, int> landingDict = new();
        public Dictionary<string, int> movDict = new();
        public Dictionary<string, int> sprintDict = new();
        public Dictionary<string, int> idleDict = new();

        public CharacterMakingIndexHandler(string pso2_binPath)
        {
            try
            {
                GenerateDictionaries(pso2_binPath);
            }
            catch
            {
                if (messageBox != null)
                {
                    messageBox.Hide();
                }
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
                if (messageBox != null)
                {
                    messageBox.Hide();
                }
                invalid = true;
                System.Windows.MessageBox.Show("Unable to read .cmx files; parts will not be editable. Please check permissions and set an appropriate pso2_bin Path.");
            }
        }

        //Generate dictionaries for storing part names and for storing part ids
        public void GenerateDictionaries(string pso2_binPath)
        {
            //Generate path strings
            /*
            namePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "names\\");
            costumeNamePath = Path.Combine(namePath, "costumeOuterNames.txt");
            basewearNamePath = Path.Combine(namePath, "basewearNames.txt");
            innerwearNamePath = Path.Combine(namePath, "innerwearNames.txt");
            castArmNamePath = Path.Combine(namePath, "castArmNames.txt");
            castLegNamePath = Path.Combine(namePath, "castLegNames.txt");*/

            //Check CMX data
            string cmxPath = Path.Combine(pso2_binPath, CharacterMakingIndex.dataDir, CharacterMakingIndexMethods.GetFileHash(CharacterMakingIndex.classicCMX));

            //Conditionally generate caches if cmx doesn't match

            if(messageBox != null)
            {
                messageBox.CenterWindowOnScreen();
                messageBox.Show();
            }

            //Read the game data
            cmx = CharacterMakingIndexMethods.ExtractCMX(pso2_binPath);
            CharacterMakingIndexMethods.ReadCMXText(pso2_binPath, out partsText, out acceText, out commonText, out commonTextReboot, out actorNameTextReboot);
            faceIds = CharacterMakingIndexMethods.GetFaceVariationLuaNameDict(pso2_binPath, faceIds);
            if (Directory.Exists(Path.Combine(pso2_binPath, CharacterMakingIndex.dataNADir)))
            {
                language = 1;
            }

            GenerateNameCache();

            //Read stored cached names
            /*
            ReadCache(costumeNamePath, costumeOuterDict);
            ReadCache(basewearNamePath, basewearDict);
            ReadCache(innerwearNamePath, innerwearDict);
            ReadCache(castArmNamePath, castArmDict);
            ReadCache(castLegNamePath, castLegDict);*/
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

        private void GenerateNameCache(bool writeToDisk = false)
        {
            //Since we have an idea of what should be there and what we're interested in parsing out, throw these into a dictionary and go
            Dictionary<string, List<List<PSO2Text.textPair>>> textByCat = new();
            Dictionary<string, List<List<PSO2Text.textPair>>> commByCat = new();
            Dictionary<string, List<List<PSO2Text.textPair>>> commRebootByCat = new();
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
            if (commonTextReboot != null)
            {
                for (int i = 0; i < commonTextReboot.text.Count; i++)
                {
                    commRebootByCat.Add(commonTextReboot.categoryNames[i], commonTextReboot.text[i]);
                }
            }
            Dictionary<string, Dictionary<int, List<string>>> subByCat = CharacterMakingIndexMethods.GatherSubCategories(commRebootByCat);

            //***Costumes/Outers
            //Build text Dict
            List<int> masterIdList = new List<int>();
            List<Dictionary<int, string>> nameDicts = new List<Dictionary<int, string>>();
            StringBuilder nameCache = new StringBuilder();
            CharacterMakingIndexMethods.GatherTextIds(textByCat, masterIdList, nameDicts, "costume", true);
            CharacterMakingIndexMethods.GatherTextIds(textByCat, masterIdList, nameDicts, "body", false);
            Dictionary<int, string> dict = nameDicts[language];

            //Add potential cmx ids that wouldn't be stored with 
            CharacterMakingIndexMethods.GatherDictKeys(masterIdList, cmx.costumeDict.Keys);
            CharacterMakingIndexMethods.GatherDictKeys(masterIdList, cmx.outerDict.Keys);

            costumeOuterDict = BuildNameCache(masterIdList, nameCache, dict);

            //Somewhat vestigial. Not needed with faster .text reading
            if (writeToDisk == true)
            {
                File.WriteAllText(costumeNamePath, nameCache.ToString());
            }

            //***Basewear
            basewearDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "basewear", basewearNamePath, cmx.baseWearDict, writeToDisk);

            //***Innerwear
            innerwearDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "innerwear", innerwearNamePath, cmx.innerWearDict, writeToDisk);

            //***Cast Arms
            castArmDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "arm", castArmNamePath, cmx.carmDict, writeToDisk);

            //***Cast Legs
            castLegDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "Leg", castLegNamePath, cmx.clegDict, writeToDisk);

            //***Body paint
            bodyPaintDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "bodypaint1", null, cmx.bodyPaintDict, writeToDisk);

            //***Stickers
            stickerDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "bodypaint2", null, cmx.stickerDict, writeToDisk);

            //***Hair
            hairDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "hair", null, cmx.hairDict, writeToDisk);

            //***Eye
            eyeDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "eye", null, cmx.eyeDict, writeToDisk);

            //***Eyebrow
            eyebrowDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "eyebrows", null, cmx.eyebrowDict, writeToDisk);

            //***Eyelash
            eyelashDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "eyelashes", null, cmx.eyelashDict, writeToDisk);

            //***Accessories
            accessoryDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "decoy", null, cmx.accessoryDict, writeToDisk);

            //***Face Models
            faceDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "", null, cmx.faceDict, writeToDisk);

            //***Face Textures
            faceTexDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "", null, cmx.faceTextureDict, writeToDisk);

            //***Face paint
            facePaintDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "facepaint1", null, cmx.fcpDict, writeToDisk);

            //***Ear
            earDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "ears", null, cmx.ngsEarDict, writeToDisk);

            //***Horn
            hornDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "horn", null, cmx.ngsHornDict, writeToDisk);

            //***Teeth
            teethDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "dental", null, cmx.ngsTeethDict, writeToDisk);

            //Motion Change Motions
            swimDict = ProcessMotion(subByCat, CharacterMakingIndex.subSwim);
            glideDict = ProcessMotion(subByCat, CharacterMakingIndex.subGlide);
            jumpDict = ProcessMotion(subByCat, CharacterMakingIndex.subJump);
            landingDict = ProcessMotion(subByCat, CharacterMakingIndex.subLanding);
            movDict = ProcessMotion(subByCat, CharacterMakingIndex.subMove);
            sprintDict = ProcessMotion(subByCat, CharacterMakingIndex.subSprint);
            idleDict = ProcessMotion(subByCat, CharacterMakingIndex.subIdle);

            if (messageBox != null)
            {
                messageBox.Hide();
            }
        }

        private Dictionary<string, int> ProcessMotion(Dictionary<string, Dictionary<int, List<string>>> subByCat, string category)
        {
            Dictionary<string, int> motions = new();
            
            foreach(int id in subByCat[category].Keys)
            {
                motions[subByCat[category][id][language]] = id;
            }

            return motions;
        }

        private Dictionary<string, int> ProcessNamesFaces<T>(Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, List<int> masterIdList,
            List<Dictionary<int, string>> nameDicts, StringBuilder nameCache, string category, string outPath, Dictionary<int, T> cmxDict, bool writeToDisk = false)
        {
            Dictionary<string, string> dict;
            List<Dictionary<string, string>> strNameDicts = new();
            CharacterMakingIndexMethods.GatherTextIdsStringRef(textByCat, new List<string>(), strNameDicts, "facevariation", true);
            dict = strNameDicts[language];

            //Add potential cmx ids that wouldn't be stored in
            CharacterMakingIndexMethods.GatherDictKeys(masterIdList, cmxDict.Keys);
            masterIdList.Sort();

            return BuildNameCacheFaces(masterIdList, nameCache, dict);
        }

        private Dictionary<string, int> BuildNameCacheFaces(List<int> masterIdList, StringBuilder nameCache, Dictionary<string, string> dict, bool writeToDisk = false)
        {
            Dictionary<string, int> finalNameDict = new();

            masterIdList.Sort();
            for (int i = 0; i < masterIdList.Count; i++)
            {
                int id = masterIdList[i]; 
                
                string realId = "";
                if (!faceIds.TryGetValue(id, out realId))
                {
                    realId = "No" + id;
                }

                string name;
                if (dict.TryGetValue(realId, out string str) && str != null && str != "" && str.Length > 0)
                {
                    name = str;
                }
                else
                {
                    name = $"[Unnamed {id}]";
                }
                finalNameDict[name] = id;

                if (writeToDisk == true)
                {
                    nameCache.AppendLine(name);
                    nameCache.AppendLine(id.ToString());
                }
            }

            return finalNameDict;
        }

        private Dictionary<string, int> ProcessNames<T>(Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, List<int> masterIdList, 
            List<Dictionary<int, string>> nameDicts, StringBuilder nameCache, string category, string outPath, Dictionary<int, T> cmxDict, bool writeToDisk = false)
        {
            Dictionary<int, string> dict;
            masterIdList.Clear();
            nameDicts.Clear();
            nameCache.Clear();
            CharacterMakingIndexMethods.GatherTextIds(textByCat, masterIdList, nameDicts, category, true);
            
            //Ensure we can use empty strings
            while(nameDicts.Count < language + 1)
            {
                nameDicts.Add(new Dictionary<int, string>());
            }
            dict = nameDicts[language];

            //Add potential cmx ids that wouldn't be stored with 
            CharacterMakingIndexMethods.GatherDictKeys(masterIdList, cmxDict.Keys);

            var finalNameDict = BuildNameCache(masterIdList, nameCache, dict);

            //Somewhat vestigial. Not needed with faster .text reading
            if(writeToDisk == true)
            {
                File.WriteAllText(outPath, nameCache.ToString());
            }

            return finalNameDict;
        }

        private static Dictionary<string, int> BuildNameCache(List<int> masterIdList, StringBuilder nameCache, Dictionary<int, string> dict, bool writeToDisk = false)
        {
            Dictionary<string, int> finalNameDict = new();

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
                finalNameDict[name] = id;

                if(writeToDisk == true)
                {
                    nameCache.AppendLine(name);
                    nameCache.AppendLine(id.ToString());
                }
            }

            return finalNameDict;
        }
    }
}
