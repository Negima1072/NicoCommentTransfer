using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NicoCommentTransfer.API
{
    public delegate void BoolEventHandler(object sender, BoolEventArgs e);
    [JsonObject("TokomeEditorJsonCommentContainer")]
    public class TokomeEditorJsonCommentContainer
    {
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("command")]
        public string Command { get; set; }
        [JsonProperty("comment")]
        public string Comment{ get; set; }
        public TokomeEditorJsonCommentContainer() { }
        public TokomeEditorJsonCommentContainer(string time, string command, string comment)
        {
            Time = time;
            Command = command;
            Comment = comment.Replace("\n", "\\n");
        }
    }
    public class BoolEventArgs : EventArgs
    {
        public bool value;
        public BoolEventArgs(bool value)
        {
            this.value = value;
        }
    }
    [JsonObject("commentData")]
    public class CommentData : INotifyPropertyChanged
    {
        public event BoolEventHandler CheckedChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        private int number;
        [JsonProperty("Number")]
        public int Number
        {
            get { return number; }
            set
            {
                if (number != value)
                {
                    number = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Number)));
                }
            }
        }
        private bool ischecked;
        public bool isChecked
        {
            get { return ischecked; }
            set
            {
                if (ischecked != value)
                {
                    ischecked = value;
                    CheckedChanged?.Invoke(this, new BoolEventArgs(value));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isChecked)));
                }
            }
        }
        private int timepos;
        [JsonProperty("TimePos")]
        public int TimePos
        {
            get { return timepos; }
            set
            {
                if (timepos != value)
                {
                    timepos = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimePos)));
                }
            }
        }
        private string time;
        [JsonProperty("Time")]
        public string Time
        {
            get { return time; }
            set
            {
                if (time != value)
                {
                    time = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
                }
            }
        }
        private string username;
        [JsonProperty("UserName")]
        public string UserName
        {
            get { return username; }
            set
            {
                if (username!= value)
                {
                    username = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserName)));
                }
            }
        }
        private string command;
        [JsonProperty("Command")]
        public string Command
        {
            get { return command; }
            set
            {
                if (command != value)
                {
                    command = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Command)));
                }
            }
        }
        private string comment;
        [JsonProperty("Comment")]
        public string Comment
        {
            get { return comment; }
            set
            {
                if (comment != value)
                {
                    comment = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comment)));
                }
            }
        }
        [JsonProperty("Nicoru")]
        public int Nicoru
        {
            get { return nicoru; }
            set
            {
                if (nicoru != value)
                {
                    nicoru = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nicoru)));
                }
            }
        }
        private int nicoru;
        [JsonProperty("Score")]
        public int Score
        {
            get { return score; }
            set
            {
                if (score != value)
                {
                    score = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Score)));
                }
            }
        }
        private int score;
        [JsonProperty("Date")]
        public string date_str { get; private set; }
        public DateTime Date {
            get { return date; }
            set
            {
                if (date != value)
                {
                    date = value;
                    date_str = value.ToString("yyyy-MM-dd HH:mm:ss");
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
                }
            }
        }
        private DateTime date;
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }
        public CommentData(int number, int timepos, string username, string command, string comment, int nicoru, int score, DateTime dt)
        {
            Number = number;
            TimePos = timepos;
            UserName = username;
            Command = command;
            Comment = comment;
            Time = ConvertVposToTime(timepos);
            isChecked = false;
            IsSelected = false;
            Nicoru = nicoru;
            Score = score;
            Date = dt;
        }
        public CommentData(CommentData commentData)
        {
            Number = commentData.Number;
            TimePos = commentData.TimePos;
            UserName = commentData.UserName;
            Command = commentData.Command;
            Comment = commentData.Comment;
            Time = commentData.Time;
            isChecked = commentData.isChecked;
            IsSelected = commentData.IsSelected;
            Nicoru = commentData.Nicoru;
            Score = commentData.Score;
            Date = commentData.Date;
        }
        public TokomeEditorJsonCommentContainer makeToukomeJsonElement()
        {
            return new TokomeEditorJsonCommentContainer(Time, Command, Comment);
        }
        public static string ConvertVposToTime(int vpos)
        {
            string a, s, d;
            a = (vpos / 6000).ToString();
            if (vpos / 6000 < 10) a = "0" + a;
            s = ((vpos % 6000) / 100).ToString();
            if ((vpos % 6000) / 100 < 10) s = "0" + s;
            d = ((vpos % 6000) % 100).ToString();
            if ((vpos % 6000) % 100 < 10) d = "0" + d;
            return a + ":" + s + "." + d;
        }
        public static int ConvertTimeToVpos(string time = "00:00.00"/*00:00.00*/)
        {
            try
            {
                char[] del = { ':', '.' };
                string[] times = time.Split(del);
                return int.Parse(times[0]) * 6000 + int.Parse(times[1]) * 100 + int.Parse(times[2]);
            }
            catch(Exception e)
            {
                Console.WriteLine("\""+time+"\"");
                throw e;
            }
        }
        public static DateTime UnixToDateTime(int unix, int unix_sec)
        {
            string st = unix.ToString() + "." + unix_sec.ToString();
            return DateTimeOffset.FromUnixTimeSeconds((long)double.Parse(st)).ToLocalTime().DateTime;
        }
        public static long GetUnixTime(DateTime targetTime)
        {
            targetTime = targetTime.ToUniversalTime();
            DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan elapsedTime = targetTime - UNIX_EPOCH;
            return (long)elapsedTime.TotalSeconds;
        }
    }
    public class DomosanParam/*Domosan*/
    {
        public static List<string> poss = new List<string>
        {
            "ue", "nake", "shita"
        };
        public static Dictionary<int, string> sizes = new Dictionary<int, string>
        {
            {9, "Big9"},
            {10, "Big10"},
            {14, "Medium14"},
            {15, "Medium15"},
            {16, "Big16"},
            {17, "Big17"},
            {21, "Small21"},
            {22, "Small22"},
            {26, "Medium26"},
            {27, "Medium27"},
            {38, "Small38"},
            {39, "Small39"},
            {61, "ozto52"},
            {92, "ozto70"},
            {94, "ozto75"},
            {153, "ozto100"}
        };
        public static Dictionary<string, Color> colors = new Dictionary<string, Color>
        {
            { "black", Code("#000000") },
            { "purple", Code("#C000FF") },
            { "blue", Code("#0000FF") },
            { "cyan", Code("#00FFFF") },
            { "green", Code("#00FF00") },
            { "yellow", Code("#FFFF00") },
            { "orange", Code("#FFC000") },
            { "pink", Code("#FF8080") },
            { "red", Code("#FF0000") },
            { "white", Code("#FFFFFF") },
            { "white2", Code("#CCCC99") },
            { "red2", Code("#CC0033") },
            { "pink2", Code("#FF33CC") },
            { "orange2", Code("#FF6600") },
            { "yellow2", Code("#999900") },
            { "green2", Code("#00CC66") },
            { "cyan2", Code("#00CCCC") },
            { "blue2", Code("#3399FF") },
            { "purple2", Code("#6633CC") },
            { "black2", Code("#666666") }
        };
        public static Color Code(string code)
        {
            System.Drawing.Color ccolor = System.Drawing.ColorTranslator.FromHtml(code);
            Color c = Color.FromArgb(ccolor.A, ccolor.R, ccolor.G, ccolor.B);
            return c;
        }
    }
    public class DataCommentItem/*Domosan*/
    {
        public int index;
        public Color Color = new Color();
        public string Pos = "naka";
        public string Mode = "Big16";
        public string Size = "Big16";
        public int Width = 16;
        public List<string> Lines = new List<string>();
        public string Name;
        public DataCommentItem() { }
        public DataCommentItem(CommentData cd, int i)
        {
            string[] commands = cd.Command.Split(' ', '　');
            string[] comments = cd.Comment.Replace("\t", "  ").Split('\n');
            index = i;
            Color = DomosanParam.Code("#000000");
            foreach (string cmnd in commands)
            {
                if (DomosanParam.colors.Keys.Contains(cmnd))
                {
                    Color = DomosanParam.colors[cmnd];
                    break;
                }
                else if (cmnd[0] == '#')
                {
                    Color = DomosanParam.Code(cmnd);
                    break;
                }
            }
            foreach (string cmnd in commands)
            {
                if (DomosanParam.poss.Contains(cmnd))
                {
                    Pos = cmnd;
                }
            }
            if (DomosanParam.sizes.ContainsKey(comments.Length))
            {
                Size = DomosanParam.sizes[comments.Length];
                Mode = DomosanParam.sizes[comments.Length];
                int lngw = 0;
                foreach (string cmnt in comments)
                {
                    Lines.Add(cmnt);
                    if (cmnt.Length > lngw) lngw = cmnt.Length;
                }
                if (lngw != 0) Width = lngw;
            }
        }
    }
    public class DataCommentSet/*Domosan*/
    {
        public string SelectedPos = "ue";
        public string SelectedSize = "Big16";
        public int CommentWidth = 16;
        public List<DataCommentItem> CommentList = new List<DataCommentItem>();
        public DataCommentSet() { }
        public DataCommentSet(CommentDataList cdlist)
        {
            int i = 1;
            foreach (CommentData cd in cdlist.Data)
            {
                CommentList.Add(new DataCommentItem(cd, i));
                i++;
            }
        }
    }
    public class CommentDataList
    {
        public event BoolEventHandler ItemPropertyChanged;
        public ObservableCollection<CommentData> Data { get; set; }
        public CommentDataList()
        {
            Data = new ObservableCollection<CommentData> {
            };
        }
        int checkedNum = 0;
        public void ItemPropertyChangedDef(object sender, BoolEventArgs e)
        {
            if (e.value) checkedNum++;
            else checkedNum--;
            ItemPropertyChanged?.Invoke(this, new BoolEventArgs(e.value));
        }
        public int getCheckedNum()
        {
            //return checkedNum;
            int s = 0;
            foreach(var d in Data)
            {
                if (d.isChecked) s++;
            }
            return s;
        }
        public int getCount(bool isChecked = false)
        {
            if (isChecked) return getCommentDatasIsChecked().Count;
            else return Data.Count;
        }
        public void addCommentData(int number, int timepos, string username, string command, string comment, int nicoru, int score, DateTime dt)
        {
            CommentData cd = new CommentData(number, timepos, username, command, comment, nicoru, score, dt);
            cd.CheckedChanged += new BoolEventHandler(ItemPropertyChangedDef);
            Data.Add(cd);
        }
        public void addCommentData(CommentData cd)
        {
            cd.CheckedChanged += new BoolEventHandler(ItemPropertyChangedDef);
            Data.Add(cd);
        }
        public void clear()
        {
            Data.Clear();
        }
        public void allCheckBoxSetter(bool isChecked)
        {
            foreach(CommentData cd in Data)
            {
                cd.isChecked = isChecked;
            }
        }
        public List<CommentData> getCommentDatasIsChecked(bool isChecked = true)
        {
            List<CommentData> commentDatas = new List<CommentData>();
            foreach (CommentData cd in Data)
            {
                if (cd.isChecked == isChecked) commentDatas.Add(cd);
            }
            return commentDatas;
        }
        public List<CommentData> getCommentDatasIsCheckedN(bool isChecked = true)
        {
            List<CommentData> commentDatas = new List<CommentData>();
            foreach (CommentData cd in Data)
            {
                if (cd.isChecked == isChecked) commentDatas.Add(new CommentData(cd));
            }
            return commentDatas;
        }
        public int excludeAnyCommand(string ecommand)
        {
            try
            {
                int r = 0;
                string[] ecommands = ecommand.Split(' ', ',');
                foreach (CommentData cd in Data)
                {
                    if (cd.Command == null || cd.Command == "") continue;
                    List<string> commands = cd.Command.Split(' ', '　').ToList();
                    r += (commands.RemoveAll(s => ecommands.Contains(s)) >= 1) ? 1 : 0;
                    cd.Command = string.Join(" ", commands);
                }
                return r;
            }catch(Exception e)
            {
                MessageBox.Show("エラーが発生しました。C296\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }
        public int addCommentDatas(List<CommentData> commentDatas, bool addPati = false, bool addCa = false, bool add184 = false)
        {
            int r = 0;
            foreach (CommentData cd in commentDatas)
            {
                r++;
                if (addCa && (cd.Command.IndexOf("ca ") < 0 && cd.Command.IndexOf(" ca") < 0)) cd.Command += " ca";
                if (addPati && cd.Command.IndexOf("patissier") < 0) cd.Command += " patissier";
                if (add184 && cd.Command.IndexOf("184") < 0) cd.Command += " 184";
                addCommentData(cd);
            }
            return r;
        }
        public void removeCommentDatasIsChecked(bool isChecked = true)
        {

            List<CommentData> tmp = Data.ToList();
            tmp.RemoveAll(s => s.isChecked == isChecked);
            Data.Clear();
            foreach(var s in tmp)
            {
                addCommentData(s);
            }
            ItemPropertyChanged(this, new BoolEventArgs(false));
        }
        public void removeCommentDatasIsSelected(bool isSelected = true)
        {
            List<CommentData> tmp = Data.ToList();
            tmp.RemoveAll(s => s.IsSelected == isSelected);
            Data.Clear();
            foreach (var s in tmp)
            {
                addCommentData(s);
            }
            ItemPropertyChanged(this, new BoolEventArgs(false));
        }
        public int removeAll(Predicate<CommentData> pcd)
        {
            List<CommentData> tmp = Data.ToList();
            int r = tmp.RemoveAll(pcd);
            Data.Clear();
            foreach (var s in tmp)
            {
                addCommentData(s);
            }
            ItemPropertyChanged(this, new BoolEventArgs(false));
            return r;
        }
        public List<CommentData> Find(Predicate<CommentData> pcd)
        {
            List<CommentData> tmp = Data.ToList();
            return tmp.FindAll(pcd);
        }
        public List<CommentData> getSelectedItems()
        {
            List<CommentData> cc = new List<CommentData>();
            foreach (CommentData cd in Data)
            {
                if (cd.IsSelected) cc.Add(cd);
            }
            return cc;
        }
        public int checkComments(List<CommentData> commentDatas, bool isChecked = true)
        {
            int r = 0;
            foreach (CommentData cd in commentDatas)
            {
                r++;
                Data[Data.IndexOf(cd)].isChecked = isChecked;
            }
            return r;
        }
        public void checkSelectedItems(bool selected = true, bool checkedi = true)
        {
            foreach (CommentData cd in Data)
            {
                if (cd.IsSelected == selected) cd.isChecked = checkedi;
            }
        }
        public void unCheckCommentDatasIsChecked()
        {
            foreach (CommentData cd in Data)
            {
                if (cd.isChecked) cd.isChecked = false;
            }
        }
        public void setCommentTime(int vpos = 0, string time = "00:00.00")
        {
            int zuraspos;
            if (vpos == 0 && time != "00:00.00")
            {
                zuraspos = CommentData.ConvertTimeToVpos(time);
            }
            else
            {
                zuraspos = vpos;
            }
            foreach (CommentData cd in Data)
            {
                cd.TimePos = zuraspos;
                cd.Time = CommentData.ConvertVposToTime(cd.TimePos);
            }
        }
        public void zurasuCommentTime(bool isPlus, int vpos = 0, string time = "00:00.00")
        {
            int zuraspos;
            if (vpos == 0 && time != "00:00.00")
            {
                zuraspos = CommentData.ConvertTimeToVpos(time);
            }
            else
            {
                zuraspos = vpos;
            }
            foreach (CommentData cd in Data)
            {
                if (isPlus) cd.TimePos += zuraspos;
                if (!isPlus) cd.TimePos = (cd.TimePos - zuraspos < 0) ? 0 : cd.TimePos - zuraspos;
                cd.Time = CommentData.ConvertVposToTime(cd.TimePos);
            }
        }
        private List<TokomeEditorJsonCommentContainer> makeTokomeContainer()
        {
            List<TokomeEditorJsonCommentContainer> cc = new List<TokomeEditorJsonCommentContainer>();
            foreach(CommentData cd in Data)
            {
                cc.Add(cd.makeToukomeJsonElement());
            }
            return cc;
        }
        public string makeTokomeEditorJson()
        {
            List<TokomeEditorJsonCommentContainer> cc = makeTokomeContainer();
            return JsonConvert.SerializeObject(cc, Formatting.Indented);
        } 
        public string makeDansukuTxt()
        {
            string r = "";
            foreach(CommentData cd in Data)
            {
                string c = cd.Comment.Replace("\n", "<br>").Replace("\t", "[tb]");
                r += "[";
                r += cd.Command;
                r += "]";
                r += cd.Comment;
                r += "\n";
            }
            return r;
        }
        public DataCommentSet makeDCAMXML()
        {
            DataCommentSet dcs = new DataCommentSet(this);
            return dcs;
        }
    }
}
