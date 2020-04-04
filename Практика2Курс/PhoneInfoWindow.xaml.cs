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

namespace Практика2Курс
{
    /// <summary>
    /// Логика взаимодействия для PhoneInfoWindow.xaml
    /// </summary>
    public partial class PhoneInfoWindow : Window
    {

        public Color PenColor { get; set; }
        public PhoneInfoWindow()
        {
            InitializeComponent();
        }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void RefuseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ChangeColor(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBoxItem)ColorComboBox.SelectedItem).Content)
            {
                case "Red":
                    PenColor = Colors.Red;
                    break;
                case "Green":
                    PenColor = Colors.Green;
                    break;
                case "Blue":
                    PenColor = Colors.Blue;
                    break;
                case "Black":
                    PenColor = Colors.Black;
                    break;
                default:
                    PenColor = Colors.Black;
                    break;
            }

            if (ColorExample != null)
            {
                ColorExample.Background = new SolidColorBrush(PenColor);
            }
        }
    }
}
