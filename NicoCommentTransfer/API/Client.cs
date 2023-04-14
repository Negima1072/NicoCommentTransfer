using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Mozilla;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
            var res = getReq(new Uri("https://account.nicovideo.jp/login/redirector?site=niconico&sec=header_pc&next_url=%2F"), new Dictionary<string, string>{ { "mail_tel", email }, {"password",password } });
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
            var res = getReq(new Uri("https://www.nicovideo.jp/"), type:Method.Get);
            if (!res.Contains("メールアドレスまたはパスワードが間違っています。"))
            {
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(res);
                JObject jsonObj = JObject.Parse(doc.GetElementById("CommonHeader").GetAttribute("data-common-header"));
                Console.WriteLine(jsonObj.ToString());
                isLogin = bool.Parse(jsonObj["initConfig"]["user"]["isLogin"].ToString());
                if (bool.Parse(jsonObj["initConfig"]["user"]["isLogin"].ToString()))
                {
                    userID = jsonObj["initConfig"]["user"]["id"].ToString();
                    isPremium = bool.Parse(jsonObj["initConfig"]["user"]["isPremium"].ToString());
                    imgUrl = Regex.Unescape(jsonObj["initConfig"]["user"]["iconUrl"].ToString());
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
                string res = getReq(new Uri("https://api.github.com/repos/Negima1072/NicoCommentTransfer/releases/latest"), type:Method.Get);
                string newversion = ((string)JsonConvert.DeserializeObject<JObject>(res)["name"]).Substring(1);
                string newversionstr = (string)JsonConvert.DeserializeObject<JObject>(res)["body"];
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
            string res = getReq(new Uri(url), type:Method.Get);
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
        public RestResponse getReqV2(Uri URI, Dictionary<string, string> parameters = null, string jsonStr = null, Method method = Method.Get, CookieContainer cookie = null, Dictionary<string, string> headers = null, string referer = null, string accept = "*/*" )
        {
            var client = new RestClient(URI.Scheme + "://" + URI.Host);
            var request = new RestRequest(URI.PathAndQuery);
            request.Method = method;
            if(parameters != null)
            {
                foreach(KeyValuePair<string, string> k in parameters)
                {
                    request.AddParameter(k.Key, k.Value, ParameterType.GetOrPost);
                }
            }
            if(jsonStr != null)
            {
                request.AddParameter("application/json", jsonStr, ParameterType.RequestBody);
            }
            if(cookie == null)
            {
                request.CookieContainer = _cookie;
                /*foreach (Cookie c in _cookie.GetCookies(URI))
                {
                    request.CookieContainer.Add(c);
                }*/
            }
            else
            {
                request.CookieContainer = _cookie;
                /*foreach (Cookie c in cookie.GetCookies(URI))
                {
                    request.CookieContainer.Add(c);
                }*/
            }
            request.AddHeader("ContentType", "application/x-www-form-urlencoded");
            request.AddHeader("User-Agent", "NicoCommentTransfter@Negima1072");
            request.AddHeader("X-Frontend-Id", "6");
            request.AddHeader("X-Frontend-Version", "0");
            request.AddHeader("X-Request-With", "https://www.nicovideo.jp");
            if (headers != null) request.AddHeaders(headers);
            if (referer != null) request.AddHeader("Referer", referer);
            if (accept != null) request.AddHeader("Accept", accept);
            RestResponse response = client.Execute(request);
            foreach(Cookie c in request.CookieContainer.GetCookies(URI))
            {
                _cookie.Add(c);
            }
            return response;
        }
        public string getReq(Uri URI, Dictionary<string, string> parameters = null, Method type = Method.Post, CookieContainer coookie = null, Dictionary<string, string> header = null, string referer = null, string accept = "*/*")
        {
            RestResponse response = getReqV2(URI, parameters, null, type, coookie, header, referer, accept);
            return response.Content;
        }
        public string getReqWithJson(Uri URI, string json, Dictionary<string, string> header = null, string referer = null, Method type = Method.Post)
        {
            RestResponse response = getReqV2(URI, null, json, type, null, header, referer, "application/json");
            return response.Content;
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
        public string getReqWithForm(Uri URI, Dictionary<string, string> param, string referer, Dictionary<string, string> header = null)
        {
            RestResponse response = getReqV2(URI, param, null, Method.Post, null, header, referer, "application/json");
            return response.Content;
        }
    }
}
