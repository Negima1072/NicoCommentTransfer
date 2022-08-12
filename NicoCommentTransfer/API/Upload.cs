using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NicoCommentTransfer.API
{
    class Upload
    {
        private int videoid = 0;
        private List<MKCommunity> communities;
        private byte[] videoBytes;
        private int createPostRequest(Client client)
        {
            try
            {
                string res = client.getReq("https://www.upload.nicovideo.jp/v2/videos", "", "POST", header: new Dictionary<string, string>() {
                    {"Accept-Language","ja,en-US;q=0.7,en;q=0.3" },
                    {"X-Frontend-Id","23" },
                    {"X-Frontend-Version","1.0.0" },
                    {"X-Request-With","N-garage" }
                }, referer: "https://www.upload.nicovideo.jp/garage/upload");
                JToken jres = JsonConvert.DeserializeObject<JToken>(res);
                if(jres["meta"]["status"].ToString() == "201")
                {
                    videoid = int.Parse(jres["data"]["id"].ToString());
                    return videoid;
                }
                else
                {
                    throw new Exception("Error: createPostRequest " + jres["meta"]["status"].ToString() + "ERROR");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string getUploadChunkStream(Client client)
        {
            string res = client.getReq("https://www.upload.nicovideo.jp/v2/videos/" + videoid.ToString() + "/upload-chunk-stream", "", "POST", header: new Dictionary<string, string>() {
                    {"Accept-Language","ja,en-US;q=0.7,en;q=0.3" },
                    {"X-Frontend-Id","23" },
                    {"X-Frontend-Version","1.0.0" },
                    {"X-Request-With","N-garage" }
                }, referer: "https://www.upload.nicovideo.jp/garage/videos/" + videoid.ToString() + "/input");
            JToken jres = JsonConvert.DeserializeObject<JToken>(res);
            if (jres["meta"]["status"].ToString() == "201")
            {
                string url = jres["data"]["url"].ToString();
                return url;
            }
            else
            {
                throw new Exception("Error: getUploadChunkStream " + jres["meta"]["status"].ToString() + "ERROR");
            }
        }
        private string getMyData(Client client)
        {
            string res = client.getReq("https://www.upload.nicovideo.jp/v2/users/me", "", "GET", header: new Dictionary<string, string>() {
                    {"Accept-Language","ja,en-US;q=0.7,en;q=0.3" },
                    {"X-Frontend-Id","23" },
                    {"X-Frontend-Version","1.0.0" },
                    {"X-Request-With","N-garage" }
                }, referer: "https://www.upload.nicovideo.jp/garage/videos/" + videoid.ToString() + "/input");
            JToken jres = JsonConvert.DeserializeObject<JToken>(res);
            if (jres["meta"]["status"].ToString() == "200")
            {
                communities = jres["data"]["communities"].ToObject<List<MKCommunity>>();
                string name = jres["data"]["username"].ToString();
                return name;
            }
            else
            {
                throw new Exception("Error: getUploadChunkStream " + jres["meta"]["status"].ToString() + "ERROR");
            }
        }
        private int makeVideo(string len, string color)
        {
            string jsons = JsonConvert.SerializeObject(new MKTemp(len, color));
            StreamWriter sw = new StreamWriter("./mv/tmp.json", false, Encoding.UTF8);
            sw.Write(jsons);
            sw.Close();
            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = System.IO.Path.GetFullPath("mv");
            info.FileName = "makeVideo.exe";
            Process proc = Process.Start(info);
            proc.WaitForExit();
            videoBytes = File.ReadAllBytes("./mv/tmp.mp4");
            return 0;
        }
        private int uploadVideoWithChunkStream(Client client, string url)
        {
            try
            {
                string uuid = Guid.NewGuid().ToString("D");
                JToken jres = client.getReqWithByte(url, videoBytes, "https://www.upload.nicovideo.jp/garage/videos/" + videoid.ToString() + "/input", "tmp.mp4", uuid);
                if (true)
                {
                    JToken jo = postChunkDone(client, url + "?done", uuid, "tmp.mp4", videoBytes.Length.ToString(), jres["totalChunk"].ToString());
                    if (true)
                    {
                        return 0;
                    }
                    else
                    {
                        throw new Exception("Error: uploadVideoWithChunkStream-postChunkDone\n" + jo.ToString());
                    }
                }
                else
                {
                    throw new Exception("Error: uploadVideoWithChunkStreamNotSuccess\n" + jres.ToString());
                }
            }
            catch(Exception e)
            {
                throw new Exception("Error: uploadVideoWithChunkStream:u112", e);
            }
        }
        private JToken postChunkDone(Client client, string url, string uuid, string filename, string filesize, string totalpart)
        {
            try
            {
                Console.WriteLine(url);
                string param = "qquuid=" + uuid + "&qqfilename=" + filename + "&qqtotalfilesize=" + filesize + "&qqtotalparts=" + totalpart;
                Console.WriteLine(param);
                string res = client.getReqWithForm(url, param, header: new Dictionary<string, string>() {
                    {"Accept-Encoding" ,"gzip, deflate, br"},
                    {"Accept-Language" ,"ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7"},
                    {"Sec-Fetch-Dest","empty"},
                    {"Sec-Fetch-Mode","cors"},
                    {"Sec-Fetch-Site","same-origin" },
                    {"X-Request-With","N-garage" }
                }, referer: "https://www.upload.nicovideo.jp/garage/videos/" + videoid.ToString() + "/input");
                JToken jres = JsonConvert.DeserializeObject<JToken>(res);
                Console.WriteLine(res);
                //Console.WriteLine(jres["success"].ToString());
                return jres;
            }
            catch(Exception e)
            {
                throw new Exception("Error: postChunkDone", e);
            }
        }
        private JToken getStatusJson(Client client)
        {
            try
            {
                string res = client.getReq("https://www.upload.nicovideo.jp/v2/videos/" + videoid.ToString() + "/status", "", "GET", header: new Dictionary<string, string>() {
                    {"Accept-Language","ja,en-US;q=0.7,en;q=0.3" },
                    {"X-Frontend-Id","23" },
                    {"X-Frontend-Version","1.0.0" },
                    {"X-Request-With","N-garage" }
                }, referer: "https://www.upload.nicovideo.jp/garage/videos/" + videoid.ToString() + "/input");
                JToken jres = JsonConvert.DeserializeObject<JToken>(res);
                if (jres["meta"]["status"].ToString() == "200")
                {
                    return jres["data"];
                }
                else
                {
                    throw new Exception("Error: getStatusJson\n" + res);
                }
            }catch(Exception e)
            {
                throw new Exception("Error: getStatusJson:u136", e);
            }
        }
        private JToken checkFormat(Client client, string title, string desc)
        {
            JObject reqj = new JObject();
            reqj.Add("title", title);
            reqj.Add("description", System.Web.HttpUtility.HtmlEncode(desc));
            reqj.Add("signature", "");
            string res = client.getReqWithJson("https://www.upload.nicovideo.jp/v2/text/format", JsonConvert.SerializeObject(reqj), header: new Dictionary<string, string>() {
                    {"Accept-Language","ja,en-US;q=0.7,en;q=0.3" },
                    {"X-Frontend-Id","23" },
                    {"X-Frontend-Version","1.0.0" },
                    {"X-Request-With","N-garage" }
                }, referer: "https://www.upload.nicovideo.jp/garage/videos/" + videoid.ToString() + "/input");
            JToken jres = JsonConvert.DeserializeObject<JToken>(res);
            return jres;
        }
        private void getThumbnail(Client client)
        {
            string url = "https://www.upload.nicovideo.jp/v2/videos/"+videoid.ToString()+"/scene-thumbnails";
            string res = client.getReq(url, "", "POST", header: new Dictionary<string, string>() {
                    {"Accept-Language","ja,en-US;q=0.7,en;q=0.3" },
                    {"X-Frontend-Id","23" },
                    {"X-Frontend-Version","1.0.0" },
                    {"X-Request-With","N-garage" }
                }, referer: "https://www.upload.nicovideo.jp/garage/videos/" + videoid.ToString() + "/input");
            JToken jres = JsonConvert.DeserializeObject<JToken>(res);
        }
        private JToken sendMetaData(Client client, string id,string title, string datail, int mode, string community)
        {
            getThumbnail(client);
            string metas;
            if ((community == null || community == "") && mode != 2) { MKMeta meta = new MKMeta(int.Parse(id), title, datail, mode); metas = JsonConvert.SerializeObject(meta); }
            else { MKMetaC meta = new MKMetaC(int.Parse(id), title, datail, mode, community); metas = JsonConvert.SerializeObject(meta); }
            string jsons = metas;
            Console.WriteLine(jsons);
            string res = client.getReqWithJson("https://www.upload.nicovideo.jp/v2/videos/" + videoid.ToString(), jsons, header: new Dictionary<string, string>() {
                    {"Accept-Language","ja,en-US;q=0.7,en;q=0.3" },
                    {"X-Frontend-Id","23" },
                    {"X-Frontend-Version","1.0.0" },
                    {"X-Request-With","N-garage" }
                }, referer: "https://www.upload.nicovideo.jp/garage/videos/" + videoid.ToString() + "/input");
            return JsonConvert.DeserializeObject<JToken>(res);
        }
        public int PostVideo(Client client, string len, string color, string title, string desc, string community, int mode/*1 = koukai 0 = hikoukai 2 = community*/)
        {
            try
            {
                createPostRequest(client);
                getUploadChunkStream(client);
                string chunkUrl = "https://www.upload.nicovideo.jp" + getUploadChunkStream(client);
                getMyData(client);
                makeVideo(len, color);
                uploadVideoWithChunkStream(client, chunkUrl);
                //Console.WriteLine(getStatusJson(client));
                JToken td = checkFormat(client, title, desc);
                sendMetaData(client, videoid.ToString(), title, desc, mode, community);
                File.Delete("./mv/tmp.mp4");
                File.Delete("./mv/tmp.json");
                return videoid;
            }catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                return 0;
            }
        }
    }
    [JsonObject("community")]
    class MKCommunity
    {
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("maxVideoCount")]
        public int maxVideoCount { get; set; }
        [JsonProperty("title")]
        public string title { get; set; }
        [JsonProperty("videoCount")]
        public int videoCount { get; set; }
    }
    [JsonObject("MKTemp")]
    class MKTemp
    {
        [JsonProperty("len")]
        public string len { get; set; }
        [JsonProperty("color")]
        public string color { get; set; }
        public MKTemp(string len, string color)
        {
            this.len = len;
            this.color = color;
        }
    }
    [JsonObject("MKMetaC")]
    class MKMetaC
    {
        [JsonProperty("id")]
        public int id;
        [JsonProperty("title")]
        public string title;
        [JsonProperty("detail")]
        public string detail;
        [JsonProperty("signature")]
        public JObject signature = new JObject();
        [JsonProperty("publish")]
        public bool publish;
        [JsonProperty("publishTimer")]
        public JObject publishTimer = new JObject();
        [JsonProperty("community")]
        public JObject community = new JObject();
        [JsonProperty("tags")]
        public JArray tags = new JArray();
        [JsonProperty("genreKey")]
        public string genreKey;
        [JsonProperty("thumbnail")]
        public JObject thumbnail = new JObject();
        [JsonProperty("permissionSettings")]
        public JObject permissionSettings = new JObject();
        [JsonProperty("notification")]
        public JObject notification = new JObject();
        [JsonProperty("excludeFromUploadList")]
        public bool excludeFromUploadList;
        [JsonProperty("thanksMessage")]
        public JObject thanksMessage = new JObject();
        public MKMetaC(int i, string t, string d, int mode, string c)
        {
            id = i;
            title = t;
            detail = d;
            signature["display"] = false;
            publish = mode != 0;
            publishTimer["use"] = false;
            community["belong"] = c != null && c != "";
            community["id"] = (c != null && c != "") ? c.Substring(2) : "";
            community["communityOnly"] = mode == 2;
            community["groupWorkFlag"] = mode == 2;
            genreKey = "none";
            thumbnail["selectThumbnailIndex"] = 0;
            thumbnail["aspectBias"] = "STANDARD";
            thumbnail["cropMode"] = 0;
            thumbnail["position"] = 0;
            permissionSettings["allowNgShareFlag"] = true;
            permissionSettings["allowOutsidePlayerFlag"] = true;
            permissionSettings["allowUadFlag"] = true;
            permissionSettings["allowNicoliveFlag"] = true;
            permissionSettings["allowUserTranslateFlag"] = true;
            permissionSettings["allowRegularUserTagEditFlag"] = true;
            notification["email"] = false;
            excludeFromUploadList = false;
            thanksMessage["isVisible"] = false;
            thanksMessage["content"] = "";
        }
    }
    [JsonObject("MKMeta")]
    class MKMeta
    {
        [JsonProperty("id")]
        public int id;
        [JsonProperty("title")]
        public string title;
        [JsonProperty("detail")]
        public string detail;
        [JsonProperty("signature")]
        public JObject signature = new JObject();
        [JsonProperty("publish")]
        public bool publish;
        [JsonProperty("publishTimer")]
        public JObject publishTimer = new JObject();
        [JsonProperty("tags")]
        public JArray tags = new JArray();
        [JsonProperty("genreKey")]
        public string genreKey;
        [JsonProperty("thumbnail")]
        public JObject thumbnail = new JObject();
        [JsonProperty("permissionSettings")]
        public JObject permissionSettings = new JObject();
        [JsonProperty("notification")]
        public JObject notification = new JObject();
        [JsonProperty("excludeFromUploadList")]
        public bool excludeFromUploadList;
        [JsonProperty("thanksMessage")]
        public JObject thanksMessage = new JObject();
        public MKMeta(int i, string t, string d, int mode)
        {
            id = i;
            title = t;
            detail = d;
            signature["display"] = false;
            publish = mode != 0;
            publishTimer["use"] = false;
            genreKey = "none";
            thumbnail["selectThumbnailIndex"] = 0;
            thumbnail["aspectBias"] = "STANDARD";
            thumbnail["cropMode"] = 0;
            thumbnail["position"] = 0;
            permissionSettings["allowNgShareFlag"] = true;
            permissionSettings["allowOutsidePlayerFlag"] = true;
            permissionSettings["allowUadFlag"] = true;
            permissionSettings["allowNicoliveFlag"] = true;
            permissionSettings["allowUserTranslateFlag"] = true;
            permissionSettings["allowRegularUserTagEditFlag"] = true;
            notification["email"] = false;
            excludeFromUploadList = false;
            thanksMessage["isVisible"] = false;
            thanksMessage["content"] = "";
        }
    }
}
