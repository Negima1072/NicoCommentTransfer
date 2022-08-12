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
        public NicoOAuth oauth;
        public LoginOAuth()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await loginView.EnsureCoreWebView2Async(null);
            loginView.CoreWebView2.Settings.UserAgent = "NicoCommentTransfer@Negima1072";
            loginView.CoreWebView2.NavigationStarting += new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs>(loginViewCore_SourceUpdated);
            loginView.CoreWebView2.Navigate("https://nct.nvcomment.net/api/v1/login");
        }

        private async void loginViewCore_SourceUpdated(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if(e.Uri.StartsWith("https://nct.nvcomment.net/api/v1/redirect"))
            {
                List<Microsoft.Web.WebView2.Core.CoreWebView2Cookie> cookies = await loginView.CoreWebView2.CookieManager.GetCookiesAsync("https://nicovideo.jp");
                cookies.ForEach((c) =>
                {
                    Console.WriteLine(c.Value);
                    if (c.Name == "user_session")
                    {
                        user_session = c.Value;
                        expiresunixtime = BrowserCookieGetter.ToUnixTime(c.Expires);
                    }
                    else if (c.Name == "user_session_secure") user_session_secure = c.Value;
                });
                auth_token = await loginView.ExecuteScriptAsync("document.documentElement.outerText");
                oauth = new NicoOAuth(auth_token);
                UserOpenIDInfo info = oauth.getOwnInfo();
                userid = info.Sub;
                isPremium = (oauth.getOwnPremium().Data.Type == "premium");
                this.Close();
            }
        }
    }
}
