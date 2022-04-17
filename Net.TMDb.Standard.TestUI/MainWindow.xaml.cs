using Gabriel.Cat.S.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.TMDb;
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

namespace Net.TMDb.Standard.TestUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string PATHIMGS = "Images";
        public MainWindow()
        {
            InitializeComponent();
            if (!Equals(Properties.Settings.Default.ApiKey, default))
            {
                txtApiKey.Text = Properties.Settings.Default.ApiKey;
            }
            if (!System.IO.Directory.Exists(PATHIMGS))
            {
                System.IO.Directory.CreateDirectory(PATHIMGS);
            }
        }
        ServiceClient? Client { get; set; }

        private void btnTryLogin_Click(object sender, RoutedEventArgs e)
        {
            Task<Movies> tGetTop;
            Task<IImageStorage> tGetImage;
            System.Windows.Controls.Image img;
            try
            {
                Client = new ServiceClient(txtApiKey.Text);
                tGetTop = Client.Movies.GetTopRatedAsync("es-ES", 1);
                tGetTop.Wait();
                ugTopMovies.Children.Clear();
                tGetImage = Client.GetImageStorageAsync();
                tGetImage.Wait();

                foreach (Movie movie in tGetTop.Result.Results)
                {
                    img = new System.Windows.Controls.Image();
                    img.Height = 450;
                    img.SetImage(tGetImage.Result.GetImageSync(movie.Poster,null,PATHIMGS));
                    ugTopMovies.Children.Add(img);
                }
                txtApiKey.Foreground = Brushes.LightGreen;
                txtApiKey.IsReadOnly = true;
                Properties.Settings.Default.ApiKey = txtApiKey.Text;
                Properties.Settings.Default.Save();
                btnTryLogin.IsEnabled = false;
            }catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                txtApiKey.Foreground = Brushes.LightGray;
            }
        }
    }
}
