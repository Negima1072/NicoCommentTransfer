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
    /// LoginOAuth.xaml の相互作用ロジック
    /// </summary>
    public partial class LoginOAuth : Window
    {
        public string user_session = "";
        public string user_session_secure = "";
        public long expiresunixtime = 0;
        public string auth_token = "";
        public string userid = "0";
        public bool isPremium = false;
        public bool isLogin = false;
        //public NicoOAuth oauth;
        public LoginOAuth(bool newed = false)
        {
            InitializeComponent();
            InitializeAsync(newed);
        }

        private async void InitializeAsync(bool newed = false)
        {
            await loginView.EnsureCoreWebView2Async(null);
            if (newed) loginView.CoreWebView2.CookieManager.DeleteAllCookies();
            loginView.CoreWebView2.Settings.UserAgent = "NicoCommentTransfer@Negima1072";
            loginView.CoreWebView2.NavigationCompleted += new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(loginViewCore_SourceUpdated);
            //loginView.CoreWebView2.Navigate("https://nct.nvcomment.net/api/v1/login");
            loginView.CoreWebView2.Navigate("https://account.nicovideo.jp/login");
        }

        private async void loginViewCore_SourceUpdated(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            //if(loginView.CoreWebView2.Source.StartsWith("https://nct.nvcomment.net/api/v1/redirect"))
            if (loginView.CoreWebView2.Source.StartsWith("https://www.nicovideo.jp"))
            {
                List<Microsoft.Web.WebView2.Core.CoreWebView2Cookie> cookies = await loginView.CoreWebView2.CookieManager.GetCookiesAsync("https://nicovideo.jp");
                cookies.ForEach((c) =>
                {
                    if (c.Name == "user_session")
                    {
                        user_session = c.Value;
                        expiresunixtime = BrowserCookieGetter.ToUnixTime(c.Expires);
                    }
                    else if (c.Name == "user_session_secure") user_session_secure = c.Value;
                });
                isLogin = ((MainWindow)this.Owner).client.LoginCookie(user_session, user_session_secure);
                /*auth_token = await loginView.ExecuteScriptAsync("document.documentElement.outerText");
                Console.WriteLine("1");
                oauth = new NicoOAuth(auth_token);
                Console.WriteLine("1");
                UserOpenIDInfo info = oauth.getOwnInfo();
                Console.WriteLine("1");*/
                //userid = info.Sub;
                Console.WriteLine("1");
                //isPremium = (oauth.getOwnPremium().Data.Type == "premium");
                //isLogin = true;
                if (isLogin)
                {
                    userid = ((MainWindow)this.Owner).client.userID;
                    isPremium = ((MainWindow)this.Owner).client.isPremium;
                }
                this.Close();
            }
        }
    }
}
