using Microsoft.Win32;
using NicoCommentTransfer.API;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace NicoCommentTransfer
{
    /// <summary>
    /// NamaHozonOption.xaml の相互作用ロジック
    /// </summary>
    public partial class NamaHozonOption : Window
    {
        Client client;
        public NamaHozonOption(Client client, string dougaid = "", bool commu = false)
        {
            InitializeComponent();
            this.client = client;
            if (dougaid != "" && dougaid != null)
            {
                dougaID.Text = dougaid;
            }
            communityC.IsChecked = commu;
        }

        private void jikkou(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = "https://nicovideo.jp/watch/" + dougaID.Text;
                NicoVideo movie = new NicoVideo();
                movie.getWatchAPIData(client, url);
                string d = movie.getVideoCommentJson(client, (bool)defaultC.IsChecked, (bool)toukouC.IsChecked, (bool)communityC.IsChecked, (bool)kantanC.IsChecked);
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "ニコ動ThreadJSON|*.json|All Files (*.*)|*.*";
                saveFileDialog.DefaultExt = ".json";
                bool? result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    if (saveFileDialog.FilterIndex == 1 /*ThreadChat*/)
                    {
                        string json = d;
                        StreamWriter sr = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8);
                        sr.Write(json);
                        sr.Close();
                    }
                }
                this.Close();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}
