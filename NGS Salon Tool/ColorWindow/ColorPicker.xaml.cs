using Character_Making_File_Tool;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NGS_Salon_Tool
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public unsafe partial class ColorPicker : Window, INotifyPropertyChanged
    {
        private SolidColorBrush trueColor;
        private byte[] internalColor = new byte[4];
        private byte* internalColorPtr = null;
        private Button internalButton = new Button();
        private bool ignoreValueChanges = true;         //Used to control access so that sliders don't interfere with each other
        public ColorPicker()
        {
            InitializeComponent();
            DataContext = this;
            trueColor = new SolidColorBrush(new Color());
            SelColor = trueColor;
            RectColor = Color.FromArgb(0xFF, 0xFF, 0, 0);
            SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
            SetInternalColor();
            ignoreValueChanges = false;
        }

        public ColorPicker(byte[] selectedColor, bool alphaEnabled = false)
        {
            InitializeComponent();
            DataContext = this;
            trueColor = new SolidColorBrush(new Color());
            SelColor = trueColor;
            RectColor = Color.FromArgb(0xFF, 0xFF, 0, 0);
            SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);

            LinkExternalColor(selectedColor, alphaEnabled);
        }

        public ColorPicker(byte[] selectedColor, Button button, bool alphaEnabled = false)
        {
            InitializeComponent();
            DataContext = this;
            trueColor = new SolidColorBrush(new Color());
            SelColor = trueColor;
            RectColor = Color.FromArgb(0xFF, 0xFF, 0, 0);
            SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);

            LinkExternalColor(selectedColor, button, alphaEnabled);
        }

        public ColorPicker(byte* selectedColor, Button button, bool alphaEnabled = false)
        {
            InitializeComponent();
            DataContext = this;
            trueColor = new SolidColorBrush(new Color());
            SelColor = trueColor;
            RectColor = Color.FromArgb(0xFF, 0xFF, 0, 0);
            SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);

            LinkExternalColor(selectedColor, button, alphaEnabled);
        }

        public void LinkExternalColor(byte* selectedColor, Button button, bool alphaEnabled = false)
        {
            internalButton = button;
            LinkExternalColor(selectedColor, alphaEnabled);
        }

        public void LinkExternalColor(byte[] selectedColor, Button button, bool alphaEnabled = false)
        {
            internalButton = button;
            LinkExternalColor(selectedColor, alphaEnabled);
        }

        public void LinkExternalColor(byte* selectedColor, bool alphaEnabled)
        {
            internalColorPtr = null;
            internalColorPtr = selectedColor;
            internalColor = ColorConversion.BytesFromFixed(internalColorPtr);
            LinkColorSetup(alphaEnabled);
        }

        public void LinkExternalColor(byte[] selectedColor, bool alphaEnabled)
        {
            internalColorPtr = null;
            internalColor = selectedColor;
            LinkColorSetup(alphaEnabled);
        }

        private void LinkColorSetup(bool alphaEnabled)
        {
            alphaUD.IsEnabled = alphaEnabled;
            Visibility vis;
            if (alphaEnabled == true)
            {
                vis = Visibility.Visible;
            }
            else
            {
                vis = Visibility.Hidden;
            }
            alphaUD.Visibility = vis;
            alphaLabel.Visibility = vis;
            SetColorUDs();
            ForceUpdateSliders();
            SetSliders();
            SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
        }

        private void SetColorUDs()
        {
            ignoreValueChanges = true;
            redUD.Value = internalColor[2];
            greenUD.Value = internalColor[1];
            blueUD.Value = internalColor[0];
            alphaUD.Value = internalColor[3];
            ignoreValueChanges = false;
        }

        private void SetSliders()
        {
            ignoreValueChanges = true;
            ColorLogic.ColorToHSV(Color.FromArgb(internalColor[3], internalColor[2], internalColor[1], internalColor[0]), out double hue, out double saturation, out double value);
            hueSlider.Value = hue;
            horizSlider.Value = saturation * horizSlider.Maximum;
            vertSlider.Value = value * vertSlider.Maximum;
            ignoreValueChanges = false;
        }
        private void ForceUpdateSliders()
        {
            ignoreValueChanges = true;
            hueSlider.Value = 361;
            horizSlider.Value = 2;
            vertSlider.Value = 2;
            ignoreValueChanges = false;
        }

        private void SetInternalColor()
        {
            internalColor[0] = SelColor.Color.B;
            internalColor[1] = SelColor.Color.G;
            internalColor[2] = SelColor.Color.R;
            internalColor[3] = SelColor.Color.A;

            if (internalColorPtr != null)
            {
                internalColorPtr[0] = internalColor[0];
                internalColorPtr[1] = internalColor[1];
                internalColorPtr[2] = internalColor[2];
                internalColorPtr[3] = internalColor[3];
            }
            if (internalButton != null && SelColor != null)
            {
                if (!(internalButton.Background is SolidColorBrush))
                {
                    internalButton.Background = new SolidColorBrush(new Color());
                }
                ((SolidColorBrush)internalButton.Background).Color = SelColor.Color;
            }
        }

        public SolidColorBrush SelColor
        {
            get
            {
                return trueColor;
            }
            set
            {
                trueColor = value;
                OnPropertyChanged("SelColor");
            }
        }

        private Color m_RectColor;
        public Color RectColor
        {
            get
            {
                return m_RectColor;
            }
            set
            {
                m_RectColor = value;
                OnPropertyChanged("RectColor");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void RedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelColor != null && ignoreValueChanges == false)
            {
                var color = SelColor.Color;
                color.R = (byte)(int)e.NewValue;
                SelColor.Color = color;
                SetInternalColor();
                SetSliders();
            }
        }
        public void GreenChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelColor != null && ignoreValueChanges == false)
            {
                var color = SelColor.Color;

                color.G = (byte)(int)e.NewValue;
                SelColor.Color = color;
                SetInternalColor();
                SetSliders();
            }
        }
        public void BlueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelColor != null && ignoreValueChanges == false)
            {
                var color = SelColor.Color;
                color.B = (byte)(int)e.NewValue;
                SelColor.Color = color;
                SetInternalColor();
                SetSliders();
            }
        }
        public void AlphaChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelColor != null && ignoreValueChanges == false)
            {
                var color = SelColor.Color;
                color.A = (byte)(int)e.NewValue;
                SelColor.Color = color;
                SetInternalColor();
                SetSliders();
            }
        }
        public void UpdateHorizSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Canvas.SetLeft(selector, e.NewValue / horizSlider.Maximum * ColorCanvas.Width);
            if (SelColor != null && ignoreValueChanges == false)
            {
                SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
                SetInternalColor();
                SetColorUDs();
            }
        }
        public void UpdateVertSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Canvas.SetTop(selector, (vertSlider.Maximum - e.NewValue) / vertSlider.Maximum * ColorCanvas.Height);
            if (SelColor != null && ignoreValueChanges == false)
            {
                SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
                SetInternalColor();
                SetColorUDs();
            }
        }

        public void UpdateHueSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RectColor = ColorLogic.ColorFromHSV(hueSlider.Value, 1, 1);
            if (SelColor != null && ignoreValueChanges == false)
            {
                SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
                SetInternalColor();
                SetColorUDs();
            }
        }
        public void MoveSelector(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pt = e.GetPosition((UIElement)sender);
                ignoreValueChanges = true;
                vertSlider.Value = vertSlider.Maximum - (pt.Y / mainBox.Height * vertSlider.Maximum);
                ignoreValueChanges = false;
                horizSlider.Value = pt.X / mainBox.Width * horizSlider.Maximum;
            }
        }
    }
}
