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

namespace NicoCommentTransfer.UserControls
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class TitleBar : UserControl
    {
        string titleText = "";
        string mtitleText = "NicoCommentTransfer";
        public TitleBar()
        {
            InitializeComponent();
            TitleBarText.Text = titleText + mtitleText;
        }

        public string ChangeTitleText(string text)
        {
            titleText = text;
            if (titleText == "")
            {
                TitleBarText.Text = titleText + mtitleText;
            }
            else
            {
                TitleBarText.Text = titleText + " - " +mtitleText;
            }
            return titleText + mtitleText;
        }

        private void TitleBarExitBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void TitleBarMaxBtnClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).MaxHeight = SystemParameters.PrimaryScreenHeight;
            Window.GetWindow(this).MaxWidth = SystemParameters.PrimaryScreenWidth;
            Window.GetWindow(this).WindowState = WindowState.Maximized;
            TitleBarUnMaxBtn.Visibility = Visibility.Visible;
            TitleBarMaxBtn.Visibility = Visibility.Hidden;
        }

        private void TitleBarMinBtnClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void UCMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).DragMove();
        }

        private void UCLoaded(object sender, RoutedEventArgs e)
        {
            //TitleBarIcon.Source = Window.GetWindow(this).Icon;
        }

        private void TitleBarUnMaxBtnClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).MaxHeight = SystemParameters.PrimaryScreenHeight;
            Window.GetWindow(this).MaxWidth = SystemParameters.PrimaryScreenWidth;
            Window.GetWindow(this).WindowState = WindowState.Normal;
            TitleBarMaxBtn.Visibility = Visibility.Visible;
            TitleBarUnMaxBtn.Visibility = Visibility.Hidden;
        }
    }
}
