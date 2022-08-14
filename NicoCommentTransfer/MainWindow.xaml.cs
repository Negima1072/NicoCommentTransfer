using NicoCommentTransfer.API;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Microsoft.Win32;
using System.Globalization;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace NicoCommentTransfer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public CommentDataList bcommList;
        public CommentDataList acommList;
        public NicoVideo bVideoData = new NicoVideo();
        public NicoVideo aVideoData = new NicoVideo();
        public Client client;
        public NicoOAuth oauth;
        Config config;
        public MainWindow()
        {
            //Config
            if (!File.Exists("config.json"))
            {
                config = new Config();
                config.Save();
            }
            else
            {
                using (StreamReader sr = new StreamReader("config.json", Encoding.UTF8))
                {
                    config = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                    if (!config.CheckVersion()) config = new Config(config);
                }
            }
            InitializeComponent();
            bcommList = new CommentDataList();
            acommList = new CommentDataList();
            bcommView.ItemsSource = bcommList.Data;
            acommView.ItemsSource = acommList.Data;
            bcommList.ItemPropertyChanged += new BoolEventHandler(changeCheckedListB);
            ContentRendered += (s, e) => { WindowContentRendered(s, e); };
        }
        private void WindowContentRendered(object sender, EventArgs e)
        {
            if (!config.isLogin)
            {
                client = new Client();
                LoginOAuth login = new LoginOAuth();
                login.Owner = this;
                login.ShowDialog();
                if (login.isLogin)
                {
                    config.isLogin = true;
                    config.loginSession = login.user_session;
                    config.loginSecure = login.user_session_secure;
                    config.CookieExpires = login.expiresunixtime;
                    config.userID = login.userid;
                    config.isPremium = login.isPremium;
                    config.authToken = login.auth_token;
                    oauth = login.oauth;
                    client.LoginCookie(login.user_session, login.user_session_secure);
                    MessageBox.Show("ログインしました。", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoginAtoShitaniUtusuShori();
                }
                else
                {
                    MessageBox.Show("ログインできませんでした。80", "Login", MessageBoxButton.OK, MessageBoxImage.Stop);
                    this.Close();
                }
            }
            else
            {
                client = new Client(config.loginSession, config.loginSecure);
                //ログインできなかったときの処理
                if (client.isLogin)
                {
                    MessageBox.Show("ログインしました。", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoginAtoShitaniUtusuShori();
                }
                else
                {
                    MessageBox.Show("ログインできませんでした。再度ログインします。95", "Login", MessageBoxButton.OK, MessageBoxImage.Stop);
                    LoginOAuth login = new LoginOAuth();
                    login.Owner = this;
                    login.ShowDialog();
                    if (login.isLogin)
                    {
                        config.isLogin = true;
                        config.loginSession = login.user_session;
                        config.loginSecure = login.user_session_secure;
                        config.CookieExpires = login.expiresunixtime;
                        config.userID = login.userid;
                        config.isPremium = login.isPremium;
                        config.authToken = login.auth_token;
                        oauth = login.oauth;
                        client.LoginCookie(login.user_session, login.user_session_secure);
                        MessageBox.Show("ログインしました。", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoginAtoShitaniUtusuShori();
                    }
                    else
                    {
                        config = new Config(config);
                        MessageBox.Show("ログインできませんでした。113", "Login", MessageBoxButton.OK, MessageBoxImage.Stop);
                        this.Close();
                    }
                }
            }
        }
        public void LoginAtoShitaniUtusuShori()
        {
            client.cookieExpires = config.CookieExpires;
            BitmapImage bmi = new BitmapImage(new Uri(client.imgUrl));
            Console.WriteLine("3");
            UserImage.Source = bmi;
            UserNameTB.Text = "Name: "+client.userName;
            UserIDTB.Text = "ID: " + client.userID;
            UserPremium.Text = "Premium: " + (client.isPremium ? "True" : "False");
            SessionExpiresTB.Text = "Cookie: " + BrowserCookieGetter.ConvertUnixToDateTime(client.cookieExpires).ToString("yyyy-MM-dd hh:mm");
            SBIDTB.Text = config.userID;
            SBPremiumTB.Text = config.isPremium ? "True" : "False";
            isAddPatissire.IsChecked = config.checkPati;
            isAddCa.IsChecked = config.checkCa;
            isAdd184.IsChecked = config.check184;
            isMoveMigiRemoven.IsChecked = config.checkMigiDelete;
            isMoveMigiUnChecked.IsChecked = config.checkMigiUnCheck;
            IsSendAfterDelete.IsChecked = config.checkPostDelete;
            IsToukomeAddorResetAdd.IsChecked = config.checkTokomeAdd;
            GetMyMemoryHidden.IsChecked = config.checkMymemory;
            client.checkIsLatestVersion();
        }
        public void doukiViewandList()
        {
            acommViewTextBox.Text = acommList.Data.Count.ToString() + "コメント";
            bcommViewTextBox.Text = bcommList.Data.Count.ToString() + "コメント";
        }
        private void HelpVersionClick(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly assembly = Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName asmName = assembly.GetName();
            System.Version version = asmName.Version;
            MessageBox.Show("NicoCommentTransfer\n"+"Version:"+version.ToString(), "Version");
        }

        private void FileEndClick(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void CenterGoButtonClick(object sender, RoutedEventArgs e)
        {
            acommList.addCommentDatas(bcommList.getCommentDatasIsCheckedN(), (bool)isAddPatissire.IsChecked, (bool)isAddCa.IsChecked, (bool)isAdd184.IsChecked);
            if ((bool)isMoveMigiRemoven.IsChecked) bcommList.removeCommentDatasIsChecked();
            else if ((bool)isMoveMigiUnChecked.IsChecked) bcommList.unCheckCommentDatasIsChecked();
            doukiViewandList();
        }

        private void LoadByFileBtnClick(object sender, RoutedEventArgs e)
        {
            msd_OpenFileDialogProcess();
        }
        private void msd_OpenFileDialogProcess()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = ""; // Default file name 既定のファイル名
            dlg.DefaultExt = ".json"; // Default file extension 既定のファイル拡張子
            dlg.Multiselect = false;
            dlg.Filter = "ニコ動投コメJSON|*.json|段スクTXT|*.txt|NCT-JSON|*.json|ニコ動ThreadJSON|*.json|All Files (*.*)|*.*";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                int selectindex = dlg.FilterIndex;
                StreamReader sr = new StreamReader(filename, Encoding.GetEncoding("UTF-8"));
                string filedata = sr.ReadToEnd(); sr.Close();
                int maenobcommdatacount = bcommList.Data.Count;
                if (selectindex == 1 /*ニコ動投コメJSON*/)
                {
                    try
                    {
                        List<TokomeEditorJsonCommentContainer> commentContainers = JsonConvert.DeserializeObject<List<TokomeEditorJsonCommentContainer>>(filedata);
                        for (int i = 1; i <= commentContainers.Count; i++)
                        {
                            bcommList.addCommentData(maenobcommdatacount + i, CommentData.ConvertTimeToVpos(commentContainers[i - 1].Time), System.IO.Path.GetFileName(filename), commentContainers[i - 1].Command, commentContainers[i - 1].Comment, 0, 0, DateTime.Now.ToLocalTime());
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("JSONの読み込みに失敗しました。Error:105", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Console.WriteLine(e.ToString());
                    }
                    doukiViewandList();
                }
                else if (selectindex == 2/*段スクTXT*/)
                {
                    try
                    {
                        List<string> filedatas = filedata.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).ToList();
                        for (int i = 1; i <= filedatas.Count; i++)
                        {
                            int commandMode = 0;
                            string command = "";
                            string comment = "";
                            foreach (char c in filedatas[i - 1])
                            {
                                if (c == ']') commandMode = 2;
                                else if (commandMode == 2) comment += c;
                                if (commandMode == 1) command += c;
                                if (c == '[') commandMode = 1;
                            }
                            comment = comment.Replace("[03]", " ").Replace("<br>", "\n").Replace("[tb]", Convert.ToChar(Convert.ToInt32("0009", 16)).ToString());
                            bcommList.addCommentData(maenobcommdatacount + i, 0, System.IO.Path.GetFileName(filename), command, comment, 0, 0, DateTime.Now.ToLocalTime());
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("TXTの読み込みに失敗しました。Error:129", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Console.WriteLine(e.ToString());
                    }
                    doukiViewandList();
                }
                else if (selectindex == 3/*NCT-JSON*/)
                {
                    try
                    {
                        List<CommentData> jsondt = JsonConvert.DeserializeObject<List<CommentData>>(filedata);
                        foreach (CommentData jobj in jsondt)
                        {
                            jobj.Number = maenobcommdatacount + jobj.Number;
                            bcommList.addCommentData(jobj);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("JSONの読み込みに失敗しました。Error:105", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Console.WriteLine(e.ToString());
                    }
                    doukiViewandList();
                }
                else if (selectindex == 3/*Chats-JSON*/) {
                    try
                    {
                        List<CommentData> jsondt = bVideoData.ConvertChatListToCommentDataList(bVideoData.DeserializeStringToChatsList(filedata));
                        foreach (CommentData jobj in jsondt)
                        {
                            jobj.Number = maenobcommdatacount + jobj.Number;
                            bcommList.addCommentData(jobj);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("JSONの読み込みに失敗しました。Error:105", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Console.WriteLine(e.ToString());
                    }
                    doukiViewandList();
                }
                else
                {
                    MessageBox.Show("対応していないファイルタイプです。Error:120", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                //MessageBox.Show("読み込みに失敗しました。Error:121", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BListClearBtnClick(object sender, RoutedEventArgs e)
        {
            bcommList.clear();
            doukiViewandList();
        }
        private void AListClearBtnClick(object sender, RoutedEventArgs e)
        {
            acommList.clear();
            doukiViewandList();
        }

        private void SaveListBtnClick(object sender, RoutedEventArgs e)
        {
            bcommView.IsEnabled = false;
            acommView.IsEnabled = false;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "ニコ動投コメJSON|*.json|NCT-JSON|*.json|DotCommentArtMakerXML|*.xml|段スク水TXT|*.txt|All Files (*.*)|*.*";
            saveFileDialog.DefaultExt = ".json";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                if (saveFileDialog.FilterIndex == 1 /*tokome*/)
                {
                    string json = acommList.makeTokomeEditorJson().Replace("\\\\n", "\\n");
                    StreamWriter sr = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8);
                    sr.Write(json);
                    sr.Close();
                }
                else if (saveFileDialog.FilterIndex == 2 /*nct*/)
                {
                    string json = JsonConvert.SerializeObject(acommList.Data.ToList(), Formatting.Indented).Replace("\\\\n", "\\n");
                    StreamWriter sr = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8);
                    sr.Write(json);
                    sr.Close();
                }
                else if (saveFileDialog.FilterIndex == 3 /*d-xml*/)
                {
                    System.Xml.Serialization.XmlSerializer serializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(DataCommentSet));
                    StreamWriter sr = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8);
                    serializer.Serialize(sr, acommList.makeDCAMXML());
                    sr.Close();
                }
                else if (saveFileDialog.FilterIndex == 4 /*dsuku-txt*/)
                {
                    string txt = acommList.makeDansukuTxt();
                    StreamWriter sr = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8);
                    sr.Write(txt);
                    sr.Close();
                }
            }
            bcommView.IsEnabled = true;
            acommView.IsEnabled = true;
        }

        private void CheckSelectedCommentListBMnClick(object sender, RoutedEventArgs e)
        {
            bcommList.checkSelectedItems(true, true);
        }

        private void UnCheckSelectedCommentListBMnClick(object sender, RoutedEventArgs e)
        {
            bcommList.checkSelectedItems(true, false);
        }

        private void DeleteSelectedCommentListBClick(object sender, RoutedEventArgs e)
        {
            bcommList.removeCommentDatasIsSelected();
            doukiViewandList();
        }

        private void CheckAllCommentListBMnClick(object sender, RoutedEventArgs e)
        {
            bcommList.checkComments(bcommList.Data.ToList());
        }

        private void UnCheckAllCommentListBMnClick(object sender, RoutedEventArgs e)
        {
            bcommList.checkComments(bcommList.Data.ToList(), false);
        }

        private void DeleteCheckedCommentListBMnClick(object sender, RoutedEventArgs e)
        {
            bcommList.removeCommentDatasIsChecked();
            doukiViewandList();
        }

        private void DeleteExcludeCheckedCommentListBMnClick(object sender, RoutedEventArgs e)
        {
            bcommList.removeCommentDatasIsChecked(false);
            doukiViewandList();
        }

        private void SearchCommentListBMnClick(object sender, RoutedEventArgs e)
        {
            ListSearchWindow lsw = new ListSearchWindow("b");
            lsw.Owner = this;
            lsw.Show();
        }

        private void AllUnSelectedListBMnClick(object sender, RoutedEventArgs e)
        {
            bcommView.UnselectAll();
        }

        private void AllSelectedListBMnClick(object sender, RoutedEventArgs e)
        {
            bcommView.SelectAll();
        }
        private void changeCheckedListB(object sender, BoolEventArgs e)
        {
            bcommViewCTextBox.Text = bcommList.getCheckedNum().ToString() + "コメント";
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            config.checkPati = (bool)isAddPatissire.IsChecked;
            config.checkCa = (bool)isAddCa.IsChecked;
            config.check184 = (bool)isAdd184.IsChecked;
            config.checkMigiDelete = (bool)isMoveMigiRemoven.IsChecked;
            config.checkMigiUnCheck = (bool)isMoveMigiUnChecked.IsChecked;
            config.checkPostDelete = (bool)IsSendAfterDelete.IsChecked;
            config.checkTokomeAdd = (bool)IsToukomeAddorResetAdd.IsChecked;
            config.checkMymemory = (bool)GetMyMemoryHidden.IsChecked;
            config.Save();
        }

        private void ReLoginBtnClick(object sender, RoutedEventArgs e)
        {
            config = new Config(config);
            LoginOAuth login = new LoginOAuth(true);
            login.Owner = this;
            login.ShowDialog();
            if (login.isLogin)
            {
                config.isLogin = true;
                config.loginSession = login.user_session;
                config.loginSecure = login.user_session_secure;
                config.CookieExpires = login.expiresunixtime;
                config.userID = login.userid;
                config.isPremium = login.isPremium;
                config.authToken = login.auth_token;
                client.LoginCookie(login.user_session, login.user_session_secure);
                MessageBox.Show("ログインしました。", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
                LoginAtoShitaniUtusuShori();
            }
            else
            {
                config = new Config(config);
                MessageBox.Show("ログインできませんでした。113", "Login", MessageBoxButton.OK, MessageBoxImage.Stop);
                this.Close();
            }
        }

        private void AddCommBtnClick(object sender, RoutedEventArgs e)
        {
            if (AddCommTimeBox.Text != null && AddCommTimeBox.Text != "" &&
                AddCommTextBox.Text != null && AddCommTextBox.Text != "")
            {
                List<CommentData> cd = new List<CommentData> { new CommentData(acommList.getCount() + 1, CommentData.ConvertTimeToVpos(AddCommTimeBox.Text), "", AddCommCommandBox.Text, AddCommTextBox.Text, 0, 0, DateTime.Now.ToLocalTime()) };
                acommList.addCommentDatas(cd, (bool)isAddPatissire.IsChecked, (bool)isAddCa.IsChecked, (bool)isAdd184.IsChecked);
                AddCommTimeBox.Text = "";
                AddCommCommandBox.Text = "";
                AddCommTextBox.Text = "";
                doukiViewandList();
            }
        }

        private void GetMovieCommentBtnClick(object sender, RoutedEventArgs e)
        {
            bool result = Regex.IsMatch(IDTextBox.Text, "(^[0-9]{5,}$)|(^sm[0-9]{1,}$)|(^nm[0-9]{1,}$)|(^so[0-9]{1,}$)");
            bool urlresult = Regex.IsMatch(IDTextBox.Text, "^s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+$");
            string movieurl = result ? "https://nicovideo.jp/watch/" + IDTextBox.Text : (urlresult ? IDTextBox.Text : "Error");
            if (movieurl == "Error") MessageBox.Show("動画IDの形式がだめです。366\n動画ページのURLもしくは(sm|nm|so)数値or数値からなる動画IDを指定してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                try
                {
                    string label = "";
                    if(SorTComboBox.SelectedIndex == 0) label = "default";
                    else if(SorTComboBox.SelectedIndex == 1) label = "owner";
                    else if(SorTComboBox.SelectedIndex == 2) label = "easy";
                    else if(SorTComboBox.SelectedIndex == 3) label = "community";
                    bVideoData.getWatchAPIData(client, movieurl);
                    SBMessageTB.Text = (bVideoData.Data != null) ? bVideoData.Data.Video.Title : "";
                    List<CommentData> cd = bVideoData.getVideoCommentDatas(client, label, (bool)GetMyMemoryHidden.IsChecked);
                    if (cd != null) {
                        bcommList.addCommentDatas(cd);
                        MessageBox.Show("コメントを追加しました。\nコメント数:" + cd.Count.ToString(), "Add Comments", MessageBoxButton.OK, MessageBoxImage.Information);
                        BitmapImage bmi = new BitmapImage(new Uri(bVideoData.Data.Video.Thumbnail.Url));
                        bMovieImage.Source = bmi;
                        bMovieTitleTB.Text = "Title:"+ bVideoData.Data.Video.Title;
                        bMovieIDTB.Text = "ID:"+ bVideoData.Data.Video.Id;
                        bMovieDataStyleTB.Text = "DataStyle:"+ label;
                        bMovieTypeTB.Text = "MovieType:"+ bVideoData.movietype;
                    }
                    else
                        MessageBox.Show("取得コメント数が0コメントでした。", "Add Comments", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("コメントを追加できませんでした。382"+ex.ToString(), "Add Comments", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            doukiViewandList();
        }

        private void TestBtnClick(object sender, RoutedEventArgs e)
        {
            isAddPatissire.IsChecked = true;
            isAddCa.IsChecked = true;
            isAdd184.IsChecked = false;
            isMoveMigiRemoven.IsChecked = true;
            isMoveMigiUnChecked.IsChecked = true;
            IsSendAfterDelete.IsChecked = false;
            IsToukomeAddorResetAdd.IsChecked = true;
            GetMyMemoryHidden.IsChecked = false;
        }
        private void CopySelectedCommentUserID(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((CommentData)bcommView.SelectedItem).UserName);
        }

        private void CopySelectedCommentCommand(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((CommentData)bcommView.SelectedItem).Command);
        }

        private void CopySelectedCommentText(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((CommentData)bcommView.SelectedItem).Comment);
        }

        private void ZurasuACommListTime(object sender, RoutedEventArgs e)
        {
            ZurasuTime zt = new ZurasuTime();
            zt.Owner = this;
            zt.Show();
            doukiViewandList();
        }

        private void SetACommListTime(object sender, RoutedEventArgs e)
        {
            SetTime zt = new SetTime();
            zt.Owner = this;
            zt.Show();
            doukiViewandList();
        }

        private void SendAListCommentBtnClick(object sender, RoutedEventArgs e)
        {
            client.checkIsCommunityFollower();
            if (client.isCommunityFollower)
            {
                bool result = Regex.IsMatch(AIDTextBox.Text, "(^[0-9]{5,}$)|(^sm[0-9]{1,}$)|(^nm[0-9]{1,}$)|(^so[0-9]{1,}$)");
                bool urlresult = Regex.IsMatch(AIDTextBox.Text, "^s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+$");
                string movieurl = result ? "https://nicovideo.jp/watch/" + AIDTextBox.Text : (urlresult ? AIDTextBox.Text : "Error");
                if (movieurl == "Error") MessageBox.Show("動画IDの形式がだめです。366\n動画ページのURLもしくは(sm|nm|so)数値or数値からなる動画IDを指定してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    try
                    {
                        string label = (ASorTComboBox.SelectedIndex == 0) ? "default" : ((ASorTComboBox.SelectedIndex == 1) ? "owner" : "community");
                        aVideoData.getWatchAPIData(client, movieurl);
                        aVideoData.getMovieType();
                        BitmapImage bmi = new BitmapImage(new Uri(aVideoData.Data.Video.Thumbnail.Url));
                        AMovieImage.Source = bmi;
                        AMovieTitleTB.Text = "Title:" + aVideoData.Data.Video.Title;
                        AMovieIDTB.Text = "ID:" + aVideoData.Data.Video.Id;
                        AMovieDataStyleTB.Text = "DataStyle:" + label;
                        AMovieTypeTB.Text = "MovieType:" + aVideoData.movietype;
                        MessageBoxResult mbres = MessageBox.Show("以下の動画に計" + acommList.Data.ToList().Count.ToString() + "コメント出力します。\n---------------\n動画ID:" + aVideoData.Data.Video.Id + "\n動画タイトル:" + aVideoData.Data.Video.Title + "\n出力スタイル:" + label + "\n出力予想終了時間" + ((label == "owner") ? DateTime.Now.ToString("MM/dd(ddd) HH:mm:ss") : DateTime.Now.AddSeconds(acommList.Data.Count * 5).ToString("MM/dd(ddd) HH:mm:ss")), "Output Comment", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (mbres == MessageBoxResult.OK)
                        {
                            if (!aVideoData.checkCommentSizeOK(acommList.Data.ToList(), (label == "owner") ? 1024 : 75))
                            {
                                MessageBox.Show(((label == "owner") ? "1024" : "75") + "文字を超えているコメントがあります。", "Output Comments", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                acommView.IsEnabled = false; bcommView.IsEnabled = false;
                                SBMessageTB.Text = (aVideoData.Data != null) ? aVideoData.Data.Video.Title : "";
                                int i = aVideoData.sendCommentsFromCommentDataList(client, acommList.Data.ToList(), label, SBMessageTB, (bool)IsToukomeAddorResetAdd.IsChecked);
                                if (i > 0) MessageBox.Show("コメント投稿が完了しました。\n合計コメント: " + i.ToString() + "コメント");
                                if ((bool)IsSendAfterDelete.IsChecked) acommList.Data.Clear();
                                acommView.IsEnabled = true; bcommView.IsEnabled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("コメントを出力できませんでした。446" + ex.Message, "Output Comments", MessageBoxButton.OK, MessageBoxImage.Error);
                        acommView.IsEnabled = true; bcommView.IsEnabled = true;
                    }
                }
            }
            else
            {
                MessageBox.Show("悪用防止のためにこの機能は作者コミュニティーフォロワー限定の機能となります。\n使用したい場合は以下のコミュニティーをフォローした後アプリケーションを再起動してください。(反映に5～10分ほどかかります)\n\nhttps://com.nicovideo.jp/community/co5033742", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (client.isLogin)
            {
                System.Diagnostics.Process.Start("https://nicovideo.jp/user/" + client.userID);
            }
        }

        private void MBImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (bVideoData.Data != null)
            {
                System.Diagnostics.Process.Start("https://nicovideo.jp/watch/" + bVideoData.Data.Video.Id);
            }
        }

        private void MAImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (aVideoData.Data != null)
            {
                System.Diagnostics.Process.Start("https://nicovideo.jp/watch/" + aVideoData.Data.Video.Id);
            }
        }

        private void Exclude184CommandA(object sender, RoutedEventArgs e)
        {
            int r = acommList.excludeAnyCommand("184");
            MessageBox.Show(r.ToString() + "件のコメントから184コマンドを除外しました。");
        }

        private void ExcludeAnyCommandA(object sender, RoutedEventArgs e)
        {
            string str = Microsoft.VisualBasic.Interaction.InputBox("除外するコマンドを半角空白もしくは[,]ごとに区切って入力してください。", "コマンドの除外", default);
            int r = acommList.excludeAnyCommand(str);
            MessageBox.Show(r.ToString() + "件のコメントから["+string.Join(" ",str.Split(' ',','))+"]コマンドを除外しました。");
        }

        private void Exclude184CommandB(object sender, RoutedEventArgs e)
        {
            int r = bcommList.excludeAnyCommand("184");
            MessageBox.Show(r.ToString() + "件のコメントから184コマンドを除外しました。");
        }

        private void ExcludeAnyCommandB(object sender, RoutedEventArgs e)
        {
            string str = Microsoft.VisualBasic.Interaction.InputBox("除外するコマンドを半角空白もしくは[,]ごとに区切って入力してください。", "コマンドの除外", default);
            int r = bcommList.excludeAnyCommand(str);
            MessageBox.Show(r.ToString() + "件のコメントから[" + string.Join(" ", str.Split(' ', ',')) + "]コマンドを除外しました。");
        }

        private void AllCommentNicoru(object sender, RoutedEventArgs e)
        {
            client.checkIsCommunityFollower();
            if (client.isCommunityFollower)
            {
                if (client.isPremium == true)
                {
                    int r = bVideoData.sendNicorus(client, "default", bVideoData.getVideoCommentDatasChat(client));
                    MessageBox.Show(r.ToString() + "件のコメントをニコりました。");
                }
                else
                {
                    MessageBox.Show("この機能はプレミアム会員限定機能です。ニコニコ動画の公式ページからプレミヤム会員になってから出直してきてください。", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("悪用防止のためにこの機能は作者コミュニティーフォロワー限定の機能となります。\n使用したい場合は以下のコミュニティーをフォローした後アプリケーションを再起動してください。(反映に5～10分ほどかかります)\n\nhttps://com.nicovideo.jp/community/co5033742", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CaCommentNicoru(object sender, RoutedEventArgs e)
        {
            client.checkIsCommunityFollower();
            if (client.isCommunityFollower)
            {
                if (client.isPremium == true)
                {
                    try
                    {
                        List<Chat> chats = bVideoData.getVideoCommentDatasChat(client);
                        chats.RemoveAll(s => (s.chat.mail == null || s.chat.mail == "") ? true : !s.chat.mail.Contains("ca"));
                        int r = bVideoData.sendNicorus(client, "default", chats);
                        MessageBox.Show(r.ToString() + "件のcaコマンド付きコメントをニコりました。");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n" + ex.ToString() + "\n" + ex.TargetSite);
                    }
                }
                else
                {
                    MessageBox.Show("この機能はプレミアム会員限定機能です。ニコニコ動画の公式ページからプレミヤム会員になってから出直してきてください。", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("悪用防止のためにこの機能は作者コミュニティーフォロワー限定の機能となります。\n使用したい場合は以下のコミュニティーをフォローした後アプリケーションを再起動してください。(反映に5～10分ほどかかります)\n\nhttps://com.nicovideo.jp/community/co5033742", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetOldMovieCommentBtnClick(object sender, RoutedEventArgs e)
        {
            string kakodate = KakologTextBox.Text;
            bool result = Regex.IsMatch(IDTextBox.Text, "(^[0-9]{5,}$)|(^sm[0-9]{1,}$)|(^nm[0-9]{1,}$)|(^so[0-9]{1,}$)");
            bool urlresult = Regex.IsMatch(IDTextBox.Text, "^s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+$");
            string movieurl = result ? "https://nicovideo.jp/watch/" + IDTextBox.Text : (urlresult ? IDTextBox.Text : "Error");
            if (movieurl == "Error") MessageBox.Show("動画IDの形式がだめです。366\n動画ページのURLもしくは(sm|nm|so)数値or数値からなる動画IDを指定してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                try
                {
                    string label = (SorTComboBox.SelectedIndex == 0) ? "default" : ((SorTComboBox.SelectedIndex == 1) ? "owner" : "community");
                    bVideoData.getWatchAPIData(client, movieurl);
                    SBMessageTB.Text = (bVideoData.Data != null) ? bVideoData.Data.Video.Title : "";
                    DateTime dt = DateTime.Parse(kakodate);
                    List<CommentData> cd = bVideoData.getKakoVideoCommentDatas(client, dt, label);
                    if (cd != null)
                    {
                        bcommList.addCommentDatas(cd);
                        MessageBox.Show("コメントを追加しました。\nコメント数:" + cd.Count.ToString(), "Add Comments", MessageBoxButton.OK, MessageBoxImage.Information);
                        BitmapImage bmi = new BitmapImage(new Uri(bVideoData.Data.Video.Thumbnail.Url));
                        bMovieImage.Source = bmi;
                        bMovieTitleTB.Text = "Title:" + bVideoData.Data.Video.Title;
                        bMovieIDTB.Text = "ID:" + bVideoData.Data.Video.Id;
                        bMovieDataStyleTB.Text = "DataStyle:" + label;
                        bMovieTypeTB.Text = "MovieType:" + bVideoData.movietype;
                    }
                    else
                        MessageBox.Show("取得コメント数が0コメントでした。", "Add Comments", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("コメントを追加できませんでした。382" + ex.ToString(), "Add Comments", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            doukiViewandList();
        }

        private void MKCTPostBtnClick(object sender, RoutedEventArgs e)
        {
            client.checkIsCommunityFollower();
            if (client.isCommunityFollower)
            {
                if (MKCTColorTB.Text == null || MKCTColorTB.Text == "" || MKCTComboBox.SelectedIndex >= 3 || MKCTDescTB.Text == null || MKCTDescTB.Text == "" || MKCTLengthTB.Text == null || MKCTLengthTB.Text == "" || MKCTTitleTB == null || MKCTTitleTB.Text == "")
                {
                    MessageBox.Show("コミュニティーID以外は全部入力してください。");
                }
                else if (MKCTCommunityTB.Text != null && MKCTCommunityTB.Text != "" && !MKCTCommunityTB.Text.Contains("co"))
                {
                    MessageBox.Show("コミュニティーIDはcoから始まってください。お願いします。\n所属させない場合は何も書かないでね");
                }
                else
                {
                    Upload uploadC = new Upload();
                    int videoid = uploadC.PostVideo(client, MKCTLengthTB.Text, MKCTColorTB.Text, MKCTTitleTB.Text, MKCTDescTB.Text, MKCTCommunityTB.Text, MKCTComboBox.SelectedIndex);
                    MessageBox.Show("動画が投稿されました。\n動画ID: sm" + videoid.ToString());
                }
            }
            else
            {
                MessageBox.Show("悪用防止のためにこの機能は作者コミュニティーフォロワー限定の機能となります。\n使用したい場合は以下のコミュニティーをフォローした後アプリケーションを再起動してください。(反映に5～10分ほどかかります)\n\nhttps://com.nicovideo.jp/community/co5033742", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NamaHozon(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "ニコ動ThreadJSON|*.json|All Files (*.*)|*.*";
            saveFileDialog.DefaultExt = ".json";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                if (saveFileDialog.FilterIndex == 1 /*ThreadChat*/)
                {
                    string json = bVideoData.lastyomikomijson;
                    StreamWriter sr = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8);
                    sr.Write(json);
                    sr.Close();
                }
            }
        }

        private void ShiteiNamaHozon(object sender, RoutedEventArgs e)
        {
            try
            {
                string movieurl = "";
                if (IDTextBox.Text != null && IDTextBox.Text != "")
                {
                    bool result = Regex.IsMatch(IDTextBox.Text, "(^[0-9]{5,}$)|(^sm[0-9]{1,}$)|(^nm[0-9]{1,}$)|(^so[0-9]{1,}$)");
                    bool urlresult = Regex.IsMatch(IDTextBox.Text, "^s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+$");
                    movieurl = result ? "https://nicovideo.jp/watch/" + IDTextBox.Text : (urlresult ? IDTextBox.Text : "Error");
                    movieurl = movieurl.Replace("https://", "");
                    movieurl = movieurl.Replace("www.", "");
                    movieurl = movieurl.Replace("watch/", "");
                    movieurl = movieurl.Replace("nicovideo.jp/", "");
                    if (movieurl.Contains("?")) movieurl = movieurl.Split(new char[] { '?', ' ', '=' })[0];
                }
                NamaHozonOption nhoWnd = new NamaHozonOption(client, movieurl, (bool)(SorTComboBox.SelectedIndex == 2));
                nhoWnd.Owner = this;
                nhoWnd.ShowDialog();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OpenHaihuPageClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://chu-commentart.ssl-lolipop.jp/2020/10/29/post-6765/");
        }

        private void NamaHozon(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "ニコ動ThreadJSON|*.json|All Files (*.*)|*.*";
            saveFileDialog.DefaultExt = ".json";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                if (saveFileDialog.FilterIndex == 1 /*ThreadChat*/)
                {
                    string json = bVideoData.lastyomikomijson;
                    StreamWriter sr = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8);
                    sr.Write(json);
                    sr.Close();
                }
            }
        }

        private void ShiteiNameHozon(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                string movieurl = "";
                if (IDTextBox.Text != null && IDTextBox.Text != "")
                {
                    bool result = Regex.IsMatch(IDTextBox.Text, "(^[0-9]{5,}$)|(^sm[0-9]{1,}$)|(^nm[0-9]{1,}$)|(^so[0-9]{1,}$)");
                    bool urlresult = Regex.IsMatch(IDTextBox.Text, "^s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+$");
                    movieurl = result ? "https://nicovideo.jp/watch/" + IDTextBox.Text : (urlresult ? IDTextBox.Text : "Error");
                    movieurl = movieurl.Replace("https://", "");
                    movieurl = movieurl.Replace("www.", "");
                    movieurl = movieurl.Replace("watch/", "");
                    movieurl = movieurl.Replace("nicovideo.jp/", "");
                    if (movieurl.Contains("?")) movieurl = movieurl.Split(new char[] { '?', ' ', '=' })[0];
                }
                NamaHozonOption nhoWnd = new NamaHozonOption(client, movieurl, (bool)(SorTComboBox.SelectedIndex == 2));
                nhoWnd.Owner = this;
                nhoWnd.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OpenGitHubPageClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Negima1072/NicoCommentTransfer");
        }
    }
}
