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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Практика2Курс
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PhoneCollection<Phone> phones;
        public MainWindow()
        {
            InitializeComponent();
            phones = new PhoneCollection<Phone>();
            MainPaintField.PreviewMouseUp += WindowMouseUp;
        }

        private void WindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Canvas)
            {
                foreach (var child in ((Canvas)sender).Children)
                {
                    if (child is Canvas)
                    {
                        foreach (var phone in phones)
                        {
                            phone.IsSelected = false;
                        }
                        ExamplePhone.Children.Clear();
                        return;
                    }
                }
            }
        }

        private void AddNormalPushButtonPhoneClick(object sender, RoutedEventArgs e)
        {
            PhoneInfoWindow phoneInfoWindow = new PhoneInfoWindow();

            if (phoneInfoWindow.ShowDialog() == true)
            {
                NormalPushButtonPhone phone = CreatePhoneInstanse<NormalPushButtonPhone>();
                phone.Name = phoneInfoWindow.PhoneNameBox.Text;
                phone.ManufactorName = phoneInfoWindow.PhoneManufactorNameBox.Text;
                phone.PenColor = phoneInfoWindow.PenColor;

                phone.Draw();
                phone.CopyOriginalPhoneTo(ExamplePhone);
                phone.LifeTimer.Tick += (obj, arg) =>
                {
                    if (phone.CurrentLifeTime <= 0)
                    {
                        ExamplePhone.Children.Clear();
                    }
                };

                this.DataContext = phone;

                phones.Add(phone);
            }
        }

        private void AddSmartPhoneClick(object sender, RoutedEventArgs e)
        {
            PhoneInfoWindow phoneInfoWindow = new PhoneInfoWindow();

            if (phoneInfoWindow.ShowDialog() == true)
            {
                Phone phone = CreatePhoneInstanse<SmartPhone>();
                phone.Name = phoneInfoWindow.PhoneNameBox.Text;
                phone.ManufactorName = phoneInfoWindow.PhoneManufactorNameBox.Text;
                phone.PenColor = phoneInfoWindow.PenColor;

                phone.Draw();
                phone.CopyOriginalPhoneTo(ExamplePhone);

                this.DataContext = phone;

                phones.Add(phone);
            }
        }

        private void AddRetractablePushButtonPhoneClick(object sender, RoutedEventArgs e)
        {
            PhoneInfoWindow phoneInfoWindow = new PhoneInfoWindow();
            
            if (phoneInfoWindow.ShowDialog() == true)
            {
                Phone phone = CreatePhoneInstanse<RetractablePushButtonPhone>();
                phone.Name = phoneInfoWindow.PhoneNameBox.Text;
                phone.ManufactorName = phoneInfoWindow.PhoneManufactorNameBox.Text;
                phone.PenColor = phoneInfoWindow.PenColor;

                phone.Draw();
                phone.CopyOriginalPhoneTo(ExamplePhone);

                this.DataContext = phone;

                phones.Add(phone);
            }
        }

        //Создаёт новый экземпляр класса с конструктором без параметров 
        private T CreatePhoneInstanse<T>() where T : Phone, new()
        {
            Phone phone = new T();
            phone.PaintField = MainPaintField;
            phone.PhoneField.PreviewMouseUp += (obj, arg) =>
            {
                if (obj is Canvas)
                {
                    foreach (var innerPhone in phones)
                    {
                        if (innerPhone.IsSelected)
                        {
                            innerPhone.CopyOriginalPhoneTo(ExamplePhone);
                            DataContext = innerPhone;
                        }
                    }
                }
            };

            return (T)phone;
        }

        //Магическое превращение канваса в картинку
        private void SaveAsImageClick(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)MainPaintField.RenderSize.Width,
                                     (int)MainPaintField.RenderSize.Height, 90, 90, System.Windows.Media.PixelFormats.Default);
            rtb.Render(MainPaintField);

            var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, (int)(MainPaintField.ActualWidth), (int)(MainPaintField.ActualHeight )));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            using (var fs = new System.IO.FileStream("logo.bmp", System.IO.FileMode.OpenOrCreate))
            {
                pngEncoder.Save(fs);
            }
        }

    }
}
