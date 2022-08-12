using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NicoCommentTransfer.API
{
    [JsonObject("Config")]
    class Config
    {
        public string[] okversion = new string[]
        {
            "1.4.0.0"
        };
        [JsonProperty("version")]
        public string version { get; set; }
        [JsonProperty("isLogin")]
        public bool isLogin { get; set; } = false;
        [JsonProperty("loginSession")]
        public string loginSession { get; set; } = "";
        [JsonProperty("loginSecure")]
        public string loginSecure { get; set; } = "";
        [JsonProperty("CookieExpires")]
        public long CookieExpires { get; set; } = 0;
        [JsonProperty("authToken")]
        public string authToken { get; set; } = "";
        [JsonProperty("userID")]
        public string userID { get; set; } = "0";
        [JsonProperty("isPremium")]
        public bool isPremium { get; set; } = false;
        [JsonProperty("checkPati")]
        public bool checkPati { get; set; } = true;
        [JsonProperty("checkCa")]
        public bool checkCa { get; set; } = true;
        [JsonProperty("check184")]
        public bool check184 { get; set; } = false;
        [JsonProperty("checkMigiDelete")]
        public bool checkMigiDelete { get; set; } = true;
        [JsonProperty("checkMigiUnCheck")]
        public bool checkMigiUnCheck { get; set; } = true;
        [JsonProperty("checkPostDelete")]
        public bool checkPostDelete { get; set; } = false;
        [JsonProperty("checkTokomeAdd")]
        public bool checkTokomeAdd { get; set; } = true;
        [JsonProperty("checkMymemory")]
        public bool checkMymemory { get; set; } = false;
        public Config()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName asmName = assembly.GetName();
            version = asmName.Version.ToString();
            isLogin = false;
            loginSession = "";
            loginSecure = "";
            CookieExpires = 0;
            authToken = "";
            userID = "0";
            isPremium = false;
            checkPati = true;
            checkCa = true;
            check184 = false;
            checkMigiDelete = true;
            checkMigiUnCheck = true;
            checkPostDelete = false;
            checkTokomeAdd = true;
            checkMymemory = false;
        }
        public Config(Config c)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName asmName = assembly.GetName();
            version = asmName.Version.ToString();
            isLogin = c.isLogin;
            loginSession = c.loginSecure;
            loginSecure = c.loginSecure;
            CookieExpires = c.CookieExpires;
            authToken = c.authToken;
            userID = c.userID;
            isPremium = c.isPremium;
            checkPati = c.checkPati;
            checkCa = c.checkCa;
            check184 = c.check184;
            checkMigiDelete = c.checkMigiDelete;
            checkMigiUnCheck = c.checkMigiUnCheck;
            checkPostDelete = c.checkPostDelete;
            checkTokomeAdd = c.checkTokomeAdd;
            checkMymemory = c.checkMymemory;
        }
        public bool CheckVersion()
        {
            bool isok = okversion.ToList().Contains(version);
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName asmName = assembly.GetName();
            if (isok) version = asmName.Version.ToString();
            return isok;
        }
        public void Save()
        {
            using (StreamWriter sw = new StreamWriter("config.json", false, Encoding.UTF8))
            {
                sw.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
}
