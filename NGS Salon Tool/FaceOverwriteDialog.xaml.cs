using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NGS_Salon_Tool
{
    /// <summary>
    /// Interaction logic for FaceOverwriteDialog.xaml
    /// </summary>
    public partial class FaceOverwriteDialog : Window
    {
        public FaceOverwriteDialog()
        {
            InitializeComponent();
        }

        public bool AltFaceSource
        {
            get
            {
                return AltFaceSourceRadio.IsChecked.HasValue && AltFaceSourceRadio.IsChecked.Value;
            }
        }

        public bool OverwriteBaseFace
        {
            get
            {
                return BaseFaceDestCheckbox.IsChecked.HasValue && BaseFaceDestCheckbox.IsChecked.Value;
            }
        }

        public bool OverwriteAltFace
        {
            get
            {
                return AltFaceDestCheckbox.IsChecked.HasValue && AltFaceDestCheckbox.IsChecked.Value;
            }
        }

        public void EnableAltFaceSource(bool IsEnabled)
        {
            AltFaceSourceRadio.IsEnabled = IsEnabled;
            AltFaceSourceRadio.IsChecked = IsEnabled && AltFaceSourceRadio.IsChecked.HasValue && AltFaceSourceRadio.IsChecked.Value;
        }

        public void EnableAltFaceDest(bool IsEnabled)
        {
            AltFaceDestCheckbox.IsEnabled = IsEnabled;
            AltFaceDestCheckbox.IsChecked = IsEnabled && AltFaceSourceRadio.IsChecked.HasValue && AltFaceSourceRadio.IsChecked.Value;
        }

        private void OnClick_OkayButton(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
