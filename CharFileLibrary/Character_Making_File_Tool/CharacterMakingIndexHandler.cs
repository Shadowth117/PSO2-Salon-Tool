using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Constants;
using AquaModelLibrary.Data.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static AquaModelLibrary.Helpers.HashHelpers;

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
        public Dictionary<string, int> skinDict = new();
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

        public Dictionary<int, string> costumeOuterDictReverse = new();
        public Dictionary<int, string> basewearDictReverse = new();
        public Dictionary<int, string> innerwearDictReverse = new();
        public Dictionary<int, string> castArmDictReverse = new();
        public Dictionary<int, string> castLegDictReverse = new();
        public Dictionary<int, string> bodyPaintDictReverse = new();
        public Dictionary<int, string> stickerDictReverse = new();
        public Dictionary<int, string> hairDictReverse = new();
        public Dictionary<int, string> eyeDictReverse = new();
        public Dictionary<int, string> eyebrowDictReverse = new();
        public Dictionary<int, string> eyelashDictReverse = new();
        public Dictionary<int, string> accessoryDictReverse = new();
        public Dictionary<int, string> faceDictReverse = new();
        public Dictionary<int, string> faceTexDictReverse = new();
        public Dictionary<int, string> facePaintDictReverse = new();
        public Dictionary<int, string> skinDictReverse = new();
        public Dictionary<int, string> earDictReverse = new();
        public Dictionary<int, string> hornDictReverse = new();
        public Dictionary<int, string> teethDictReverse = new();

        public Dictionary<int, string> swimDictReverse = new();
        public Dictionary<int, string> glideDictReverse = new();
        public Dictionary<int, string> jumpDictReverse = new();
        public Dictionary<int, string> landingDictReverse = new();
        public Dictionary<int, string> movDictReverse = new();
        public Dictionary<int, string> sprintDictReverse = new();
        public Dictionary<int, string> idleDictReverse = new();

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

        public CharacterMakingIndexHandler()
        {

        }

        public CharacterMakingIndexHandler(string pso2_binPath, WIPBox box)
        {
            messageBox = box;
            try
            {
                GenerateDictionaries(pso2_binPath);
            }
            catch(Exception e)
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
            //Check CMX data
            string cmxPath = Path.Combine(pso2_binPath, CharacterMakingIndex.dataDir, GetFileHash(CharacterMakingIce.classicCMX));

            //Conditionally generate caches if cmx doesn't match

            if (messageBox != null)
            {
                messageBox.CenterWindowOnScreen();
                messageBox.Show();
            }

            //Read the game data
            cmx = ReferenceGenerator.ExtractCMX(pso2_binPath);
            ReferenceGenerator.ReadCMXText(pso2_binPath, out partsText, out acceText, out commonText, out commonTextReboot);
            faceIds = ReferenceGenerator.GetFaceVariationLuaNameDict(pso2_binPath, faceIds);
            if (Directory.Exists(Path.Combine(pso2_binPath, CharacterMakingIndex.dataNADir)))
            {
                language = 1;
            }

            GenerateNameCache();
            GC.Collect();

        }

        private void ReadCache(string cachePath, Dictionary<string, int> dict)
        {
            string[] cache = File.ReadAllLines(cachePath);
            for (int i = 0; i < cache.Length - 1; i += 2)
            {
                if (!dict.ContainsKey(cache[i]))
                {
                    dict[cache[i]] = Int32.Parse(cache[i + 1]);
                }
            }
            cache = null;
        }

        private void GenerateNameCache(bool writeToDisk = false)
        {
            //Since we have an idea of what should be there and what we're interested in parsing out, throw these into a dictionary and go
            Dictionary<string, List<List<PSO2Text.TextPair>>> textByCat = new();
            Dictionary<string, List<List<PSO2Text.TextPair>>> commByCat = new();
            Dictionary<string, List<List<PSO2Text.TextPair>>> commRebootByCat = new();
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
            Dictionary<string, Dictionary<int, List<string>>> subByCat = ReferenceGenerator.GatherSubCategories(commRebootByCat);

            //***Costumes/Outers
            //Build text Dict
            List<int> masterIdList = new List<int>();
            List<Dictionary<int, string>> nameDicts = new List<Dictionary<int, string>>();
            StringBuilder nameCache = new StringBuilder();
            ReferenceGenerator.GatherTextIds(textByCat, masterIdList, nameDicts, "costume", true);
            ReferenceGenerator.GatherTextIds(textByCat, masterIdList, nameDicts, "body", false);
            Dictionary<int, string> dict = nameDicts[language];

            //Add potential cmx ids that wouldn't be stored with 
            ReferenceGenerator.GatherDictKeys(masterIdList, cmx.costumeDict.Keys);
            ReferenceGenerator.GatherDictKeys(masterIdList, cmx.outerDict.Keys);

            costumeOuterDict = BuildNameCache(masterIdList, nameCache, dict, out costumeOuterDictReverse);

            //Somewhat vestigial. Not needed with faster .text reading
            if (writeToDisk == true)
            {
                File.WriteAllText(costumeNamePath, nameCache.ToString());
            }

            //***Basewear
            basewearDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "basewear", basewearNamePath, cmx.baseWearDict, out basewearDictReverse, writeToDisk);

            //***Innerwear
            innerwearDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "innerwear", innerwearNamePath, cmx.innerWearDict, out innerwearDictReverse, writeToDisk);

            //***Cast Arms
            castArmDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "arm", castArmNamePath, cmx.carmDict, out castArmDictReverse, writeToDisk);

            //***Cast Legs
            castLegDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "Leg", castLegNamePath, cmx.clegDict, out castLegDictReverse, writeToDisk);

            //***Body paint
            bodyPaintDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "bodypaint1", null, cmx.bodyPaintDict, out bodyPaintDictReverse, writeToDisk);

            //***Stickers
            stickerDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "bodypaint2", null, cmx.stickerDict, out stickerDictReverse, writeToDisk);

            //***Hair
            hairDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "hair", null, cmx.hairDict, out hairDictReverse, writeToDisk);

            //***Eye
            eyeDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "eye", null, cmx.eyeDict, out eyeDictReverse, writeToDisk);

            //***Eyebrow
            eyebrowDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "eyebrows", null, cmx.eyebrowDict, out eyebrowDictReverse, writeToDisk);

            //***Eyelash
            eyelashDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "eyelashes", null, cmx.eyelashDict, out eyelashDictReverse, writeToDisk);

            //***Accessories
            accessoryDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "decoy", null, cmx.accessoryDict, out accessoryDictReverse, writeToDisk);

            //***Face Models
            faceDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "", null, cmx.faceDict, out faceDictReverse, writeToDisk);

            //***Face Textures
            faceTexDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "", null, cmx.faceTextureDict, out faceTexDictReverse, writeToDisk);

            //***Face paint
            facePaintDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "facepaint2", null, cmx.fcpDict, out facePaintDictReverse, writeToDisk);

            //***NGS Skin
            skinDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "skin", null, cmx.ngsSkinDict, out skinDictReverse, writeToDisk);

            //***Ear
            earDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "ears", null, cmx.ngsEarDict, out earDictReverse, writeToDisk);

            //***Horn
            hornDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "horn", null, cmx.ngsHornDict, out hornDictReverse, writeToDisk);

            //***Teeth
            teethDict = ProcessNames(textByCat, masterIdList, nameDicts, nameCache, "dental", null, cmx.ngsTeethDict, out teethDictReverse, writeToDisk);

            //Motion Change Motions
            swimDict = ProcessMotion(subByCat, CharacterMakingDynamic.subSwim, out swimDictReverse);
            glideDict = ProcessMotion(subByCat, CharacterMakingDynamic.subGlide, out glideDictReverse);
            jumpDict = ProcessMotion(subByCat, CharacterMakingDynamic.subJump, out jumpDictReverse);
            landingDict = ProcessMotion(subByCat, CharacterMakingDynamic.subLanding, out landingDictReverse);
            movDict = ProcessMotion(subByCat, CharacterMakingDynamic.subMove, out movDictReverse);
            sprintDict = ProcessMotion(subByCat, CharacterMakingDynamic.subSprint, out sprintDictReverse);
            idleDict = ProcessMotion(subByCat, CharacterMakingDynamic.subIdle, out idleDictReverse);

            if (messageBox != null)
            {
                messageBox.Hide();
            }
        }

        private Dictionary<string, int> ProcessMotion(Dictionary<string, Dictionary<int, List<string>>> subByCat, string category, out Dictionary<int, string> motionsReverse)
        {
            Dictionary<string, int> motions = new();
            motionsReverse = new();

            foreach (int id in subByCat[category].Keys)
            {
                //Account for broken listings
                string defaultName;
                if (subByCat[category][id].Count > 0)
                {
                    defaultName = subByCat[category][id][0];
                }
                else //This condition should never happen, but still.
                {
                    defaultName = id.ToString() + "_Fallback";
                }

                //Add in default if needed
                while (subByCat[category][id].Count < language + 1)
                {
                    subByCat[category][id].Add(defaultName);
                }
                motions[subByCat[category][id][language]] = id;
                motionsReverse[id] = subByCat[category][id][language];
            }

            return motions;
        }

        private Dictionary<string, int> ProcessNamesFaces<T>(Dictionary<string, List<List<PSO2Text.TextPair>>> textByCat, List<int> masterIdList,
            List<Dictionary<int, string>> nameDicts, StringBuilder nameCache, string category, string outPath, Dictionary<int, T> cmxDict, bool writeToDisk = false)
        {
            Dictionary<string, string> dict;
            List<Dictionary<string, string>> strNameDicts = new();
            ReferenceGenerator.GatherTextIdsStringRef(textByCat, new List<string>(), strNameDicts, "facevariation", true);
            dict = strNameDicts[language];

            //Add potential cmx ids that wouldn't be stored in
            ReferenceGenerator.GatherDictKeys(masterIdList, cmxDict.Keys);
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

        private Dictionary<string, int> ProcessNames<T>(Dictionary<string, List<List<PSO2Text.TextPair>>> textByCat, List<int> masterIdList,
            List<Dictionary<int, string>> nameDicts, StringBuilder nameCache, string category, string outPath, Dictionary<int, T> cmxDict, out Dictionary<int, string> nameCacheDictReverse, bool writeToDisk = false)
        {
            Dictionary<int, string> dict;
            masterIdList.Clear();
            nameDicts.Clear();
            nameCache.Clear();
            ReferenceGenerator.GatherTextIds(textByCat, masterIdList, nameDicts, category, true);

            //Ensure we can use empty strings
            while (nameDicts.Count < language + 1)
            {
                nameDicts.Add(new Dictionary<int, string>());
            }
            dict = nameDicts[language];

            //Add potential cmx ids that wouldn't be stored with 
            ReferenceGenerator.GatherDictKeys(masterIdList, cmxDict.Keys);

            var finalNameDict = BuildNameCache(masterIdList, nameCache, dict, out nameCacheDictReverse);

            //Somewhat vestigial. Not needed with faster .text reading
            if (writeToDisk == true)
            {
                File.WriteAllText(outPath, nameCache.ToString());
            }

            return finalNameDict;
        }

        private static Dictionary<string, int> BuildNameCache(List<int> masterIdList, StringBuilder nameCache, Dictionary<int, string> dict, out Dictionary<int, string> nameCacheReverse, bool writeToDisk = false)
        {
            Dictionary<string, int> finalNameDict = new();
            nameCacheReverse = new();

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
                nameCacheReverse[id] = name;

                if (writeToDisk == true)
                {
                    nameCache.AppendLine(name);
                    nameCache.AppendLine(id.ToString());
                }
            }

            return finalNameDict;
        }
    }
}
