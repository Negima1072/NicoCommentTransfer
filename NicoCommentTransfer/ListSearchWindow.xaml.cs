using NicoCommentTransfer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// ListSearchWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ListSearchWindow : Window
    {
        string ListType = "b";
        public ListSearchWindow(string listtype = "b")
        {
            InitializeComponent();
            this.Topmost = true;
            ListType = listtype;
            if(listtype != "b")
            {
                MigiBtn.IsEnabled = false;
            }
        }

        private void CheckBtnClick(object sender, RoutedEventArgs e)
        {
            if (ListType == "b")
            {
                try
                {
                    int r = ((MainWindow)this.Owner).bcommList.checkComments(
                    ((MainWindow)this.Owner).bcommList.Find(s =>
                        ((UserTB.Text == null || UserTB.Text == "") ? true : (((bool)UserCB.IsChecked) ? s.UserId == UserTB.Text : Regex.IsMatch(s.UserId == null ? "" : s.UserId, UserTB.Text))) &&
                        ((CommandTB.Text == null || CommandTB.Text == "") ? true : (((bool)CommandCB.IsChecked) ? s.Commands == CommandTB.Text : Regex.IsMatch(s.Commands == null ? "" : s.Commands, CommandTB.Text))) &&
                        ((CommentTB.Text == null || CommentTB.Text == "") ? true : (((bool)CommentCB.IsChecked) ? s.Body == CommentTB.Text : Regex.IsMatch(s.Body == null ? "" : s.Body, CommentTB.Text))) &&
                        ((TimeTB.Text == null || TimeTB.Text == "") ? true : (TimeTB.Text.Contains("-") ? (s.getVposMs() >= CommentData.ConvertTimeToVpos(TimeTB.Text.Split('-')[0]) && s.getVposMs() <= CommentData.ConvertTimeToVpos(TimeTB.Text.Split('-')[1])) : s.Vpos == TimeTB.Text)) &&
                        ((NicoruTB.Text == null || NicoruTB.Text == "") ? true : (TimeTB.Text.Contains("-") ? (s.getVposMs() >= int.Parse(NicoruTB.Text.Split('-')[0]) && s.getVposMs() <= int.Parse(NicoruTB.Text.Split('-')[1])) : s.NicoruCount == int.Parse(NicoruTB.Text)))
                    ));
                    MessageBox.Show("成功しました。\nChecked element:" + r.ToString(), "検索", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch(Exception exce)
                {
                    Console.WriteLine(exce.ToString());
                    MessageBox.Show("検索に失敗しました。Error:L51\n"+exce.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteBtnClick(object sender, RoutedEventArgs e)
        {
            if(ListType == "b")
            {
                try
                {
                    int r = ((MainWindow)this.Owner).bcommList.removeAll(s =>
                        ((UserTB.Text == null || UserTB.Text == "") ? true : (((bool)UserCB.IsChecked) ? s.UserId == UserTB.Text : Regex.IsMatch(s.UserId == null ? "" : s.UserId, UserTB.Text))) &&
                        ((CommandTB.Text == null || CommandTB.Text == "") ? true : (((bool)CommandCB.IsChecked) ? s.Commands == CommandTB.Text : Regex.IsMatch(s.Commands == null ? "" : s.Commands, CommandTB.Text))) &&
                        ((CommentTB.Text == null || CommentTB.Text == "") ? true : (((bool)CommentCB.IsChecked) ? s.Body == CommentTB.Text : Regex.IsMatch(s.Body == null ? "" : s.Body, CommentTB.Text))) &&
                        ((TimeTB.Text == null || TimeTB.Text == "") ? true : (TimeTB.Text.Contains("-") ? (s.getVposMs() >= CommentData.ConvertTimeToVpos(TimeTB.Text.Split('-')[0]) && s.getVposMs() <= CommentData.ConvertTimeToVpos(TimeTB.Text.Split('-')[1])) : s.Vpos == TimeTB.Text)) &&
                        ((NicoruTB.Text == null || NicoruTB.Text == "") ? true : (TimeTB.Text.Contains("-") ? (s.getVposMs() >= int.Parse(NicoruTB.Text.Split('-')[0]) && s.getVposMs() <= int.Parse(NicoruTB.Text.Split('-')[1])) : s.NicoruCount == int.Parse(NicoruTB.Text)))
                    );
                    //CommentData f = ((MainWindow)this.Owner).bcommList.Data[0];
                    //Console.WriteLine(((UserTB.Text == null || UserTB.Text == "") ? true : (((bool)UserCB.IsChecked) ? f.UserName == UserTB.Text : Regex.IsMatch(f.UserName, UserTB.Text))).ToString() + "/" +
                    //    ((CommandTB.Text == null || CommandTB.Text == "") ? true : (((bool)CommandCB.IsChecked) ? f.Command == CommandTB.Text : Regex.IsMatch(f.Command, CommandTB.Text))).ToString() + "/" +
                    //    ((CommentTB.Text == null || (CommentTB.Text == "") ? true : (((bool)CommentCB.IsChecked) ? f.Comment == CommentTB.Text : Regex.IsMatch(f.Comment, CommentTB.Text))).ToString() + "/" +
                    //    ((TimeTB.Text == null || TimeTB.Text == "") ? true : (TimeTB.Text.Contains("-") ? (f.TimePos >= CommentData.ConvertTimeToVpos(TimeTB.Text.Split('-')[0]) && f.TimePos <= CommentData.ConvertTimeToVpos(TimeTB.Text.Split('-')[1])) : f.Time == TimeTB.Text)).ToString()));
                    ((MainWindow)this.Owner).doukiViewandList();
                    MessageBox.Show("成功しました。\nRemoved element:"+r.ToString(), "検索", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception exce)
                {
                    Console.WriteLine(exce.ToString());
                    MessageBox.Show("検索に失敗しました。Error:L73\n" + exce.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MoveMigiBtnClick(object sender, RoutedEventArgs e)
        {
            if (ListType == "b")
            {
                try
                {
                    Predicate<NvCommentData> pcd = (s =>
                        ((UserTB.Text == null || UserTB.Text == "") ? true : (((bool)UserCB.IsChecked) ? s.UserId == UserTB.Text : Regex.IsMatch(s.UserId == null ? "" : s.UserId, UserTB.Text))) &&
                        ((CommandTB.Text == null || CommandTB.Text == "") ? true : (((bool)CommandCB.IsChecked) ? s.Commands == CommandTB.Text : Regex.IsMatch(s.Commands == null ? "" : s.Commands, CommandTB.Text))) &&
                        ((CommentTB.Text == null || CommentTB.Text == "") ? true : (((bool)CommentCB.IsChecked) ? s.Body == CommentTB.Text : Regex.IsMatch(s.Body == null ? "" : s.Body, CommentTB.Text))) &&
                        ((TimeTB.Text == null || TimeTB.Text == "") ? true : (TimeTB.Text.Contains("-") ? (s.getVposMs() >= CommentData.ConvertTimeToVpos(TimeTB.Text.Split('-')[0]) && s.getVposMs() <= CommentData.ConvertTimeToVpos(TimeTB.Text.Split('-')[1])) : s.Vpos == TimeTB.Text)) &&
                        ((NicoruTB.Text == null || NicoruTB.Text == "") ? true : (TimeTB.Text.Contains("-") ? (s.getVposMs() >= int.Parse(NicoruTB.Text.Split('-')[0]) && s.getVposMs() <= int.Parse(NicoruTB.Text.Split('-')[1])) : s.NicoruCount == int.Parse(NicoruTB.Text)))
                    );
                    int r = ((MainWindow)this.Owner).acommList.addCommentDatas(((MainWindow)this.Owner).bcommList.Find(pcd), (bool)((MainWindow)this.Owner).isAddPatissire.IsChecked, (bool)((MainWindow)this.Owner).isAddCa.IsChecked);
                    if ((bool)((MainWindow)this.Owner).isMoveMigiRemoven.IsChecked) ((MainWindow)this.Owner).bcommList.removeAll(pcd);
                    else if ((bool)((MainWindow)this.Owner).isMoveMigiUnChecked.IsChecked) ((MainWindow)this.Owner).bcommList.checkComments(((MainWindow)this.Owner).bcommList.Find(pcd), false);
                    ((MainWindow)this.Owner).doukiViewandList();
                    MessageBox.Show("成功しました。\nMoved element:" + r.ToString(), "検索", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception exce)
                {
                    Console.WriteLine(exce.Message);
                    MessageBox.Show("検索に失敗しました。Error:L95\n" + exce.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
