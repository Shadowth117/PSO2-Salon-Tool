using System.Windows;

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
