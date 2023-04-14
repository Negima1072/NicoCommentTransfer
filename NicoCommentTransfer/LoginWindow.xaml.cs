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
    /// Login.xaml の相互作用ロジック
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool isLogin = false;
        public string user_session = "";
        public string user_session_secure = "";
        public long expiresunixtime = 0;
        public string userid = "0";
        public bool isPremium = false;
        public LoginWindow()
        {
            InitializeComponent();
            UserNameTB.Text = "ログインをしてください。";
        }

        private void OKBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (!isLogin) this.Owner.Close();
        }

        private void EmailLoginBtnClick(object sender, RoutedEventArgs e)
        {
            isLogin = ((MainWindow)this.Owner).client.Login(EmailTBox.Text, PassTBox.Password);
            if (isLogin)
            {
                UserNameTB.Text = ((MainWindow)this.Owner).client.userName;
                UserIDTB.Text = ((MainWindow)this.Owner).client.userID;
                UserPremium.Text = "P:" + ((MainWindow)this.Owner).client.isPremium.ToString();
                BitmapImage imageSource = new BitmapImage(new Uri(((MainWindow)this.Owner).client.imgUrl));
                UserImage.Source = imageSource;
                expiresunixtime = ((MainWindow)this.Owner).client.getLoginCookieExpires();
                SessionExpiresTB.Text = BrowserCookieGetter.ConvertUnixToDateTime(expiresunixtime).ToString("yyyy-MM-dd hh:mm");
                string[] usc = ((MainWindow)this.Owner).client.getLoginCookies();
                user_session = usc[0];
                user_session_secure = usc[1];
                userid = ((MainWindow)this.Owner).client.userID; ;
                isPremium = ((MainWindow)this.Owner).client.isPremium;
                OKBtn.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("ログインに失敗しました。L65", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChromeLoginBtnClick(object sender, RoutedEventArgs e)
        {
            string[] d = BrowserCookieGetter.GetChromeCookie();
            user_session = d[0];
            user_session_secure = d[1];
            expiresunixtime = long.Parse(d[2]);
            isLogin = ((MainWindow)this.Owner).client.LoginCookie(user_session, user_session_secure);
            if (isLogin)
            {
                UserNameTB.Text = ((MainWindow)this.Owner).client.userName;
                UserIDTB.Text = ((MainWindow)this.Owner).client.userID;
                UserPremium.Text = "P:"+((MainWindow)this.Owner).client.isPremium.ToString();
                BitmapImage imageSource = new BitmapImage(new Uri(((MainWindow)this.Owner).client.imgUrl));
                UserImage.Source = imageSource;
                SessionExpiresTB.Text = BrowserCookieGetter.ConvertUnixToDateTime(expiresunixtime).ToString("yyyy-MM-dd hh:mm");
                ((MainWindow)this.Owner).client.cookieExpires = long.Parse(d[2]);
                userid = ((MainWindow)this.Owner).client.userID; ;
                isPremium = ((MainWindow)this.Owner).client.isPremium;
                OKBtn.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("ログインに失敗しました。L91", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FirefoxLoginBtnClick(object sender, RoutedEventArgs e)
        {
            string[] d = BrowserCookieGetter.GetFirefoxCookie();
            user_session = d[0];
            user_session_secure = d[1];
            expiresunixtime = long.Parse(d[2]);
            isLogin = ((MainWindow)this.Owner).client.LoginCookie(user_session, user_session_secure);
            if (isLogin)
            {
                UserNameTB.Text = ((MainWindow)this.Owner).client.userName;
                UserIDTB.Text = ((MainWindow)this.Owner).client.userID;
                UserPremium.Text = "P:" + ((MainWindow)this.Owner).client.isPremium.ToString();
                BitmapImage imageSource = new BitmapImage(new Uri(((MainWindow)this.Owner).client.imgUrl));
                UserImage.Source = imageSource;
                SessionExpiresTB.Text = BrowserCookieGetter.ConvertUnixToDateTime(expiresunixtime).ToString("yyyy-MM-dd hh:mm");
                ((MainWindow)this.Owner).client.cookieExpires = long.Parse(d[2]);
                userid = ((MainWindow)this.Owner).client.userID;
                isPremium = ((MainWindow)this.Owner).client.isPremium;
                OKBtn.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("ログインに失敗しました。L117", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLogin)
            {
                System.Diagnostics.Process.Start("https://nicovideo.jp/user/"+userid);
            }
        }
    }
}
