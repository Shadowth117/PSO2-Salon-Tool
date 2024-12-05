using Character_Making_File_Tool;
using System;
using System.ComponentModel;
using System.Windows;

namespace NGS_Salon_Tool
{
    /// <summary>
    /// Interaction logic for AccessoryWindow.xaml
    /// </summary>
    public unsafe partial class AccessoryWindow : Window
    {
        CharacterHandlerReboot.xxpGeneralReboot _xxp = new CharacterHandlerReboot.xxpGeneralReboot();
        int _accessoryNum = 0;
        Action<int> _setWindowState;
        bool enabled = false;
        public AccessoryWindow(int accessoryNum, CharacterHandlerReboot.xxpGeneralReboot xxp, Action<int> setWindowState)
        {
            _accessoryNum = accessoryNum;
            _xxp = xxp;
            _setWindowState = setWindowState;
            InitializeComponent();
            InitializeValues();
            Title = $"Accessory {accessoryNum + 1} Settings";
        }

        public void InitializeValues()
        {
            posXUD.Value = _xxp.accessorySlidersRebootExtended.posSliders[_accessoryNum * 3];
            posYUD.Value = _xxp.accessorySlidersRebootExtended.posSliders[(_accessoryNum * 3) + 1];
            posZUD.Value = _xxp.accessorySlidersRebootExtended.posSliders[(_accessoryNum * 3) + 2];

            rotXUD.Value = _xxp.accessorySlidersRebootExtended.rotSliders[_accessoryNum * 3];
            rotYUD.Value = _xxp.accessorySlidersRebootExtended.rotSliders[(_accessoryNum * 3) + 1];
            rotZUD.Value = _xxp.accessorySlidersRebootExtended.rotSliders[(_accessoryNum * 3) + 2];

            sclXUD.Value = _xxp.accessorySlidersRebootExtended.scaleSliders[_accessoryNum * 3];
            sclYUD.Value = _xxp.accessorySlidersRebootExtended.scaleSliders[(_accessoryNum * 3) + 1];
            sclZUD.Value = _xxp.accessorySlidersRebootExtended.scaleSliders[(_accessoryNum * 3) + 2];

            attachPointUD.Value = _xxp.accessoryMiscDataExtended.accessoryAttach[_accessoryNum];
            color1UD.Value = _xxp.accessoryMiscDataExtended.accessoryColorChoices[_accessoryNum * 2];
            color2UD.Value = _xxp.accessoryMiscDataExtended.accessoryColorChoices[(_accessoryNum * 2) + 1];
            enabled = true;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _setWindowState(_accessoryNum);
        }

        private void PosXChanged(object sender, RoutedEventArgs e)
        {
            if (posXUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.posSliders[_accessoryNum * 3] = (sbyte)posXUD.Value;
            }
        }
        private void PosYChanged(object sender, RoutedEventArgs e)
        {
            if (posYUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.posSliders[(_accessoryNum * 3) + 1] = (sbyte)posYUD.Value;
            }
        }
        private void PosZChanged(object sender, RoutedEventArgs e)
        {
            if (posZUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.posSliders[(_accessoryNum * 3) + 2] = (sbyte)posZUD.Value;
            }
        }
        private void RotXChanged(object sender, RoutedEventArgs e)
        {
            if (rotXUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.rotSliders[_accessoryNum * 3] = (sbyte)rotXUD.Value;
            }
        }
        private void RotYChanged(object sender, RoutedEventArgs e)
        {
            if (rotYUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.rotSliders[(_accessoryNum * 3) + 1] = (sbyte)rotYUD.Value;
            }
        }
        private void RotZChanged(object sender, RoutedEventArgs e)
        {
            if (rotZUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.rotSliders[(_accessoryNum * 3) + 2] = (sbyte)rotZUD.Value;
            }
        }
        private void SclXChanged(object sender, RoutedEventArgs e)
        {
            if (sclXUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.scaleSliders[_accessoryNum * 3] = (sbyte)sclXUD.Value;
            }
        }
        private void SclYChanged(object sender, RoutedEventArgs e)
        {
            if (sclYUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.scaleSliders[(_accessoryNum * 3) + 1] = (sbyte)sclYUD.Value;
            }
        }
        private void SclZChanged(object sender, RoutedEventArgs e)
        {
            if (sclZUD.Value != null && enabled)
            {
                _xxp.accessorySlidersRebootExtended.scaleSliders[(_accessoryNum * 3) + 2] = (sbyte)sclZUD.Value;
            }
        }
        private void AttachChanged(object sender, RoutedEventArgs e)
        {
            if (attachPointUD.Value != null && enabled)
            {
                _xxp.accessoryMiscDataExtended.accessoryAttach[_accessoryNum] = (byte)attachPointUD.Value;
            }
        }
        private void Color1Changed(object sender, RoutedEventArgs e)
        {
            if (color1UD.Value != null && enabled)
            {
                _xxp.accessoryMiscDataExtended.accessoryColorChoices[_accessoryNum * 2] = (byte)color1UD.Value;
            }
        }
        private void Color2Changed(object sender, RoutedEventArgs e)
        {
            if (color2UD.Value != null && enabled)
            {
                _xxp.accessoryMiscDataExtended.accessoryColorChoices[(_accessoryNum * 2) + 1] = (byte)color2UD.Value;
            }
        }
    }
}
