using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace NicoCommentTransfer.API
{
    public class Client
    {
        public string userID = "0";
        public string userName = "";
        public bool isPremium = false;
        public string imgUrl = "";
        public static CookieContainer _cookie = new CookieContainer();
        public long cookieExpires = 0;
        public bool isLogin = false;
        public bool isCommunityFollower = false;
        public Client()
        {
        }
        public Client(bool eml, string email, string password)
        {
            Login(email, password);
        }
        public Client(string session, string secure)
        {
            LoginCookie(session, secure);
        }
        public bool Login(string email, string password)
        {
            var res = getReq("https://secure.nicovideo.jp/secure/login?site=niconico", "mail=" + email + "&password=" + password);
            if (!res.Contains("メールアドレスまたはパスワードが間違っています。"))
            {
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(res);
                JObject jsonObj = JObject.Parse(doc.GetElementById("CommonHeader").GetAttribute("data-common-header"));
                isLogin = bool.Parse(jsonObj["initConfig"]["user"]["isLogin"].ToString());
                if (bool.Parse(jsonObj["initConfig"]["user"]["isLogin"].ToString()))
                {
                    userID = jsonObj["initConfig"]["user"]["id"].ToString();
                    isPremium = bool.Parse(jsonObj["initConfig"]["user"]["isPremium"].ToString());
                    imgUrl = jsonObj["initConfig"]["user"]["iconUrl"].ToString();
                    userName = jsonObj["initConfig"]["user"]["nickname"].ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool LoginCookie(string user_session, string user_session_secure)
        {
            _cookie = new CookieContainer();
            _cookie.Add(new Cookie("user_session", user_session, "", ".nicovideo.jp"));
            _cookie.Add(new Cookie("user_session_secure", user_session_secure, "", ".nicovideo.jp"));
            var res = getReq("https://www.nicovideo.jp/", "a=a");
            if (!res.Contains("メールアドレスまたはパスワードが間違っています。"))
            {
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(res);
                JObject jsonObj = JObject.Parse(doc.GetElementById("CommonHeader").GetAttribute("data-common-header"));
                isLogin = bool.Parse(jsonObj["initConfig"]["user"]["isLogin"].ToString());
                if (bool.Parse(jsonObj["initConfig"]["user"]["isLogin"].ToString()))
                {
                    userID = jsonObj["initConfig"]["user"]["id"].ToString();
                    isPremium = bool.Parse(jsonObj["initConfig"]["user"]["isPremium"].ToString());
                    imgUrl = jsonObj["initConfig"]["user"]["iconUrl"].ToString();
                    userName = jsonObj["initConfig"]["user"]["nickname"].ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool checkIsLatestVersion()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                AssemblyName asmName = assembly.GetName();
                string version = asmName.Version.ToString();
                string res = getReq("https://ngmsrv.com/api/nct/version.json", "", "GET");
                string newversion = (string)JsonConvert.DeserializeObject<JObject>(res)["data"]["version"];
                string newversionstr = (string)JsonConvert.DeserializeObject<JObject>(res)["data"]["str"];
                if (version != newversion)
                {
                    MessageBox.Show("使用中のバージョンが最新ではありません。配布サイトから更新をしてください。\n\n最新バージョン:" + newversion + "\n\n更新内容: " + newversionstr, "VersionChecker", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }catch(Exception e)
            {
                MessageBox.Show("バージョン情報を取得できませんでした。" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        public bool checkIsCommunityFollower()
        {
            string url = "https://com.nicovideo.jp/api/v1/communities/5033742/authority.json";
            string res = getReq(url, "", "GET");
            isCommunityFollower = bool.Parse((string)JsonConvert.DeserializeObject<JObject>(res)["data"]["is_member"]);
            return isCommunityFollower;
        }
        public static Dictionary<string, string> QueryToDictionary(CookieCollection query)
        {
            Dictionary<string, string> r = new Dictionary<string, string>();
            foreach (Cookie c in query)
            {
                r[c.Name] = c.Value;
            }
            return r;
        }
        public string[] getLoginCookies()
        {
            Dictionary<string, string> dic = QueryToDictionary(_cookie.GetCookies(new Uri("http://nicovideo.jp")));
            return new string[] { dic["user_session"], dic["user_session_secure"] };
        }
        public long getLoginCookieExpires()
        {
            cookieExpires = BrowserCookieGetter.ToUnixTime(_cookie.GetCookies(new Uri("http://nicovideo.jp"))[0].Expires);
            return cookieExpires;
        }
        public string getReq(string URI, string parameters, string type = "POST", CookieContainer coookie = null, Dictionary<string, string> header = null, string referer = null, string accept = "*/*")
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URI);
            if (coookie != null)
            {
                _cookie = coookie;
                req.CookieContainer = coookie;
            }
            else
            {
                req.CookieContainer = _cookie;
            }
            req.Accept = accept;
            req.Method = type;
            req.KeepAlive = true;
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.193 Safari/537.36";
            if (referer != null)
            {
                req.Referer = referer;
            }
            if (header != null)
            {
                foreach (KeyValuePair<string, string> k in header)
                {
                    req.Headers.Add(k.Key, k.Value);
                }
            }
            byte[] postDataBytes = System.Text.Encoding.ASCII.GetBytes(parameters);
            req.ContentLength = postDataBytes.Length;
            if (type == "POST")
            {
                Stream rdat = req.GetRequestStream();
                StreamWriter sw = new StreamWriter(rdat);
                sw.Write(parameters);
                sw.Close();
                rdat.Close();
            }
            Console.WriteLine(req.Headers.ToString());
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream data = resp.GetResponseStream();
            StreamReader reader = new StreamReader(data, Encoding.UTF8);
            string s = reader.ReadToEnd();
            data.Close();
            resp.Close();
            reader.Close();
            return s;
        }
        public string getReqWithJson(string URI, string json, Dictionary<string, string> header = null, string referer = null)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URI);
            req.CookieContainer = _cookie;
            req.Accept = "*/*";
            req.Method = "POST";
            req.KeepAlive = true;
            req.ContentType = "application/json";
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36";
            if (header != null)
            {
                foreach (KeyValuePair<string, string> k in header)
                {
                    req.Headers.Add(k.Key, k.Value);
                }
            }
            if(referer != null)
            {
                req.Referer = referer;
            }
            Stream rdat = req.GetRequestStream();
            StreamWriter sw = new StreamWriter(rdat);
            sw.Write(json);
            sw.Close();
            Console.WriteLine(req.Headers.ToString());
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream data = resp.GetResponseStream();
            StreamReader reader = new StreamReader(data, Encoding.UTF8);
            string s = reader.ReadToEnd();
            data.Close();
            resp.Close();
            reader.Close();
            rdat.Close();
            return s;
        }
        public JToken getReqWithByte(string URI, byte[] data, string referer, string filename, string uuid)
        {
            try
            {
                string s = "";
                string boundary = "----WebKitFormBoundary" + System.Environment.TickCount.ToString();
                //10MBgoto
                int remain = data.Length;
                int position = 0;
                int split;
                List<byte[]> list = new List<byte[]>();
                while (remain > 0)
                {
                    split = remain < 10000000 ? remain : 10000000;
                    remain -= split;
                    byte[] bs240 = new byte[split];
                    Array.Copy(data, position, bs240, 0, split);
                    list.Add(bs240);
                    position += split;
                }
                //
                for (int i = 0; i < list.Count; i++)
                {
                    string postData = "";
                    postData = "--" + boundary + "\r\n" +
                        "Content-Disposition: form-data; name=\"qqpartindex\"\r\n\r\n" +
                        i.ToString() + "\r\n" +
                        "--" + boundary + "\r\n" +
                        "Content-Disposition: form-data; name=\"qqpartbyteoffset\"\r\n\r\n" +
                        data.Length.ToString() + "\r\n" +
                        "--" + boundary + "\r\n" +
                        "Content-Disposition: form-data; name=\"qqchunksize\"\r\n\r\n" +
                        list[i].Length.ToString() + "\r\n" +
                        "--" + boundary + "\r\n" +
                        "Content-Disposition: form-data; name=\"qqtotalparts\"\r\n\r\n" +
                        list.Count.ToString() + "\r\n" +
                        "--" + boundary + "\r\n" +
                        "Content-Disposition: form-data; name=\"qqtotalfilesize\"\r\n\r\n" +
                        data.Length.ToString() + "\r\n" +
                        "--" + boundary + "\r\n" +
                        "Content-Disposition: form-data; name=\"qqfilename\"\r\n\r\n" +
                        filename + "\r\n" +
                        "--" + boundary + "\r\n" +
                        "Content-Disposition: form-data; name=\"qquuid\"\r\n\r\n" +
                        uuid + "\r\n" +
                        "--" + boundary + "\r\n" +
                        "Content-Disposition: form-data; name=\"qqfile\"; filename=\"" +
                            "blob" + "\"\r\n" +
                        "Content-Type: application/octet-stream\r\n\r\n";
                    //バイト型配列に変換
                    System.Text.Encoding enc = System.Text.Encoding.GetEncoding("shift_jis");
                    byte[] startData = enc.GetBytes(postData);
                    postData = "\r\n--" + boundary + "--\r\n";
                    byte[] endData = enc.GetBytes(postData);
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(URI);
                    webRequest.Method = "POST";
                    webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                    webRequest.ContentLength = list[i].Length + startData.Length + endData.Length;
                    webRequest.CookieContainer = _cookie;
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.193 Safari/537.36";
                    webRequest.Headers.Add("X-Request-With", "N-garage");
                    webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    webRequest.Referer = referer;
                    using (Stream postStream = webRequest.GetRequestStream())
                    {
                        postStream.Write(startData, 0, startData.Length);
                        postStream.Write(list[i], 0, list[i].Length);
                        postStream.Write(endData, 0, endData.Length);
                        postStream.Close();
                        webRequest.Headers.ToString();
                        HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                        Stream res = resp.GetResponseStream();
                        StreamReader reader = new StreamReader(res, Encoding.UTF8);
                        s = reader.ReadToEnd();
                        res.Close();
                        resp.Close();
                        reader.Close();
                    }
                }
                JToken rt = JsonConvert.DeserializeObject<JToken>(s);
                rt["totalChunk"] = list.Count.ToString();
                return rt;
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        public string getReqWithForm(string URI, string param, string referer, Dictionary<string, string> header = null)
        {
            string s = "";
            // qquuid = 32d226b6 - 86a2 - 4028 - 95ee - 56a4031e1fee & qqfilename = ba.mp4 & qqtotalfilesize = 22099168 & qqtotalparts = 3
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(URI);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.CookieContainer = _cookie;
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.193 Safari/537.36";
                foreach(var h in header)
                {
                    webRequest.Headers.Add(h.Key, h.Value);
                }
                webRequest.Referer = referer;
                webRequest.Accept = "application/json";
                using (Stream postStream = webRequest.GetRequestStream())
                {
                    postStream.Write(Encoding.ASCII.GetBytes(param), 0, Encoding.ASCII.GetBytes(param).Length);
                    postStream.Close();
                    Console.WriteLine(webRequest.Headers.ToString());
                    WebResponse response = webRequest.GetResponse();
                    Stream resStream = response.GetResponseStream();
                    Encoding encode = Encoding.GetEncoding("utf-8");
                    StreamReader reader = new StreamReader(resStream, encode);
                    Console.WriteLine(reader.ReadToEnd());
                    reader.Close();
                }
                return s;
            }catch(Exception e)
            {
                throw new Exception("getReqWithForm", e);
            }
        }
    }
}
