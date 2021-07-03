using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AquaModelLibrary;
using Character_Making_File_Tool;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace NGS_Salon_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CharacterMakingIndexHandler cmxHandler = null;
        CommonOpenFileDialog pso2BinSelect = new CommonOpenFileDialog()
        {
            IsFolderPicker = true,
            Title = "Select pso2_bin",
        };
        string pso2BinCachePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Names\\pso2Bin.txt");
        string pso2_binDir = null;
        public MainWindow()
        {
            InitializeComponent();
            if(File.Exists(pso2BinCachePath))
            {
                LoadGameData();
            }
            else
            {
                MessageBox.Show("pso2_bin path is missing!\n" +
                    "Without it, character part names and icons won't appear!\n" +
                    "It can be set through File->Set pso2_bin directory");
            }
        }

        private void LoadGameData()
        {
            pso2_binDir = File.ReadAllText(pso2BinCachePath);
            cmxHandler = new CharacterMakingIndexHandler(pso2_binDir);
            costumeCB.Items.Clear();
            costumeCB.ItemsSource = cmxHandler.costumeOuterDict.Keys;
            basewearCB.Items.Clear();
            basewearCB.ItemsSource = cmxHandler.basewearDict.Keys;
            innerwearCB.Items.Clear();
            innerwearCB.ItemsSource = cmxHandler.innerwearDict.Keys;
        }

        private void SetPSO2Bin(object sender, RoutedEventArgs e)
        {
            if(pso2BinSelect.ShowDialog() == CommonFileDialogResult.Ok)
            {
                File.WriteAllText(pso2BinCachePath, pso2BinSelect.FileName);
                LoadGameData();
            }
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
    }
}
