using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AquaModelLibrary;
using Character_Making_File_Tool;
using Microsoft.WindowsAPICodePack.Dialogs;
using Reloaded.Memory.Streams;

namespace NGS_Salon_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CharacterHandlerReboot.xxpGeneralReboot xxpHandler = new CharacterHandlerReboot.xxpGeneralReboot();
        CharacterMakingIndexHandler cmxHandler = null;
        CommonOpenFileDialog pso2BinSelect = new CommonOpenFileDialog()
        {
            IsFolderPicker = true,
            Title = "Select pso2_bin",
        };
        CommonOpenFileDialog folderPick = new CommonOpenFileDialog()
        {
            IsFolderPicker = true
        };
        System.Windows.Forms.OpenFileDialog fileOpen = new()
        {
            Filter = "All character files (*.cml,*.xxp,*.xxpu,*.bin)|*.fhp;*.fnp;*.fcp;*.fdp;*.mhp;*.mnp;*.mcp;*.mdp;" +
                            "*.fhpu;*.fnpu;*.fcpu;*.fdpu;*.mhpu;*.mnpu;*.mcpu;*.mdpu;*.cml;*.eml|" +
                            "Character Making files (*.cml,*.eml)|*.cml;*.eml|" +
                            "Salon files (*.xxp)|*.fhp;*.fnp;*.fcp;*.fdp;*.mhp;*.mnp;*.mcp;*.mdp|" +
                            "Unencrypted Salon files(*.xxpu)| *.fhpu; *.fnpu; *.fcpu; *.fdpu; *.mhpu; *.mnpu; *.mcpu; *.mdpu",
            Title = "Open character file"
        };
        System.Windows.Forms.SaveFileDialog saveFileDialog = new()
        {
            Title = "Save character file"
        };
        string[] xxpSearchPattern = new string[] {"*.fhp", "*.fnp", "*.fcp", "*.fdp", "*.mhp", "*.mnp", "*.mcp", "*.mdp",
                            "*.fhpu", "*.fnpu", "*.fcpu", "*.fdpu", "*.mhpu", "*.mnpu", "*.mcpu", "*.mdpu"};
        string[] cmlSearchPattern = new string[] { "*.cml", "*.eml" };
        string namesPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "names\\");
        string pso2BinCachePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "names\\pso2Bin.txt");
        string pso2_binDir = null;
        private string openInitialDirectory;
        private string savedInitialDirectory;
        private string openedFileName;
        WIPBox wipBox = null;
        ColorPicker colorPicker = null;

        private bool motionWait = false; //Used to avoid loops when changing motion controls

        public unsafe MainWindow()
        {
            InitializeComponent();

#if !DEBUG
            menu.Items.Remove(debugOptions);
#endif

            //Disable unused items
            tabControl.Items.Remove(proportionsTab);
            tabControl.Items.Remove(customizeExpressionsTab);
            saveButton.IsEnabled = false;
            saveAsButton.IsEnabled = false;

            colorPicker = new();
            colorPicker.Hide();
            if (File.Exists(pso2BinCachePath))
            {
                LoadGameData();
            }
            else
            {
                MessageBox.Show("pso2_bin path is missing!\n" +
                    "Without it, character part names and icons won't appear!\n" +
                    "It can be set through File->Set pso2_bin directory");
            }
            SetEnabledState(false);

        }

        private void SetEnabledState(bool enabled)
        {
            if(enabled == false)
            {
                tabControl.SelectedItem = startupTab;
            } else
            {
                tabControl.SelectedItem = basicSettingsTab;
            }
            basicSettingsTab.IsEnabled = enabled;
            proportionsTab.IsEnabled = enabled;
            colorsTab.IsEnabled = enabled;
            partSelectionTab.IsEnabled = enabled;
            motionChangeTab.IsEnabled = enabled;
            customizeExpressionsTab.IsEnabled = enabled;
        }

        private void ToCML(object sender, RoutedEventArgs e)
        {
            folderPick.Title = "Please select the folder with the desired Salon xxp files";

            if (folderPick.ShowDialog() == CommonFileDialogResult.Ok)
            {
                List<string> files = new List<string>();
                foreach(string ext in xxpSearchPattern)
                {
                    files.AddRange(Directory.EnumerateFiles(folderPick.FileName, ext, SearchOption.TopDirectoryOnly));
                }

                foreach(string file in files)
                {
#if DEBUG
                    Trace.WriteLine(file);
#endif
                    byte[] data;
                    //Handle encrypted files
                    if (file.LastOrDefault() == 'p')
                    {
                        data = CharacterHandler.DecryptXXP(file);
                    }
                    else
                    {
                        data = File.ReadAllBytes(file);
                    }

                    CharacterHandlerReboot.xxpGeneralReboot tempxxp;
                    using (Stream stream = new MemoryStream(data))
                    using (var streamReader = new BufferedStreamReader(stream, 8192))
                    {
                        tempxxp = OpenXXP(streamReader);
                    }
                    WriteCML(tempxxp, file + ".cml");
                }
            }

        }

        private void ToXXP(object sender, RoutedEventArgs e)
        {
            folderPick.Title = "Please select the folder with the desired cml/eml files";

            if (folderPick.ShowDialog() == CommonFileDialogResult.Ok)
            {
                List<string> files = new List<string>();
                foreach (string ext in cmlSearchPattern)
                {
                    files.AddRange(Directory.EnumerateFiles(folderPick.FileName, ext, SearchOption.TopDirectoryOnly));
                }

                foreach (string file in files)
                {
                    CharacterHandlerReboot.xxpGeneralReboot tempxxp;
                    using (Stream stream = new MemoryStream(File.ReadAllBytes(file)))
                    using (var streamReader = new BufferedStreamReader(stream, 8192))
                    {
                        tempxxp = CMLHandler.ParseCML(streamReader);
                    }

                    GetXXPWildcards(out string letterOne, out string letterTwo);
                    WriteXXP(tempxxp, file + $".{letterOne}{letterTwo}p");
                }
            }
        }

        private void OpenCharacterFile(object sender, RoutedEventArgs e)
        {
            fileOpen.InitialDirectory = openInitialDirectory;
            if (fileOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    byte[] data;

                    //Handle encrypted files
                    if(fileOpen.FileName.LastOrDefault() == 'p')
                    {
                        data = CharacterHandler.DecryptXXP(fileOpen.FileName);
                    } else
                    {
                        data = File.ReadAllBytes(fileOpen.FileName);
                    }

                    using (Stream stream = new MemoryStream(data))
                    using (var streamReader = new BufferedStreamReader(stream, 8192))
                    {
                        string extension = Path.GetExtension(fileOpen.FileName).ToLower();
                        switch (extension)
                        {
                            case ".cml":
                            case ".eml": //Rarely used "enemy" cml. Seemingly just the same format?
                                xxpHandler = CMLHandler.ParseCML(streamReader);
                                break;
                            default:
                                xxpHandler = OpenXXP(streamReader);
                                break;
                        }
                    }

                    openedFileName = Path.GetFileName(fileOpen.FileName);
                    this.Title = "NGS Salon Tool - " + Path.GetFileName(fileOpen.FileName);
                    saveAsButton.IsEnabled = true;
                    saveButton.IsEnabled = true;


                    //Ensure parts exist in the dropdowns if they don't
                    PartCheck();

                    //Assign data
                    BasicSettings();
                    ColorButtons();
                    Proportions();
                    PartDropdowns();
                    MotionDropdowns();
                    ExpressionsData();
                    SetEnabledState(true);
                }
                catch
                {
                    MessageBox.Show("Unable to open file. Check permissions and file type.");
                }
                savedInitialDirectory = Path.GetDirectoryName(fileOpen.FileName);
                openInitialDirectory = Path.GetDirectoryName(fileOpen.FileName);
                fileOpen.FileName = "";
            }
        }

        private void SaveCharFileAs(object sender, RoutedEventArgs e)
        {
            string letterOne;
            string letterTwo;
            saveFileDialog.InitialDirectory = savedInitialDirectory;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(openedFileName);

            GetXXPWildcards(out letterOne, out letterTwo);

            saveFileDialog.Filter = $"V{xxpHandler.xxpVersion} Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p";

            /*
            if (unencryptCheckBox.Checked == true)
            {
                saveFileDialog.Filter = "V9 Salon files (*." + letterOne + letterTwo + "pu)|*." + letterOne + letterTwo + "pu";
                //+ "|V6 (Ep4 Char Creator) Salon files (*." + letterOne + letterTwo + "pu)|*." + letterOne + letterTwo + "pu|" +
                //"V5 Salon files (*." + letterOne + letterTwo + "pu)|*." + letterOne + letterTwo + "pu|" +
                //"V2 (Ep1 Char Creator) Salon files (*." + letterOne + letterTwo + "pu)|*." + letterOne + letterTwo + "pu";
            }
            else
            {
                saveFileDialog.Filter = "V9 Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p|" +
                "V6 (Ep4 Char Creator) Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p|" +
                "V5 Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p|" +
                "V2 (Ep1 Char Creator) Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p";

            }*/

            saveFileDialog.Filter += "|cml files (*.cml)|*.cml";
            //+ "|Data Dump (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        SaveXXP(saveFileDialog.FileName);
                        break;
                    case 2:
                        SaveCML(saveFileDialog.FileName);
                        break;
                    default:
                        break;
                }
            }
            savedInitialDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
            openedFileName = Path.GetFileName(saveFileDialog.FileName);
            saveFileDialog.FileName = "";

        }

        private void SaveCurrentCharFile(object sender, RoutedEventArgs e)
        {
            string ext = Path.GetExtension(openedFileName);
            if (ext != ".cml")
            {
                string letterOne;
                string letterTwo;
                GetXXPWildcards(out letterOne, out letterTwo);
                openedFileName = Path.GetFileNameWithoutExtension(openedFileName) + $".{letterOne}{letterTwo}p";
            }

            switch (ext)
            {
                case ".cml":
                    SaveCML(savedInitialDirectory + @"\" + openedFileName);
                    break;
                default:
                    SaveXXP(savedInitialDirectory + @"\" + openedFileName);
                    break;
            }

        }

        private void GetXXPWildcards(out string letterOne, out string letterTwo)
        {
            switch (xxpHandler.baseDOC.gender)
            {
                case 0:
                    letterOne = "m";
                    break;
                case 1:
                    letterOne = "f";
                    break;
                default:
                    letterOne = "m";
                    break;
            }

            switch (xxpHandler.baseDOC.race)
            {
                case 0:
                    letterTwo = "h";
                    break;
                case 1:
                    letterTwo = "n";
                    break;
                case 2:
                    letterTwo = "c";
                    break;
                case 3:
                    letterTwo = "d";
                    break;
                default:
                    letterTwo = "h";
                    break;
            }
        }

        private void SaveCML(string filename)
        {
            File.WriteAllBytes(filename, CMLHandler.GetNGSCML(xxpHandler));
            savedInitialDirectory = Path.GetDirectoryName(filename);
        }

        private void SaveXXP(string filename)
        {
            WriteXXP(xxpHandler, filename);
            savedInitialDirectory = Path.GetDirectoryName(filename);
        }

        private static void WriteCML(CharacterHandlerReboot.xxpGeneralReboot xxp, string fileName)
        {
            File.WriteAllBytes(fileName, CMLHandler.GetNGSCML(xxp));
        }

        private static void WriteXXP(CharacterHandlerReboot.xxpGeneralReboot xxp, string fileName)
        {

            int version;
            if (xxp.xxpVersion >= 0xA)
            {
                version = xxp.xxpVersion;
            } else
            {
                version = CharacterConstants.ngsSalonToolSizes.Keys.ToArray().Last();
                xxp.xxpVersion = version;
            }
            int fileSize = CharacterConstants.ngsSalonToolSizes[version];
            List<byte> fileData = new List<byte>();
            var body = xxp.GetBytes();
            body = CharacterHandler.EncryptData(body, fileSize, out int hash);

            fileData.AddRange(BitConverter.GetBytes(version));
            fileData.AddRange(BitConverter.GetBytes(fileSize));
            fileData.AddRange(BitConverter.GetBytes(hash));
            fileData.AddRange(new byte[] { 0, 0, 0, 0 });
            fileData.AddRange(body);

            File.WriteAllBytes(fileName, fileData.ToArray());
        }

        private static CharacterHandlerReboot.xxpGeneralReboot OpenXXP(BufferedStreamReader streamReader)
        {
            int version = streamReader.Read<int>();
            streamReader.Seek(0x10, SeekOrigin.Begin);

            switch (version)
            {
                case 2:
                    return new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterMainStructs.XXPV2>());
                case 5:
                    return new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterMainStructs.XXPV5>());
                case 6:
                    return new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterMainStructs.XXPV5>());
                case 7:
                    return new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterMainStructs.XXPV7>());
                case 8:
                case 9:
                    return new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterMainStructs.XXPV9>());
                case 10:
                    return new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterMainStructs.XXPV10>());
                case 11:
                    return new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterMainStructs.XXPV11>());
                case 12:
                    return new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterMainStructs.XXPV12>());
                default:
                    MessageBox.Show("Error: File version unknown. If this is a proper salon file, please report this!");
                    return null;
            }
        }

        private void PartCheck()
        {
            int castPart = (int)xxpHandler.baseSLCT.costumePart;
            int basePart = (int)xxpHandler.baseSLCT2.basewearPart;
            int innerPart = (int)xxpHandler.baseSLCT2.innerwearPart;
            int armPart = (int)xxpHandler.baseSLCT.armPart;
            int legPart = (int)xxpHandler.baseSLCT.legPart;
            int bodyPaint = (int)xxpHandler.baseSLCT.bodyPaintPart;
            int bodyPaint2 = (int)xxpHandler.baseSLCT2.bodyPaint2Part;
            int stickerPart = (int)xxpHandler.baseSLCT.stickerPart;
            int hairPart = (int)xxpHandler.baseSLCT.hairPart;
            int rightEyePart = (int)xxpHandler.baseSLCT.eyePart;
            int leftEyePart = (int)xxpHandler.leftEyePart;
            int eyebrowPart = (int)xxpHandler.baseSLCT.eyebrowPart;
            int eyelashPart = (int)xxpHandler.baseSLCT.eyelashPart;
            int facePart = (int)xxpHandler.baseSLCT.faceTypePart;
            int faceTexPart = (int)xxpHandler.baseSLCT.faceTexPart;
            int earPart = (int)xxpHandler.baseSLCTNGS.earsPart;
            int hornPart = (int)xxpHandler.baseSLCTNGS.hornPart;
            int teethPart = (int)xxpHandler.baseSLCTNGS.teethPart;
            int acc1Part = (int)xxpHandler.baseSLCT.acc1Part;
            int acc2Part = (int)xxpHandler.baseSLCT.acc2Part;
            int acc3Part = (int)xxpHandler.baseSLCT.acc3Part;
            int acc4Part = (int)xxpHandler.baseSLCT2.acc4Part;
            int acc5Part = (int)xxpHandler.baseSLCTNGS.acc5Part;
            int acc6Part = (int)xxpHandler.baseSLCTNGS.acc6Part;
            int acc7Part = (int)xxpHandler.baseSLCTNGS.acc7Part;
            int acc8Part = (int)xxpHandler.baseSLCTNGS.acc8Part;
            int acc9Part = (int)xxpHandler.baseSLCTNGS.acc9Part;
            int acc10Part = (int)xxpHandler.baseSLCTNGS.acc10Part;
            int acc11Part = (int)xxpHandler.baseSLCTNGS.acc11Part;
            int acc12Part = (int)xxpHandler.baseSLCTNGS.acc12Part;
            if(cmxHandler == null)
            {
                cmxHandler = new CharacterMakingIndexHandler();
            }
            AddPartIfMissing(castPart, cmxHandler.costumeOuterDict, cmxHandler.costumeOuterDictReverse);
            AddPartIfMissing(basePart, cmxHandler.basewearDict, cmxHandler.basewearDictReverse);
            AddPartIfMissing(innerPart, cmxHandler.innerwearDict, cmxHandler.innerwearDictReverse);
            AddPartIfMissing(armPart, cmxHandler.castArmDict, cmxHandler.castArmDictReverse);
            AddPartIfMissing(legPart, cmxHandler.castLegDict, cmxHandler.castLegDictReverse);
            AddPartIfMissing(bodyPaint, cmxHandler.bodyPaintDict, cmxHandler.bodyPaintDictReverse);
            AddPartIfMissing(bodyPaint2, cmxHandler.bodyPaintDict, cmxHandler.bodyPaintDictReverse);
            AddPartIfMissing(stickerPart, cmxHandler.stickerDict, cmxHandler.stickerDictReverse);
            AddPartIfMissing(hairPart, cmxHandler.hairDict, cmxHandler.hairDictReverse);
            AddPartIfMissing(rightEyePart, cmxHandler.eyeDict, cmxHandler.eyeDictReverse);
            AddPartIfMissing(leftEyePart, cmxHandler.eyeDict, cmxHandler.eyeDictReverse);
            AddPartIfMissing(eyebrowPart, cmxHandler.eyebrowDict, cmxHandler.eyebrowDictReverse);
            AddPartIfMissing(eyelashPart, cmxHandler.eyelashDict, cmxHandler.eyelashDictReverse);
            AddPartIfMissing(facePart, cmxHandler.faceDict, cmxHandler.faceDictReverse);
            AddPartIfMissing(faceTexPart, cmxHandler.faceTexDict, cmxHandler.faceTexDictReverse);
            AddPartIfMissing(earPart, cmxHandler.earDict, cmxHandler.earDictReverse);
            AddPartIfMissing(hornPart, cmxHandler.hornDict, cmxHandler.hornDictReverse);
            AddPartIfMissing(teethPart, cmxHandler.teethDict, cmxHandler.teethDictReverse);
            AddPartIfMissing(acc1Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc2Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc3Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc4Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc5Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc6Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc7Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc8Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc9Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc10Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc11Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
            AddPartIfMissing(acc12Part, cmxHandler.accessoryDict, cmxHandler.accessoryDictReverse);
        }

        private void AddPartIfMissing(int key, Dictionary<string, int> dict, Dictionary<int, string> dictReverse)
        {
            if(!dictReverse.ContainsKey(key))
            {
                string name = $"[Unnamed {key}]";
                dictReverse[key] = name;
                dict[name] = key;

            }
        }

        private void BasicSettings()
        {
            raceUD.Value = (int?)xxpHandler.baseDOC.race;
            genderUD.Value = (int?)xxpHandler.baseDOC.gender;
            muscleUD.Value = (int?)xxpHandler.baseDOC.muscleMass;
            skinVariantUD.Value = xxpHandler.skinVariant;
            cmlVariableUD.Value = xxpHandler.cmlVariant;
            eyebrowDensityUD.Value = xxpHandler.eyebrowDensity;
        }
        
        private void Proportions()
        {
        }

        private unsafe void ColorButtons()
        {
            //Colors
            fixed (byte* localArr = xxpHandler.ngsCOL2.outerColor1) { outerColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.outerColor2) { outerColor2Button.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.baseColor1) { baseColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.baseColor2) { baseColor2Button.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.innerColor1) { innerColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.innerColor2) { innerColor2Button.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.hairColor1) { hairColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.hairColor2) { hairColor2Button.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.skinColor1) { skinColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.skinColor2) { skinColor2Button.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.leftEyeColor) { leftEyeColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.rightEyeColor) { rightEyeColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.eyebrowColor) { eyebrowsColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.eyelashColor) { eyelashesColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.mainColor) { mainColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.subColor1) { subcolor1Button.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.subColor2) { subcolor2Button.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.subColor3) { subcolor3Button.Background = new SolidColorBrush(ColorConversion.ColorFromBGRA(ColorConversion.BytesFromFixed(localArr, true))); };
            
        }

        private void PartDropdowns()
        {
            costumeCB.SelectedIndex = costumeCB.Items.IndexOf(cmxHandler.costumeOuterDictReverse[(int)xxpHandler.baseSLCT.costumePart]);
            basewearCB.SelectedIndex = basewearCB.Items.IndexOf(cmxHandler.basewearDictReverse[(int)xxpHandler.baseSLCT2.basewearPart]);
            innerwearCB.SelectedIndex = innerwearCB.Items.IndexOf(cmxHandler.innerwearDictReverse[(int)xxpHandler.baseSLCT2.innerwearPart]);
            castArmCB.SelectedIndex = castArmCB.Items.IndexOf(cmxHandler.castArmDictReverse[(int)xxpHandler.baseSLCT.armPart]);
            castLegCB.SelectedIndex = castLegCB.Items.IndexOf(cmxHandler.castLegDictReverse[(int)xxpHandler.baseSLCT.legPart]);
            bodyPaintCB.SelectedIndex = bodyPaintCB.Items.IndexOf(cmxHandler.bodyPaintDictReverse[(int)xxpHandler.baseSLCT.bodyPaintPart]);
            bodyPaint2CB.SelectedIndex = bodyPaint2CB.Items.IndexOf(cmxHandler.bodyPaintDictReverse[(int)xxpHandler.baseSLCT2.bodyPaint2Part]);
            stickerCB.SelectedIndex = stickerCB.Items.IndexOf(cmxHandler.stickerDictReverse[(int)xxpHandler.baseSLCT.stickerPart]);
            hairCB.SelectedIndex = hairCB.Items.IndexOf(cmxHandler.hairDictReverse[(int)xxpHandler.baseSLCT.hairPart]);
            rightEyeCB.SelectedIndex = rightEyeCB.Items.IndexOf(cmxHandler.eyeDictReverse[(int)xxpHandler.baseSLCT.eyePart]);
            leftEyeCB.SelectedIndex = leftEyeCB.Items.IndexOf(cmxHandler.eyeDictReverse[(int)xxpHandler.leftEyePart]);
            eyebrowsCB.SelectedIndex = eyebrowsCB.Items.IndexOf(cmxHandler.eyebrowDictReverse[(int)xxpHandler.baseSLCT.eyebrowPart]);
            eyelashesCB.SelectedIndex = eyelashesCB.Items.IndexOf(cmxHandler.eyelashDictReverse[(int)xxpHandler.baseSLCT.eyelashPart]);
            faceModelCB.SelectedIndex = faceModelCB.Items.IndexOf(cmxHandler.faceDictReverse[(int)xxpHandler.baseSLCT.faceTypePart]);
            faceTexCB.SelectedIndex = faceTexCB.Items.IndexOf(cmxHandler.faceTexDictReverse[(int)xxpHandler.baseSLCT.faceTexPart]);
            earsCB.SelectedIndex = earsCB.Items.IndexOf(cmxHandler.earDictReverse[(int)xxpHandler.baseSLCTNGS.earsPart]);
            hornsCB.SelectedIndex = hornsCB.Items.IndexOf(cmxHandler.hornDictReverse[(int)xxpHandler.baseSLCTNGS.hornPart]);
            teethCB.SelectedIndex = teethCB.Items.IndexOf(cmxHandler.teethDictReverse[(int)xxpHandler.baseSLCTNGS.teethPart]);
            acce1CB.SelectedIndex = acce1CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCT.acc1Part]);
            acce2CB.SelectedIndex = acce2CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCT.acc2Part]);
            acce3CB.SelectedIndex = acce3CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCT.acc3Part]);
            acce4CB.SelectedIndex = acce4CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCT2.acc4Part]);

            acce5CB.SelectedIndex = acce5CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCTNGS.acc5Part]);
            acce6CB.SelectedIndex = acce6CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCTNGS.acc6Part]);
            acce7CB.SelectedIndex = acce7CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCTNGS.acc7Part]);
            acce8CB.SelectedIndex = acce8CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCTNGS.acc8Part]);

            acce9CB.SelectedIndex = acce9CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCTNGS.acc9Part]);
            acce10CB.SelectedIndex = acce10CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCTNGS.acc10Part]);
            acce11CB.SelectedIndex = acce11CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCTNGS.acc11Part]);
            acce12CB.SelectedIndex = acce12CB.Items.IndexOf(cmxHandler.accessoryDictReverse[(int)xxpHandler.baseSLCTNGS.acc12Part]);
        }

        private void MotionDropdowns()
        {
            swimUD.Value = xxpHandler.ngsMTON.swimMotion;
            glideUD.Value = xxpHandler.ngsMTON.glideMotion;
            movUD.Value = xxpHandler.ngsMTON.walkRunMotion;
            sprintUD.Value = xxpHandler.ngsMTON.dashMotion;
            landingUD.Value = xxpHandler.ngsMTON.landingMotion;
            jumpUD.Value = xxpHandler.ngsMTON.jumpMotion;
            idleUD.Value = xxpHandler.ngsMTON.idleMotion;
        }

        private void ExpressionsData()
        {
        }

        private void LoadGameData()
        {
            pso2_binDir = File.ReadAllText(pso2BinCachePath);
            if(wipBox == null)
            {
                wipBox = new WIPBox("Generating name cache.");
            }
            cmxHandler = new CharacterMakingIndexHandler(pso2_binDir, wipBox);

            //Wire part text
            costumeCB.ItemsSource = cmxHandler.costumeOuterDict.Keys;
            basewearCB.ItemsSource = cmxHandler.basewearDict.Keys;
            innerwearCB.ItemsSource = cmxHandler.innerwearDict.Keys;
            castArmCB.ItemsSource = cmxHandler.castArmDict.Keys;
            castLegCB.ItemsSource = cmxHandler.castLegDict.Keys;
            bodyPaintCB.ItemsSource = cmxHandler.bodyPaintDict.Keys;
            bodyPaint2CB.ItemsSource = cmxHandler.bodyPaintDict.Keys;
            hairCB.ItemsSource = cmxHandler.hairDict.Keys;
            stickerCB.ItemsSource = cmxHandler.stickerDict.Keys;
            rightEyeCB.ItemsSource = cmxHandler.eyeDict.Keys;
            eyebrowsCB.ItemsSource = cmxHandler.eyebrowDict.Keys;
            eyelashesCB.ItemsSource = cmxHandler.eyelashDict.Keys;
            leftEyeCB.ItemsSource = cmxHandler.eyeDict.Keys;
            faceModelCB.ItemsSource = cmxHandler.faceDict.Keys;
            faceTexCB.ItemsSource = cmxHandler.faceTexDict.Keys;
            earsCB.ItemsSource = cmxHandler.earDict.Keys;
            hornsCB.ItemsSource = cmxHandler.hornDict.Keys;
            teethCB.ItemsSource = cmxHandler.teethDict.Keys;
            acce1CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce2CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce3CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce4CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce5CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce6CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce7CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce8CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce9CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce10CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce11CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            acce12CB.ItemsSource = cmxHandler.accessoryDict.Keys;
            swimCB.ItemsSource = cmxHandler.swimDict.Keys;
            glideCB.ItemsSource = cmxHandler.glideDict.Keys;
            jumpCB.ItemsSource = cmxHandler.jumpDict.Keys;
            landingCB.ItemsSource = cmxHandler.landingDict.Keys;
            movCB.ItemsSource = cmxHandler.movDict.Keys;
            sprintCB.ItemsSource = cmxHandler.sprintDict.Keys;
            idleCB.ItemsSource = cmxHandler.idleDict.Keys;
        }
        private void SetPSO2Bin(object sender, RoutedEventArgs e)
        {
            if(pso2BinSelect.ShowDialog() == CommonFileDialogResult.Ok)
            {
                //Ensure paths are created and ready
                Directory.CreateDirectory(namesPath);

                File.WriteAllText(pso2BinCachePath, pso2BinSelect.FileName);
                LoadGameData();
            }
        }

        private void DecryptXXPs(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Select xxp file(s)",
                Filter = "File |*.*p",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    File.WriteAllBytes(file + "u", CharacterHandler.DecryptXXP(file));
                }
            }
        }

        private void EncryptXXPs(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Select unencrypted xxp file(s)",
                Filter = "File |*.*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    CharacterHandler.EncryptAndWrite(file);
                }
            }
        }

        private void OverwriteFaceModel(object sender, RoutedEventArgs e)
        {
            if (openedFileName == null)
            {
                MessageBox.Show("You must have data loaded to overwrite face model.");
                return;
            }

            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Select xxp file(s)",
                Filter = "File |*.*p"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var data = File.ReadAllBytes(openFileDialog.FileName);
                CharacterHandlerReboot.xxpGeneralReboot faceSourceCharacterHandler;
                using (Stream stream = new MemoryStream(data))
                using (var streamReader = new BufferedStreamReader(stream, 8192))
                {
                    faceSourceCharacterHandler = OpenXXP(streamReader);
                }

                FaceOverwriteDialog faceOverwriteDialog = new FaceOverwriteDialog();

                if (faceSourceCharacterHandler.xxpVersion < 12)
                {
                    faceOverwriteDialog.EnableAltFaceSource(false);
                }

                if (xxpHandler.xxpVersion < 12)
                {
                    faceOverwriteDialog.EnableAltFaceDest(false);
                }

                faceOverwriteDialog.ShowDialog();
                
                if (faceOverwriteDialog.DialogResult.HasValue && faceOverwriteDialog.DialogResult.Value)
                {
                    CharacterDataStructsReboot.AltFaceFIGR sourceFaceFIGR = faceSourceCharacterHandler.GetFaceData();

                    if (faceOverwriteDialog.AltFaceSource)
                    {
                        if (faceSourceCharacterHandler.xxpVersion >= 12)
                        {
                            sourceFaceFIGR = faceSourceCharacterHandler.GetAltFaceData();
                        }
                    }

                    if (faceOverwriteDialog.OverwriteBaseFace)
                    {
                        xxpHandler.SetFaceData(sourceFaceFIGR);
                    }

                    if (faceOverwriteDialog.OverwriteAltFace)
                    {
                        xxpHandler.SetAltFaceData(sourceFaceFIGR);
                    }
                }
            }
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RaceUDChanged(object sender, RoutedEventArgs e)
        {
            xxpHandler.baseDOC.race = (uint)raceUD.Value;
        }
        private void GenderUDChanged(object sender, RoutedEventArgs e)
        {
            xxpHandler.baseDOC.gender = (uint)genderUD.Value;
        }
        private void MuscleUDChanged(object sender, RoutedEventArgs e)
        {
            xxpHandler.baseDOC.muscleMass = (float)muscleUD.Value;
        }
        private void SkinVariantUDChanged(object sender, RoutedEventArgs e)
        {
            xxpHandler.skinVariant = (byte)skinVariantUD.Value;
        }
        private void EyebrowDensityUDChanged(object sender, RoutedEventArgs e)
        {
            xxpHandler.eyebrowDensity = (sbyte)eyebrowDensityUD.Value;
        }
        private void CMLVariableUDChanged(object sender, RoutedEventArgs e)
        {
            xxpHandler.cmlVariant = (short)cmlVariableUD.Value;
        }
        private void CostumeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if(text == null)
            {
                costumeIcon.Source = null;
                return;
            }
            costumeIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.costumeOuterDict[text], CharacterMakingIndex.costumeIcon);
            xxpHandler.baseSLCT.costumePart = (uint)cmxHandler.costumeOuterDict[text];
        }
        private void BasewearSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                basewearIcon.Source = null;
                return;
            }
            basewearIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.basewearDict[text], CharacterMakingIndex.basewearIcon);
            xxpHandler.baseSLCT2.basewearPart = (uint)cmxHandler.basewearDict[text];
        }
        private void InnerwearSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                innerwearIcon.Source = null;
                return;
            }
            innerwearIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.innerwearDict[text], CharacterMakingIndex.innerwearIcon);
            xxpHandler.baseSLCT2.innerwearPart = (uint)cmxHandler.innerwearDict[text];
        }
        private void CastArmSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                castArmIcon.Source = null;
                return;
            }
            castArmIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.castArmDict[text], CharacterMakingIndex.castArmIcon);
            xxpHandler.baseSLCT.armPart = (uint)cmxHandler.castArmDict[text];
        }
        private void CastLegSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                castLegIcon.Source = null;
                return;
            }
            castLegIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.castLegDict[text], CharacterMakingIndex.castLegIcon);
            xxpHandler.baseSLCT.legPart = (uint)cmxHandler.castLegDict[text];
        }
        private void BodyPaint1SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                bodyPaintIcon.Source = null;
                return;
            }
            bodyPaintIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.bodyPaintDict[text], CharacterMakingIndex.bodyPaintIcon);
            xxpHandler.baseSLCT.bodyPaintPart = (uint)cmxHandler.bodyPaintDict[text];
        }
        private void BodyPaint2SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                bodyPaint2Icon.Source = null;
                return;
            }
            bodyPaint2Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.bodyPaintDict[text], CharacterMakingIndex.bodyPaintIcon);
            xxpHandler.baseSLCT2.bodyPaint2Part = (uint)cmxHandler.bodyPaintDict[text];
        }
        private void StickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                stickerIcon.Source = null;
                return;
            }
            stickerIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.stickerDict[text], CharacterMakingIndex.stickerIcon);
            xxpHandler.baseSLCT.stickerPart = (uint)cmxHandler.stickerDict[text];
        }
        private void HairSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                hairIcon.Source = null;
                return;
            }
            hairIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.hairDict[text], CharacterMakingIndex.hairIcon);
            xxpHandler.baseSLCT.hairPart = (uint)cmxHandler.hairDict[text];
        }
        private void RightEyeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                rightEyeIcon.Source = null;
                return;
            }
            rightEyeIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyeDict[text], CharacterMakingIndex.eyeIcon);
            xxpHandler.baseSLCT.eyePart = (uint)cmxHandler.eyeDict[text];
        }
        private void EyebrowsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                eyebrowsIcon.Source = null;
                return;
            }
            eyebrowsIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyebrowDict[text], CharacterMakingIndex.eyebrowsIcon);
            xxpHandler.baseSLCT.eyebrowPart = (uint)cmxHandler.eyebrowDict[text];
        }
        private void EyelashesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                eyelashesIcon.Source = null;
                return;
            }
            eyelashesIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyelashDict[text], CharacterMakingIndex.eyelashesIcon);
            xxpHandler.baseSLCT.eyelashPart = (uint)cmxHandler.eyelashDict[text];
        }
        private void LeftEyeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                leftEyeIcon.Source = null;
                return;
            }
            leftEyeIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyeDict[text], CharacterMakingIndex.eyeIcon);
            xxpHandler.leftEyePart = (uint)cmxHandler.eyeDict[text];
        }
        private void FaceModelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                return;
            }
            xxpHandler.baseSLCT.faceTypePart = (uint)cmxHandler.faceDict[text];
        }
        private void FaceTexSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                return;
            }
            xxpHandler.baseSLCT.faceTexPart = (uint)cmxHandler.faceTexDict[text];
        }
        private void EarsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                earsIcon.Source = null;
                return;
            }
            earsIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.earDict[text], CharacterMakingIndex.earIcon);
            xxpHandler.baseSLCTNGS.earsPart = (uint)cmxHandler.earDict[text];
        }
        private void HornsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                hornsIcon.Source = null;
                return;
            }
            hornsIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.hornDict[text], CharacterMakingIndex.hornIcon);
            xxpHandler.baseSLCTNGS.hornPart = (uint)cmxHandler.hornDict[text];
        }
        private void TeethSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                teethIcon.Source = null;
                return;
            }
            teethIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.teethDict[text], CharacterMakingIndex.teethIcon);
            xxpHandler.baseSLCTNGS.teethPart = (uint)cmxHandler.teethDict[text];
        }
        private void Acce1SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce1Icon.Source = null;
                return;
            }
            acce1Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCT.acc1Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce2SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce2Icon.Source = null;
                return;
            }
            acce2Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCT.acc2Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce3SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce3Icon.Source = null;
                return;
            }
            acce3Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCT.acc3Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce4SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce4Icon.Source = null;
                return;
            }
            acce4Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCT2.acc4Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce5SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce5Icon.Source = null;
                return;
            }
            acce5Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCTNGS.acc5Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce6SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce6Icon.Source = null;
                return;
            }
            acce6Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCTNGS.acc6Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce7SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce7Icon.Source = null;
                return;
            }
            acce7Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCTNGS.acc7Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce8SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce8Icon.Source = null;
                return;
            }
            acce8Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCTNGS.acc8Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce9SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce9Icon.Source = null;
                return;
            }
            acce9Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCTNGS.acc9Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce10SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce10Icon.Source = null;
                return;
            }
            acce10Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCTNGS.acc10Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce11SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce11Icon.Source = null;
                return;
            }
            acce11Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCTNGS.acc11Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void Acce12SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            if (text == null)
            {
                acce12Icon.Source = null;
                return;
            }
            acce12Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
            xxpHandler.baseSLCTNGS.acc12Part = (uint)cmxHandler.accessoryDict[text];
        }
        private void SwimSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(motionWait == false && cmxHandler != null && cmxHandler.swimDict.Count > 0 && swimCB.SelectedIndex != -1)
            {
                motionWait = true;
                swimUD.Value = cmxHandler.swimDict[(string)swimCB.SelectedItem];
                xxpHandler.ngsMTON.swimMotion = (int)swimUD.Value;
                motionWait = false;
            }
        }
        private void SwimUDChanged(object sender, RoutedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.swimDict.Count > 0)
            {
                motionWait = true;
                if(cmxHandler.swimDictReverse.TryGetValue((int)swimUD.Value, out string nameKey))
                {
                    swimCB.SelectedIndex = swimCB.Items.IndexOf(nameKey);
                } else
                {
                    swimCB.SelectedIndex = -1;
                }
                motionWait = false;
            }
        }
        private void GlideSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.glideDict.Count > 0 && glideCB.SelectedIndex != -1)
            {
                motionWait = true;
                glideUD.Value = cmxHandler.glideDict[(string)glideCB.SelectedItem];
                xxpHandler.ngsMTON.glideMotion = (int)glideUD.Value;
                motionWait = false;
            }
        }
        private void GlideUDChanged(object sender, RoutedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.glideDict.Count > 0)
            {
                motionWait = true;
                if (cmxHandler.glideDictReverse.TryGetValue((int)glideUD.Value, out string nameKey))
                {
                    glideCB.SelectedIndex = glideCB.Items.IndexOf(nameKey);
                }
                else
                {
                    glideCB.SelectedIndex = -1;
                }
                motionWait = false;
            }
        }
        private void JumpSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.jumpDict.Count > 0 && jumpCB.SelectedIndex != -1)
            {
                motionWait = true;
                jumpUD.Value = cmxHandler.jumpDict[(string)jumpCB.SelectedItem];
                xxpHandler.ngsMTON.jumpMotion = (int)jumpUD.Value;
                motionWait = false;
            }
        }
        private void JumpUDChanged(object sender, RoutedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.jumpDict.Count > 0)
            {
                motionWait = true;
                if (cmxHandler.jumpDictReverse.TryGetValue((int)jumpUD.Value, out string nameKey))
                {
                    jumpCB.SelectedIndex = jumpCB.Items.IndexOf(nameKey);
                }
                else
                {
                    jumpCB.SelectedIndex = -1;
                }
                motionWait = false;
            }
        }
        private void LandingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.landingDict.Count > 0 && landingCB.SelectedIndex != -1)
            {
                motionWait = true;
                landingUD.Value = cmxHandler.landingDict[(string)landingCB.SelectedItem];
                xxpHandler.ngsMTON.landingMotion = (int)landingUD.Value;
                motionWait = false;
            }
        }
        private void LandingUDChanged(object sender, RoutedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.landingDict.Count > 0)
            {
                motionWait = true;
                if (cmxHandler.landingDictReverse.TryGetValue((int)landingUD.Value, out string nameKey))
                {
                    landingCB.SelectedIndex = landingCB.Items.IndexOf(nameKey);
                }
                else
                {
                    landingCB.SelectedIndex = -1;
                }
                motionWait = false;
            }
        }
        private void MovSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.movDict.Count > 0 && movCB.SelectedIndex != -1)
            {
                motionWait = true;
                movUD.Value = cmxHandler.movDict[(string)movCB.SelectedItem];
                xxpHandler.ngsMTON.walkRunMotion = (int)movUD.Value;
                motionWait = false;
            }
        }
        private void MovUDChanged(object sender, RoutedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.movDict.Count > 0)
            {
                motionWait = true;
                if (cmxHandler.movDictReverse.TryGetValue((int)movUD.Value, out string nameKey))
                {
                    movCB.SelectedIndex = movCB.Items.IndexOf(nameKey);
                }
                else
                {
                    movCB.SelectedIndex = -1;
                }
                motionWait = false;
            }
        }
        private void SprintSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.sprintDict.Count > 0 && sprintCB.SelectedIndex != -1)
            {
                motionWait = true;
                sprintUD.Value = cmxHandler.sprintDict[(string)sprintCB.SelectedItem];
                xxpHandler.ngsMTON.dashMotion = (int)sprintUD.Value;
                motionWait = false;
            }
        }
        private void SprintUDChanged(object sender, RoutedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.sprintDict.Count > 0)
            {
                motionWait = true;
                if (cmxHandler.sprintDictReverse.TryGetValue((int)sprintUD.Value, out string nameKey))
                {
                    sprintCB.SelectedIndex = sprintCB.Items.IndexOf(nameKey);
                }
                else
                {
                    sprintCB.SelectedIndex = -1;
                }
                motionWait = false;
            }
        }
        private void IdleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.idleDict.Count > 0 && idleCB.SelectedIndex != -1)
            {
                motionWait = true;
                idleUD.Value = cmxHandler.idleDict[(string)idleCB.SelectedItem];
                xxpHandler.ngsMTON.idleMotion = (int)idleUD.Value;
                motionWait = false;
            }
        }
        private void IdleUDChanged(object sender, RoutedEventArgs e)
        {
            if (motionWait == false && cmxHandler != null && cmxHandler.idleDict.Count > 0)
            {
                motionWait = true;
                if (cmxHandler.idleDictReverse.TryGetValue((int)idleUD.Value, out string nameKey))
                {
                    idleCB.SelectedIndex = idleCB.Items.IndexOf(nameKey);
                }
                else
                {
                    idleCB.SelectedIndex = -1;
                }
                motionWait = false;
            }
        }
        private unsafe void SetupColorPicker(object sender, byte* col2)
        {
            if (colorPicker != null && colorPicker.IsLoaded == true)
            {
                colorPicker.LinkExternalColor(col2, (Button)sender);
            }
            else
            {
                colorPicker = new ColorPicker(col2, (Button)sender);
            }
        }

        private unsafe void SetOuterColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.outerColor1)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }

        private unsafe void SetOuterColor2(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.outerColor2)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetBaseColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.baseColor1)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetBaseColor2(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.baseColor2)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetInnerColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.innerColor1)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetInnerColor2(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.innerColor2)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetSkinColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.skinColor1)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetSkinColor2(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.skinColor2)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetHairColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.hairColor1)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetHairColor2(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.hairColor2)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetLeftEyeColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.leftEyeColor)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetRightEyeColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.rightEyeColor)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetEyelashColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.eyelashColor)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetEyebrowColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.eyebrowColor)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetMainColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.mainColor)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetSubColor1(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.subColor1)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetSubColor2(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.subColor2)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }
        private unsafe void SetSubColor3(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.subColor3)
            {
                SetupColorPicker(sender, col2);
            }
            colorPicker.Show();
        }

        public void SetGlobalMin(object sender, RoutedEventArgs e)
        {
            if(openedFileName == null)
            {
                MessageBox.Show("You must have data loaded to fix it!");
                return;
            }
            if(xxpHandler.baseFIGR.bodyVerts.X < CharacterConstants.MinHeightBodySliderNGS)
            {
                xxpHandler.baseFIGR.bodyVerts.X = CharacterConstants.MinHeightBodySliderNGS;
            }
            if(xxpHandler.baseFIGR.legVerts.X < CharacterConstants.MinHeightLegSliderNGS)
            {
                xxpHandler.baseFIGR.legVerts.X = CharacterConstants.MinHeightLegSliderNGS;
            }
        }

        //This bug happens when the value is above the expected range. In the past, this was treated as human color. Now, the game doesn't like it. Setting to 0 fixes it.
        //This is something that may have resulted from older cml character file edits.
        public void FixSkinColorBug(object sender, RoutedEventArgs e)
        {
            xxpHandler.skinVariant = 0; 
        }

        public void ExportCharModel(object sender, RoutedEventArgs e)
        {

        }
    }
}
