using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        System.Windows.Forms.OpenFileDialog fileOpen = new()
        {
            Filter = "All character files (*.cml,*.xxp,*.xxpu,*.bin)|*.fhp;*.fnp;*.fcp;*.fdp;*.mhp;*.mnp;*.mcp;*.mdp;" +
                            "*.fhpu;*.fnpu;*.fcpu;*.fdpu;*.mhpu;*.mnpu;*.mcpu;*.mdpu;*.cml;*.eml|" +
                            "Character Making files (*.cml,*.eml)|*.cml;*.eml|" +
                            "Salon files (*.xxp)|*.fhp;*.fnp;*.fcp;*.fdp;*.mhp;*.mnp;*.mcp;*.mdp|" +
                            "Unencrypted Salon files(*.xxpu)| *.fhpu; *.fnpu; *.fcpu; *.fdpu; *.mhpu; *.mnpu; *.mcpu; *.mdpu",
            Title = "Open character file"
        };
        string namesPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "names\\");
        string pso2BinCachePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "names\\pso2Bin.txt");
        string pso2_binDir = null;
        WIPBox wipBox = null;

        ColorPicker colorPicker = null;
        public MainWindow()
        {
            InitializeComponent();
            colorPicker = new ColorPicker();
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

        private void OpenCharacterFile(object sender, RoutedEventArgs e)
        {
            if (fileOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //try
                //{
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
                                OpenXXP(streamReader);
                                break;
                        }
                    }
                    SetEnabledState(true);
                /*}
                catch
                {
                    MessageBox.Show("Unable to open file. Check permissions and file type.");
                }*/
            }
        }

        private void OpenXXP(BufferedStreamReader streamReader)
        {
            int version = streamReader.Read<int>();
            streamReader.Seek(0x10, SeekOrigin.Begin);

            switch (version)
            {
                case 2:
                    xxpHandler = new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterHandlerReboot.XXPV2>());
                    break;
                case 5:
                    xxpHandler = new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterHandlerReboot.XXPV5>());
                    break;
                case 6:
                    xxpHandler = new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterHandlerReboot.XXPV5>());
                    break;
                case 7:
                    xxpHandler = new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterHandlerReboot.XXPV7>());
                    break;
                case 8:
                case 9:
                    xxpHandler = new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterHandlerReboot.XXPV9>());
                    break;
                case 10:
                    xxpHandler = new CharacterHandlerReboot.xxpGeneralReboot(streamReader.Read<CharacterHandlerReboot.XXPV10>());
                    break;
                default:
                    MessageBox.Show("Error: File version unknown. If this is a proper salon file, please report this!");
                    return;
            }

            ColorButtons();
        }

        private unsafe void ColorButtons()
        {
            //Colors
            fixed (byte* localArr = xxpHandler.ngsCOL2.outerColor1) { outerColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromRGBA(ColorConversion.BytesFromFixed(localArr))); };
            fixed (byte* localArr = xxpHandler.ngsCOL2.baseColor1) { baseColorButton.Background = new SolidColorBrush(ColorConversion.ColorFromRGBA(ColorConversion.BytesFromFixed(localArr))); };
        }

        private void LoadGameData()
        {
            pso2_binDir = File.ReadAllText(pso2BinCachePath);
            if(wipBox == null)
            {
                wipBox = new WIPBox("Generating name cache. This will be\n" +
                                    "done any time the .cmx is patched.\n" +
                                    "It may take a moment.");
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

        private unsafe void SetOuterColor(object sender, RoutedEventArgs e)
        {
            fixed(byte* col2 = xxpHandler.ngsCOL2.outerColor1)
            {
                if(colorPicker != null && colorPicker.IsLoaded == true)
                {
                    colorPicker.LinkExternalColor(col2, (Button)sender);
                } else
                {
                    colorPicker = new ColorPicker(col2, (Button)sender);
                }
            }
            colorPicker.Show();
        }

        private unsafe void SetBaseColor(object sender, RoutedEventArgs e)
        {
            fixed (byte* col2 = xxpHandler.ngsCOL2.baseColor1)
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
            colorPicker.Show();
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

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CostumeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            costumeIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.costumeOuterDict[text], CharacterMakingIndex.costumeIcon);
        }
        private void BasewearSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            basewearIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.basewearDict[text], CharacterMakingIndex.basewearIcon);
        }
        private void InnerwearSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            innerwearIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.innerwearDict[text], CharacterMakingIndex.innerwearIcon);
        }
        private void CastArmSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            castArmIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.castArmDict[text], CharacterMakingIndex.castArmIcon);
        }
        private void CastLegSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            castLegIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.castLegDict[text], CharacterMakingIndex.castLegIcon);
        }
        private void BodyPaint1SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            bodyPaintIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.bodyPaintDict[text], CharacterMakingIndex.bodyPaintIcon);
        }
        private void BodyPaint2SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            bodyPaint2Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.bodyPaintDict[text], CharacterMakingIndex.bodyPaintIcon);
        }
        private void StickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            stickerIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.stickerDict[text], CharacterMakingIndex.stickerIcon);
        }
        private void HairSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            hairIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.hairDict[text], CharacterMakingIndex.hairIcon);
        }
        private void RightEyeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            rightEyeIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyeDict[text], CharacterMakingIndex.eyeIcon);
        }
        private void EyebrowsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            eyebrowsIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyebrowDict[text], CharacterMakingIndex.eyebrowsIcon);
        }
        private void EyelashesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            eyelashesIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyelashDict[text], CharacterMakingIndex.eyelashesIcon);
        }
        private void LeftEyeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            leftEyeIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyeDict[text], CharacterMakingIndex.eyeIcon);
        }
        private void FaceModelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            string text = (sender as ComboBox).SelectedItem as string;
            leftEyeIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyeDict[text], CharacterMakingIndex.eyeIcon);
            */
        }
        private void FaceTexSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            string text = (sender as ComboBox).SelectedItem as string;
            leftEyeIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.eyeDict[text], CharacterMakingIndex.eyeIcon);
            */
        }
        private void EarsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            earsIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.earDict[text], CharacterMakingIndex.earIcon);
        }
        private void HornsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            hornsIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.hornDict[text], CharacterMakingIndex.hornIcon);
        }
        private void TeethSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            teethIcon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.teethDict[text], CharacterMakingIndex.teethIcon);
        }
        private void Acce1SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce1Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce2SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce2Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce3SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce3Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce4SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce4Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce5SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce5Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce6SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce6Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce7SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce7Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce8SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce8Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce9SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce9Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce10SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce10Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce11SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce11Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void Acce12SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            acce12Icon.Source = IceHandler.GetIconFromIce(pso2_binDir, cmxHandler.accessoryDict[text], CharacterMakingIndex.accessoryIcon);
        }
        private void SwimSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
        }
        private void GlideSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
        }
        private void JumpSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
        }
        private void LandingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
        }
        private void MovSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
        }
        private void SprintSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
        }
        private void IdleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
        }
    }
}
