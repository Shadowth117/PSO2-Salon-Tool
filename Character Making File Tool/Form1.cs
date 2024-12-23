﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Windows.Forms;

namespace Character_Making_File_Tool
{
    public partial class Form1 : Form
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private CharacterHandler characterHandler;
        private List<RadioButton> genderButtons;
        private List<RadioButton> raceButtons;
        private string originalName;
        private string openedFileName;
        private string openInitialDirectory;
        private string savedInitialDirectory;

        public Form1()
        {
            InitializeComponent();
            debugBatchButton.Visible = false;
            debugBatchButton.Enabled = false;
            debugEncryptButton.Visible = false;
            debugEncryptButton.Enabled = false;

#if DEBUG
            debugBatchButton.Visible = true;
            debugBatchButton.Enabled = true;
            debugEncryptButton.Visible = true;
            debugEncryptButton.Enabled = true;
#endif
            originalName = Text;
            saveButton.Enabled = false;
            quitButton.Visible = false;
            openFileDialog = new OpenFileDialog()
            {
                Filter = "All character files (*.cml,*.xxp,*.xxpu,*.bin)|*.fhp;*.fnp;*.fcp;*.fdp;*.mhp;*.mnp;*.mcp;*.mdp;" +
                            "*.fhpu;*.fnpu;*.fcpu;*.fdpu;*.mhpu;*.mnpu;*.mcpu;*.mdpu;*.cml;*.bin|" +
                            "Character Markup Language files (*.cml)|*.cml|" +
                            "Salon files (*.xxp)|*.fhp;*.fnp;*.fcp;*.fdp;*.mhp;*.mnp;*.mcp;*.mdp|" +
                            "Unencrypted Salon files(*.xxpu)| *.fhpu; *.fnpu; *.fcpu; *.fdpu; *.mhpu; *.mnpu; *.mcpu; *.mdpu|" +
                            "Packet Data (*.bin)| *.bin",
                Title = "Open character file"
            };
            saveFileDialog = new SaveFileDialog()
            {
                Title = "Save character file"
            };
            characterHandler = new CharacterHandler();
            genderButtons = new List<RadioButton>()
            {
                maleButton,
                femaleButton
            };
            raceButtons = new List<RadioButton>()
            {
                humanButton,
                newmanButton,
                castButton,
                deumanButton
            };

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openButton_Click(object sender, EventArgs e)
        {
            showFileSubmenu(fileButtonSubmenu);
            openFileDialog.InitialDirectory = openInitialDirectory;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    switch (extension)
                    {
                        case ".cml":
                            characterHandler.ParseCML(openFileDialog.FileName);
                            break;
                        case ".bin":
                            characterHandler.ParsePacket(openFileDialog.FileName);
                            break;
                        default:
                            characterHandler.ParseToStruct(characterHandler.DecryptFile(openFileDialog.FileName));
                            break;
                    }

                    //Setup UI
                    genderButtons[(int)characterHandler.xxpGeneral.baseDOC.gender].Checked = true;
                    raceButtons[(int)characterHandler.xxpGeneral.baseDOC.race].Checked = true;

                    if (characterHandler.getVersion() != -1)
                    {
                        versionLabel.Text = "Version: " + characterHandler.getVersion();
                    }
                    else
                    {
                        versionLabel.Text = "Version: cml";
                    }
                    Text = originalName + " - " + Path.GetFileName(openFileDialog.FileName);
                    openedFileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    openInitialDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                    saveButton.Enabled = true;

                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }


        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            showFileSubmenu(fileButtonSubmenu);
            string letterOne;
            string letterTwo;

            saveFileDialog.InitialDirectory = savedInitialDirectory;
            saveFileDialog.FileName = openedFileName;

            switch (characterHandler.xxpGeneral.baseDOC.gender)
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

            switch (characterHandler.xxpGeneral.baseDOC.race)
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

            if (unencryptCheckBox.Checked == true)
            {
                saveFileDialog.Filter = "V9 Salon files (*." + letterOne + letterTwo + "pu)|*." + letterOne + letterTwo + "pu|" +
                "V6 (Ep4 Char Creator) Salon files (*." + letterOne + letterTwo + "pu)|*." + letterOne + letterTwo + "pu|" +
                "V5 Salon files (*." + letterOne + letterTwo + "pu)|*." + letterOne + letterTwo + "pu|" +
                "V2 (Ep1 Char Creator) Salon files (*." + letterOne + letterTwo + "pu)|*." + letterOne + letterTwo + "pu";
            }
            else
            {
                saveFileDialog.Filter = "V9 Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p|" +
                "V6 (Ep4 Char Creator) Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p|" +
                "V5 Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p|" +
                "V2 (Ep1 Char Creator) Salon files (*." + letterOne + letterTwo + "p)|*." + letterOne + letterTwo + "p";

            }
            saveFileDialog.Filter += "|Character Markup Language files (*.cml)|*.cml|" +
                "Data Dump (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                characterHandler.EncryptAndSaveFile(saveFileDialog.FileName, saveFileDialog.FilterIndex - 1, heightNACheckBox.Checked, unencryptCheckBox.Checked, out string windowVersion);
                Text = originalName + " - " + Path.GetFileName(saveFileDialog.FileName);
                versionLabel.Text = "Version: " + windowVersion;
            }
            savedInitialDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
            saveFileDialog.FileName = "";
            openedFileName = "";
        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        //Hide 'File' Contents
        private void hideFileSubmenu()
        {
            if (fileButtonSubmenu.Visible == true)
                fileButtonSubmenu.Visible = false;

        }
        //Show 'File'Contents
        private void showFileSubmenu(Panel subMenu)
        {
            if (subMenu.Visible == false)
            {
                hideFileSubmenu();
                subMenu.Visible = true;
                quitButton.Visible = true;
            }
            else
            {
                subMenu.Visible = false;
                quitButton.Visible = false;
            }
        }

        private void fileButton_Click(object sender, EventArgs e)
        {
            showFileSubmenu(fileButtonSubmenu);
        }

        private void genderButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked == true)
            {
                characterHandler.xxpGeneral.baseDOC.gender = UInt32.Parse(((RadioButton)sender).Tag.ToString());
            }
        }

        private void raceButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked == true)
            {
                characterHandler.xxpGeneral.baseDOC.race = UInt32.Parse(((RadioButton)sender).Tag.ToString());
            }
        }

        private void setPathButton_Click(object sender, EventArgs e)
        {
            characterHandler.SetPso2BinPath();
        }

        private void debugBatchButton_Click(object sender, EventArgs e)
        {

            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select a folder containing pso2 .xxp files",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string[] extensions = new string[] { "*.mhp", "*.fhp", "*.mcp", "*.fcp", "*.mnp", "*.fnp", "*.mdp", "*.fdp" };
                List<string> files = new List<string>();
                foreach (string ext in extensions)
                {
                    files.AddRange(Directory.GetFiles(goodFolderDialog.FileName, ext));
                }

                foreach (var file in files)
                {
                    characterHandler.DecryptFile(file);
                }
            }
        }

        private void debugEncrypt_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Select file",
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

        private void debugDecrypt_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select file",
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
    }
}
