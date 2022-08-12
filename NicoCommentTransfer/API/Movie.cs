using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Management;
using System.Windows;
using System.Windows.Controls;

namespace NicoCommentTransfer.API
{
    public class NicoVideo
    {
        public string lastyomikomijson = "";
        public string movietype = null;
        public NicoVideo()
        {
            Data = null;
        }
        public WatchAPIData Data { get; set; }
        public WatchAPIData getWatchAPIData(Client client, string MovieURL)
        {
            try
            {
                string res = client.getReq(MovieURL, "");
                Console.WriteLine(res);
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(res);
                string d = doc.GetElementById("js-initial-watch-data") == null ? throw new Exception("Elementが見つかりませんでした。") : doc.GetElementById("js-initial-watch-data").GetAttribute("data-api-data");
                Data = JsonConvert.DeserializeObject<WatchAPIData>(d);
                Console.WriteLine(JsonConvert.SerializeObject(Data));
            }
            catch(Exception e)
            {
                MessageBox.Show("データが取得できませんでした。M26\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Data;
        }
        public string getMovieType()
        {
            try
            {
                if (Data == null) throw new Exception();
                if (false/*Default false => Data.context.isMyMemory*/) return "MyMemory";
                else if (Data.Video.IsDeleted) return "Deleted";
                else if (Data.Video.IsPrivate && Data.Video.Viewer.IsOwner) return "YourPrivate";
                else if (Data.Video.IsPrivate && !Data.Video.Viewer.IsOwner) return "Private";
                else if (Data.Channel != null) return "Channel";
                else if (Data.Community != null) return "Community";
                else return "Movie";
            }
            catch
            {
                throw new Exception();
            }
        }
        private Thread getThreadFromName(string name)
        {
            foreach(Thread t in Data.Comment.Threads)
            {
                if (t.Label == name) return t;
            }
            return null;
        }
        public List<CommentData> ConvertChatListToCommentDataList(List<Chat> chats)
        {
            List<CommentData> cd = new List<CommentData>();
            foreach(Chat c in chats)
            {
                DateTime dt = CommentData.UnixToDateTime(c.chat.date, c.chat.date_usec);
                Console.WriteLine("dt:"+dt.ToString("yyyy-MM-dd HH-mm-ss"));
                cd.Add(new CommentData(c.chat.no, c.chat.vpos, c.chat.user_id, c.chat.mail, c.chat.content, c.chat.nicoru, c.chat.score, dt));
            }
            return cd;
        }
        public List<Chat> DeserializeStringToChatsList(string json)
        {
            JArray resp = JArray.Parse(json);
            Console.WriteLine(json);
            List<Chat> chats = new List<Chat>();
            foreach (JObject c in resp)
            {
                if (c.ContainsKey("chat"))
                {
                    chats.Add(JsonConvert.DeserializeObject<Chat>(JsonConvert.SerializeObject(c, Formatting.None)));
                }
            }
            chats.RemoveAll(s => s.chat.thread == null || s.chat.no == 0 || s.chat.content == null);
            return chats;
        }
        public List<ResultThread> DeserializeStringToResultThreadsList(string json)
        {
            JArray resp = JArray.Parse(json);
            List<ResultThread> threads = new List<ResultThread>();
            foreach (JObject c in resp)
            {
                if (c.ContainsKey("thread"))
                {
                    threads.Add(JsonConvert.DeserializeObject<ResultThread>(JsonConvert.SerializeObject(c["thread"], Formatting.None)));
                }
            }
            return threads;
        }
        public List<JObject> DeserializeStringToAnyList(string json, string anykey)
        {
            JArray resp = JArray.Parse(json);
            List<JObject> threads = new List<JObject>();
            foreach (JObject c in resp)
            {
                if (c.ContainsKey(anykey))
                {
                    threads.Add(JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(c[anykey], Formatting.None)));
                }
            }
            return threads;
        }
        public List<CommentData> getVideoCommentDatas(Client client, string label = "default", bool mymemory = false)
        {
            try
            {
                movietype = getMovieType();
                Console.WriteLine(movietype);
                int lenminuit = (Data.Video.Duration / 60 + 1);
                if (movietype == "Private") return null;
                else if (movietype == "MyMemory")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("mymemory");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        Console.WriteLine(jsonr);
                        Console.WriteLine("k");
                        List<Chat> c = DeserializeStringToChatsList(jsonr);
                        c.RemoveAll(s => (mymemory ? false : !(s.chat.deleted != 1 && s.chat.deleted != 2)));
                        return ConvertChatListToCommentDataList(c);
                    }
                    else
                    {
                        Console.WriteLine("j");
                        return null;
                    }
                }
                else if (movietype == "YourPrivate" || movietype == "Movie" || movietype == "Deleted")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("default");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        Console.WriteLine(jsonr);
                        List<Chat> chats = DeserializeStringToChatsList(jsonr);
                        Console.WriteLine("chats:"+chats.Count.ToString());
                        List<CommentData> cds = ConvertChatListToCommentDataList(chats);
                        Console.WriteLine("cds:"+cds.Count.ToString());
                        return cds;
                    }
                    else if (label == "owner")
                    {
                        Thread t = getThreadFromName("owner");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (movietype == "Channel" || movietype == "Community")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("default");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else if (label == "community")
                    {
                        Thread t = getThreadFromName("community");
                        List<object> cm = new List<object>();
                        var queryDict = HttpUtility.ParseQueryString(client.getReq("http://flapi.nicovideo.jp/api/getthreadkey?thread=" + t.Id, ""));
                        string threadkey = queryDict["threadkey"];
                        string force_184 = queryDict["force_184"];
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThreadWithThreadKey(t.Id.ToString(), t.Fork, client.userID, threadkey, force_184, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeavesWithThreadKey(t.Id.ToString(), Data.Video.Duration, client.userID, threadkey, force_184));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else if (movietype == "Community" && label == "owner")
                    {
                        Thread t = getThreadFromName("owner");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }
        public List<CommentData> getKakoVideoCommentDatas(Client client, DateTime dt, string label = "default")
        {
            try
            {
                movietype = getMovieType();
                int lenminuit = (Data.Video.Duration / 60 + 1);
                long unixtime = CommentData.GetUnixTime(dt);
                if (movietype == "Private" || movietype == "MyMemory") return null;
                else if (movietype == "YourPrivate" || movietype == "Movie" || movietype == "Deleted")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("default");
                        string waybackkey = getWaybackKey(client, t.Id.ToString());
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JKThread(t.Id.ToString(), t.Fork, client.userID, waybackkey, unixtime, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JKThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, waybackkey, unixtime));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else if (label == "owner")
                    {
                        Thread t = getThreadFromName("owner");
                        string waybackkey = getWaybackKey(client, t.Id.ToString());
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JKThread(t.Id.ToString(), t.Fork, client.userID, waybackkey, unixtime));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (movietype == "Channel" || movietype == "Community")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("default");
                        string waybackkey = getWaybackKey(client, t.Id.ToString());
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JKThread(t.Id.ToString(), t.Fork, client.userID, waybackkey, unixtime, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JKThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, waybackkey, unixtime));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else if (label == "community")
                    {
                        Thread t = getThreadFromName("community");
                        string waybackkey = getWaybackKey(client, t.Id.ToString());
                        List<object> cm = new List<object>();
                        var queryDict = HttpUtility.ParseQueryString(client.getReq("http://flapi.nicovideo.jp/api/getthreadkey?thread=" + t.Id, ""));
                        string threadkey = queryDict["threadkey"];
                        string force_184 = queryDict["force_184"];
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JKThreadWithThreadKey(t.Id.ToString(), t.Fork, client.userID, threadkey, force_184, waybackkey, unixtime, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JKThreadLeavesWithThreadKey(t.Id.ToString(), Data.Video.Duration, client.userID, threadkey, force_184, waybackkey, unixtime));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else if (movietype == "Community" && label == "owner")
                    {
                        Thread t = getThreadFromName("owner");
                        string waybackkey = getWaybackKey(client, t.Id.ToString());
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JKThread(t.Id.ToString(), t.Fork, client.userID, waybackkey, unixtime));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return ConvertChatListToCommentDataList(DeserializeStringToChatsList(jsonr));
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public string getVideoCommentJson(Client client, bool def, bool own, bool com, bool esy)
        {
            try
            {
                if(!def && !own && !com && !esy)
                {
                    return "";
                }
                int ps = 0;
                movietype = getMovieType();
                int lenminuit = (Data.Video.Duration / 60 + 1);
                List<object> cm = new List<object>();
                cm.Add(new Ping("rs:0"));
                if (def)
                {
                    Thread t = getThreadFromName("default");
                    cm.Add(new Ping("ps:"+ps.ToString()));//0
                    cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey, "20090904"));
                    cm.Add(new Ping("pf:" + ps.ToString()));//0
                    ps++;
                    cm.Add(new Ping("ps:" + ps.ToString()));//1
                    cm.Add(new JThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, Data.Comment.Keys.UserKey));
                    cm.Add(new Ping("pf:" + ps.ToString()));//1
                    ps++;
                }
                if (own)
                {
                    Thread t = getThreadFromName("owner");
                    cm.Add(new Ping("ps:" + ps.ToString()));//2
                    cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                    cm.Add(new Ping("pf:" + ps.ToString()));//2
                    ps++;
                }
                if (com)
                {
                    Thread t = getThreadFromName("community");
                    var queryDict = HttpUtility.ParseQueryString(client.getReq("http://flapi.nicovideo.jp/api/getthreadkey?thread=" + t.Id, ""));
                    string threadkey = queryDict["threadkey"];
                    string force_184 = queryDict["force_184"];
                    cm.Add(new Ping("ps:" + ps.ToString()));
                    cm.Add(new JThreadWithThreadKey(t.Id.ToString(), t.Fork, client.userID, threadkey, force_184, "20090904"));
                    cm.Add(new Ping("pf:" + ps.ToString()));
                    ps++;
                    cm.Add(new Ping("ps:" + ps.ToString()));
                    cm.Add(new JThreadLeavesWithThreadKey(t.Id.ToString(), Data.Video.Duration, client.userID, threadkey, force_184));
                    cm.Add(new Ping("pf:" + ps.ToString()));
                    ps++;
                }
                if (esy)
                {
                    Thread t = getThreadFromName("easy");
                    cm.Add(new Ping("ps:" + ps.ToString()));//0
                    cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey, "20090904"));
                    cm.Add(new Ping("pf:" + ps.ToString()));//0
                    ps++;
                    cm.Add(new Ping("ps:" + ps.ToString()));//1
                    cm.Add(new JThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, Data.Comment.Keys.UserKey));
                    cm.Add(new Ping("pf:" + ps.ToString()));//1
                    ps++;
                }
                cm.Add(new Ping("rf:0"));
                string jsond = JsonConvert.SerializeObject(cm);
                string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                lastyomikomijson = jsonr;
                return jsonr;
            }catch(Exception e)
            {
                throw new Exception("getVideoCommentJson", e);
            }
        }
        public List<Chat> getVideoCommentDatasChat(Client client, string label = "default", bool mymemory = false)
        {
            try
            {
                movietype = getMovieType();
                Console.WriteLine(movietype);
                int lenminuit = (Data.Video.Duration / 60 + 1);
                if (movietype == "Private") return null;
                else if (movietype == "MyMemory")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("mymemory");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        Console.WriteLine(jsonr);
                        Console.WriteLine("k");
                        List<Chat> c = DeserializeStringToChatsList(jsonr);
                        c.RemoveAll(s => (mymemory ? false : !(s.chat.deleted != 1 && s.chat.deleted != 2)));
                        return c;
                    }
                    else
                    {
                        Console.WriteLine("j");
                        return null;
                    }
                }
                else if (movietype == "YourPrivate" || movietype == "Movie" || movietype == "Deleted")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("default");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return DeserializeStringToChatsList(jsonr);
                    }
                    else if (label == "owner")
                    {
                        Thread t = getThreadFromName("owner");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return DeserializeStringToChatsList(jsonr);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (movietype == "Channel" || movietype == "Community")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("default");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return DeserializeStringToChatsList(jsonr);
                    }
                    else if (label == "community")
                    {
                        Thread t = getThreadFromName("community");
                        List<object> cm = new List<object>();
                        var queryDict = HttpUtility.ParseQueryString(client.getReq("http://flapi.nicovideo.jp/api/getthreadkey?thread=" + t.Id, ""));
                        string threadkey = queryDict["threadkey"];
                        string force_184 = queryDict["force_184"];
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThreadWithThreadKey(t.Id.ToString(), t.Fork, client.userID, threadkey, force_184, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeavesWithThreadKey(t.Id.ToString(), Data.Video.Duration, client.userID, threadkey, force_184));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return DeserializeStringToChatsList(jsonr);
                    }
                    else if (movietype == "Community" && label == "owner")
                    {
                        Thread t = getThreadFromName("owner");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lastyomikomijson = jsonr;
                        return DeserializeStringToChatsList(jsonr);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public string getNicoruTicket(Client client, string label)
        {
            try
            {
                Thread t = getThreadFromName(label);
                string res = client.getReq("https://nvapi.nicovideo.jp/v1/nicorukey?language=0&threadId=" + t.Id.ToString() + "&fork=" + t.Fork.ToString() + "&isVideoOwnerNicoruEnabled=true", "", "GET", header: new Dictionary<string, string>() { { "X-Frontend-Id", "6" }, { "X-Frontend-Version", "0" }, { "X-Request-With", "https://www.nicovideo.jp" }, { "Accept-Language", "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7" } });
                if (res == null || res == "") return null;
                else
                {
                    JObject j = JsonConvert.DeserializeObject<JObject>(res);
                    if(j["meta"]["status"].ToString() == "200")
                    {
                        return j["data"]["nicorukey"].ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
        public int sendNicorus(Client client, string label, List<Chat> chats)
        {
            int res = 0;
            int retry = 0;
            foreach(Chat c in chats)
            {
            nicorusaisho:
                int r = sendNicoru(client, label, c);
                if (r == 1 && retry <= 3)
                {
                    retry++;
                    System.Threading.Thread.Sleep(2000);
                    goto nicorusaisho;
                }
                else
                {
                    retry = 0;
                }
                res += (r == 0) ? 1 : 0;
            }
            return res;
        }
        public int sendNicoru(Client client, string label, Chat cd)
        {
            try
            {
                Thread t = getThreadFromName(label);
                if (label == "default")
                {
                    List<object> cm = new List<object>();
                    cm.Add(new Ping("rs:9"));
                    cm.Add(new Ping("ps:55"));
                    cm.Add(new Nicoru(cd.chat.content, 0, t.Fork, cd.chat.no.ToString(), 0, getNicoruTicket(client, label), "1", cd.chat.date.ToString() + "." + cd.chat.date_usec.ToString(), client.isPremium ? 1 : 0, t.Id.ToString(), client.userID));
                    cm.Add(new Ping("pf:55"));
                    cm.Add(new Ping("rf:9"));
                    string jsond = JsonConvert.SerializeObject(cm);
                    string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                    Console.WriteLine(jsonr);
                    List<JObject> lrt = DeserializeStringToAnyList(jsonr, "nicoru_result");
                    return int.Parse(lrt[0]["status"].ToString());
                }
                else
                {
                    return 1;
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return 1;
            }
        }
        public string getSendChatTicket(Client client, string label)
        {
            try
            {
                movietype = getMovieType();
                Console.WriteLine(movietype);
                int lenminuit = (Data.Video.Duration / 60 + 1);
                List<ResultThread> lrt;
                if (movietype == "Deleted" || movietype == "Private" || movietype == "MyMemory") return null;
                else if (movietype == "YourPrivate" || movietype == "Movie")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("default");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lrt = DeserializeStringToResultThreadsList(jsonr);
                    }
                    else if (label == "owner")
                    {
                        Thread t = getThreadFromName("owner");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lrt = DeserializeStringToResultThreadsList(jsonr);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (movietype == "Channel" || movietype == "Community")
                {
                    if (label == "default")
                    {
                        Thread t = getThreadFromName("default");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeaves(t.Id.ToString(), Data.Video.Duration, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lrt = DeserializeStringToResultThreadsList(jsonr);
                    }
                    else if (label == "community")
                    {
                        Thread t = getThreadFromName("community");
                        List<object> cm = new List<object>();
                        var queryDict = HttpUtility.ParseQueryString(client.getReq("http://flapi.nicovideo.jp/api/getthreadkey?thread=" + t.Id, ""));
                        string threadkey = queryDict["threadkey"];
                        string force_184 = queryDict["force_184"];
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThreadWithThreadKey(t.Id.ToString(), t.Fork, client.userID, threadkey, force_184, "20090904"));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("ps:1"));
                        cm.Add(new JThreadLeavesWithThreadKey(t.Id.ToString(), Data.Video.Duration, client.userID, threadkey, force_184));
                        cm.Add(new Ping("pf:1"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lrt = DeserializeStringToResultThreadsList(jsonr);
                    }
                    else if (movietype == "Community" && label == "owner")
                    {
                        Thread t = getThreadFromName("owner");
                        List<object> cm = new List<object>();
                        cm.Add(new Ping("rs:0"));
                        cm.Add(new Ping("ps:0"));
                        cm.Add(new JThread(t.Id.ToString(), t.Fork, client.userID, Data.Comment.Keys.UserKey));
                        cm.Add(new Ping("pf:0"));
                        cm.Add(new Ping("rf:0"));
                        string jsond = JsonConvert.SerializeObject(cm);
                        string jsonr = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsond);
                        lrt = DeserializeStringToResultThreadsList(jsonr);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
                foreach (ResultThread rt in lrt)
                {
                    if (rt.resultcode == 0)
                    {
                        Console.WriteLine(rt.ticket);
                        return rt.ticket;
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
        public string getPostKey(Client client, string thread)
        {
            string url = "https://flapi.nicovideo.jp/api/getpostkey?thread=" + thread + "&block_no=" + Math.Floor((double)(Data.Video.Count.Comment+1) / 100).ToString() + "&yugi=&device=1&version=1&version_sub=6";
            string r = client.getReq(url, "", "GET");
            if (r == null || r == "") return null;
            else return r.Substring(8);
        }
        public string getWaybackKey(Client client, string thread)
        {
            string url = "https://flapi.nicovideo.jp/api/getwaybackkey?thread=" + thread;
            string r = client.getReq(url, "", "GET");
            if (r == null || r == "") return null;
            else return r.Substring(11);
        }
        public string makeSendCommentRequest(Client client,string thread, int vpos, string mail, string content, string ticket)
        {
            string postkeyd = getPostKey(client, thread);
            JArray requests = new JArray();
            requests.Add(JObject.Parse(JsonConvert.SerializeObject(new Ping("rs:1"), Formatting.None)));
            requests.Add(JObject.Parse(JsonConvert.SerializeObject(new Ping("ps:17"), Formatting.None)));
            requests.Add(JObject.Parse(JsonConvert.SerializeObject(new SendChat( thread, ticket,
                vpos, client.userID ,mail,postkeyd,client.isPremium?"1":"0", content), Formatting.None)));
            requests.Add(JObject.Parse(JsonConvert.SerializeObject(new Ping("pf:17"), Formatting.None)));
            requests.Add(JObject.Parse(JsonConvert.SerializeObject(new Ping("rf:1"), Formatting.None)));
            Console.WriteLine(JsonConvert.SerializeObject(requests, Formatting.None));
            return JsonConvert.SerializeObject(requests, Formatting.None);
        }
        public int getSendCommentResponseStatus(string res)
        {
            /*Success = 0,
            Failure = 1,
            InvalidThread = 2,
            InvalidTicket = 3,
            InvalidPostkey = 4,
            Locked = 5,
            Readonly = 6,
            TooLong = 8*/
            JArray resp = JArray.Parse(res);
            foreach (JObject c in resp)
            {
                if (c.ContainsKey("chat_result"))
                {
                    return int.Parse((string)JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(c["chat_result"], Formatting.None))["status"]);
                }
            }
            return 1;
        }
        public int getSendCommentResponseNo(string res)
        {
            /*no*/
            JArray resp = JArray.Parse(res);
            foreach (JObject c in resp)
            {
                if (c.ContainsKey("chat_result"))
                {
                    return int.Parse((string)JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(c["chat_result"], Formatting.None))["no"]);
                }
            }
            return 1;
        }
        public int getUpdateCommentResponseStatus(string res)
        {
            /*Success = 0,
            Failure = 1,
            InvalidThread = 2,
            InvalidTicket = 3,
            InvalidPostkey = 4,
            Locked = 5,
            Readonly = 6,
            TooLong = 8*/
            JArray resp = JArray.Parse(res);
            foreach (JObject c in resp)
            {
                if (c.ContainsKey("update_thread"))
                {
                    return int.Parse((string)JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(c["update_thread"], Formatting.None))["status"]);
                }
            }
            return 1;
        }
        public bool checkCommentSizeOK(List<CommentData> cc, int num = 75)
        {
            foreach(var cd in cc)
            {
                if (cd.Comment.Length > num) return false;
            }
            return true;
        }
        public string getOwnerThreadUpdataKey(Client client, string thread, string ticket)
        {
            string url = "https://flapi.nicovideo.jp/api/getupdatekey?thread=" + thread + "&ticket=" + ticket;
            string r = client.getReq(url, "");
            if (r == null || r == "") return null;
            else return r.Substring(10);
        }
        public UpdataItem ConvertCommentDataToUpdataItem(CommentData cd)
        {
            UpdataItem udi = new UpdataItem(cd.TimePos, cd.Command, cd.Comment);
            return udi;
        }
        public void updataMovieData(Client client)
        {
            Data = getWatchAPIData(client, "https://nicovideo.jp/watch/" + Data.Video.Id);
        }
        public int sendCommentsFromCommentDataList(Client client, List<CommentData> cc, string label, TextBlock sbbar, bool ToukomeisAdd = true)
        {
            movietype = getMovieType();
            Console.WriteLine("label" + label);
            if (movietype == "Deleted" || movietype == "Private" || movietype == "MyMemory") return 0;
            else if (label == "owner" && !Data.Video.Viewer.IsOwner) return 0;
            else if (label == "owner")
            {
                if (movietype == "YourPrivate" || movietype == "Movie" || movietype == "Community")
                {
                    Thread t = getThreadFromName("owner");
                    string ticket = getSendChatTicket(client, label);
                    if (ticket == null) return 0;
                    List<CommentData> ncc;
                    if (ToukomeisAdd) ncc = getVideoCommentDatas(client, "owner");
                    else ncc = new List<CommentData>();
                    foreach (var cd in cc) ncc.Add(cd);
                    string updtkey = getOwnerThreadUpdataKey(client, t.Id.ToString(), ticket);
                    JArray requests = new JArray();
                    requests.Add(JObject.Parse(JsonConvert.SerializeObject(new Ping("rs:1"), Formatting.None)));
                    foreach(var cd in ncc)
                    {
                        requests.Add(JObject.Parse(JsonConvert.SerializeObject(ConvertCommentDataToUpdataItem(cd), Formatting.None)));
                    }
                    requests.Add(JObject.Parse(JsonConvert.SerializeObject(new Ping("ps:195"), Formatting.None)));
                    requests.Add(JObject.Parse(JsonConvert.SerializeObject(new UpdateThread(ncc.Count, client.isPremium ? 1 : 0, t.Id.ToString(), ticket, updtkey, client.userID), Formatting.None)));
                    requests.Add(JObject.Parse(JsonConvert.SerializeObject(new Ping("pf:195"), Formatting.None)));
                    requests.Add(JObject.Parse(JsonConvert.SerializeObject(new Ping("rf:1"), Formatting.None)));
                    string jsons = JsonConvert.SerializeObject(requests, Formatting.None);
                    string res = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", jsons);
                    int restatus = getUpdateCommentResponseStatus(res);
                    if(restatus == 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            sbbar.Text = "送信完了 :" + cc.Count.ToString();
                        });
                        return cc.Count;
                    }
                    else
                    {
                        MessageBox.Show("送信に失敗しました。" + restatus.ToString());
                        Console.WriteLine(jsons);
                        Console.WriteLine(res);
                        return 0;
                    }
                }
                else return 0;
            }
            else if ((movietype == "YourPrivate" || movietype == "Movie") && label == "default")
            {
                Thread t = getThreadFromName("default");
                string ticket = getSendChatTicket(client, label);
                if (ticket == null) return 0;
                int numrestat = 0;
                int sumiSendChatnum = 0;
                foreach(CommentData cd in cc)
                {
                    saisho:
                    System.Threading.Thread.Sleep(5 * 1000);
                    string req = makeSendCommentRequest(client, t.Id.ToString(), cd.TimePos, cd.Command, cd.Comment, ticket);
                    string res = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", req);
                    int restatus = getSendCommentResponseStatus(res);
                    Data.Video.Count.Comment = getSendCommentResponseNo(res);
                    if (numrestat <= 3) {
                        if (restatus == 3) { ticket = getSendChatTicket(client, label); numrestat++; goto saisho; }
                        else if (restatus == 4) { numrestat++; goto saisho; }
                        else if (restatus == 0)
                        {
                            numrestat = 0; sumiSendChatnum++;
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                sbbar.Text = "送信完了 " + sumiSendChatnum.ToString() + "/" + cc.Count.ToString();
                            });
                        }
                        else { MessageBox.Show("送信に失敗しました。" + restatus.ToString()); break; }
                    } else { numrestat = 0; MessageBox.Show("送信に失敗しました。"+restatus.ToString()); break; }
                }
                return sumiSendChatnum;
            }
            else if (movietype == "Channel" || movietype == "Community")
            {
                if(label == "default")
                {
                    Thread t = getThreadFromName("default");
                    string ticket = getSendChatTicket(client, label);
                    if (ticket == null) return 0;
                    int numrestat = 0;
                    int sumiSendChatnum = 0;
                    foreach (CommentData cd in cc)
                    {
                        saisho2:
                        System.Threading.Thread.Sleep(5 * 1000);
                        string req = makeSendCommentRequest(client, t.Id.ToString(), cd.TimePos, cd.Command, cd.Comment, ticket);
                        string res = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", req);
                        int restatus = getSendCommentResponseStatus(res);
                        Data.Video.Count.Comment = getSendCommentResponseNo(res);
                        if (numrestat <= 3)
                        {
                            if (restatus == 3) { ticket = getSendChatTicket(client, label); numrestat++; goto saisho2; }
                            else if (restatus == 4) { numrestat++; goto saisho2; }
                            else if (restatus == 0)
                            {
                                numrestat = 0;
                                sumiSendChatnum++;
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    sbbar.Text = "送信完了 " + sumiSendChatnum.ToString() + "/" + cc.Count.ToString();
                                });
                            }
                            else { MessageBox.Show("送信に失敗しました。" + restatus.ToString()); break; }
                        }
                        else { numrestat = 0; MessageBox.Show("送信に失敗しました。" + restatus.ToString()); break; }
                    }
                    return sumiSendChatnum;
                }
                else if (label == "community")
                {
                    Thread t = getThreadFromName("community");
                    string ticket = getSendChatTicket(client, label);
                    if (ticket == null) return 0;
                    int numrestat = 0;
                    int sumiSendChatnum = 0;
                    foreach (CommentData cd in cc)
                    {
                        saisho3:
                        System.Threading.Thread.Sleep(5 * 1000);
                        string req = makeSendCommentRequest(client, t.Id.ToString(), cd.TimePos, cd.Command, cd.Comment, ticket);
                        string res = client.getReqWithJson("https://nvcomment.nicovideo.jp/legacy/api.json/thread", req);
                        int restatus = getSendCommentResponseStatus(res);
                        Data.Video.Count.Comment = getSendCommentResponseNo(res);
                        if (numrestat <= 3)
                        {
                            if (restatus == 3) { ticket = getSendChatTicket(client, label); numrestat++; goto saisho3; }
                            else if (restatus == 4) { updataMovieData(client); numrestat++; goto saisho3; }
                            else if (restatus == 0)
                            {
                                numrestat = 0;
                                sumiSendChatnum++;
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    sbbar.Text = "送信完了 " + sumiSendChatnum.ToString() + "/" + cc.Count.ToString();
                                });
                            }
                            else { MessageBox.Show("送信に失敗しました。" + restatus.ToString()); break; }
                        }
                        else { numrestat = 0; MessageBox.Show("送信に失敗しました。" + restatus.ToString()); break; }
                    }
                    return sumiSendChatnum;
                }
                else return 0;
            }
            return 0;
        }
    }
    [JsonObject("nicoru_content")]
    public class NicoruContent
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("nicorukey")]
        public string nicorukey { get; set; }
        [JsonProperty("fork")]
        public int fork { get; set; }
        [JsonProperty("postdate")]
        public string postdate { get; set; }
        [JsonProperty("language")]
        public int language { get; set; }
        [JsonProperty("premium")]
        public int premium { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        [JsonProperty("contributor")]
        public int contributor { get; set; }
        [JsonProperty("player_device")]
        public string player_device { get; set; }
        public NicoruContent(string content, int contributor, int fork, string id, int language, string nicorukey, string player_device, string postdate, int premium, string thread, string user_id)
        {
            this.thread = thread;
            this.id = id;
            this.nicorukey = nicorukey;
            this.fork = fork;
            this.postdate = postdate;
            this.language = language;
            this.premium = premium;
            this.user_id = user_id;
            this.content = content;
            this.contributor = contributor;
            this.player_device = player_device;
        }
    }
    [JsonObject("nicoru")]
    public class Nicoru
    {
        [JsonProperty("nicoru")]
        public NicoruContent nicoru { get; set; }
        public Nicoru(string content, int contributor, int fork, string id, int language, string nicorukey, string player_device, string postdate, int premium, string thread, string user_id)
        {
            nicoru = new NicoruContent(content, contributor, fork, id, language, nicorukey, player_device, postdate, premium, thread, user_id);
        }
    }
    [JsonObject("result_thread")]
    public class ResultThread
    {
        [JsonProperty("resultcode")]
        public int resultcode { get; set; }
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("server_time")]
        public long server_time { get; set; }
        [JsonProperty("last_res")]
        public int last_res { get; set; }
        [JsonProperty("ticket")]
        public string ticket { get; set; }
        [JsonProperty("revision")]
        public int revision { get; set; }
    }
    [JsonObject("chat_content")]
    public class ChatContent
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("no")]
        public int no { get; set; }
        [JsonProperty("vpos")]
        public int vpos { get; set; }
        [JsonProperty("leaf")]
        public int leaf { get; set; }
        [JsonProperty("date")]
        public int date { get; set; }
        [JsonProperty("date_usec")]
        public int date_usec { get; set; }
        [JsonProperty("premium")]
        public int premium { get; set; }
        [JsonProperty("anonymity")]
        public int anonymity { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("mail")]
        public string mail { get; set; }
        [JsonProperty("deleted")]
        public int deleted { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("score")]
        public int score { get; set; }
    }
    [JsonObject("chat")]
    public class Chat
    {
        [JsonProperty("chat")]
        public ChatContent chat { get; set; } = new ChatContent();
    }
    //for tokome editor
    [JsonObject("updata_item_content")]
    public class UpdataItemContent
    {
        [JsonProperty("vpos")]
        public int vpos { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("mail")]
        public string mail { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        public UpdataItemContent(int vpos, string mail, string content)
        {
            this.vpos = vpos;
            this.mail = mail;
            this.content = content;
            this.name = "";
        }
    }
    [JsonObject("updata_item")]
    public class UpdataItem
    {
        [JsonProperty("add_thread_item")]
        public UpdataItemContent updata_item { get; set; }
        public UpdataItem(int vpos, string mail, string content)
        {
            updata_item = new UpdataItemContent(vpos, mail, content);
        }
    }
    [JsonObject("update_thread_content")]
    public class UpdateThreadContent
    {
        [JsonProperty("count")]
        public int count { get; set; }
        [JsonProperty("premium")]
        public int premium { get; set; }
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("ticket")]
        public string ticket { get; set; }
        [JsonProperty("updatekey")]
        public string updatekey { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        public UpdateThreadContent(int count, int premium, string thread, string ticket, string updatekey, string userid)
        {
            this.count = count;
            this.premium = premium;
            this.thread = thread;
            this.ticket = ticket;
            this.updatekey = updatekey;
            this.user_id = userid;
        }
    }
    [JsonObject("update_thread")]
    public class UpdateThread
    {
        [JsonProperty("update_thread")]
        public UpdateThreadContent update_item { get; set; }
        public UpdateThread(int count, int premium, string thread, string ticket, string updatekey, string userid)
        {
            update_item = new UpdateThreadContent(count, premium, thread, ticket, updatekey, userid);
        }
    }
    [JsonObject("send_chat_content")]
    public class SendChatContent
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("vpos")]
        public int vpos { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("ticket")]
        public string ticket { get; set; }
        [JsonProperty("mail")]
        public string mail { get; set; }
        [JsonProperty("postkey")]
        public string postkey { get; set; }
        [JsonProperty("premium")]
        public string premium { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        public SendChatContent(string thread,string ticket, int vpos, string userid, string mail, string postkey, string premium, string content)
        {
            this.thread = thread;
            this.vpos = vpos;
            this.user_id = userid;
            this.mail = mail;
            this.postkey = postkey;
            this.premium = premium;
            this.content = content;
            this.ticket = ticket;
        }
    }
    [JsonObject("send_chat")]
    public class SendChat
    {
        [JsonProperty("chat")]
        public SendChatContent chat { get; set; }
        public SendChat(string thread,string ticket, int vpos, string userid, string mail, string postkey, string premium, string content)
        {
            chat = new SendChatContent(thread,ticket, vpos, userid, mail, postkey, premium, content);
        }
    }
    [JsonObject("ping_content")]
    public class PingContent
    {
        [JsonProperty("content")]
        public string content { get; set; }
        public PingContent(string c)
        {
            content = c;
        }
    }
    [JsonObject("ping")]
    public class Ping
    {
        [JsonProperty("ping")]
        public PingContent ping { get; set; }
        public Ping(string content)
        {
            ping = new PingContent(content);
        }
    }
    [JsonObject("jthread_content")]
    public class JThreadContent
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("version")]
        public string version { get; set; }
        [JsonProperty("fork")]
        public int fork { get; set; }
        [JsonProperty("language")]
        public int language { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("res_from")]
        public int res_from { get; set; }
        [JsonProperty("with_global")]
        public int with_global { get; set; }
        [JsonProperty("scores")]
        public int scores { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("userkey")]
        public string userkey { get; set; }
        public JThreadContent(string thread, int fork, string userid, string userkey, string version = "20061206")
        {
            this.thread = thread;
            this.version = version;
            this.fork = fork;
            language = 0;
            user_id = userid;
            res_from = -1000;
            with_global = 1;
            scores = 1;
            nicoru = 3;
            this.userkey = userkey;
        }
    }
    [JsonObject("jthread")]
    public class JThread
    {
        [JsonProperty("thread")]
        public JThreadContent thread { get; set; }
        public JThread(string thread, int fork, string userid, string userkey, string version = "20061206")
        {
            this.thread = new JThreadContent(thread, fork, userid, userkey, version);
        }
    }
    //
    [JsonObject("jkthread_content")]
    public class JKThreadContent
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("version")]
        public string version { get; set; }
        [JsonProperty("fork")]
        public int fork { get; set; }
        [JsonProperty("language")]
        public int language { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("res_from")]
        public int res_from { get; set; }
        [JsonProperty("with_global")]
        public int with_global { get; set; }
        [JsonProperty("scores")]
        public int scores { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("waybackkey")]
        public string waybackkey { get; set; }
        [JsonProperty("when")]
        public long when { get; set; }
        public JKThreadContent(string thread, int fork, string userid, string waybackkey,long when, string version = "20061206")
        {
            this.thread = thread;
            this.version = version;
            this.fork = fork;
            language = 0;
            user_id = userid;
            res_from = -1000;
            with_global = 1;
            scores = 1;
            nicoru = 3;
            this.waybackkey = waybackkey;
            this.when = when;
        }
    }
    [JsonObject("jkthread")]
    public class JKThread
    {
        [JsonProperty("thread")]
        public JKThreadContent thread { get; set; }
        public JKThread(string thread, int fork, string userid, string waybackkey, long when, string version = "20061206")
        {
            this.thread = new JKThreadContent(thread, fork, userid, waybackkey, when, version);
        }
    }
    [JsonObject("jthread_content_with_threadkey")]
    public class JThreadContentWithThreadKey
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("version")]
        public string version { get; set; }
        [JsonProperty("fork")]
        public int fork { get; set; }
        [JsonProperty("language")]
        public int language { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("with_global")]
        public int with_global { get; set; }
        [JsonProperty("scores")]
        public int scores { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("threadkey")]
        public string threadkey { get; set; }
        [JsonProperty("force_184")]
        public string force_184 { get; set; }
        public JThreadContentWithThreadKey(string thread, int fork, string userid, string threadkey, string force_184, string version = "20090904")
        {
            this.thread = thread;
            this.version = version;
            this.fork = fork;
            language = 0;
            user_id = userid;
            with_global = 1;
            scores = 1;
            nicoru = 3;
            this.threadkey = threadkey;
            this.force_184 = force_184;
        }
    }
    [JsonObject("jthread_with_threadkey")]
    public class JThreadWithThreadKey
    {
        [JsonProperty("thread")]
        public JThreadContentWithThreadKey thread { get; set; }
        public JThreadWithThreadKey(string thread, int fork, string userid, string threadkey, string force_184, string version = "20061206")
        {
            this.thread = new JThreadContentWithThreadKey(thread, fork, userid, threadkey, force_184, version);
        }
    }
    [JsonObject("jkthread_content_with_threadkey")]
    public class JKThreadContentWithThreadKey
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("version")]
        public string version { get; set; }
        [JsonProperty("fork")]
        public int fork { get; set; }
        [JsonProperty("language")]
        public int language { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("with_global")]
        public int with_global { get; set; }
        [JsonProperty("scores")]
        public int scores { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("threadkey")]
        public string threadkey { get; set; }
        [JsonProperty("force_184")]
        public string force_184 { get; set; }
        [JsonProperty("waybackkey")]
        public string waybackkey { get; set; }
        [JsonProperty("when")]
        public long when { get; set; }
        public JKThreadContentWithThreadKey(string thread, int fork, string userid, string threadkey, string force_184,string waybackkey, long when, string version = "20090904")
        {
            this.thread = thread;
            this.version = version;
            this.fork = fork;
            language = 0;
            user_id = userid;
            with_global = 1;
            scores = 1;
            nicoru = 3;
            this.threadkey = threadkey;
            this.force_184 = force_184;
            this.waybackkey = waybackkey;
            this.when = when;
        }
    }
    [JsonObject("jkthread_with_threadkey")]
    public class JKThreadWithThreadKey
    {
        [JsonProperty("thread")]
        public JKThreadContentWithThreadKey thread { get; set; }
        public JKThreadWithThreadKey(string thread, int fork, string userid, string threadkey, string force_184,string waybackkey, long when, string version = "20061206")
        {
            this.thread = new JKThreadContentWithThreadKey(thread, fork, userid, threadkey, force_184, waybackkey, when, version);
        }
    }
    [JsonObject("jkthread_leaf_content")]
    public class JKThreadLeavesContent
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("version")]
        public int language { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        [JsonProperty("with_global")]
        public int scores { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("waybackkey")]
        public string waybackkey { get; set; }
        [JsonProperty("when")]
        public long when { get; set; }
        public JKThreadLeavesContent(string thread, int duration, string userid, string waybackkey, long when)
        {
            this.thread = thread;
            language = 0;
            user_id = userid;
            content = "0-" + Math.Ceiling((double)duration / 60).ToString() + ":100,1000,nicoru:100";
            scores = 1;
            nicoru = 3;
            this.waybackkey = waybackkey;
            this.when = when;
        }
    }
    [JsonObject("jkthread_leaf")]
    public class JKThreadLeaves
    {
        [JsonProperty("thread_leaves")]
        public JKThreadLeavesContent thread { get; set; }
        public JKThreadLeaves(string thread, int duration, string userid, string waybackkey, long when)
        {
            this.thread = new JKThreadLeavesContent(thread, duration, userid, waybackkey, when);
        }
    }
    [JsonObject("jthread_leaf_content")]
    public class JThreadLeavesContent
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("version")]
        public int language { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        [JsonProperty("with_global")]
        public int scores { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("userkey")]
        public string userkey { get; set; }
        public JThreadLeavesContent(string thread, int duration, string userid, string userkey)
        {
            this.thread = thread;
            language = 0;
            user_id = userid;
            content = "0-" + Math.Ceiling((double)duration / 60).ToString() + ":100,1000,nicoru:100";
            scores = 1;
            nicoru = 3;
            this.userkey = userkey;
        }
    }
    [JsonObject("jthread_leaf")]
    public class JThreadLeaves
    {
        [JsonProperty("thread_leaves")]
        public JThreadLeavesContent thread { get; set; }
        public JThreadLeaves(string thread, int duration, string userid, string userkey)
        {
            this.thread = new JThreadLeavesContent(thread, duration, userid, userkey);
        }
    }
    [JsonObject("jkthread_leaf_content_with_threadkey")]
    public class JKThreadLeavesContentWithThreadKey
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("version")]
        public int language { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        [JsonProperty("with_global")]
        public int scores { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("threadkey")]
        public string threadkey { get; set; }
        [JsonProperty("force_184")]
        public string force_184 { get; set; }
        [JsonProperty("waybackkey")]
        public string waybackkey { get; set; }
        [JsonProperty("when")]
        public long when { get; set; }
        public JKThreadLeavesContentWithThreadKey(string thread, int duration, string userid, string threadkey, string force_184, string waybackkey, long when)
        {
            this.thread = thread;
            language = 0;
            user_id = userid;
            content = "0-" + Math.Ceiling((double)duration / 60).ToString() + ":100,1000,nicoru:100";
            scores = 1;
            nicoru = 3;
            this.threadkey = threadkey;
            this.force_184 = force_184;
            this.waybackkey = waybackkey;
            this.when = when;
        }
    }
    [JsonObject("jkthread_leaf_with_threadkey")]
    public class JKThreadLeavesWithThreadKey
    {
        [JsonProperty("thread_leaves")]
        public JKThreadLeavesContentWithThreadKey thread { get; set; }
        public JKThreadLeavesWithThreadKey(string thread, int duration, string userid, string threadkey, string force_184, string waybackkey, long when)
        {
            this.thread = new JKThreadLeavesContentWithThreadKey(thread, duration, userid, threadkey, force_184, waybackkey, when);
        }
    }
    [JsonObject("jthread_leaf_content_with_threadkey")]
    public class JThreadLeavesContentWithThreadKey
    {
        [JsonProperty("thread")]
        public string thread { get; set; }
        [JsonProperty("version")]
        public int language { get; set; }
        [JsonProperty("user_id")]
        public string user_id { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        [JsonProperty("with_global")]
        public int scores { get; set; }
        [JsonProperty("nicoru")]
        public int nicoru { get; set; }
        [JsonProperty("threadkey")]
        public string threadkey { get; set; }
        [JsonProperty("force_184")]
        public string force_184 { get; set; }
        public JThreadLeavesContentWithThreadKey(string thread, int duration, string userid, string threadkey, string force_184)
        {
            this.thread = thread;
            language = 0;
            user_id = userid;
            content = "0-" + Math.Ceiling((double)duration / 60).ToString() + ":100,1000,nicoru:100";
            scores = 1;
            nicoru = 3;
            this.threadkey = threadkey;
            this.force_184 = force_184;
        }
    }
    [JsonObject("jthread_leaf_with_threadkey")]
    public class JThreadLeavesWithThreadKey
    {
        [JsonProperty("thread_leaves")]
        public JThreadLeavesContentWithThreadKey thread { get; set; }
        public JThreadLeavesWithThreadKey(string thread, int duration, string userid, string threadkey, string force_184)
        {
            this.thread = new JThreadLeavesContentWithThreadKey(thread, duration, userid, threadkey, force_184);
        }
    }
    public class _Client
    {
        [JsonProperty("nicosid")]
        public string Nicosid { get; set; }

        [JsonProperty("watchId")]
        public string WatchId { get; set; }

        [JsonProperty("watchTrackId")]
        public string WatchTrackId { get; set; }
    }

    public class Server
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Keys
    {
        [JsonProperty("userKey")]
        public string UserKey { get; set; }
    }

    public class ThreadId
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fork")]
        public int Fork { get; set; }
    }

    public class Layer
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("isTranslucent")]
        public bool IsTranslucent { get; set; }

        [JsonProperty("threadIds")]
        public List<ThreadId> ThreadIds { get; set; }
    }

    public class Thread
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fork")]
        public int Fork { get; set; }

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

    public class NgScore
    {
        [JsonProperty("isDisabled")]
        public bool IsDisabled { get; set; }
    }

    public class Item
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("registeredAt")]
        public DateTime RegisteredAt { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isCategory")]
        public bool IsCategory { get; set; }

        [JsonProperty("isCategoryCandidate")]
        public bool IsCategoryCandidate { get; set; }

        [JsonProperty("isNicodicArticleExists")]
        public bool IsNicodicArticleExists { get; set; }

        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }
    }

    public class Viewer
    {
        [JsonProperty("revision")]
        public int Revision { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("isFollowing")]
        public bool IsFollowing { get; set; }

        [JsonProperty("isEditable")]
        public bool IsEditable { get; set; }

        [JsonProperty("uneditableReason")]
        public object UneditableReason { get; set; }

        [JsonProperty("editKey")]
        public string EditKey { get; set; }

        [JsonProperty("isOwner")]
        public bool IsOwner { get; set; }

        [JsonProperty("like")]
        public Like Like { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("isPremium")]
        public bool IsPremium { get; set; }

        [JsonProperty("existence")]
        public Existence Existence { get; set; }
    }

    public class Ng
    {
        [JsonProperty("ngScore")]
        public NgScore NgScore { get; set; }

        [JsonProperty("channel")]
        public List<object> Channel { get; set; }

        [JsonProperty("owner")]
        public List<object> Owner { get; set; }

        [JsonProperty("viewer")]
        public Viewer Viewer { get; set; }
    }

    public class Comment
    {
        [JsonProperty("server")]
        public Server Server { get; set; }

        [JsonProperty("keys")]
        public Keys Keys { get; set; }

        [JsonProperty("layers")]
        public List<Layer> Layers { get; set; }

        [JsonProperty("threads")]
        public List<Thread> Threads { get; set; }

        [JsonProperty("ng")]
        public Ng Ng { get; set; }

        [JsonProperty("isAttentionRequired")]
        public bool IsAttentionRequired { get; set; }

        [JsonProperty("isDefaultInvisible")]
        public bool IsDefaultInvisible { get; set; }
    }

    public class Nicodic
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("viewTitle")]
        public string ViewTitle { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }

    public class Phras
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("nicodic")]
        public Nicodic Nicodic { get; set; }
    }

    public class EasyComment
    {
        [JsonProperty("phrases")]
        public List<Phras> Phrases { get; set; }
    }

    public class Commons
    {
        [JsonProperty("hasContentTree")]
        public bool HasContentTree { get; set; }
    }

    public class Ichiba
    {
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }
    }

    public class External
    {
        [JsonProperty("commons")]
        public Commons Commons { get; set; }

        [JsonProperty("ichiba")]
        public Ichiba Ichiba { get; set; }
    }

    public class Genre
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("isImmoral")]
        public bool IsImmoral { get; set; }

        [JsonProperty("isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonProperty("isNotSet")]
        public bool IsNotSet { get; set; }
    }

    public class Marquee
    {
        [JsonProperty("isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonProperty("tagRelatedLead")]
        public object TagRelatedLead { get; set; }
    }

    public class Loudness
    {
        [JsonProperty("integratedLoudness")]
        public double IntegratedLoudness { get; set; }

        [JsonProperty("truePeak")]
        public double TruePeak { get; set; }
    }

    public class LoudnessCollection
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        [JsonProperty("samplingRate")]
        public int SamplingRate { get; set; }

        [JsonProperty("loudness")]
        public Loudness Loudness { get; set; }

        [JsonProperty("levelIndex")]
        public int LevelIndex { get; set; }

        [JsonProperty("loudnessCollection")]
        public List<LoudnessCollection> LoudnessCollection { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("resolution")]
        public Resolution Resolution { get; set; }

        [JsonProperty("recommendedHighestAudioLevelIndex")]
        public int RecommendedHighestAudioLevelIndex { get; set; }
    }

    public class Audio
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isAvailable")]
        public bool IsAvailable { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class Resolution
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class Video
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isAvailable")]
        public bool IsAvailable { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class AuthTypes
    {
        [JsonProperty("http")]
        public string Http { get; set; }

        [JsonProperty("hls")]
        public string Hls { get; set; }

        [JsonProperty("storyboard")]
        public string Storyboard { get; set; }
    }

    public class Url
    {
        [JsonProperty("url")]
        public string _Url { get; set; }

        [JsonProperty("isWellKnownPort")]
        public bool IsWellKnownPort { get; set; }

        [JsonProperty("isSsl")]
        public bool IsSsl { get; set; }
    }

    public class Session
    {
        [JsonProperty("recipeId")]
        public string RecipeId { get; set; }

        [JsonProperty("playerId")]
        public string PlayerId { get; set; }

        [JsonProperty("videos")]
        public List<string> Videos { get; set; }

        [JsonProperty("audios")]
        public List<string> Audios { get; set; }

        [JsonProperty("movies")]
        public List<object> Movies { get; set; }

        [JsonProperty("protocols")]
        public List<string> Protocols { get; set; }

        [JsonProperty("authTypes")]
        public AuthTypes AuthTypes { get; set; }

        [JsonProperty("serviceUserId")]
        public string ServiceUserId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("contentId")]
        public string ContentId { get; set; }

        [JsonProperty("heartbeatLifetime")]
        public int HeartbeatLifetime { get; set; }

        [JsonProperty("contentKeyTimeout")]
        public int ContentKeyTimeout { get; set; }

        [JsonProperty("priority")]
        public double Priority { get; set; }

        [JsonProperty("transferPresets")]
        public List<string> TransferPresets { get; set; }

        [JsonProperty("urls")]
        public List<Url> Urls { get; set; }
    }

    public class Movie
    {
        [JsonProperty("contentId")]
        public string ContentId { get; set; }

        [JsonProperty("audios")]
        public List<Audio> Audios { get; set; }

        [JsonProperty("videos")]
        public List<Video> Videos { get; set; }

        [JsonProperty("session")]
        public Session Session { get; set; }
    }

    public class Image
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class Storyboard
    {
        [JsonProperty("contentId")]
        public string ContentId { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("session")]
        public Session Session { get; set; }
    }

    public class Delivery
    {
        [JsonProperty("recipeId")]
        public string RecipeId { get; set; }

        [JsonProperty("encryption")]
        public object Encryption { get; set; }

        [JsonProperty("movie")]
        public Movie Movie { get; set; }

        [JsonProperty("storyboard")]
        public Storyboard Storyboard { get; set; }

        [JsonProperty("trackingId")]
        public string TrackingId { get; set; }
    }

    public class Media
    {
        [JsonProperty("delivery")]
        public Delivery Delivery { get; set; }

        [JsonProperty("deliveryLegacy")]
        public object DeliveryLegacy { get; set; }
    }

    public class Owner
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("channel")]
        public object Channel { get; set; }

        [JsonProperty("live")]
        public object Live { get; set; }

        [JsonProperty("isVideosPublic")]
        public bool IsVideosPublic { get; set; }

        [JsonProperty("isMylistsPublic")]
        public bool IsMylistsPublic { get; set; }

        [JsonProperty("viewer")]
        public Viewer Viewer { get; set; }
    }

    public class VideoEnd
    {
        [JsonProperty("bannerIn")]
        public object BannerIn { get; set; }

        [JsonProperty("overlay")]
        public object Overlay { get; set; }
    }

    public class PcWatchPage
    {
        [JsonProperty("tagRelatedBanner")]
        public object TagRelatedBanner { get; set; }

        [JsonProperty("videoEnd")]
        public VideoEnd VideoEnd { get; set; }

        [JsonProperty("showOwnerMenu")]
        public bool ShowOwnerMenu { get; set; }

        [JsonProperty("showOwnerThreadCoEditingLink")]
        public bool ShowOwnerThreadCoEditingLink { get; set; }

        [JsonProperty("showMymemoryEditingLink")]
        public bool ShowMymemoryEditingLink { get; set; }
    }

    public class InitialPlayback
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("positionSec")]
        public decimal PositionSec { get; set; }
    }

    public class Player
    {
        [JsonProperty("initialPlayback")]
        public InitialPlayback InitialPlayback { get; set; }

        [JsonProperty("comment")]
        public Comment Comment { get; set; }

        [JsonProperty("layerMode")]
        public int LayerMode { get; set; }
    }

    public class Ranking
    {
        [JsonProperty("genre")]
        public object Genre { get; set; }

        [JsonProperty("popularTag")]
        public List<object> PopularTag { get; set; }
    }

    public class _System
    {
        [JsonProperty("serverTime")]
        public DateTime ServerTime { get; set; }

        [JsonProperty("isPeakTime")]
        public bool IsPeakTime { get; set; }
    }

    public class Edit
    {
        [JsonProperty("isEditable")]
        public bool IsEditable { get; set; }

        [JsonProperty("uneditableReason")]
        public object UneditableReason { get; set; }

        [JsonProperty("editKey")]
        public string EditKey { get; set; }
    }

    public class Tag
    {
        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("hasR18Tag")]
        public bool HasR18Tag { get; set; }

        [JsonProperty("isPublishedNicoscript")]
        public bool IsPublishedNicoscript { get; set; }

        [JsonProperty("edit")]
        public Edit Edit { get; set; }

        [JsonProperty("viewer")]
        public Viewer Viewer { get; set; }
    }

    public class Count
    {
        [JsonProperty("view")]
        public int View { get; set; }

        [JsonProperty("comment")]
        public int Comment { get; set; }

        [JsonProperty("mylist")]
        public int Mylist { get; set; }

        [JsonProperty("like")]
        public int Like { get; set; }
    }

    public class Thumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("middleUrl")]
        public object MiddleUrl { get; set; }

        [JsonProperty("largeUrl")]
        public object LargeUrl { get; set; }

        [JsonProperty("player")]
        public string Player { get; set; }

        [JsonProperty("ogp")]
        public string Ogp { get; set; }
    }

    public class Rating
    {
        [JsonProperty("isAdult")]
        public bool IsAdult { get; set; }
    }

    public class Like
    {
        [JsonProperty("isLiked")]
        public bool IsLiked { get; set; }

        [JsonProperty("count")]
        public object Count { get; set; }
    }

    public class Video2
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

        [JsonProperty("viewer")]
        public Viewer Viewer { get; set; }

        [JsonProperty("watchableUserTypeForPayment")]
        public string WatchableUserTypeForPayment { get; set; }

        [JsonProperty("commentableUserTypeForPayment")]
        public string CommentableUserTypeForPayment { get; set; }

        [JsonProperty("9d091f87")]
        public bool _9d091f87 { get; set; }
    }

    public class AdditionalParams
    {
        [JsonProperty("videoId")]
        public string VideoId { get; set; }

        [JsonProperty("videoDuration")]
        public int VideoDuration { get; set; }

        [JsonProperty("isAdultRatingNG")]
        public bool IsAdultRatingNG { get; set; }

        [JsonProperty("isAuthenticationRequired")]
        public bool IsAuthenticationRequired { get; set; }

        [JsonProperty("isR18")]
        public bool IsR18 { get; set; }

        [JsonProperty("nicosid")]
        public string Nicosid { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }

        [JsonProperty("watchTrackId")]
        public string WatchTrackId { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }
    }

    public class VideoAds
    {
        [JsonProperty("additionalParams")]
        public AdditionalParams AdditionalParams { get; set; }

        [JsonProperty("items")]
        public List<object> Items { get; set; }

        [JsonProperty("reason")]
        public object Reason { get; set; }
    }

    public class Existence
    {
        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("prefecture")]
        public string Prefecture { get; set; }

        [JsonProperty("sex")]
        public string Sex { get; set; }
    }

    public class TagRelatedBanner
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isEvent")]
        public bool IsEvent { get; set; }

        [JsonProperty("linkUrl")]
        public string LinkUrl { get; set; }

        [JsonProperty("isNewWindow")]
        public bool IsNewWindow { get; set; }
    }

    public class TagRelatedMarquee
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("linkUrl")]
        public string LinkUrl { get; set; }

        [JsonProperty("isNewWindow")]
        public bool IsNewWindow { get; set; }
    }

    public class Waku
    {
        [JsonProperty("information")]
        public object Information { get; set; }

        [JsonProperty("bgImages")]
        public List<object> BgImages { get; set; }

        [JsonProperty("addContents")]
        public object AddContents { get; set; }

        [JsonProperty("addVideo")]
        public object AddVideo { get; set; }

        [JsonProperty("tagRelatedBanner")]
        public TagRelatedBanner TagRelatedBanner { get; set; }

        [JsonProperty("tagRelatedMarquee")]
        public TagRelatedMarquee TagRelatedMarquee { get; set; }
    }
    [JsonObject("WatchAPIData")]
    public class WatchAPIData
    {
        [JsonProperty("ads")]
        public object Ads { get; set; }

        [JsonProperty("category")]
        public object Category { get; set; }

        [JsonProperty("channel")]
        public object Channel { get; set; }

        [JsonProperty("client")]
        public _Client Client { get; set; }

        [JsonProperty("comment")]
        public Comment Comment { get; set; }

        [JsonProperty("community")]
        public object Community { get; set; }

        [JsonProperty("easyComment")]
        public EasyComment EasyComment { get; set; }

        [JsonProperty("external")]
        public External External { get; set; }

        [JsonProperty("genre")]
        public Genre Genre { get; set; }

        [JsonProperty("marquee")]
        public Marquee Marquee { get; set; }

        [JsonProperty("media")]
        public Media Media { get; set; }

        [JsonProperty("okReason")]
        public string OkReason { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("pcWatchPage")]
        public PcWatchPage PcWatchPage { get; set; }

        [JsonProperty("player")]
        public Player Player { get; set; }

        [JsonProperty("ppv")]
        public object Ppv { get; set; }

        [JsonProperty("ranking")]
        public Ranking Ranking { get; set; }

        [JsonProperty("series")]
        public object Series { get; set; }

        [JsonProperty("smartphone")]
        public object Smartphone { get; set; }

        [JsonProperty("system")]
        public _System System { get; set; }

        [JsonProperty("tag")]
        public Tag Tag { get; set; }

        [JsonProperty("video")]
        public Video2 Video { get; set; }

        [JsonProperty("videoAds")]
        public VideoAds VideoAds { get; set; }

        [JsonProperty("viewer")]
        public Viewer Viewer { get; set; }

        [JsonProperty("waku")]
        public Waku Waku { get; set; }
    }
}