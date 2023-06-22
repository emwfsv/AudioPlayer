using AudioPlayer.Classes;
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

namespace AudioPlayer.Forms
{
    /// <summary>
    /// Interaction logic for About_Form.xaml
    /// </summary>
    public partial class Settings_Form : Window
    {
        public Settings_Form(ApplicationColorScheme applicationColorScheme)
        {
            InitializeComponent();

            ApplicationColorScheme = applicationColorScheme;
        }

        public event EventHandler UpdatedColorscheme;
        public ApplicationColorScheme ApplicationColorScheme;

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            UpdatedColorscheme?.Invoke(ApplicationColorScheme, EventArgs.Empty);
            this.Close();
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //Constructors
                var radioButton = (RadioButton)sender;
                ApplicationColorScheme = new ApplicationColorScheme();

                //Set the values
                switch (radioButton.Content)
                {
                    case "Pink":
                        ApplicationColorScheme.ApplicationBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCB9B2"));
                        ApplicationColorScheme.ApplicationBanner = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8C2F39"));
                        ApplicationColorScheme.ApplicationLogo = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#461220"));
                        ApplicationColorScheme.ApplicationButtonText = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCB9B2"));
                        ApplicationColorScheme.ApplicationDatagridText = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));
                        break;
                    case "Dark":
                        ApplicationColorScheme.ApplicationBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000411"));
                        ApplicationColorScheme.ApplicationBanner = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#160C28"));
                        ApplicationColorScheme.ApplicationLogo = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AEB7B3"));
                        ApplicationColorScheme.ApplicationButtonText = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFCB68"));
                        ApplicationColorScheme.ApplicationDatagridText = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFCB68"));
                        break;
                }

                //Set Scheme colors that could be returned.



                //Set form colors.
                btnExit.Foreground = ApplicationColorScheme.ApplicationButtonText;
                btnExit.Background = ApplicationColorScheme.ApplicationBanner;
                btnSet.Foreground = ApplicationColorScheme.ApplicationButtonText;
                btnSet.Background = ApplicationColorScheme.ApplicationBanner;
                grdBackground.Background = ApplicationColorScheme.ApplicationBackground;
                grdBanner.Background = ApplicationColorScheme.ApplicationBanner;
                iconLogo.Foreground = ApplicationColorScheme.ApplicationLogo;
                iconLogo.Background= ApplicationColorScheme.ApplicationBanner;

                b1.Foreground = ApplicationColorScheme.ApplicationButtonText;
                b2.Foreground = ApplicationColorScheme.ApplicationButtonText;
                b3.Foreground = ApplicationColorScheme.ApplicationButtonText;
                lblDescription.Foreground = ApplicationColorScheme.ApplicationDatagridText;
            }
            catch { }
        }

        //============================================
        //              Private Methods
        //============================================
    }
}
