using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ColorPicker : Window, INotifyPropertyChanged
    {
        private SolidColorBrush trueColor;
        public ColorPicker()
        {
            InitializeComponent();
            DataContext = this;
            trueColor = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0, 0xFF));
            SelColor = trueColor;
            RectColor = Color.FromArgb(0xFF, 0xFF, 0, 0);
            SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
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

        public void UpdateHorizSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Canvas.SetLeft(selector, e.NewValue / horizSlider.Maximum * ColorCanvas.Width);
            if(SelColor != null)
            {
                SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
            }
        }
        public void UpdateVertSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Canvas.SetTop(selector, (vertSlider.Maximum - e.NewValue) / vertSlider.Maximum * ColorCanvas.Height);
            if (SelColor != null)
            {
                SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
            }
        }

        public void UpdateHueSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RectColor = ColorLogic.ColorFromHSV(hueSlider.Value, 1, 1);
            if (SelColor != null)
            {
                SelColor.Color = ColorLogic.ColorFromHSV(hueSlider.Value, horizSlider.Value / horizSlider.Maximum, vertSlider.Value / vertSlider.Maximum);
            }
        }
    }
}
