using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace NicoCommentTransfer.API
{
    public class NicoOAuth
    {
        public string auth_token = "";
        public NicoOAuth(string auth_token)
        {
            this.auth_token = auth_token.Replace("\"", "");
        }

        private string getRequest(string URI, string parameters, string type = "POST", CookieContainer coookie = null, Dictionary<string, string> header = null, string referer = null, string accept = "*/*")
        {
            reqStart:
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URI);
                req.Accept = accept;
                req.Method = type;
                req.KeepAlive = true;
                req.ContentType = "application/x-www-form-urlencoded";
                req.UserAgent = "NicoCommentTransfer@Negima1072";
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
                req.Headers.Add("Authorization", "Bearer " + auth_token);
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
            catch (WebException e)
            {
                refresh();
                goto reqStart;
            }
        }

        public UserOpenIDInfo getOwnInfo()
        {
            string res = getRequest("https://oauth.nicovideo.jp/open_id/userinfo", "", "GET");
            return JsonConvert.DeserializeObject<UserOpenIDInfo>(res);
        }

        public AbcOAuthResponse<PremiumData> getOwnPremium()
        {
            string res = getRequest("https://oauth.nicovideo.jp/v1/user/premium.json", "", "GET");
            return JsonConvert.DeserializeObject<AbcOAuthResponse<PremiumData>>(res);
        }

        public void refresh()
        {
            string res = getRequest("https://nct.nvcomment.net/api/v1/refresh?token=" + auth_token, "", "GET");
            Console.WriteLine(res);
        }
        
    }

    [JsonObject("userinfo")]
    public class UserOpenIDInfo
    {
        [JsonProperty("sub")]
        public string Sub { get; set; }
        [JsonProperty("nickname")]
        public string Nickname { get; set; }
        [JsonProperty("profile")]
        public string Profile { get; set; }
        [JsonProperty("picture")]
        public string Picture { get; set; }
        [JsonProperty("gender")]
        public string Gender { get; set; }
        [JsonProperty("zoneinfo")]
        public string Zoneinfo { get; set; }
        [JsonProperty("updated_at")]
        public int UpdatedAt { get; set; }
    }

    public class AbcOAuthResponse<T>
    {
        [JsonProperty("meta")]
        public OAuthMeta Meta { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
    }

    public class OAuthMeta
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
    }

    public class PremiumData
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("expireTime")]
        public DateTime ExpireTime { get; set; }
    }
}