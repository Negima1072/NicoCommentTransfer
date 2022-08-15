using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NicoCommentTransfer.API
{
    public class NvComment
    {
        private WatchAPIV3Data data;

        public NvComment(string apijson)
        {
            data = JsonConvert.DeserializeObject<APIResponse<WatchAPIV3Data>>(apijson).Data;
        }

        public NvCommentThreadsResData getComments(Client client, long? unixTime = null)
        {
            reqstart:
            NvCommentThreadsReq req = new NvCommentThreadsReq();
            req.Params = data.Comment.NvComment.Params;
            req.ThreadKey = data.Comment.NvComment.ThreadKey;
            if (unixTime != null) req.Additionals = new NvCommentThreadsAdditionals(unixTime);
            string res = client.getReqWithJson(new Uri("https://nvcomment.nicovideo.jp/v1/threads"), JsonConvert.SerializeObject(req));
            APIResponse<NvCommentThreadsResData> rescl = JsonConvert.DeserializeObject<APIResponse<NvCommentThreadsResData>>(res);
            //Console.WriteLine(JsonConvert.SerializeObject(rescl));
            if (rescl.Meta.Status == 400 && rescl.Meta.ErrorCode == "EXPIRED_TOKEN")
            {
                data.Comment.NvComment.ThreadKey = getThreadKey(client);
                System.Threading.Thread.Sleep(5 * 1000);
                goto reqstart;
            }
            return rescl.Data;
        }

        public List<NvCommentData> getForkComments(Client client, string label, long? unixTime = null)
        {
            NvCommentThreadsResData res = getComments(client, unixTime);
            List<NvCommentData> comments = new List<NvCommentData>();
            data.Comment.Threads.ForEach((t) =>
            {
                if (t.Label == label)
                {
                    res.Threads.ForEach((t2) =>
                    {
                        if (t2.Id == t.Id.ToString() && t2.Fork == t.ForkLabel)
                        {
                            t2.Comments.ForEach((c) =>
                            {
                                try
                                {
                                    comments.Add(new NvCommentData(c));
                                }
                                catch(Exception e)
                                {
                                    Console.WriteLine(JsonConvert.SerializeObject(c));
                                    Console.WriteLine(e.Message);
                                    throw (e);
                                }
                            });
                        }
                    });
                }
            });
            return comments;
        }

        public string getThreadKey(Client client)
        {
            string res = client.getReq(new Uri("https://nvapi.nicovideo.jp/v1/comment/keys/thread?videoId=" + data.Video.Id), type: RestSharp.Method.Get);
            return JsonConvert.DeserializeObject<APIResponse<NvThreadKeyResData>>(res).Data.ThreadKey;
        }

        public string getPostKey(Client client, string label = "default")
        {
            string res = client.getReq(new Uri("https://nvapi.nicovideo.jp/v1/comment/keys/post?threadId=" + getThreadID(label)), type: RestSharp.Method.Get);
            return JsonConvert.DeserializeObject<APIResponse<NvPostKeyResData>>(res).Data.PostKey;
        }

        public string getThreadID(string label)
        {
            string id = null;
            data.Comment.Threads.ForEach((t) =>
            {
                if (t.Label == label) id = t.Id.ToString();
            });
            return id;
        }

        public APIResponse<NvPostCommentResData> postComment(Client client, string body, string command, int vposMs, string label)
        {
            NvPostCommentReq req = new NvPostCommentReq();
            req.Body = body;
            req.Commands = command.Split(' ');
            req.PostKey = getPostKey(client, label);
            req.VideoId = data.Video.Id;
            req.VposMs = vposMs;
            string res = client.getReqWithJson(new Uri("https://nvcomment.nicovideo.jp/v1/threads/"+getThreadID(label)+"/comments"), JsonConvert.SerializeObject(req));
            return JsonConvert.DeserializeObject<APIResponse<NvPostCommentResData>>(res);
        }

        public string getOwnerCommentUpdateKey(Client client)
        {
            string res = client.getReq(new Uri("https://nvapi.nicovideo.jp/v1/comment/keys/update?threadId=" + getThreadID("owner")), type: RestSharp.Method.Get);
            return JsonConvert.DeserializeObject<APIResponse<NvUpdateKeyResData>>(res).Data.UpdateKey;
        }

        public APIResponse<object> updateOwnerComments(Client client, List<NvCommentData> comments)
        {
            NvOwnerCommentUpdateReq req = new NvOwnerCommentUpdateReq();
            req.UpdateKey = getOwnerCommentUpdateKey(client);
            req.VideoId = data.Video.Id;
            req.Comments = new List<NvOwnerCommentData>();
            string threadId = getThreadID("owner");
            long unixTime = NvCommentData.GetUnixTime(DateTime.Now) * 1000;
            int no = 1;
            comments.ForEach((c) => { req.Comments.Add(new NvOwnerCommentData(c, threadId, unixTime, client.isPremium, client.userID, no)); no++; });
            Console.WriteLine(JsonConvert.SerializeObject(req));
            string res = client.getReqWithJson(new Uri("https://nvcomment.nicovideo.jp/v1/threads/" + threadId + "/owner-comments"), JsonConvert.SerializeObject(req), type: RestSharp.Method.Put);
            return JsonConvert.DeserializeObject<APIResponse<object>>(res);
        }
    }

    public class NvCommentDataList
    {
        public event BoolEventHandler ItemPropertyChanged;
        public ObservableCollection<NvCommentData> Data { get; set; }
        public NvCommentDataList()
        {
            Data = new ObservableCollection<NvCommentData>{ };
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
            int s = 0;
            foreach (var d in Data)
            {
                if (d.IsChecked) s++;
            }
            return s;
        }
        public int getCount(bool isChecked = false)
        {
            if (isChecked) return getCommentDatasIsChecked().Count;
            else return Data.Count;
        }
        public void addCommentData(int number, int timepos, string username, string command, string comment, int nicoru, int score, string dt)
        {
            NvCommentData cd = new NvCommentData(number, timepos, username, command, comment, nicoru, score, dt);
            cd.CheckedChanged += new BoolEventHandler(ItemPropertyChangedDef);
            Data.Add(cd);
        }
        public void addCommentData(NvCommentData cd)
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
            foreach (NvCommentData cd in Data)
            {
                cd.IsChecked = isChecked;
            }
        }
        public List<NvCommentData> getCommentDatasIsChecked(bool isChecked = true)
        {
            List<NvCommentData> commentDatas = new List<NvCommentData>();
            foreach (NvCommentData cd in Data)
            {
                if (cd.IsChecked == isChecked) commentDatas.Add(cd);
            }
            return commentDatas;
        }
        public List<NvCommentData> getCommentDatasIsCheckedN(bool isChecked = true)
        {
            List<NvCommentData> commentDatas = new List<NvCommentData>();
            foreach (NvCommentData cd in Data)
            {
                if (cd.IsChecked == isChecked) commentDatas.Add(cd);
            }
            return commentDatas;
        }
        public int excludeAnyCommand(string ecommand)
        {
            try
            {
                int r = 0;
                string[] ecommands = ecommand.Split(' ', ',');
                foreach (NvCommentData cd in Data)
                {
                    if (cd.Commands == null || cd.Commands == "") continue;
                    List<string> commands = cd.Commands.Split(' ', '　').ToList();
                    r += (commands.RemoveAll(s => ecommands.Contains(s)) >= 1) ? 1 : 0;
                    cd.Commands = string.Join(" ", commands);
                }
                return r;
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーが発生しました。C296\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }
        public int addCommentDatas(List<NvCommentData> commentDatas, bool addPati = false, bool addCa = false, bool add184 = false)
        {
            int r = 0;
            foreach (NvCommentData cd in commentDatas)
            {
                r++;
                if (addCa && (cd.Commands.IndexOf("ca ") < 0 && cd.Commands.IndexOf(" ca") < 0)) cd.Commands += " ca";
                if (addPati && cd.Commands.IndexOf("patissier") < 0) cd.Commands += " patissier";
                if (add184 && cd.Commands.IndexOf("184") < 0) cd.Commands += " 184";
                addCommentData(cd);
            }
            return r;
        }
        public void removeCommentDatasIsChecked(bool isChecked = true)
        {

            List<NvCommentData> tmp = Data.ToList();
            tmp.RemoveAll(s => s.IsChecked == isChecked);
            Data.Clear();
            foreach (var s in tmp)
            {
                addCommentData(s);
            }
            ItemPropertyChanged(this, new BoolEventArgs(false));
        }
        public void removeCommentDatasIsSelected(bool isSelected = true)
        {
            List<NvCommentData> tmp = Data.ToList();
            tmp.RemoveAll(s => s.IsSelected == isSelected);
            Data.Clear();
            foreach (var s in tmp)
            {
                addCommentData(s);
            }
            ItemPropertyChanged(this, new BoolEventArgs(false));
        }
        public int removeAll(Predicate<NvCommentData> pcd)
        {
            List<NvCommentData> tmp = Data.ToList();
            int r = tmp.RemoveAll(pcd);
            Data.Clear();
            foreach (var s in tmp)
            {
                addCommentData(s);
            }
            ItemPropertyChanged(this, new BoolEventArgs(false));
            return r;
        }
        public List<NvCommentData> Find(Predicate<NvCommentData> pcd)
        {
            List<NvCommentData> tmp = Data.ToList();
            return tmp.FindAll(pcd);
        }
        public List<NvCommentData> getSelectedItems()
        {
            List<NvCommentData> cc = new List<NvCommentData>();
            foreach (NvCommentData cd in Data)
            {
                if (cd.IsSelected) cc.Add(cd);
            }
            return cc;
        }
        public int checkComments(List<NvCommentData> commentDatas, bool isChecked = true)
        {
            int r = 0;
            foreach (NvCommentData cd in commentDatas)
            {
                r++;
                Data[Data.IndexOf(cd)].IsChecked = isChecked;
            }
            return r;
        }
        public void checkSelectedItems(bool selected = true, bool checkedi = true)
        {
            foreach (NvCommentData cd in Data)
            {
                if (cd.IsSelected == selected) cd.IsChecked = checkedi;
            }
        }
        public void unCheckCommentDatasIsChecked()
        {
            foreach (NvCommentData cd in Data)
            {
                if (cd.IsChecked) cd.IsChecked = false;
            }
        }
        public void setCommentTime(int vpos = 0, string time = "00:00.00")
        {
            int zuraspos;
            if (vpos == 0 && time != "00:00.00")
            {
                zuraspos = NvCommentData.ConvertTime2VposMs(time);
            }
            else
            {
                zuraspos = vpos;
            }
            foreach (NvCommentData cd in Data)
            {
                cd.Vpos = NvCommentData.ConvertVposMs2Time(zuraspos);
            }
        }
        public void zurasuCommentTime(bool isPlus, int vpos = 0, string time = "00:00.00")
        {
            int zuraspos;
            if (vpos == 0 && time != "00:00.00")
            {
                zuraspos = NvCommentData.ConvertTime2VposMs(time);
            }
            else
            {
                zuraspos = vpos;
            }
            foreach (NvCommentData cd in Data)
            {
                if (isPlus) cd.setVposMs(zuraspos + cd.getVposMs());
                if (!isPlus) cd.setVposMs((cd.getVposMs() - zuraspos < 0) ? 0 : cd.getVposMs() - zuraspos);
            }
        }
        private List<TokomeEditorJsonCommentContainer> makeTokomeContainer()
        {
            List<TokomeEditorJsonCommentContainer> cc = new List<TokomeEditorJsonCommentContainer>();
            foreach (NvCommentData cd in Data)
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
            foreach (NvCommentData cd in Data)
            {
                string c = cd.Body.Replace("\n", "<br>").Replace("\t", "[tb]");
                r += "[";
                r += cd.Commands;
                r += "]";
                r += cd.Body;
                r += "\n";
            }
            return r;
        }
        public DataNvCommentSet makeDCAMXML()
        {
            DataNvCommentSet dcs = new DataNvCommentSet(this);
            return dcs;
        }
    }

    public class DataNvCommentItem/*Domosan*/
    {
        public int index;
        public Color Color = new Color();
        public string Pos = "naka";
        public string Mode = "Big16";
        public string Size = "Big16";
        public int Width = 16;
        public List<string> Lines = new List<string>();
        public string Name;
        public DataNvCommentItem() { }
        public DataNvCommentItem(NvCommentData cd, int i)
        {
            string[] commands = cd.Commands.Split(' ', '　');
            string[] comments = cd.Body.Replace("\t", "  ").Split('\n');
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

    public class DataNvCommentSet/*Domosan*/
    {
        public string SelectedPos = "ue";
        public string SelectedSize = "Big16";
        public int CommentWidth = 16;
        public List<DataNvCommentItem> CommentList = new List<DataNvCommentItem>();
        public DataNvCommentSet() { }
        public DataNvCommentSet(NvCommentDataList cdlist)
        {
            int i = 1;
            foreach (NvCommentData cd in cdlist.Data)
            {
                CommentList.Add(new DataNvCommentItem(cd, i));
                i++;
            }
        }
    }

    public class APIResponse<T>
    {
        [JsonProperty("meta")]
        public APIResponseMeta Meta;
        [JsonProperty("data")]
        public T Data;
    }

    public class APIResponseMeta
    {
        [JsonProperty("status")]
        public int Status;
        [JsonProperty("errorCode")]
        public string ErrorCode;
    }

    public class NvThreadKeyResData
    {
        [JsonProperty("threadKey")]
        public string ThreadKey;
    }

    public class NvPostKeyResData
    {
        [JsonProperty("postKey")]
        public string PostKey;
    }

    public class NvUpdateKeyResData
    {
        [JsonProperty("updateKey")]
        public string UpdateKey;
    }

    public class NvPostCommentReq
    {
        [JsonProperty("body")]
        public string Body;
        [JsonProperty("commands")]
        public string[] Commands;
        [JsonProperty("postKey")]
        public string PostKey;
        [JsonProperty("videoId")]
        public string VideoId;
        [JsonProperty("vposMs")]
        public int VposMs;
    }

    public class NvOwnerCommentUpdateReq
    {
        [JsonProperty("comments")]
        public List<NvOwnerCommentData> Comments;
        [JsonProperty("updateKey")]
        public string UpdateKey;
        [JsonProperty("videoId")]
        public string VideoId;
    }

    public class NvPostCommentResData
    {
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("no")]
        public int No;
    }

    public class WatchAPIV3Data
    {
        //[JsonProperty("ads")]
        //[JsonProperty("category")]
        [JsonProperty("channel")]
        public object Chennel;
        [JsonProperty("client")]
        public object Client;
        [JsonProperty("comment")]
        public WatchAPIComment Comment;
        [JsonProperty("community")]
        public object Community;
        [JsonProperty("easyComment")]
        public object EasyComment;
        [JsonProperty("external")]
        public object External;
        [JsonProperty("genre")]
        public object Genre;
        [JsonProperty("marquee")]
        public object Marquee;
        [JsonProperty("media")]
        public object Media;
        [JsonProperty("okReason")]
        public string OkReason;
        [JsonProperty("owner")]
        public object Owner;
        [JsonProperty("payment")]
        public object Payment;
        [JsonProperty("pcWatchPage")]
        public object PcWatchPage;
        [JsonProperty("player")]
        public object Player;
        [JsonProperty("ppv")]
        public object Ppv;
        [JsonProperty("ranking")]
        public object Ranking;
        [JsonProperty("series")]
        public object Series;
        [JsonProperty("smartphone")]
        public object Smartphone;
        [JsonProperty("system")]
        public object System;
        [JsonProperty("tag")]
        public object Tag;
        [JsonProperty("video")]
        public WarchAPIVideo Video;
        [JsonProperty("videoAds")]
        public object VideoAds;
        [JsonProperty("videoLive")]
        public object VideoLive;
        [JsonProperty("viewer")]
        public object Viewer;
        [JsonProperty("waku")]
        public object Waku;
    }

    public class WarchAPIVideo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("count")]
        public Count Count { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail { get; set; }

        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        [JsonProperty("registeredAt")]
        public DateTime RegisteredAt { get; set; }

        [JsonProperty("isPrivate")]
        public bool IsPrivate { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("isNoBanner")]
        public bool IsNoBanner { get; set; }

        [JsonProperty("isAuthenticationRequired")]
        public bool IsAuthenticationRequired { get; set; }

        [JsonProperty("isEmbedPlayerAllowed")]
        public bool IsEmbedPlayerAllowed { get; set; }

        [JsonProperty("isGiftAllowed")]
        public bool IsGiftAllowed { get; set; }

        [JsonProperty("viewer")]
        public object Viewer { get; set; }

        [JsonProperty("watchableUserTypeForPayment")]
        public string WatchableUserTypeForPayment { get; set; }

        [JsonProperty("commentableUserTypeForPayment")]
        public string CommentableUserTypeForPayment { get; set; }

        [JsonProperty("9d091f87")]
        public bool _9d091f87 { get; set; }
    }

    public class WatchAPIComment
    {
        [JsonProperty("server")]
        public WatchAPICommentServer Server;
        [JsonProperty("keys")]
        public WatchAPICommentKeys Keys;
        [JsonProperty("layers")]
        public List<WatchAPICommentLayer> Layers;
        [JsonProperty("threads")]
        public List<WatchAPICommentThread> Threads;
        [JsonProperty("ng")]
        public object Ng;
        [JsonProperty("isAttentionRequired")]
        public bool IsAttentionRequired;
        [JsonProperty("nvComment")]
        public WatchAPICommentNv NvComment;
    }

    public class WatchAPICommentThread
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fork")]
        public int Fork { get; set; }

        [JsonProperty("forkLabel")]
        public string ForkLabel { get; set; }

        [JsonProperty("videoId")]
        public string VideoId { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("isDefaultPostTarget")]
        public bool IsDefaultPostTarget { get; set; }

        [JsonProperty("isEasyCommentPostTarget")]
        public bool IsEasyCommentPostTarget { get; set; }

        [JsonProperty("isLeafRequired")]
        public bool IsLeafRequired { get; set; }

        [JsonProperty("isOwnerThread")]
        public bool IsOwnerThread { get; set; }

        [JsonProperty("isThreadkeyRequired")]
        public bool IsThreadkeyRequired { get; set; }

        [JsonProperty("threadkey")]
        public object Threadkey { get; set; }

        [JsonProperty("is184Forced")]
        public bool Is184Forced { get; set; }

        [JsonProperty("hasNicoscript")]
        public bool HasNicoscript { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("postkeyStatus")]
        public int PostkeyStatus { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }
    }

    public class WatchAPICommentServer
    {
        [JsonProperty("url")]
        public string Url;
    }

    public class WatchAPICommentKeys
    {
        [JsonProperty("userKey")]
        public string UserKey;
    }

    public class WatchAPICommentLayer
    {
        [JsonProperty("index")]
        public int Index;
        [JsonProperty("isTranslucent")]
        public bool IsTranslucent;
        [JsonProperty("threadIds")]
        public List<WatchAPICommentThreadId> ThreadIds;
    }

    public class WatchAPICommentThreadId
    {
        [JsonProperty("id")]
        public int Id;
        [JsonProperty("fork")]
        public int Fork;
        [JsonProperty("forkLabel")]
        public string ForkLabel;
    }

    public class WatchAPICommentNv
    {
        [JsonProperty("threadKey")]
        public string ThreadKey;
        [JsonProperty("server")]
        public string Server;
        [JsonProperty("params")]
        public WatchAPICommentParams Params;
    }

    public class WatchAPICommentParams
    {
        [JsonProperty("targets")]
        public List<WatchAPICommentTargets> Targets;
        [JsonProperty("language")]
        public string Language;
    }

    public class WatchAPICommentTargets
    {
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("fork")]
        public string Fork;
    }

    public class NvCommentThreadsReq
    {
        [JsonProperty("additionals")]
        public NvCommentThreadsAdditionals Additionals;
        [JsonProperty("params")]
        public WatchAPICommentParams Params;
        [JsonProperty("threadKey")]
        public string ThreadKey;
    }

    public class NvCommentThreadsAdditionals
    {
        [JsonProperty("when")]
        public long? When;
        public NvCommentThreadsAdditionals(long? when)
        {
            When = when;
        }
    }

    public class NvCommentThreadsResData
    {
        [JsonProperty("globalComments")]
        public List<NvCommentThreadsGlobalComment> GlobalComments;
        [JsonProperty("threads")]
        public List<NvCommentThreadsThread> Threads;
    }

    public class NvCommentThreadsGlobalComment
    {
        [JsonProperty("count")]
        public int Count;
        [JsonProperty("id")]
        public string Id;
    }

    public class NvCommentThreadsThread
    {
        [JsonProperty("commentCount")]
        public int CommentCount;
        [JsonProperty("comments")]
        public List<NvCommentThreadsComment> Comments;
        [JsonProperty("fork")]
        public string Fork;
        [JsonProperty("id")]
        public string Id;
    }

    public class NvCommentThreadsComment
    {
        [JsonProperty("body")]
        public string Body;
        [JsonProperty("commands")]
        public List<string> Commands;
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("isMyPost")]
        public bool IsMyPost;
        [JsonProperty("isPremium")]
        public bool IsPremium;
        [JsonProperty("nicoruCount")]
        public int NicoruCount;
        [JsonProperty("nicoruId")]
        public string NicoruId;
        [JsonProperty("no")]
        public int No;
        [JsonProperty("postedAt")]
        public string PostedAt;
        [JsonProperty("score")]
        public int Score;
        [JsonProperty("source")]
        public string Source;
        [JsonProperty("userId")]
        public string UserId;
        [JsonProperty("vposMs")]
        public int VposMs;
    }

    public class NvCommentData : INotifyPropertyChanged
    {
        public event BoolEventHandler CheckedChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        private string _body;
        [JsonProperty("body")]
        public string Body
        {
            get { return _body; }
            set
            {
                if (_body != value)
                {
                    _body = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Body)));
                }
            }
        }
        private List<string> _commands = new List<string>();
        [JsonProperty("commands")]
        public List<string> CommandsList
        {
            get { return _commands; }
            set
            {
                if (_commands != value)
                {
                    _commands = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandsList)));
                }
            }
        }
        [JsonIgnore]
        public string Commands
        {
            get { return string.Join(" ", _commands); }
            set
            {
                if (string.Join(" ", _commands) != value)
                {
                    _commands = value.Split(' ').ToList();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Commands)));
                }
            }
        }
        private string _id;
        [JsonProperty("id")]
        public string Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
                }
            }
        }
        private bool _isMyPost;
        [JsonProperty("isMyPost")]
        public bool IsMyPost
        {
            get { return _isMyPost; }
            set
            {
                if (_isMyPost != value)
                {
                    _isMyPost = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMyPost)));
                }
            }
        }
        private bool _isPremium;
        [JsonProperty("isPremium")]
        public bool IsPremium
        {
            get { return _isPremium; }
            set
            {
                if (_isPremium != value)
                {
                    _isPremium = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPremium)));
                }
            }
        }
        private int _nicoruCount;
        [JsonProperty("nicoruCount")]
        public int NicoruCount
        {
            get { return _nicoruCount; }
            set
            {
                if (_nicoruCount != value)
                {
                    _nicoruCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NicoruCount)));
                }
            }
        }
        private string _nicoruId;
        [JsonProperty("nicoruId")]
        public string NicoruId
        {
            get { return _nicoruId; }
            set
            {
                if (_nicoruId != value)
                {
                    _nicoruId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NicoruId)));
                }
            }
        }
        private int _no;
        [JsonProperty("no")]
        public int No
        {
            get { return _no; }
            set
            {
                if (_no != value)
                {
                    _no = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(No)));
                }
            }
        }
        private DateTime _postedAt = DateTime.Now;
        [JsonIgnore]
        public string PostedAt
        {
            get { return _postedAt.ToString("yyyy-MM-dd HH:mm:ss"); }
            set
            {
                if (_postedAt != DateTime.Parse(value))
                {
                    _postedAt = DateTime.Parse(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PostedAt)));
                }
            }
        }
        [JsonProperty("postedAt")]
        public string PostAtFormated
        {
            get { return _postedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); }
            set
            {
                if (_postedAt != DateTime.Parse(value))
                {
                    _postedAt = DateTime.Parse(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PostAtFormated)));
                }
            }
        }
        private int _score;
        [JsonProperty("score")]
        public int Score
        {
            get { return _score; }
            set
            {
                if (_score != value)
                {
                    _score = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Score)));
                }
            }
        }
        private string _source;
        [JsonProperty("source")]
        public string Source
        {
            get { return _source; }
            set
            {
                if (_source != value)
                {
                    _source = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
                }
            }
        }
        private string _userId;
        [JsonProperty("userId")]
        public string UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserId)));
                }
            }
        }
        private int _vposMs;
        [JsonIgnore]
        public string Vpos
        {
            get { return ConvertVposMs2Time(_vposMs); }
            set
            {
                if (_vposMs != ConvertTime2VposMs(value))
                {
                    _vposMs = ConvertTime2VposMs(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Vpos)));
                }
            }
        }
        [JsonProperty("vposMs")]
        public int VposMs
        {
            get { return _vposMs; }
            set
            {
                if (_vposMs != value)
                {
                    _vposMs = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VposMs)));
                }
            }
        }
        private bool _isSelected;
        [JsonIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
        private bool _isChecked;
        [JsonIgnore]
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    CheckedChanged?.Invoke(this, new BoolEventArgs(value));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
                }
            }
        }
        public NvCommentData(NvCommentData comment)
        {
            Body = comment.Body;
            Commands = comment.Commands;
            Id = comment.Id;
            IsMyPost = comment.IsMyPost;
            IsPremium = comment.IsPremium;
            NicoruCount = comment.NicoruCount;
            NicoruId = comment.NicoruId;
            No = comment.No;
            PostedAt = comment.PostedAt;
            Score = comment.Score;
            Source = comment.Source;
            UserId = comment.UserId;
            Vpos = comment.Vpos;
            IsSelected = comment.IsSelected;
            IsChecked = comment.IsChecked;
        }
        public NvCommentData(NvCommentThreadsComment comment)
        {
            Body = comment.Body;
            Commands = string.Join(" ", comment.Commands.ToArray());
            Id = comment.Id;
            IsMyPost = comment.IsMyPost;
            IsPremium = comment.IsPremium;
            NicoruCount = comment.NicoruCount;
            NicoruId = comment.NicoruId;
            No = comment.No;
            PostedAt = comment.PostedAt;
            Score = comment.Score;
            Source = comment.Source;
            UserId = comment.UserId;
            Vpos = ConvertVposMs2Time(comment.VposMs);
            IsSelected = false;
            IsChecked = false;
        }
        public NvCommentData(int number, int timepos, string username, string command, string comment, int nicoru, int score, string dt)
        {
            Body = comment;
            Commands = command;
            Id = null;
            IsMyPost = false;
            IsPremium = false;
            NicoruCount = nicoru;
            NicoruId = null;
            No = number;
            PostedAt = dt;
            Score = score;
            Source = null;
            UserId = username;
            Vpos = ConvertVposMs2Time(timepos);
            IsSelected = false;
            IsChecked = false;
        }
        public static string ConvertVposMs2Time(int vposMs)
        {
            int vpos = vposMs / 10;
            string a, s, d;
            a = (vpos / 6000).ToString();
            if (vpos / 6000 < 10) a = "0" + a;
            s = ((vpos % 6000) / 100).ToString();
            if ((vpos % 6000) / 100 < 10) s = "0" + s;
            d = ((vpos % 6000) % 100).ToString();
            if ((vpos % 6000) % 100 < 10) d = "0" + d;
            return a + ":" + s + "." + d;
        }
        public static int ConvertTime2VposMs(string time = "00:00.00")
        {
            try
            {
                char[] del = { ':', '.' };
                string[] times = time.Split(del);
                return (int.Parse(times[0]) * 6000 + int.Parse(times[1]) * 100 + int.Parse(times[2])) * 10;
            }
            catch (Exception e)
            {
                Console.WriteLine("\"" + time + "\"");
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
        public TokomeEditorJsonCommentContainer makeToukomeJsonElement()
        {
            return new TokomeEditorJsonCommentContainer(Vpos, Commands, Body);
        }
        public void setVposMs(int vposMs)
        {
            Vpos = ConvertVposMs2Time(vposMs);
        }
        public int getVposMs()
        {
            return ConvertTime2VposMs(Vpos);
        }
    }

    public class NvOwnerCommentData : NvCommentData
    {
        [JsonProperty("$fork")]
        public string Fork;
        [JsonProperty("$original")]
        public object Original;
        [JsonProperty("$postedInWatch")]
        public bool PostedInWatch;
        [JsonProperty("$threadId")]
        public string ThreadId;
        [JsonProperty("$timestamp")]
        public long TimeStamp;
        public NvOwnerCommentData(NvCommentData nvCommentData, string threadId, long unixTime, bool isPremium, string userId, int no) : base(nvCommentData)
        {
            Fork = "owner";
            Original = null;
            PostedInWatch = true;
            ThreadId = threadId;
            TimeStamp = unixTime;
            Id = Guid.NewGuid().ToString();
            IsMyPost = true;
            IsPremium = isPremium;
            UserId = userId;
            No = no;
        }
    }
}
