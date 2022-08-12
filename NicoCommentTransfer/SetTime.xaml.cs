using NicoCommentTransfer.API;
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

namespace NicoCommentTransfer
{
    /// <summary>
    /// ZurasuTime.xaml の相互作用ロジック
    /// </summary>
    public partial class SetTime : Window
    {
        public SetTime()
        {
            InitializeComponent();
        }

        private void OKBtnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("j");
            string time = TimeTB.Text;
            ((MainWindow)Application.Current.MainWindow).acommList.setCommentTime(time: time);
            MessageBox.Show("完了しました。");
            this.Close();
        }

        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
