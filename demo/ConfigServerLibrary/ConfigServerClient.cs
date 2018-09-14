using System;
using System.Collections.Generic;

namespace ConfigServerLibrary
{
    public  class ConfigServerClient : IConfigServerClient
    {
        public ConfigServerClient() {

        }

        public Uri ConfigServerAddress { get; set; }

        public ConfigServerClient(Uri configServerAddress)
        {
            this.ConfigServerAddress = configServerAddress;
        }

        public Dictionary<string,string> GetAll()
        {
            var d = new Dictionary<string, string>();

            return d;
        }

        public Dictionary<string, string> GetGlobal()
        {
            var d = new Dictionary<string, string>();

            return d;
        }

        public Dictionary<string, string> GetApp(string app)
        {
            var d = new Dictionary<string, string>();

            return d;
        }


        public Dictionary<string, string> GetAppEnv(string app, string env)
        {
            var d = new Dictionary<string, string>();

            return d;
        }
    }
}
