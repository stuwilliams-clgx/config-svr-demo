using System;
using System.Collections.Generic;

namespace ConfigServerLibrary
{
    public interface IConfigServerClient
    {
        Uri ConfigServerAddress { get; set; }

        Dictionary<string, string> GetAll();
        Dictionary<string, string> GetApp(string app);
        Dictionary<string, string> GetAppEnv(string app, string env);
        Dictionary<string, string> GetGlobal();
    }
}