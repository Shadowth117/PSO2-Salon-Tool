using AquaModelLibrary;
using Character_Making_File_Tool;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;
using static AquaModelLibrary.CharacterMakingIndex;

namespace NGS_Salon_Tool
{
    public static class CMLSearchUtility
    {
        public static Dictionary<char, char> illegalChars = new Dictionary<char, char>() {
            { '<', '[' },
            { '>', ']'},
            {':', '-'},
            {'"', '\''},
            {'/', '_'},
            {'\\', '_'},
            {'|', '_'},
            {'?', '_'},
            {'*', '_'},
        };
        public static void DumpCMLs(string pso2_binDir)
        {
            if (pso2_binDir == null)
            {
                MessageBox.Show("You must have a pso2_bin selected to get the files from!");
                return;
            }
            using (CommonOpenFileDialog dumpPicker = new CommonOpenFileDialog())
            {
                dumpPicker.IsFolderPicker = true;
                dumpPicker.Title = "Select folder to output NPC files";

                if (dumpPicker.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    bool isGlobal = false;
                    int startId = 0;
                    if (Directory.Exists(Path.Combine(pso2_binDir, dataNADir)) || Directory.Exists(Path.Combine(pso2_binDir, dataRebootNA)))
                    {
                        isGlobal = true;
                        startId = 1;
                    }
                    Dictionary<string, List<List<PSO2Text.textPair>>> actorNameByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
                    Dictionary<string, List<List<PSO2Text.textPair>>> rbActorNameByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
                    //Get NPC name references from classic and reboot
                    //Classic
                    string actorNameIce = GetFileHash(classicActorName);
                    string actorNameTextPath = Path.Combine(pso2_binDir, dataDir, actorNameIce);
                    string actorNameTextPathNA = Path.Combine(pso2_binDir, dataNADir, actorNameIce);
                    PSO2Text actorNameText = null;

                    if (File.Exists(actorNameTextPath))
                    {
                        actorNameText = AquaMiscMethods.GetTextConditional(actorNameTextPath, actorNameTextPathNA, actorNameName);

                        if (actorNameText != null)
                        {
                            for (int i = 0; i < actorNameText.text.Count; i++)
                            {
                                actorNameByCat.Add(actorNameText.categoryNames[i], actorNameText.text[i]);
                            }
                        }
                    }

                    //Reboot
                    string rbActorNameIce = GetRebootHash(GetFileHash(rebootActorNameNPC));
                    string rbActorNameTextPath = Path.Combine(pso2_binDir, dataReboot, rbActorNameIce);
                    string rbActorNameTextPathNA = Path.Combine(pso2_binDir, dataRebootNA, rbActorNameIce);
                    PSO2Text rbActorNameText = null;
                    if (File.Exists(rbActorNameTextPath))
                    {
                        rbActorNameText = AquaMiscMethods.GetTextConditional(rbActorNameTextPath, rbActorNameTextPathNA, actorNameNPCName);

                        if (rbActorNameText != null)
                        {
                            for (int i = 0; i < rbActorNameText.text.Count; i++)
                            {
                                rbActorNameByCat.Add(rbActorNameText.categoryNames[i], rbActorNameText.text[i]);
                            }
                        }
                    }


                    //Get all files
                    var files = Directory.EnumerateFiles(pso2_binDir, "*.*", SearchOption.AllDirectories).ToArray();

                    //Failsafe for if the standard pc name text routes aren't valid
                    if (actorNameText == null && rbActorNameText == null)
                    {
                        var baseName = Path.GetFileName(rebootActorName);
                        foreach (var filePath in files)
                        {
                            if ((filePath.Contains(baseName) && filePath.Contains("datareboot")) || filePath.Contains(rbActorNameIce) && rbActorNameText == null)
                            {
                                rbActorNameText = AquaMiscMethods.GetTextConditional(filePath, filePath, actorNameNPCName);

                                if (rbActorNameText != null)
                                {
                                    for (int i = 0; i < rbActorNameText.text.Count; i++)
                                    {
                                        rbActorNameByCat.Add(rbActorNameText.categoryNames[i], rbActorNameText.text[i]);
                                    }
                                }
                            }
                            else if (filePath.Contains(baseName) || filePath.Contains(actorNameIce) && actorNameText == null)
                            {
                                actorNameText = AquaMiscMethods.GetTextConditional(filePath, filePath, actorNameName);

                                if (actorNameText != null)
                                {
                                    for (int i = 0; i < actorNameText.text.Count; i++)
                                    {
                                        actorNameByCat.Add(actorNameText.categoryNames[i], actorNameText.text[i]);
                                    }
                                }
                            }

                            //Stop early if both are found
                            if (actorNameText != null && rbActorNameText != null)
                            {
                                break;
                            }
                        }
                    }

                    //Set up text dictionaries
                    //Event names
                    List<Dictionary<string, string>> evtDicts = new List<Dictionary<string, string>>();
                    GatherTextIdsStringRefToLower(actorNameByCat, evtDicts, "ObjectExGuide");

                    //Categories
                    List<Dictionary<string, string>> categoryDicts = new List<Dictionary<string, string>>();
                    GatherTextIdsStringRefToLower(actorNameByCat, categoryDicts, "NpcShop");
                    GatherTextIdsStringRefToLower(actorNameByCat, categoryDicts, "NpcNickName");
                    GatherTextIdsStringRefToLower(rbActorNameByCat, categoryDicts, "NpcNickName");
                    GatherTextIdsStringRefToLower(actorNameByCat, categoryDicts, "NpcPlace");         //Only use if not "dummy"
                    GatherTextIdsStringRefToLower(actorNameByCat, categoryDicts, "NpcPlace_EP");

                    //Names
                    List<Dictionary<string, string>> npcNameDicts = new List<Dictionary<string, string>>();
                    GatherTextIdsStringRefToLower(actorNameByCat, npcNameDicts, "Npc");
                    GatherTextIdsStringRefToLower(rbActorNameByCat, npcNameDicts, "Npc", true);
                    GatherTextIdsStringRefToLower(actorNameByCat, npcNameDicts, "ApcConsole");
                    GatherTextIdsStringRefToLower(actorNameByCat, npcNameDicts, "Enemy");
                    GatherTextIdsStringRefToLower(actorNameByCat, npcNameDicts, "Object");

                    //Iterate all files
                    Parallel.ForEach(files, iceFilePath =>
                    {
                        var size = new System.IO.FileInfo(iceFilePath).Length;

                        if (size < 4 || size > 1073741824 || iceFilePath.Contains("GameGuard"))
                        {
                            return;
                        }
                        var bytes = File.ReadAllBytes(iceFilePath);
                        var magic = System.Text.Encoding.ASCII.GetString(bytes, 0, 4);

                        if (magic == "ICE\0")
                        {
                            bool isRbFile = iceFilePath.Contains("datareboot") || iceFilePath.Contains("win32reboot");
                            Zamboni.IceFile ice = null;
                            ice = IceHandler.GetIceFile(iceFilePath);
                            if (ice != null)
                            {
                                var iceFiles = new List<byte[]>();
                                iceFiles.AddRange(ice.groupOneFiles);
                                iceFiles.AddRange(ice.groupTwoFiles);
                                Dictionary<string, byte[]> cmls = new Dictionary<string, byte[]>();
                                Dictionary<string, byte[]> texts = new Dictionary<string, byte[]>();
                                List<Dictionary<string, string>> localEvtDicts = new List<Dictionary<string, string>>();
                                string iceEvt = "";

                                foreach (var file in iceFiles)
                                {
                                    if (file.Length < 0x40) //Should never trigger, but might happen if you have corrupted or broken files 
                                    {
                                        continue;
                                    }
                                    var fileName = Zamboni.IceFile.getFileName(file);
                                    var ext = Path.GetExtension(fileName);
                                    switch (ext)
                                    {
                                        case ".cml":
                                        case ".eml":
                                            cmls.Add(fileName, file);
                                            break;
                                        case ".skit":
                                        case ".evt":
                                            if (iceEvt == "")
                                            {
                                                iceEvt = Path.GetFileNameWithoutExtension(fileName) + "_";
                                                string evtNameStr = "";
                                                CheckCategory(iceEvt, ref evtNameStr, startId, evtDicts);

                                                if (evtNameStr != "")
                                                {
                                                    iceEvt += evtNameStr + "_";
                                                }
                                            }
                                            break;
                                        case ".text":
                                            texts.Add(fileName, file);
                                            break;
                                    }
                                }

                                //NGS event 'name'. Less prevalent, but exists sometimes in this very specific way
                                if (iceEvt != "" && texts.ContainsKey(iceEvt))
                                {
                                    var textFile = AquaMiscMethods.ReadPSO2Text(texts[iceEvt]);
                                    if (textFile.categoryNames.Contains("Basic"))
                                    {
                                        var id = textFile.categoryNames.IndexOf("Basic");
                                        var countId = startId;
                                        while (countId > -1)
                                        {
                                            for (int i = 0; i < textFile.text[id][countId].Count; i++)
                                            {
                                                var pair = textFile.text[id][countId][i];
                                                if (pair.name == "Title")
                                                {
                                                    iceEvt += pair.str + "_";
                                                    countId = -1;
                                                    break;
                                                }
                                            }
                                            countId -= 1;
                                        }
                                    }
                                }

                                foreach (var fileSet in cmls)
                                {
                                    var file = fileSet.Value;
                                    var fileName = fileSet.Key;

                                    var iceFileName = Path.GetFileNameWithoutExtension(iceFilePath);
                                    string outDir;
                                    if (isRbFile)
                                    {
                                        var dirName0 = Path.GetDirectoryName(iceFilePath);
                                        var dirName1 = Path.GetDirectoryName(dirName0);
                                        var folderName = dirName0.Replace(dirName1, "").Replace("\\", "");
                                        var icefullFileName = iceEvt + folderName + "_" + iceFileName;
                                        outDir = dumpPicker.FileName + "\\NGS\\" + GetDirectoryString(icefullFileName, fileName, categoryDicts, npcNameDicts, isGlobal);
                                        Directory.CreateDirectory(outDir);
                                    }
                                    else
                                    {
                                        var icefullFileName = iceEvt + iceFileName;
                                        outDir = dumpPicker.FileName + "\\Classic\\" + GetDirectoryString(icefullFileName, fileName, categoryDicts, npcNameDicts, isGlobal);
                                        Directory.CreateDirectory(outDir);
                                    }

                                    string outPath = outDir + fileName;
                                    File.WriteAllBytes(outPath, file);
                                    try
                                    {
                                        var xxp = CMLHandler.ParseCML(file);
                                        xxp.GetXXPWildcards(out string letterOne, out string letterTwo);
                                        if (xxp.xxpVersion < 0xD || xxp.xxpVersion > 0xD)
                                        {
                                            xxp.xxpVersion = 0xD;
                                        }
                                        int fileSize;

                                        if (CharacterConstants.ngsSalonToolSizes.ContainsKey(xxp.xxpVersion))
                                        {
                                            fileSize = CharacterConstants.ngsSalonToolSizes[xxp.xxpVersion];
                                        }
                                        else
                                        {
                                            fileSize = CharacterConstants.ngsSalonToolSizes[0xD];
                                        }

                                        var body = xxp.GetBytes();
                                        body = CharacterHandler.EncryptData(body, fileSize, out int hash);
                                        List<byte> fileData = new List<byte>();

                                        fileData.AddRange(BitConverter.GetBytes(xxp.xxpVersion));
                                        fileData.AddRange(BitConverter.GetBytes(fileSize));
                                        fileData.AddRange(BitConverter.GetBytes(hash));
                                        fileData.AddRange(new byte[] { 0, 0, 0, 0 });
                                        fileData.AddRange(body);

                                        File.WriteAllBytes(Path.ChangeExtension(outPath, $".{letterOne}{letterTwo}p"), fileData.ToArray());
                                    }
                                    catch
                                    {
                                        Debug.WriteLine($"Could not convert {outPath} from CML");
#if DEBUG
                                        Directory.CreateDirectory("C:\\DebugCharFiles\\");
                                        File.WriteAllBytes("C:\\DebugCharFiles\\" + Path.GetFileName(outPath), file);
#endif
                                    }

                                }


                            }
                        }

                    });
                }
            }
        }

        public static string GetDirectoryString(string iceString, string fileName, List<Dictionary<string, string>> categoryDicts, List<Dictionary<string, string>> npcNameDicts, bool isGlobal)
        {
            string clippedName = Path.GetFileNameWithoutExtension(fileName).Replace("np_", "").Replace("en_", "");
            string category = "";
            string npcName = "";
            string unkCategory;
            string unkNPC;
            int startId;

            if (isGlobal)
            {
                unkCategory = "[]NoCategory[]";
                unkNPC = "[]UnknownNPC[]";
                startId = 1;
            }
            else
            {
                unkCategory = "[]なし[]";
                unkNPC = "[]名無し[]";
                startId = 0;
            }

            CheckCategory(clippedName, ref category, startId, categoryDicts);
            CheckCategory(clippedName, ref npcName, startId, npcNameDicts);

            if (category == "")
            {
                category = unkCategory;
            }

            if (npcName == "")
            {
                npcName = unkNPC;
            }

            string finalPath = NixIllegalCharacters(category) + "\\" + NixIllegalCharacters(npcName) + "\\" + iceString + "\\";
            return finalPath;
        }

        public static void CheckCategory(string name, ref string nameToSet, int startId, List<Dictionary<string, string>> nameDicts)
        {
            while (startId > -1)
            {
                if (nameDicts[startId].ContainsKey(name))
                {
                    nameToSet = nameDicts[startId][name];
                    break;
                }
                startId -= 1;
            }
        }

        public static void GatherTextIdsStringRefToLower(Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, List<Dictionary<string, string>> nameDicts, string category, bool rebootNPC = false)
        {
            for (int sub = 0; sub < textByCat[category].Count; sub++)
            {
                if (nameDicts.Count == sub)
                {
                    nameDicts.Add(new Dictionary<string, string>());
                }
                foreach (var pair in textByCat[category][sub])
                {
                    string key = pair.name.ToLower();
                    if (rebootNPC && category == "Npc")
                    {
                        key = key.Insert(8, "_"); //Adjust to match cml layout. The second _ is missing normally
                    }
                    if (!nameDicts[sub].ContainsKey(key) && pair.str != "dummy")
                    {
                        nameDicts[sub].Add(key, pair.str);
                    }
                }
            }
        }

        public static string NixIllegalCharacters(string str)
        {
            foreach (var ch in illegalChars.Keys)
            {
                if (str.Contains(ch))
                {
                    str = str.Replace(ch, illegalChars[ch]);
                }
            }

            return str;
        }

    }
}
