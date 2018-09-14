using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ConfigServerLibrary.Lib
{
    /// <summary>
    /// Helper: Configuration from Config Server
    /// </summary>
    public class ConfigHelper
    {

        private ConfigHelper() { }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="webRoot">UNC path to web root</param>
        public ConfigHelper(string webRoot)
        {
            if (string.IsNullOrWhiteSpace(webRoot)) throw new ArgumentNullException("webRoot", "Required Parameter");
            this.WebRoot = webRoot;
        }

        /// <summary>
        /// Web Root as a UNC path e.g. w. drive + path...
        /// </summary>
        public string WebRoot { get; set; }

        private List<Models.ConfigurationModel> _config = null;

        /// <summary>
        /// Clear configuration
        /// </summary>
        public void Clear()
        {
            _config = null;
        }

        /// <summary>
        /// Get the configration from Config Server
        /// </summary>
        public IEnumerable<Models.ConfigurationModel> Get(string appName, string env = "")
        {
            if (_config == null)
            {
                FetchConfig(appName, env);
            }
            return _config;
        }

        private void FetchConfig(string appName, string env = "")
        {
            _config = new List<Models.ConfigurationModel>();
            var cs = ConfigurationServerSettings();
            GetToken(ref cs);
            GetConfiguration(cs, appName, env);
        }

        private const string VCAP_EnvironmentVariable = "VCAP_SERVICES";

        private Models.ConfigurationServerSettings ConfigurationServerSettings()
        {
            var cs = new Models.ConfigurationServerSettings()
            {
                SecurityToken = string.Empty
            };

            var vcap = Environment.GetEnvironmentVariable(VCAP_EnvironmentVariable);
            if (string.IsNullOrEmpty(vcap))
            {
                throw new InvalidOperationException($"{VCAP_EnvironmentVariable} is empty");
            }

            JObject jo = JObject.Parse(vcap);

            var svr = jo.SelectToken("$..p-config-server");

            if (svr != null)
            {
                var credsTokens = svr.SelectToken("$..credentials");
                var list = credsTokens.Children();
                foreach (var item in list)
                {
                    var key = ((Newtonsoft.Json.Linq.JProperty)item).Name;
                    var value = ((Newtonsoft.Json.Linq.JProperty)item).Value.ToString();
                    switch (key.ToLowerInvariant())
                    {
                        case "access_token_uri":
                            cs.Access_Token_Uri = value;
                            break;
                        case "client_id":
                            cs.Client_Id = value;
                            break;
                        case "client_secret":
                            cs.Client_Secret = value;
                            break;
                        case "uri":
                            cs.Configuration_Server_Url = value;
                            break;
                    }
                }
            }

            return cs;
        }

        private void GetToken(ref Models.ConfigurationServerSettings cs)
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri(cs.Access_Token_Uri);
                var requestPath = uri.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped);
                client.BaseAddress = new Uri(uri.GetLeftPart(UriPartial.Authority));

                client.Timeout = new TimeSpan(0, 0, 3000);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36");

                var clientCreds = string.Format("{0}:{1}", cs.Client_Id, cs.Client_Secret);
                var byteArray = Encoding.UTF8.GetBytes(clientCreds);
                var b64 = Convert.ToBase64String(byteArray);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", b64);

                var pairs = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                    };

                var content = new FormUrlEncodedContent(pairs);

                var response = client.PostAsync(requestPath, content).Result;
                if (!response.IsSuccessStatusCode) throw new HttpRequestException(string.Format("GetToken: {0} {1}", response.StatusCode, response.ReasonPhrase));

                var text = response.Content.ReadAsStringAsync().Result;

                JObject jo = JObject.Parse(text);

                var tobj = jo.SelectToken("$..access_token");
                var value = ((Newtonsoft.Json.Linq.JValue)tobj).Value.ToString();

                cs.SecurityToken = value;
            }
        }

        private void GetConfiguration(Models.ConfigurationServerSettings cs, string appName, string env = "")
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(cs.Configuration_Server_Url);
                client.Timeout = new TimeSpan(0, 0, 3000);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + cs.SecurityToken);

                var response = client.GetAsync("/" + appName + "/" + env).Result;
                if (!response.IsSuccessStatusCode) throw new HttpRequestException(string.Format("GetConfiguration: {0} {1}", response.StatusCode, response.ReasonPhrase));

                var text = response.Content.ReadAsStringAsync().Result;

                JObject jo = JObject.Parse(text);

                var ps = jo.SelectTokens("$..propertySources");

                foreach (var src in ps)
                {
                    var tlist = src.Children();

                    foreach (var titem in tlist)
                    {
                        string source = string.Empty;
                        foreach (var item in titem.Children())
                        {
                            var name = ((Newtonsoft.Json.Linq.JProperty)item).Name;

                            switch (name.ToLowerInvariant())
                            {
                                case "name":
                                    source = ((Newtonsoft.Json.Linq.JProperty)item).Value.ToString();
                                    break;

                                case "source":
                                    var coll = ((Newtonsoft.Json.Linq.JObject)((Newtonsoft.Json.Linq.JContainer)item).First).Children();
                                    foreach (var child in coll)
                                    {
                                        var key = ((Newtonsoft.Json.Linq.JProperty)child).Name;
                                        var value = ((Newtonsoft.Json.Linq.JProperty)child).Value.ToString();
                                        _config.Add(new Models.ConfigurationModel()
                                        {
                                            Source = source,
                                            Name = key,
                                            Value = value
                                        });
                                    }
                                    break;
                            }
                        }
                    }
                }

            }
        }
    }
}
