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
    /// TextBoxV.xaml の相互作用ロジック
    /// </summary>
    public partial class TextBoxV : UserControl
    {
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set { SetValue(TextProperty, value); textBox.Text = value; }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextBoxV), new PropertyMetadata("", OnTextChanged));
        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            TextBoxV ctrl = obj as TextBoxV;
            if (ctrl != null)
            {
                ctrl.textBox.Text = ctrl.Text;
            }
        }

        public string BackText
        {
            get => (string)GetValue(BackTextProperty);
            set { SetValue(BackTextProperty, value); textBlock.Text = value; }
        }
        public static readonly DependencyProperty BackTextProperty = DependencyProperty.Register(nameof(BackText), typeof(string), typeof(TextBoxV), new PropertyMetadata("", OnBackTextChanged));
        private static void OnBackTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            TextBoxV ctrl = obj as TextBoxV;
            if (ctrl != null)
            {
                ctrl.textBlock.Text = ctrl.BackText;
            }
        }
        public TextBoxV()
        {
            InitializeComponent();
            textBlock.Text = BackText;
            textBox.Text = Text;
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox.Text == null || textBox.Text == "") textBlock.Visibility = Visibility.Visible;
            else textBlock.Visibility = Visibility.Collapsed;
            Text = textBox.Text;
        }
    }
}
